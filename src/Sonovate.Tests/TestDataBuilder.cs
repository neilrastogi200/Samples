using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Services;
using Xunit;

namespace Sonovate.Tests
{
    public class TestDataBuilder
    {
        private string _filenameResult;
        private IEnumerable _recordsResult;
      
        public void VerifyCsvRecords<T>(BacsExportType bacsExportType, Action<T[]> verify)
        {
            var filename = BacsExportService.GetFilename(bacsExportType);
            Assert.Equal(filename, _filenameResult);
            var records = Assert.IsAssignableFrom<IEnumerable<T>>(_recordsResult);
            verify(records.ToArray());
        }

        public Mock<ICsvWriterWrapper> GetMockedCsvWriterWrapper()
        {
            var csvWriter = new Mock<ICsvWriterWrapper>();
            csvWriter.Setup(x => x.WriteRecords(It.IsAny<IEnumerable>(), It.IsAny<string>()))
                .Callback<IEnumerable, string>((x, y) =>
                {
                    _recordsResult = x;
                    _filenameResult = y;
                });
            return csvWriter;
        }

        public List<Agency> AddSingleAgency()
        {
            var agencyList = new List<Agency>()
            {
                new Agency(){Id = "Agency 1",BankDetails = new BankDetails(){AccountNumber = "0123457", AccountName = "testAccount",SortCode = "401314"}}
            };

            return agencyList;
        }

        public List<Agency> AddMultipleAgencies()
        {
            var agencyList = new List<Agency>()
            {
                new Agency(){Id = "Agency 1",BankDetails = new BankDetails(){AccountNumber = "0123457", AccountName = "testAccount",SortCode = "401314"}},

                new Agency(){Id = "Agency 2",BankDetails = new BankDetails(){AccountNumber = "0123489", AccountName = "testAccount2",SortCode = "401344"}}
            };

            return agencyList;
        }

        public List<Payment> AddSinglePayment()
        {
            var paymentData = new List<Payment>()
            {
                new Payment { AgencyId = "Agency 1", Balance = 20000.00m, PaymentDate = new DateTime(2019, 9, 01)},
            };

            return paymentData;
        }

        public List<Payment> AddMultiplePayment()
        {
            var paymentData = new List<Payment>()
            {
                new Payment { AgencyId = "Agency 1", Balance = 20000.00m, PaymentDate = new DateTime(2019, 9, 01)},
                new Payment { AgencyId = "Agency 2", Balance = 20000.00m, PaymentDate = new DateTime(2019, 9, 11)}
            };

            return paymentData;
        }

        public IEnumerable<BacsResult> AddSingleAgencyResult()
        {
            DateTime paymentDate = new DateTime(2019, 9, 01);

            IEnumerable<BacsResult> expectedResult = new List<BacsResult>()
            {
                new BacsResult()
                {
                    AccountName = "testAccount",
                    SortCode = "401314",
                    AccountNumber = "0123457",
                    Amount = 20000.00m,
                    Ref = $"SONOVATE{paymentDate:ddMMyyyy}"
                }
            };

            return expectedResult;
        }

        public IEnumerable<BacsResult> AddMultipleAgencyResult()
        {
            DateTime paymentDate = new DateTime(2019, 9, 01);
            DateTime paymentDate1 = new DateTime(2019,9,11);

            IEnumerable<BacsResult> expectedResult = new List<BacsResult>()
            {
                new BacsResult()
                {
                    AccountName = "testAccount",
                    SortCode = "401314",
                    AccountNumber = "0123457",
                    Amount = 20000.00m,
                    Ref = $"SONOVATE{paymentDate:ddMMyyyy}"
                },

                new BacsResult()
                {
                    AccountName = "testAccount2",
                    SortCode = "401344",
                    AccountNumber = "0123489",
                    Amount = 20000.00m,
                    Ref = $"SONOVATE{paymentDate1:ddMMyyyy}"
                }
            };

            return expectedResult;
        }

        internal Dictionary<string, Candidate> AddSingleCandidateData()
        {
            var candidateData = new Dictionary<string, Candidate>()
            {
                {
                    "Supplier 1", new Candidate()
                    {
                        BankDetails = new BankDetails
                        {
                            AccountName = "Account 1",
                            AccountNumber = "00000001",
                            SortCode = "00-00-01"
                        }
                    }
                }
            };

            return candidateData;
        }

        internal List<InvoiceTransaction> AddSingleInvoiceTransaction()
        {
            var invoiceData = new List<InvoiceTransaction>()
            {
                new InvoiceTransaction { InvoiceDate = new DateTime(2019, 4, 26), InvoiceId = "0001", SupplierId = "Supplier 1", InvoiceRef = "Ref0001", Gross = 10000.00m},
            };

            return invoiceData;
        }

        internal List<SupplierBacs> AddSingleSupplierResult()
        {
            var expectedResult = new List<SupplierBacs>()
            {
                new SupplierBacs()
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    InvoiceReference = "Ref0001",
                    PaymentAmount = 10000.00m,
                    PaymentReference = "SONOVATE26042019",
                    SortCode = "00-00-01",
                }
            };
            return expectedResult;
        }

        internal Dictionary<string, Candidate> AddMultipleCandidateData()
        {
            var candidateData = new Dictionary<string,Candidate>()
            {
                {"Supplier 1", new Candidate(){BankDetails = new BankDetails
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    SortCode = "00-00-01"
                }}},

                {"Supplier 2", new Candidate(){ BankDetails = new BankDetails
                {
                    AccountName = "Account 2",
                    AccountNumber = "00000001",
                    SortCode = "00-00-02"
                }}}
            };

            return candidateData;
        }

        internal List<InvoiceTransaction> AddMultipleInvoiceTranaction()
        {
            var testInvoiceData = new List<InvoiceTransaction>()
            {
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 26),
                    InvoiceId = "0001",
                    SupplierId = "Supplier 1",
                    InvoiceRef = "Ref0001",
                    Gross = 10000.00m
                },
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 14),
                    InvoiceId = "0002",
                    SupplierId = "Supplier 2",
                    InvoiceRef = "Ref0002",
                    Gross = 7300.00m
                },
            };

            return testInvoiceData;
        }

        internal List<SupplierBacs> AddMultipleSupplierResult()
        {
            var expectedResult = new List<SupplierBacs>()
            {
                new SupplierBacs()
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    InvoiceReference = "Ref0001",
                    PaymentAmount = 10000.00m,
                    PaymentReference = "SONOVATE26042019",
                    SortCode = "00-00-01",
                },

                new SupplierBacs()
                {
                    AccountName = "Account 2",
                    AccountNumber = "00000001",
                    InvoiceReference = "Ref0002",
                    PaymentAmount = 7300.00m,
                    PaymentReference = "SONOVATE14042019",
                    SortCode = "00-00-02",
                }
            };

            return expectedResult;
        }
    }
}
