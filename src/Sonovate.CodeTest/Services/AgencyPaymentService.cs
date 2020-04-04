using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Repositories;

namespace Sonovate.CodeTest.Services
{
    public class AgencyPaymentService : IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IApplicationWrapper _applicationWrapper;

        internal AgencyPaymentService(IPaymentsRepository paymentsRepository, IAgencyRepository agencyRepository, IApplicationWrapper applicationWrapper)
        {
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _agencyRepository = agencyRepository ?? throw new ArgumentNullException(nameof(agencyRepository));
            _applicationWrapper = applicationWrapper ?? throw new ArgumentException(nameof(applicationWrapper));
        }

        public bool ArePaymentsEnabled()
        {
            if (_applicationWrapper["EnableAgencyPayments"]
                .Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public async Task<IEnumerable> GetPayments(DateTime startDate, DateTime endDate)
        {
            var payments = _paymentsRepository.GetBetweenDates(startDate, endDate);

            if (!payments.Any())
            {
                throw new InvalidOperationException(
                    $"No agency payments found between dates {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}");
            }

            var agencyIds = payments.Select(x => x.AgencyId).Distinct().ToList();

            var agencies = await _agencyRepository.GetAgenciesForPayments(agencyIds);

            return BuildAgencyPayments(payments, agencies);
            
        }

        private List<BacsResult> BuildAgencyPayments(IEnumerable<Payment> payments, List<Agency> agencies)
        {
            return (from p in payments
                let agency = agencies.FirstOrDefault(x => x.Id == p.AgencyId)
                where agency?.BankDetails != null
                let bank = agency.BankDetails
                select new BacsResult
                {
                    AccountName = bank.AccountName,
                    AccountNumber = bank.AccountNumber,
                    SortCode = bank.SortCode,
                    Amount = p.Balance,
                    Ref = $"SONOVATE{p.PaymentDate:ddMMyyyy}"
                }).ToList();
        }
    }
}
