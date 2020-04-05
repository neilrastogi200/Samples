using System;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Repositories;
using Sonovate.CodeTest.Services;

namespace Sonovate.CodeTest.Factory
{
    public class PaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IInvoiceTransactionRepository _invoiceTransactionRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IApplicationWrapper _applicationWrapper;

        internal PaymentServiceFactory(IPaymentsRepository paymentsRepository, IInvoiceTransactionRepository invoiceTransactionRepository, IAgencyRepository agencyRepository, ICandidateRepository candidateRepository, IApplicationWrapper applicationWrapper)
        {
            _paymentsRepository = paymentsRepository;
            _invoiceTransactionRepository = invoiceTransactionRepository;
            _agencyRepository = agencyRepository;
            _candidateRepository = candidateRepository;
            _applicationWrapper = applicationWrapper;
        }

        internal PaymentServiceFactory() : this(new PaymentsRepository(), new InvoiceTransactionRepository(),
            new AgencyRepository(), new CandidateRepository(), new ApplicationWrapper())
        {

        }

        public IPaymentService GetPaymentTypeService(BacsExportType bacsExportType)
        {
            try
            {
                switch (bacsExportType)
                {
                    case BacsExportType.Agency:
                        return new AgencyPaymentService(_paymentsRepository,_agencyRepository, _applicationWrapper);
                    case BacsExportType.Supplier:
                        return new SupplierPaymentService(_invoiceTransactionRepository, _candidateRepository);
                    default:
                        throw new Exception("Invalid BACS Export Type.");
                }

            }
            catch (InvalidOperationException inOpEx)
            {
                throw new Exception(inOpEx.Message);
            }
        }
    }
}
