using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.SAP
{
    public class OSapDoc
    {
        public int DocEntry { get; set; }
        public int Docnum { get; set; }
        public String DocStatus { get; set; }
        public String CANCELED { get; set; }

        public String DocCur { get; set; }
        public String ObjType { get; set; }


        public String CardCode { get; set; }
        public String CardName { get; set; }
        public String NumAtCard { get; set; }
        public String DocType { get; set; }



        public String Comments { get; set; }

        public Decimal DocTotal { get; set; }
        public Decimal VatSum { get; set; }

        public String U_FacNit { get; set; }



    }
}