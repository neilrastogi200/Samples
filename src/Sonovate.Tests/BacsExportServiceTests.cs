using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Factory;
using Sonovate.CodeTest.Repositories;
using Sonovate.CodeTest.Services;
using Xunit;

namespace Sonovate.Tests
{
    public class BacsExportServiceTests
    {
        private readonly Mock<IPaymentsRepository> _mockPaymentsRepository;
        private readonly Mock<ICandidateRepository> _mockCandidateRepository;
        private readonly Mock<IAgencyRepository> _mockAgencyRepository;
        private readonly Mock<IInvoiceTransactionRepository> _mockInvoiceTransactionRepository;
        private readonly Mock<IDateTimeService> _mockDateTimeService;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IApplicationSettingsWrapper> _mockApplicationWrapper;

        private readonly IPaymentServiceFactory _paymentServiceFactory;
        
        private readonly TestDataBuilder _testDataBuilder;

        private readonly BacsExportService _bacsExportService;
        readonly DateTime startDate = new DateTime(2019, 04, 05);
        readonly DateTime endDateTime = DateTime.Now;


        public BacsExportServiceTests()
        {
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockAgencyRepository = new Mock<IAgencyRepository>();
            _mockCandidateRepository = new Mock<ICandidateRepository>();
            _mockInvoiceTransactionRepository = new Mock<IInvoiceTransactionRepository>();
            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockApplicationWrapper = new Mock<IApplicationSettingsWrapper>();

            _testDataBuilder = new TestDataBuilder();
            var mockWriterWrapper = _testDataBuilder.GetMockedCsvWriterWrapper();

            _paymentServiceFactory = new PaymentServiceFactory(_mockPaymentsRepository.Object, _mockInvoiceTransactionRepository.Object, _mockAgencyRepository.Object, _mockCandidateRepository.Object, _mockApplicationWrapper.Object);

            _mockDateTimeService.Setup(x => x.GetStartDateTime()).Returns(startDate);
            _mockDateTimeService.Setup(x => x.GetCurrentDateTime()).Returns(endDateTime);

            _bacsExportService = new BacsExportService(_paymentServiceFactory,_mockDateTimeService.Object, mockWriterWrapper.Object);
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
            var agencyIds = new[] {"Agency 1"};
            var agencyList = _testDataBuilder.AddSingleAgency();
            var paymentData = _testDataBuilder.AddSinglePayment();
            var expectedResult = _testDataBuilder.AddSingleAgencyResult();

            _mockPaymentsRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(paymentData);
            _mockAgencyRepository.Setup(x => x.GetAgenciesForPayments(agencyIds)).ReturnsAsync(agencyList);
            _mockApplicationWrapper.Setup(x => x["EnableAgencyPayments"]).Returns("true");
            _paymentServiceFactory.GetPaymentTypeService(bacsExportType);

            //Act
            await _bacsExportService.ExportZip(bacsExportType);

            //Assert/Verify
            _testDataBuilder.VerifyCsvRecords<BacsResult>(bacsExportType, x =>
            {
                var record = Assert.Single(x);
                Assert.Equal(expectedResult.ToList()[0].SortCode, record.SortCode);
                Assert.Equal(expectedResult.ToList()[0].AccountName, record.AccountName);
                Assert.Equal(expectedResult.ToList()[0].AccountNumber, record.AccountNumber);
                Assert.Equal(expectedResult.ToList()[0].Ref, record.Ref);
                Assert.Equal(expectedResult.ToList()[0].Amount, record.Amount);
            });
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Supplier_Successfully_Returns_Csv_File_With_1_Record_Populated()
        {
            //Arrange
            var bacsExportType = BacsExportType.Supplier;
            var candidateData = _testDataBuilder.AddSingleCandidateData();
            var invoiceData = _testDataBuilder.AddSingleInvoiceTransaction();
            var expectedResult = _testDataBuilder.AddSingleSupplierResult();

            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(invoiceData);
            _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData);
            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);
             _paymentServiceFactory.GetPaymentTypeService(bacsExportType);
     
            await _bacsExportService.ExportZip(bacsExportType);

            //Assert/Verify
            _testDataBuilder.VerifyCsvRecords<SupplierBacs>(BacsExportType.Supplier, record =>
            {
                Assert.Equal(expectedResult[0].SortCode, record[0].SortCode);
                Assert.Equal(expectedResult[0].AccountName,record[0].AccountName);
                Assert.Equal(expectedResult[0].InvoiceReference,record[0].InvoiceReference);
                Assert.Equal(expectedResult[0].AccountName,record[0].AccountName);
            });
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

            //Assert/Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _bacsExportService.ExportZip(BacsExportType.Supplier));
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Supplier_And_There_Is_No_Matching_CandidateRepository_Data_Throws_InvalidOperationException()
        {
            //Arrange
            var testInvoiceData = _testDataBuilder.AddSingleInvoiceTransaction();
            var supplierNotExistCandidateData = new Dictionary<string,Candidate>()
            {
                {"Supplier 10", new Candidate(){BankDetails = new BankDetails
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    SortCode = "00-00-01"
                }}}
            };

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);
            _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Supplier);
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);
            _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(supplierNotExistCandidateData);

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bacsExportService.ExportZip(BacsExportType.Supplier));
        }

        [Fact]
         public async void ExportZip_When_Supplier_Is_Enum_And_Multiple_Matching_InvoiceTranaction_Data_Should_Return_Collection_Of_SupplierBacs()
        {
            //Arrange
            var candidateData = _testDataBuilder.AddMultipleCandidateData();
            var testInvoiceData = _testDataBuilder.AddMultipleInvoiceTranaction();
            var expectedResult = _testDataBuilder.AddMultipleSupplierResult();

            _mockPaymentService.Setup(x => x.ArePaymentsEnabled()).Returns(true);
            _paymentServiceFactory.GetPaymentTypeService(BacsExportType.Supplier);
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);
            _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData);
             
                //Act
                await _bacsExportService.ExportZip(BacsExportType.Supplier);

            //Assert/Verify   
            _testDataBuilder.VerifyCsvRecords<SupplierBacs>(BacsExportType.Supplier, record =>
            {
                Assert.Equal(expectedResult[0].SortCode, record[0].SortCode);
                Assert.Equal(expectedResult[0].AccountName,record[0].AccountName);
                Assert.Equal(expectedResult[0].InvoiceReference,record[0].InvoiceReference);
                Assert.Equal(expectedResult[0].AccountName,record[0].AccountName);
                Assert.Equal(expectedResult[1].SortCode, record[1].SortCode);
                Assert.Equal(expectedResult[1].AccountName,record[1].AccountName);
                Assert.Equal(expectedResult[1].InvoiceReference,record[1].InvoiceReference);
                Assert.Equal(expectedResult[1].AccountName,record[1].AccountName);

                record.Should().BeEquivalentTo(expectedResult);
            });
            
        }

        [Fact]
        public async void ExportZip_When_Enum_Is_Agency_Successfully_Returns_Csv_File_With_Multiple_Records_Populated()
        {
            //Arrange
            var bacsExportType = BacsExportType.Agency;
            var agencyIds = new[] {"Agency 1","Agency 2"};
            var agencyList = _testDataBuilder.AddMultipleAgencies();
            var paymentData = _testDataBuilder.AddMultiplePayment();
            var expectedResult = _testDataBuilder.AddMultipleAgencyResult();

            _mockPaymentsRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(paymentData);
            _mockAgencyRepository.Setup(x => x.GetAgenciesForPayments(agencyIds)).ReturnsAsync(agencyList);
            _mockApplicationWrapper.Setup(x => x["EnableAgencyPayments"]).Returns("true");
            _paymentServiceFactory.GetPaymentTypeService(bacsExportType);

            //Act
            await _bacsExportService.ExportZip(bacsExportType);

            //AssertVerify
            _testDataBuilder.VerifyCsvRecords<BacsResult>(bacsExportType, record =>
                {
                    record.Should().BeEquivalentTo(expectedResult);
                });
        }
    }
}
