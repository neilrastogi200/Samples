using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Moq;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Repositories;
using Sonovate.CodeTest.Services;
using Xunit;
using Xunit.Sdk;

namespace Sonovate.Tests
{
    public class AgencyPaymentServiceTests
    {
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<IAgencyRepository> _mockAgencyRepository;
        private Mock<IApplicationWrapper> _mockApplicationWrapper;
        private IPaymentService _paymentService;
        private IFixture _fixture;


        public AgencyPaymentServiceTests()
        {
            _mockAgencyRepository = new Mock<IAgencyRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockApplicationWrapper = new Mock<IApplicationWrapper>();

            _paymentService = new AgencyPaymentService(_mockPaymentsRepository.Object, _mockAgencyRepository.Object,_mockApplicationWrapper.Object);
        }

        [Fact]
        public async void GetPayments_Should_Return()
        {
            //Arrange

            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;
            DateTime paymentDate = new DateTime(2019, 9, 01);

            List<Agency> agencyList = new List<Agency>()
            {
                new Agency(){Id = "Agency 1",BankDetails = new BankDetails(){AccountNumber = "0123457", AccountName = "testAccount",SortCode = "401314"}}
            };

            var payments = new List<Payment>()
            {
                new Payment { AgencyId = "Agency 1", Balance = 20000.00m, PaymentDate = new DateTime(2019, 9, 01)},
                new Payment { AgencyId = "Agency 2", Balance = 7500.00m, PaymentDate = new DateTime(2019, 9, 16)},
                new Payment { AgencyId = "Agency 3", Balance = 960.25m, PaymentDate = new DateTime(2019, 9, 20)},
                new Payment { AgencyId = "Agency 4", Balance = 14000.50m, PaymentDate = new DateTime(2019, 9, 11)},
                new Payment { AgencyId = "Agency 5", Balance = 70500.00m, PaymentDate = new DateTime(2019, 9, 29)},
            };

            IEnumerable<BacsResult> expectedbacsResults = new List<BacsResult>()
            {
                new BacsResult()
                {
                    AccountName = "testAccount",
                    AccountNumber = "0123457",
                    Amount = 20000.00m,
                    Ref = $"SONOVATE{paymentDate:ddMMyyyy}",
                    SortCode = "401314",
                }
            };

            _mockPaymentsRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(payments);

            _mockAgencyRepository.Setup(x => x.GetAgenciesForPayments(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(agencyList);

            _mockApplicationWrapper.Setup(x => x["EnableAgencyPayments"]).Returns("true");


            //act
            var actualResult = await _paymentService.GetPayments(startDate, endDateTime);

            //assert
            actualResult.Should().BeEquivalentTo(expectedbacsResults);
        }


        [Fact]
        public async void GetPayments_When_PaymentRepository_Returns_Empty_List_Should_InvalidOperationException()
        {
            //Arrange

            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;

            _mockPaymentsRepository.Setup(x => x.GetBetweenDates(startDate, endDateTime)).Returns(new List<Payment>());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _paymentService.GetPayments(startDate, endDateTime));
        }

        [Fact]
        public async void GetPayments_When_PaymentRepository_Returns_Empty_List_Should_ArgumentNullException()
        {
            //Arrange
            DateTime startDate = new DateTime(2019, 04, 05);
            DateTime endDateTime = DateTime.Now;

            //Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _paymentService.GetPayments(startDate, endDateTime));
        }
    }
}
