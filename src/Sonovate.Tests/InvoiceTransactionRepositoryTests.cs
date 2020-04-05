using System;
using System.Collections.Generic;
using FluentAssertions;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Repositories;
using Xunit;

namespace Sonovate.Tests
{
    public class InvoiceTransactionRepositoryTests
    {
        private readonly IInvoiceTransactionRepository _invoiceTransactionRepository;

        public InvoiceTransactionRepositoryTests()
        {
            _invoiceTransactionRepository = new InvoiceTransactionRepository();
        }

        [Fact]
        public void GetBetweenDates_When_Valid_DateRange_Entered_Returns_InvoiceTransactionCollection()
        {
            //Arrange
            DateTime startDate = new DateTime(2019, 03, 05);
            DateTime endDateTime = DateTime.Now;

            var expectedInvoiceData = new List<InvoiceTransaction>
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
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 17),
                    InvoiceId = "0003",
                    SupplierId = "Supplier 3",
                    InvoiceRef = "Ref0003",
                    Gross = 2000.60m
                },
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 1),
                    InvoiceId = "0004",
                    SupplierId = "Supplier 4",
                    InvoiceRef = "Ref0004",
                    Gross = 9800.00m
                },
                new InvoiceTransaction
                {
                    InvoiceDate = new DateTime(2019, 4, 5),
                    InvoiceId = "0005",
                    SupplierId = "Supplier 5",
                    InvoiceRef = "Ref0005",
                    Gross = 4000.60m
                }
            };
            
        //Act
            var actualResult =_invoiceTransactionRepository.GetBetweenDates(startDate, endDateTime);

            //Assert
            actualResult.Should().BeOfType<List<InvoiceTransaction>>();
            actualResult.Should().HaveCount(5);
            actualResult.Should().BeEquivalentTo(expectedInvoiceData);
        }

         [Fact]
        public void GetBetweenDates_When_InValid_DateRange_Entered_Returns_Empty_InvoiceTransactionCollection()
        {
            //Arrange
            DateTime startDate = new DateTime(2020, 03, 05);
            DateTime endDateTime = DateTime.Now;

        //Act
            var actualResult =_invoiceTransactionRepository.GetBetweenDates(startDate, endDateTime);

            //Assert
            actualResult.Should().BeOfType<List<InvoiceTransaction>>();
            actualResult.Should().HaveCount(0);
            actualResult.Should().BeEmpty();
        }
    }
}
