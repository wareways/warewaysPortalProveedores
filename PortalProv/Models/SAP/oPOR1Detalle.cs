using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.SAP
{
    public class oPOR1Detalle
    {
        public int Docnum { get; set; }
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string Dscription { get; set; }
        public Decimal PriceAfVAT { get; set; }
        public string AcctCode { get; set; }
        public string OcrCode { get; set; }
        public string TaxCode { get; set; }
        public string Currency { get; set; }
        public string ItemCode { get; set; }
        public Decimal Quantity { get; set; }
    }
}