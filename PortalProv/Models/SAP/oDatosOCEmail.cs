using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.SAP
{
    public class oDatosOCEmail
    {
        public int DocEntry { get; set; }

        public int DocNum { get; set; }

        public String NumAtCard { get; set; }

        public DateTime TaxDate { get; set; }

        public String EsAKIol { get; set; }

        public String E_Mail { get; set; }
    }
}