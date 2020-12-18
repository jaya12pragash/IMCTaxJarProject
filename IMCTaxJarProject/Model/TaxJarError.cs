using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
namespace IMCTaxJarProject.Model
{
    public class TaxjarError
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("status")]
        public string StatusCode { get; set; }
    }
}
