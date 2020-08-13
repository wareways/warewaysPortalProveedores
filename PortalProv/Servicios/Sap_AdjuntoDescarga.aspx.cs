using ConnectUNCWithCredentials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Wareways.PortalProv.Servicios
{
    public partial class Sap_AdjuntoDescarga : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["FileName"] != null)
            {
                using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
                {
                    var uncpath = @"\\10.0.2.213\sapimagenes";
                    if (unc.NetUseWithCredentials(uncpath, "Administrator", "", "m64$hp3$h"))
                    {
                        lblMensaje.Visible = true;

                        // Read the file and convert it to Byte Array
                        string filePath = "";
                        string filename = HttpUtility.UrlDecode(Request.QueryString["FileName"]);
                        string contenttype = "application/" + Path.GetExtension(filename).Replace(".", "");
                        if (File.Exists(filePath + filename))
                        {
                            FileStream fs = new FileStream(filePath + filename, FileMode.Open, FileAccess.Read);
                            BinaryReader br = new BinaryReader(fs);
                            Byte[] bytes = br.ReadBytes((Int32)fs.Length);
                            br.Close();
                            fs.Close();

                            //Write the file to response Stream
                            Response.Buffer = true;
                            Response.Charset = "";
                            Response.Cache.SetCacheability(HttpCacheability.NoCache);
                            Response.ContentType = contenttype;
                            Response.AddHeader("content-disposition", "inline;filename=" + filename);
                            Response.BinaryWrite(bytes);
                            Response.Flush();
                            Response.End();
                        }
                        else
                        {
                            lblMensaje.Visible = true;
                        }
                    }

                }
            }
        }
    }
}