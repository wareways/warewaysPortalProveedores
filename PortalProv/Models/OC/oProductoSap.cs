using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.OC
{
    public class oProductoSap
    {
        public String WhsCode { get; set; }
        public String ItemCode { get; set; }
        public String ItemName { get; set; }
        public String UOM { get; set; }

        public Decimal AvgPrice { get; set; }
    }
}