using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.SAP
{
    public class OSapDetalle
    {
        public int LineNum { get; set; }
        public decimal Quantity { get; set; }
        public String ItemCode { get; set; }                
        public String Dscription { get; set; }
        public decimal PriceAfVAT { get; set; }
        public decimal LineTotal { get; set; }
        public String AcctCode { get; set; }

        public String WhsCode { get; set; }
        public String unitMsr { get; set; }

        public Decimal VatSum { get; set; }

        public String OcrCode { get; set; }

        public String TaxCode { get; set; }

    }
}