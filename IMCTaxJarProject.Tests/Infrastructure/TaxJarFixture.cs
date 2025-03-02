﻿using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace IMCTaxJarProject.Tests.Infrastructure
{
    public class TaxjarFixture
    {
        public static string GetJSON(string fixturePath)
        {
            using (StreamReader file = File.OpenText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../", "Fixtures", fixturePath)))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject response = (JObject)JToken.ReadFrom(reader);
                var resString = response.ToString(Formatting.None).Replace(@"\", "");
                return response.ToString(Formatting.None).Replace(@"\", "");
            }
        }
    }
}
