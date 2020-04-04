using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Raven.Client.Documents;
using Sonovate.CodeTest;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Factory;
using Sonovate.CodeTest.Repositories;
using Sonovate.CodeTest.Services;
using Sonovate.Tests.TestHelpers;
using Xunit;
using IApplicationWrapper = Sonovate.CodeTest.Services.IApplicationWrapper;

namespace Sonovate.Tests
{
    public class BacsExportServiceTests
    {
        private BacsExportService _bacsExportService;
        private  Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<ICandidateRepository> _mockCandidateRepository;
        private  Mock<IAgencyRepository> _mockAgencyRepository;
        private Mock<IInvoiceTransactionRepository> _mockInvoiceTransactionRepository;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Mock<IPaymentService> _mockPaymentService;
        private Mock<IPaymentServiceFactory> _mockPaymentServiceFactory;
        private Mock<ICsvWriterWrapper> _mockWriterWrapper;
        private Mock<CodeTest.Services.IApplicationWrapper> _mockApplicationWrapper;

        private IPaymentServiceFactory _paymentServiceFactory;
        private IPaymentService _paymentService;
        private TestDataBuilder _testDataBuilder = null;


        public BacsExportServiceTests()
        {
           // _mockWriterWrapper = new Mock<ICsvWriterWrapper>();
            _mockPaymentServiceFactory = new Mock<IPaymentServiceFactory>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockAgencyRepository = new Mock<IAgencyRepository>();
            _mockCandidateRepository = new Mock<ICandidateRepository>();
            _mockInvoiceTransactionRepository = new Mock<IInvoiceTransactionRepository>();
            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockApplicationWrapper = new Mock<IApplicationWrapper>();

            _testDataBuilder = new TestDataBuilder();
            _mockWriterWrapper = _testDataBuilder.GetMockedCsvWriterWrapper();

           

            _paymentServiceFactory = new PaymentServiceFactory(_mockPaymentsRepository.Object, _mockInvoiceTransactionRepository.Object, _mockAgencyRepository.Object, _mockCandidateRepository.Object, _mockApplicationWrapper.Object);

            //_bacsExportService = new BacsExportService(_paymentServiceFactory,_mockDateTimeService.Object, _mockWriterWrapper.Object);

            _bacsExportService = new BacsExportService(_paymentServiceFactory,_mockDateTimeService.Object, _mockWriterWrapper.Object);
        }


        [Fact]
        public async void ExportZip_When_Enum_Is_None_Throws_Exception()
        {
            //Arrange
            var bacsExportType = BacsExportType.None;
            //Act/Assert
            await Assert.ThrowsAsync<Exception>(() => _bacsExportService.ExportZip(bacsExportType));
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Agency_Successfully_Returns_Csv_File_With_1_Record_Populated()
        {
            //Arrange
            var bacsExportType = BacsExportType.Agency;
            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;
            DateTime paymentDate = new DateTime(2019, 9, 01);
            var fileName = "Agency_BACSExport.csv";

            var agencyIds = new[] {"Agency 1"};
 
            List<Agency> agencyList = new List<Agency>()
            {
                new Agency(){Id = "Agency 1",BankDetails = new BankDetails(){AccountNumber = "0123457", AccountName = "testAccount",SortCode = "401314"}}
            };

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

            List<Payment> paymentData = new List<Payment>()
            {
                new Payment { AgencyId = "Agency 1", Balance = 20000.00m, PaymentDate = new DateTime(2019, 9, 01)},
            };

            _mockPaymentsRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(paymentData);

            _mockAgencyRepository.Setup(x => x.GetAgenciesForPayments(agencyIds)).ReturnsAsync(agencyList);

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);

            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _mockApplicationWrapper.Setup(x => x["EnableAgencyPayments"]).Returns("true");
       
            _paymentServiceFactory.GetPaymentTypeService(bacsExportType);

            _mockWriterWrapper.Setup(x => x.WriteRecords(expectedResult,fileName)).Verifiable();

            //Act
            await _bacsExportService.ExportZip(bacsExportType);

            //Verify
            _mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<BacsResult>>( y => y.Any() && y.Count() == 1),fileName),Times.Once);

            _mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<BacsResult>>( y => y.ToList()[0].AccountName=="testAccount" && y.ToList()[0].Ref==$"SONOVATE{paymentDate:ddMMyyyy}"),fileName),Times.Once);
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Supplier_Successfully_Returns_Csv_File_With_1_Record_Populated()
        {
            //Arrange
            var bacsExportType = BacsExportType.Supplier;
            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;
            var fileName = "Supplier_BACSExport.csv";

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
            
            var invoiceData = new List<InvoiceTransaction>()
            {
                new InvoiceTransaction { InvoiceDate = new DateTime(2019, 4, 26), InvoiceId = "0001", SupplierId = "Supplier 1", InvoiceRef = "Ref0001", Gross = 10000.00m},
            };

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

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);

            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(invoiceData);

            _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData);

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);

             _paymentServiceFactory.GetPaymentTypeService(bacsExportType);

            _mockWriterWrapper.Setup(x => x.WriteRecords(expectedResult, fileName));
                
            await _bacsExportService.ExportZip(bacsExportType);

            //Verify
            _mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<SupplierBacs>>( y => y.Any() && y.Count() == 1),fileName),Times.Once);

            _mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<SupplierBacs>>( y => y.ToList()[0].AccountName=="Account 1" && y.ToList()[0].PaymentReference=="SONOVATE26042019"),fileName),Times.Once);

        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Agency_PaymentsAreEnabled_Returns_False_Throws_Exception()
        {
            //Arrange
            _mockApplicationWrapper.Setup(x => x["EnableAgencyPayments"]).Returns("ff");
            
            _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Agency);

            //Assert/Act
            await Assert.ThrowsAsync<Exception>(() => _bacsExportService.ExportZip(BacsExportType.Agency));
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Supplier_And_InvoiceTransactionRepository_Returns_Empty_List_Should_Throw_InvalidOperationException()
        {
            //Arrange
            DateTime startDate = new DateTime(2020, 03, 05);
            DateTime endDateTime = DateTime.Now;

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);

            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);

            _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Supplier);
           
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(new List<InvoiceTransaction>());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _bacsExportService.ExportZip(BacsExportType.Supplier));
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Supplier_And_There_Is_No_Matching_CandidateRepository_Data_Throws_InvalidOperationException()
        {
            //Arrange
            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;

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
            };

            var candidateData = new Dictionary<string,Candidate>()
            {
                {"Supplier 10", new Candidate(){BankDetails = new BankDetails
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    SortCode = "00-00-01"
                }}}
            };

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);

            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);

            _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Supplier);

            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);

            _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData);

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bacsExportService.ExportZip(BacsExportType.Supplier));
        }

        [Fact]
         public async void ExportZip_When_Supplier_Is_Enum_And_Multiple_Matching_InvoiceTranaction_Data_Should_Return_Collection_Of_SupplierBacs()
        {
            //Arrange
            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;
            var filename = "Supplier_BACSExport.csv";

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

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);

            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);

             _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Supplier);
           
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);

              _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData);
             
                //Act
                await _bacsExportService.ExportZip(BacsExportType.Supplier);


            //Verify   
            _testDataBuilder.VerifyCsvRecords<SupplierBacs>(BacsExportType.Supplier, x =>
            {
                Assert.Equal(expectedResult[0].SortCode, x[0].SortCode);
                Assert.Equal(expectedResult[0].AccountName,x[0].AccountName);
                Assert.Equal(expectedResult[0].InvoiceReference,x[0].InvoiceReference);
                Assert.Equal(expectedResult[0].AccountName,x[0].AccountName);
                Assert.Equal(expectedResult[1].SortCode, x[1].SortCode);
                Assert.Equal(expectedResult[1].AccountName,x[1].AccountName);
                Assert.Equal(expectedResult[1].InvoiceReference,x[1].InvoiceReference);
                Assert.Equal(expectedResult[1].AccountName,x[1].AccountName);

            });
            
            //_mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<SupplierBacs>>( y => y.Any() && y.Count() == 2),filename),Times.Once);

            //_mockWriterWrapper.Verify(x => x.WriteRecords(It.Is<IEnumerable<SupplierBacs>>( y => y.ToList()[0].AccountName=="Account 1" && y.ToList()[0].PaymentReference=="SONOVATE26042019"),filename),Times.Once);

        }

    }
}
