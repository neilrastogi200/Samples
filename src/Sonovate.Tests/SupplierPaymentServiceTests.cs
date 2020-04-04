using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Repositories;
using Sonovate.CodeTest.Services;
using Xunit;

namespace Sonovate.Tests
{
    public class SupplierPaymentServiceTests
    {
        private Mock<IInvoiceTransactionRepository> _mockInvoiceTransactionRepository;
        private Mock<ICandidateRepository> _mockCandidateRepository;
        private IPaymentService _paymentService;

        public SupplierPaymentServiceTests()
        {
            _mockInvoiceTransactionRepository = new Mock<IInvoiceTransactionRepository>();
            _mockCandidateRepository = new Mock<ICandidateRepository>();
            _paymentService = new SupplierPaymentService(_mockInvoiceTransactionRepository.Object, _mockCandidateRepository.Object);
        }

        [Fact]
        public async void GetPayments_When_InvoiceTransactionRepository_Returns_Empty_List_Should_Throw_InvalidOperationException()
        {
            //Arrange

            DateTime startDate = new DateTime(2020, 03, 05);
            DateTime endDateTime = DateTime.Now;
           
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(new List<InvoiceTransaction>());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _paymentService.GetPayments(startDate, endDateTime));
        }

        [Fact]
        public async void GetPayments_When_InvoiceTransactionRepository_Returns_Null_Should_Throw_ArgumentNullException()
        {
            //Arrange
            DateTime startDate = new DateTime(2020, 04, 05);
            DateTime endDateTime = DateTime.Now;
           
            //Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _paymentService.GetPayments(startDate, endDateTime));
        }

        [Fact]
        public async void GetPayments_Should_Return_Collection_Of_BacResult()
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

            var candidateData = new Candidate()
            {
                BankDetails = new BankDetails
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    SortCode = "00-00-01"
                }
            };
            
           var expectedResult = new List<SupplierBacs>()
           {
               new SupplierBacs()
               {
                   AccountName = candidateData.BankDetails.AccountName,
                   AccountNumber = candidateData.BankDetails.AccountNumber,
                   InvoiceReference = "Ref0001",
                   PaymentAmount = 10000.00m,
                   PaymentReference = "SONOVATE26042019",
                   SortCode = candidateData.BankDetails.SortCode,
               }
           };
           
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);

            //_mockCandidateRepository.Setup(x => x.GetById("Supplier 1")).Returns(candidateData);

            //Act
           var actualResult = await _paymentService.GetPayments(startDate, endDateTime);

            //Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        
        [Fact]
        //how to unit test this with multiple candidates values? 
        public async void GetPayments_Should_Return_Collection_Of_BacResult2()
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
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 14),
                    InvoiceId = "0002",
                    SupplierId = "Supplier 2",
                    InvoiceRef = "Ref0002",
                    Gross = 7300.00m
                },
            };

            var candidateData1 = new Dictionary<string,Candidate>()
            {
                {"Supplier 10", new Candidate(){BankDetails = new BankDetails
                {
                    AccountName = "Account 1",
                    AccountNumber = "00000001",
                    SortCode = "00-00-01"
                }}},

                {"Supplier 11", new Candidate(){ BankDetails = new BankDetails
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
           
            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);

              _mockCandidateRepository.Setup(x => x.GetCandidateData()).Returns(candidateData1);
                
                //Act
                var actualResult = await _paymentService.GetPayments(startDate, endDateTime);

                //Assert
                actualResult.Should().BeEquivalentTo(expectedResult);
        }

         [Fact]
        public async void GetPayments_When_There_Is_No_Matching_CandidateData_Throws_InvalidOperationException()
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
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 14),
                    InvoiceId = "0002",
                    SupplierId = "Supplier 2",
                    InvoiceRef = "Ref0002",
                    Gross = 7300.00m
                },
            };

            _mockInvoiceTransactionRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime))
                .Returns(testInvoiceData);

                //Assert
               await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await _paymentService.GetPayments(startDate, endDateTime));
        }
    
    }
}
