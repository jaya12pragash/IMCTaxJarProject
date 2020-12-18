using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using WireMock.Server;
using WireMock.Settings;

namespace IMCTaxJarProject.Tests
{
    [SetUpFixture]
    public class Ressolver
    {
        public static TaxjarApi client;
        public static FluentMockServer server;
        public static string apiKey;

        [OneTimeSetUp]
        public static void Init()
        {
            if (server == null)
            {
                server = FluentMockServer.Start(new FluentMockServerSettings
                {
                    Urls = new[] { "http://localhost:9191" }
                });
            }

            var options = GetTestOptions();
            apiKey = options.ApiToken;
            client = new TaxjarApi(apiKey, new { apiUrl = "http://localhost:9191" });
        }

        [OneTimeTearDown]
        public static void Destroy()
        {
            server.Stop();
        }

        private static TestOptions GetTestOptions()
        {
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../", "Settings", @"Token.json");
            var json = File.ReadAllText(path);
            var options = JsonConvert.DeserializeObject<TestOptions>(json);
            return options;
        }

        private class TestOptions
        {
            public string ApiToken { get; set; }
        }
    }
}
