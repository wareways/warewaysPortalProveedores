using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Wareways.PortalProv.Reportes
{
    public partial class chkUtil : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            

            if (!IsPostBack)
            {
                foreach (ConnectionStringSettings c in System.Configuration.ConfigurationManager.ConnectionStrings)
                {
                    lstConnections.Items.Add(c.Name);
                }
            }
        }

        protected void getDBinfo(string constring, string qry, ListBox lst)
        {
            // get the db info
            SqlConnection myConn = new SqlConnection(constring);
            SqlDataReader reader;
            SqlCommand cmd = new SqlCommand(qry, myConn);
            cmd.CommandType = CommandType.Text;
            myConn.Open();
            try
            {
                lblInfo.Text = string.Empty;
                reader = cmd.ExecuteReader();
                lst.DataSource = reader;
                lst.DataTextField = "Name";
                lst.DataBind();
            }
            catch (Exception ex)
            {
                lblInfo.Text = ex.ToString();
            }
            finally
            {
                if (myConn != null)
                    myConn.Dispose();
            }
        }

        protected void lstDBConnections(object sender, EventArgs e)
        {
            string db = lstConnections.SelectedItem.Text;
            string constring = WebConfigurationManager.ConnectionStrings[db].ToString();
            string qry = "";
            string table = "%";
            hdnConstring.Value = constring;
            // fetch info from table
            qry = "SELECT Name from Sysobjects where xtype = 'u'";
            getDBinfo(constring, qry, lstTables);
            // fetch info from Fields
            qry = "select c.name as Name from sys.columns c inner join sys.tables t on t.object_id = c.object_id and t.name like '" + table + "' and t.type = 'U'";
            getDBinfo(constring, qry, lstFields);
            // fetch info from Stored Procedures
            qry = "select name from sys.procedures";
            getDBinfo(constring, qry, lstSP);
            // fetch info from Views
            qry = "select name from sys.views";
            getDBinfo(constring, qry, lstViews);
        }

        protected void lstTableSelect(object sender, EventArgs e)
        {
            string constring = hdnConstring.Value;
            // list all the table fields  
            string qry = "select c.name as Name from sys.columns c inner join sys.tables t on t.object_id = c.object_id and t.name like '" + lstTables.SelectedItem.Text + "' and t.type = 'U'";
            getDBinfo(constring, qry, lstFields);
        }

        protected void lstSPSelect(object sender, EventArgs e)
        {
            string constring = hdnConstring.Value;
            // get the Stored Procedure definition  
            string qry = "select ROUTINE_DEFINITION from INFORMATION_SCHEMA.ROUTINES Where ROUTINE_NAME='" + lstSP.SelectedItem.Text + "'";
            getSchemaInfo(constring, qry, txtQuery);
        }

        protected void lstViewsSelect(object sender, EventArgs e)
        {
            string constring = hdnConstring.Value;
            // get the View definition  
            string qry = "select VIEW_DEFINITION from INFORMATION_SCHEMA.VIEWS Where TABLE_NAME='" + lstViews.SelectedItem.Text + "'";
            getSchemaInfo(constring, qry, txtQuery);
            // fields: SELECT * FROM INFORMATION_SCHEMA.COLUMNS where table_name='exportdb' and column_name='country'
        }

        protected void getSchemaInfo(string constring, string qry, TextBox txt)
        {
            // used for retrieving the db schema info rendering it in a label
            constring = hdnConstring.Value;
            SqlConnection myConn = new SqlConnection(constring);
            SqlDataReader reader;
            SqlCommand cmd = new SqlCommand(qry, myConn);
            cmd.CommandType = CommandType.Text;
            myConn.Open();
            try
            {
                lblInfo.Text = string.Empty;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    txt.Text = reader.GetString(0);
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = ex.ToString();
            }
            finally
            {
                if (myConn != null)
                    myConn.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

        }

        private void QueryDataBind(GridView grd)
        {
            // grid databind
            string constring = hdnConstring.Value;
            SqlConnection myConn = new SqlConnection(constring);
            SqlDataReader reader;
            string qry = txtQuery.Text;
            SqlCommand cmd = new SqlCommand(qry, myConn);
            cmd.CommandType = CommandType.Text;
            myConn.Open();
            try
            {
                lblInfo.Text = string.Empty;
                reader = cmd.ExecuteReader();
                grd.DataSource = reader;
                grd.DataBind();
            }
            catch (Exception ex)
            {
                lblInfo.Text = ex.ToString();
                //throw;
            }
            finally
            {
                if (myConn != null)
                    myConn.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }
        }

        protected void btnCommitClick(object sender, EventArgs e)
        {
            lblInfo.Text = "";
            grdResult.DataSource = null;
            grdResult.DataBind();

            this.QueryDataBind(grdResult);
        }

        protected void ExportToExcel(GridView grd)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=sqlTableExport.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";
            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                //To Export all pages
                grd.HeaderRow.BackColor = Color.White;
                foreach (TableCell cell in grd.HeaderRow.Cells)
                {
                    cell.BackColor = grd.HeaderStyle.BackColor;
                }
                foreach (GridViewRow row in grd.Rows)
                {
                    row.BackColor = Color.White;
                    foreach (TableCell cell in row.Cells)
                    {
                        if (row.RowIndex % 2 == 0)
                        {
                            cell.BackColor = grd.AlternatingRowStyle.BackColor;
                        }
                        else
                        {
                            cell.BackColor = grd.RowStyle.BackColor;
                        }
                        cell.CssClass = "textmode";
                    }
                }
                grd.RenderControl(hw);
                //style to format numbers to string
                string style = @"<style> .textmode { } </style>";
                Response.Write(style);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            lblInfo.Text = "";
            grdResult.DataSource = null;
            grdResult.DataBind();

        }

        protected void btnExport_Click(object sender, EventArgs e)
        {

        }
    }
}