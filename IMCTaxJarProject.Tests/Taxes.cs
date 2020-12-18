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
    public class TaxesTests
    {
        [SetUp]
        public static void Init()
        {
            Ressolver.client = new TaxjarApi(Ressolver.apiKey, new { apiUrl = "http://localhost:9191" });
            Ressolver.server.ResetMappings();
        }

        [Test]
        public void when_calculating_sales_tax_for_an_order()
        {
            var body = JsonConvert.DeserializeObject<TaxResponse>(TaxjarFixture.GetJSON("taxes.json"));

            Ressolver.server.Given(
                Request.Create()
                    .WithPath("/v2/taxes")
                    .UsingPost()
            ).RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(body)
            );

            var rates = Ressolver.client.TaxForOrder(new
            {
                from_country = "US",
                from_zip = "12207",
                from_state = "NY",
                to_country = "US",
                to_zip = "10001",
                to_state = "NY",
                amount = 60,
                shipping = 10,
                exemption_type = "non_exempt",
                line_items = new[] {
                    new
                    {
                        quantity = 1,
                        unit_price = 50
                    }
                }
            });

            Assert.AreEqual(60, rates.OrderTotalAmount);
            Assert.AreEqual(10, rates.Shipping);
            Assert.AreEqual(60, rates.TaxableAmount);
            Assert.AreEqual(6.53, rates.AmountToCollect);
            Assert.AreEqual(0.10875, rates.Rate);
            Assert.AreEqual(true, rates.HasNexus);
            Assert.AreEqual(true, rates.FreightTaxable);
            Assert.AreEqual("destination", rates.TaxSource);
            Assert.AreEqual("non_exempt", rates.ExemptionType);

            // Jurisdictions
            Assert.AreEqual("US", rates.Jurisdictions.Country);
            Assert.AreEqual("NY", rates.Jurisdictions.State);
            Assert.AreEqual("NEW YORK", rates.Jurisdictions.County);
            Assert.AreEqual("NEW YORK", rates.Jurisdictions.City);

            // Breakdowns
            Assert.AreEqual(10, rates.Breakdown.Shipping.TaxableAmount);
            Assert.AreEqual(0.89, rates.Breakdown.Shipping.TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.Shipping.CombinedTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.StateTaxableAmount);
            Assert.AreEqual(0.4, rates.Breakdown.Shipping.StateAmount);
            Assert.AreEqual(0.04, rates.Breakdown.Shipping.StateSalesTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.CountyTaxableAmount);
            Assert.AreEqual(0.1, rates.Breakdown.Shipping.CountyAmount);
            Assert.AreEqual(0.01, rates.Breakdown.Shipping.CountyTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.CityTaxableAmount);
            Assert.AreEqual(0.49, rates.Breakdown.Shipping.CityAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.Shipping.CityTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.Shipping.SpecialDistrictTaxRate);
            Assert.AreEqual(0.1, rates.Breakdown.Shipping.SpecialDistrictAmount);

            Assert.AreEqual(60, rates.Breakdown.TaxableAmount);
            Assert.AreEqual(6.53, rates.Breakdown.TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.CombinedTaxRate);
            Assert.AreEqual(60, rates.Breakdown.StateTaxableAmount);
            Assert.AreEqual(0.04, rates.Breakdown.StateTaxRate);
            Assert.AreEqual(2.4, rates.Breakdown.StateTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.CountyTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.CountyTaxRate);
            Assert.AreEqual(0.6, rates.Breakdown.CountyTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.CityTaxableAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.CityTaxRate);
            Assert.AreEqual(2.93, rates.Breakdown.CityTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.SpecialDistrictTaxRate);
            Assert.AreEqual(0.6, rates.Breakdown.SpecialDistrictTaxCollectable);

            // Line Items
            Assert.AreEqual("1", rates.Breakdown.LineItems[0].Id);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].TaxableAmount);
            Assert.AreEqual(4.44, rates.Breakdown.LineItems[0].TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.LineItems[0].CombinedTaxRate);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].StateTaxableAmount);
            Assert.AreEqual(0.04, rates.Breakdown.LineItems[0].StateSalesTaxRate);
            Assert.AreEqual(2, rates.Breakdown.LineItems[0].StateAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].CountyTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.LineItems[0].CountyTaxRate);
            Assert.AreEqual(0.5, rates.Breakdown.LineItems[0].CountyAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].CityTaxableAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.LineItems[0].CityTaxRate);
            Assert.AreEqual(2.44, rates.Breakdown.LineItems[0].CityAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.LineItems[0].SpecialTaxRate);
            Assert.AreEqual(0.5, rates.Breakdown.LineItems[0].SpecialDistrictAmount);
        }

        [Test]
        public async Task when_calculating_sales_tax_for_an_order_async()
        {
            var body = JsonConvert.DeserializeObject<TaxResponse>(TaxjarFixture.GetJSON("taxes.json"));

            Ressolver.server.Given(
                Request.Create()
                    .WithPath("/v2/taxes")
                    .UsingPost()
            ).RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(body)
            );

            var rates = await Ressolver.client.TaxForOrderAsync(new
            {
                from_country = "US",
                from_zip = "12207",
                from_state = "NY",
                to_country = "US",
                to_zip = "10001",
                to_state = "NY",
                amount = 60,
                shipping = 10,
                exemption_type = "non_exempt",
                line_items = new[] {
                    new
                    {
                        quantity = 1,
                        unit_price = 50
                    }
                }
            });

            Assert.AreEqual(60, rates.OrderTotalAmount);
            Assert.AreEqual(10, rates.Shipping);
            Assert.AreEqual(60, rates.TaxableAmount);
            Assert.AreEqual(6.53, rates.AmountToCollect);
            Assert.AreEqual(0.10875, rates.Rate);
            Assert.AreEqual(true, rates.HasNexus);
            Assert.AreEqual(true, rates.FreightTaxable);
            Assert.AreEqual("destination", rates.TaxSource);
            Assert.AreEqual("non_exempt", rates.ExemptionType);

            // Jurisdictions
            Assert.AreEqual("US", rates.Jurisdictions.Country);
            Assert.AreEqual("NY", rates.Jurisdictions.State);
            Assert.AreEqual("NEW YORK", rates.Jurisdictions.County);
            Assert.AreEqual("NEW YORK", rates.Jurisdictions.City);

            // Breakdowns
            Assert.AreEqual(10, rates.Breakdown.Shipping.TaxableAmount);
            Assert.AreEqual(0.89, rates.Breakdown.Shipping.TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.Shipping.CombinedTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.StateTaxableAmount);
            Assert.AreEqual(0.4, rates.Breakdown.Shipping.StateAmount);
            Assert.AreEqual(0.04, rates.Breakdown.Shipping.StateSalesTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.CountyTaxableAmount);
            Assert.AreEqual(0.1, rates.Breakdown.Shipping.CountyAmount);
            Assert.AreEqual(0.01, rates.Breakdown.Shipping.CountyTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.CityTaxableAmount);
            Assert.AreEqual(0.49, rates.Breakdown.Shipping.CityAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.Shipping.CityTaxRate);
            Assert.AreEqual(10, rates.Breakdown.Shipping.SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.Shipping.SpecialDistrictTaxRate);
            Assert.AreEqual(0.1, rates.Breakdown.Shipping.SpecialDistrictAmount);

            Assert.AreEqual(60, rates.Breakdown.TaxableAmount);
            Assert.AreEqual(6.53, rates.Breakdown.TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.CombinedTaxRate);
            Assert.AreEqual(60, rates.Breakdown.StateTaxableAmount);
            Assert.AreEqual(0.04, rates.Breakdown.StateTaxRate);
            Assert.AreEqual(2.4, rates.Breakdown.StateTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.CountyTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.CountyTaxRate);
            Assert.AreEqual(0.6, rates.Breakdown.CountyTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.CityTaxableAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.CityTaxRate);
            Assert.AreEqual(2.93, rates.Breakdown.CityTaxCollectable);
            Assert.AreEqual(60, rates.Breakdown.SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.SpecialDistrictTaxRate);
            Assert.AreEqual(0.6, rates.Breakdown.SpecialDistrictTaxCollectable);

            // Line Items
            Assert.AreEqual("1", rates.Breakdown.LineItems[0].Id);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].TaxableAmount);
            Assert.AreEqual(4.44, rates.Breakdown.LineItems[0].TaxCollectable);
            Assert.AreEqual(0.10875, rates.Breakdown.LineItems[0].CombinedTaxRate);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].StateTaxableAmount);
            Assert.AreEqual(0.04, rates.Breakdown.LineItems[0].StateSalesTaxRate);
            Assert.AreEqual(2, rates.Breakdown.LineItems[0].StateAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].CountyTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.LineItems[0].CountyTaxRate);
            Assert.AreEqual(0.5, rates.Breakdown.LineItems[0].CountyAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].CityTaxableAmount);
            Assert.AreEqual(0.04875, rates.Breakdown.LineItems[0].CityTaxRate);
            Assert.AreEqual(2.44, rates.Breakdown.LineItems[0].CityAmount);
            Assert.AreEqual(50, rates.Breakdown.LineItems[0].SpecialDistrictTaxableAmount);
            Assert.AreEqual(0.01, rates.Breakdown.LineItems[0].SpecialTaxRate);
            Assert.AreEqual(0.5, rates.Breakdown.LineItems[0].SpecialDistrictAmount);
        }
    }
}
