using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Wareways.PortalProv.Controllers.Api
{
    public class EmisionFEController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [System.Web.Http.HttpPost]
        public String Post([FromBody] Data formData)
        {
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();

            var oData = Newtonsoft.Json.JsonConvert.DeserializeObject<Datas>(bodyText);

         
            //string decryptedApiKey2 = Decrypt(bytes, pass, iv);
           


            //string decryptedApiKey = AesDecryptionService.Decrypt(oData.Data.infileCredentials.apiKey);
            
            //string decryptedApiKey = Aes256CbcEncrypter.Decrypt(Base64Encode(oData.Data.infileCredentials.apiKey), "Z4t@9x!Lz1wG@pB2$F4cV$XoR1ZdT#K6");

            return "ok";
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

    }

    public class AesDecryptionService
    {
        private static readonly string SecretKey = "Z4t@9x!Lz1wG@pB2$F4cV$XoR1ZdT#K6"; // 32 caracteres

        

        
    }

    public class Datas
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Invoice invoice { get; set; }
        public InfileCredentials infileCredentials { get; set; }
    }

    public class Detail
    {
        public string description { get; set; }
        public int quantity { get; set; }
        public int subTotal { get; set; }
        public int tax { get; set; }
        public int total { get; set; }
    }

    public class InfileCredentials
    {
        public string apiKey { get; set; }
        public string apiUser { get; set; }
    }

    public class Invoice
    {
        public string invoiceId { get; set; }
        public string invoiceDate { get; set; }
        public string invoiceNit { get; set; }
        public int subTotal { get; set; }
        public int tax { get; set; }
        public int total { get; set; }
        public string clientId { get; set; }
        public List<Detail> detail { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }

}