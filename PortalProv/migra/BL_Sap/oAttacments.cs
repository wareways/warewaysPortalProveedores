using ConnectUNCWithCredentials;
using Sap_Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;

using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;

/// <summary>
/// Summary description for oAttacments
/// </summary>
public class oAttacments
{
    public oAttacments()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public string EntregaCompras_AgregarArchivos(Int32 DocEntry, String UbicacionArchivo, Int32 EmpresaSap)
    {
        var _Retorna = "";
        SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany(EmpresaSap);

        using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
        {
            var uncpath = @"\\10.0.2.213\sapimagenes";
            if (unc.NetUseWithCredentials(uncpath, "Administrator", "", "m64$hp3$h"))
            {
                SAPbobsCOM.Documents oPurchaseDeliveryNotes = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
                Boolean _EntregaEncontrada = oPurchaseDeliveryNotes.GetByKey(DocEntry);
                SAPbobsCOM.Attachments2 oAtt = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oAttachments2) as SAPbobsCOM.Attachments2;
                if (oPurchaseDeliveryNotes.AttachmentEntry == 0)
                {
                    oAtt.Lines.SourcePath = Path.GetDirectoryName(UbicacionArchivo);
                    oAtt.Lines.FileName = Path.GetFileName(UbicacionArchivo).Split('.')[0];
                    oAtt.Lines.FileExtension = Path.GetExtension(UbicacionArchivo).Replace(".", "");
                    oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                    var _Resultado = oAtt.Add();
                    if (_Resultado == 0)
                    {
                        var iAttEntry = int.Parse(oCompany.GetNewObjectKey());
                        oPurchaseDeliveryNotes.AttachmentEntry = iAttEntry;
                        var _Resultado2 = oPurchaseDeliveryNotes.Update();
                        if (_Resultado2 == 0)
                        {

                        }
                        else
                        {
                            _Retorna = oCompany.GetLastErrorDescription();
                        }
                    }
                    else
                    {
                        _Retorna = oCompany.GetLastErrorDescription();
                    }

                }
                else
                {
                    oAtt.GetByKey(oPurchaseDeliveryNotes.AttachmentEntry);
                    oAtt.Lines.Add();
                    oAtt.Lines.SourcePath = Path.GetDirectoryName(UbicacionArchivo);
                    oAtt.Lines.FileName = Path.GetFileName(UbicacionArchivo).Split('.')[0];
                    oAtt.Lines.FileExtension = Path.GetExtension(UbicacionArchivo).Replace(".", "");
                    oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                    var _Resultado = oAtt.Update();
                }
            }
            else
            {
                // The connection has failed. Use the LastError to get the system error code
                _Retorna ="Failed to connect to " + uncpath +
                                "\r\nLastError = " + unc.LastError.ToString();
            }
            // When it reaches the end of the using block, the class deletes the connection.
        }
      
     

        oCompany.Disconnect();

        return _Retorna;
    }

}

namespace ConnectUNCWithCredentials
{
    public class UNCAccessWithCredentials : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct USE_INFO_2
        {
            internal LPWSTR ui2_local;
            internal LPWSTR ui2_remote;
            internal LPWSTR ui2_password;
            internal DWORD ui2_status;
            internal DWORD ui2_asg_type;
            internal DWORD ui2_refcount;
            internal DWORD ui2_usecount;
            internal LPWSTR ui2_username;
            internal LPWSTR ui2_domainname;
        }

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseAdd(
            LPWSTR UncServerName,
            DWORD Level,
            ref USE_INFO_2 Buf,
            out DWORD ParmError);

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseDel(
            LPWSTR UncServerName,
            LPWSTR UseName,
            DWORD ForceCond);

        private bool disposed = false;

        private string sUNCPath;
        private string sUser;
        private string sPassword;
        private string sDomain;
        private int iLastError;

        /// <summary>
        /// A disposeable class that allows access to a UNC resource with credentials.
        /// </summary>
        public UNCAccessWithCredentials()
        {
        }

        /// <summary>
        /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
        /// </summary>
        public int LastError
        {
            get { return iLastError; }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                NetUseDelete();
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects to a UNC path using the credentials supplied.
        /// </summary>
        /// <param name="UNCPath">Fully qualified domain name UNC path</param>
        /// <param name="User">A user with sufficient rights to access the path.</param>
        /// <param name="Domain">Domain of User.</param>
        /// <param name="Password">Password of User</param>
        /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
        public bool NetUseWithCredentials(string UNCPath, string User, string Domain, string Password)
        {
            sUNCPath = UNCPath;
            sUser = User;
            sPassword = Password;
            sDomain = Domain;
            return NetUseWithCredentials();
        }

        private bool NetUseWithCredentials()
        {
            uint returncode;
            try
            {
                USE_INFO_2 useinfo = new USE_INFO_2();

                useinfo.ui2_remote = sUNCPath;
                useinfo.ui2_username = sUser;
                useinfo.ui2_domainname = sDomain;
                useinfo.ui2_password = sPassword;
                useinfo.ui2_asg_type = 0;
                useinfo.ui2_usecount = 1;
                uint paramErrorIndex;
                returncode = NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
                iLastError = (int)returncode;
                return returncode == 0;
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        /// <summary>
        /// Ends the connection to the remote resource 
        /// </summary>
        /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
        public bool NetUseDelete()
        {
            uint returncode;
            try
            {
                returncode = NetUseDel(null, sUNCPath, 2);
                iLastError = (int)returncode;
                return (returncode == 0);
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        ~UNCAccessWithCredentials()
        {
            Dispose();
        }

    }
}
