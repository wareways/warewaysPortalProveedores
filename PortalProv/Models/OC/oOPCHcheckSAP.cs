using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.OC
{
    public class oOPCHcheckSAP
    {
        public Int32 DocEntry { get; set; }
        public Int32 DocNum { get; set; }
        public string U_WW_SyncId { get; set; }

        public string U_WW_SyncDate { get; set; }

        public Int32 TransId { get; set; }
    }
}