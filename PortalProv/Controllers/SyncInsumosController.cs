using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers
{
    public class SyncInsumosController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vServicio = new VServicio();


        // GET: SyncInsumos
        public ActionResult Index()
        {
            var syncPedniente = _Db.V_PPROV_InsumosPendientes.ToList();
            foreach (var oPendiente in syncPedniente)
            {
                var detect = _Db.PPROV_Documento.Where(p => p.Doc_Autorizacion == oPendiente.U_FELUUID && p.Doc_Estado != "Rechazado").ToList();
                if (detect.Count == 0)
                {
                    // No se encontro UUID
                    var nuevo = new PPROV_Documento();
                    nuevo.Doc_Id = Guid.NewGuid();
                    nuevo.Doc_CardCorde = oPendiente.EM_CardCode;
                    nuevo.Doc_CardName = oPendiente.EM_CardName;
                    nuevo.Doc_Estado = "Enviado";
                    nuevo.Doc_Numero = oPendiente.U_FELNumero;
                    nuevo.Doc_Serie = oPendiente.U_FELSerie;
                    nuevo.Doc_Fecha = DateTime.Parse( oPendiente.U_FELFecha);
                    nuevo.Doc_EmpresaId = 1;
                    nuevo.Doc_MontoNeto = oPendiente.FAP_Doctotal;
                    nuevo.Doc_Moneda = oPendiente.FAP_DocCur;
                    nuevo.Doc_Autorizacion = oPendiente.U_FELUUID;
                    nuevo.Doc_Observaciones = "";
                    nuevo.Doc_TipoDocumento = "Fact";
                    nuevo.Doc_UsuarioCarga = "insumos@aki.com.gt";
                    nuevo.Doc_FechaCarga = DateTime.Now;
                    nuevo.Doc_NumeroOC = oPendiente.OC_DocNum;

                    nuevo.Doc_PdfCotiza = "";
                    nuevo.Doc_PdfFactura = DescargarFactInsumos(oPendiente.FAP_DocNum); // Implementar
                    nuevo.Doc_PdfOC = DescargarOC(oPendiente.OC_DocNum); // Implementar
                    nuevo.Doc_OC_Multiple = oPendiente.OC_DocNum.ToString();
                    nuevo.Doc_EM_Multiple = oPendiente.EM_DocNum.ToString();
                    nuevo.Doc_EM_MontoSum = oPendiente.EM_DocTotal;
                    nuevo.Doc_ActivoFijo = false;
                    nuevo.Doc_OC_TipoDoc = "Servicio";

                    _Db.PPROV_Documento.Add(nuevo);
                    _Db.SaveChanges();
                    vServicio.UpdateOPDN_Insumos(oPendiente.EM_DocNum, nuevo.Doc_Id.ToString());

                }
                else
                {
                    vServicio.UpdateOPDN_Insumos(oPendiente.EM_DocNum, detect[0].Doc_Id.ToString());

                    // Se encontro UUID
                }

            
            }
            return View();
        }

        private string DescargarOC(int? oC_DocNum)
        {
            var Nombretemp ="~/Cargados/" + Guid.NewGuid().ToString() + ".pdf";
            var localFilePath = Server.MapPath(Nombretemp);
            String url = string.Format(ConfigurationManager.AppSettings["LinkOcInsumos"], oC_DocNum); // Replace with your URL
            //string localFilePath = "downloadedFile2.pdf";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        DownloadStreamToFile(responseStream, localFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during HTTP request: {ex.Message}");
            }

            return Nombretemp.Replace("~","");
        }

        private string DescargarFactInsumos(int? oC_DocNum)
        {
            var Nombretemp = "~/Cargados/" + Guid.NewGuid().ToString() + ".pdf";
            var localFilePath = Server.MapPath(Nombretemp);
            String url = string.Format(ConfigurationManager.AppSettings["LinkFactInsumos"], oC_DocNum); // Replace with your URL
            //string localFilePath = "downloadedFile2.pdf";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        DownloadStreamToFile(responseStream, localFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during HTTP request: {ex.Message}");
            }

            return Nombretemp.Replace("~", "");
        }

        public static void DownloadStreamToFile(Stream sourceStream, string filePath)
        {
            try
            {
                // Create a FileStream to write the downloaded data
                using (FileStream destinationStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192]; // Buffer size for reading in chunks
                    int bytesRead;

                    // Read from the source stream and write to the destination stream
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, bytesRead);
                    }
                }

                Console.WriteLine($"Stream successfully downloaded to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading stream: {ex.Message}");
            }
            finally
            {
                // Ensure the source stream is closed (if it's not managed by a 'using' statement elsewhere)
                if (sourceStream != null)
                {
                    sourceStream.Close();
                }
            }
        }
    }
}