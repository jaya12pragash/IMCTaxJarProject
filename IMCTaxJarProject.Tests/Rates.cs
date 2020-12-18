using IMCTaxJarProject.Model;
using IMCTaxJarProject.Tests.Infrastructure;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace IMCTaxJarProject.Tests
{
    [TestFixture]
    public class RatesTests
    {
        [Test]
        public void when_showing_tax_rates_for_a_location()
        {
            var body = JsonConvert.DeserializeObject<RateResponse>(TaxjarFixture.GetJSON("rates.json"));

            Ressolver.server.Given(
                Request.Create()
                    .WithPath("/v2/rates/90002")
                    .UsingGet()
            ).RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(body)
            );

            var rates = Ressolver.client.RatesForLocation("90002");

            Assert.AreEqual("90002", rates.Zip);
            Assert.AreEqual("CA", rates.State);
            Assert.AreEqual(0.065, rates.StateRate);
            Assert.AreEqual("LOS ANGELES", rates.County);
            Assert.AreEqual(0.01, rates.CountyRate);
            Assert.AreEqual("WATTS", rates.City);
            Assert.AreEqual(0, rates.CityRate);
            Assert.AreEqual(0.015, rates.CombinedDistrictRate);
            Assert.AreEqual(0.09, rates.CombinedRate);
            Assert.AreEqual(false, rates.FreightTaxable);
        }

        [Test]
        public async Task when_showing_tax_rates_for_a_location_async()
        {
            var body = JsonConvert.DeserializeObject<RateResponse>(TaxjarFixture.GetJSON("rates.json"));

            Ressolver.server.Given(
                Request.Create()
                    .WithPath("/v2/rates/90002")
                    .UsingGet()
            ).RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(body)
            );

            var rates = await Ressolver.client.RatesForLocationAsync("90002");

            Assert.AreEqual("90002", rates.Zip);
            Assert.AreEqual("CA", rates.State);
            Assert.AreEqual(0.065, rates.StateRate);
            Assert.AreEqual("LOS ANGELES", rates.County);
            Assert.AreEqual(0.01, rates.CountyRate);
            Assert.AreEqual("WATTS", rates.City);
            Assert.AreEqual(0, rates.CityRate);
            Assert.AreEqual(0.015, rates.CombinedDistrictRate);
            Assert.AreEqual(0.09, rates.CombinedRate);
            Assert.AreEqual(false, rates.FreightTaxable);
        }
    }
    }
