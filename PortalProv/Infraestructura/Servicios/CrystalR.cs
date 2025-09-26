using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;

namespace Wareways.PortalProv.Infraestructura.Servicios
{
    public class CrystalR
    {
        public void GenerarOC(string GenerarOC, string listRecipientes, Models.SAP.oDatosOCEmail oDatosOCEmail, String serverPath)
        {

            ReportDocument Report = new ReportDocument();
            var ArchivoReporte = serverPath +@"Infraestructura\Reportes\OrdenCompraAKI.rpt";


            Report.Load(ArchivoReporte);


            string fileName = String.Format("OC{0}_{1}_{2}.pdf", "1234", "Operadora", DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
            Report.SummaryInfo.ReportTitle = "ImpresionOC";

            Report.DataDefinition.RecordSelectionFormula = "{Tbl_OrdenCompra.Num_OrdenCompra}='" + GenerarOC + "'";


            System.Data.Common.DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
            dbConnectionStringBuilder.ConnectionString = ConfigurationManager.ConnectionStrings["AKIOLDB"].ConnectionString;

            ConnectionInfo crConnectionInfo = new ConnectionInfo();
            crConnectionInfo.ServerName = (String)dbConnectionStringBuilder["Data Source"];
            crConnectionInfo.DatabaseName = (String)dbConnectionStringBuilder["Initial Catalog"];
            crConnectionInfo.UserID = (String)dbConnectionStringBuilder["User ID"];
            crConnectionInfo.Password = (String)dbConnectionStringBuilder["Password"];
            crConnectionInfo.IntegratedSecurity = false;

            TableLogOnInfo crTableLogoninfo = new TableLogOnInfo();

            foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in Report.Database.Tables)
            {
                crTableLogoninfo = CrTable.LogOnInfo;
                crTableLogoninfo.ConnectionInfo = crConnectionInfo;
                CrTable.ApplyLogOnInfo(crTableLogoninfo);
            }
            foreach (ReportDocument subreport in Report.Subreports)
            {
                foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in subreport.Database.Tables)
                {
                    crTableLogoninfo = CrTable.LogOnInfo;
                    crTableLogoninfo.ConnectionInfo = crConnectionInfo;
                    CrTable.ApplyLogOnInfo(crTableLogoninfo);
                }
            }


            string body = string.Empty;
            string title = string.Format("Portal Proveedores AKI - Orden Compra Generada {0}- {1}", GenerarOC, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            var htmlcorreo = serverPath +@"Infraestructura\Reportes\FomratoCorreoSTD.html";
            StreamReader reader2 = new StreamReader(htmlcorreo);
            body = reader2.ReadToEnd();



            // Leer Formato Standard


            body = body.Replace("**NumeroOrdenGenerada**", GenerarOC);
            body = body.Replace("**FechaOrdenGenerada**", oDatosOCEmail.TaxDate.ToString("dd/MM/yyyy") );
            


            foreach (var Recipiente in listRecipientes.Split(';'))
            {
                SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

                using (var mailClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port))
                {
                    mailClient.UseDefaultCredentials = false;
                    mailClient.EnableSsl = true;
                    mailClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);



                    using (var message = new MailMessage())
                    {
                        message.To.Add(new MailAddress(Recipiente));
                        try { message.Bcc.Add(new MailAddress(ConfigurationManager.AppSettings["CorreoCopiaAKIOL"])); } catch { }

                        message.Body = body;
                        message.Subject = title;



                        message.IsBodyHtml = true;

                        System.IO.Stream s = Report.ExportToStream(ExportFormatType.PortableDocFormat);
                        s.Seek(0, System.IO.SeekOrigin.Begin);

                        message.Attachments.Add(new Attachment(s, fileName));

                        mailClient.Send(message);
                    }
                }
            }

        }
    }
}