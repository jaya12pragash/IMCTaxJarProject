using IMCTaxJarProject.Model;
using System;
using System.Net;
namespace IMCTaxJarProject.Infra
{
        [Serializable]
        public class TaxjarException : ApplicationException
        {
            public HttpStatusCode HttpStatusCode { get; set; }
            public TaxjarError TaxjarError { get; set; }

            public TaxjarException()
            {
            }

            public TaxjarException(HttpStatusCode statusCode, TaxjarError taxjarError, string message) : base(message)
            {
                HttpStatusCode = statusCode;
                TaxjarError = taxjarError;
            }
        }
    }

