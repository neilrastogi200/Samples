﻿ using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Linq;
 using System.Text;
 using System.Threading.Tasks;
 using Sonovate.CodeTest.Domain;
 using Sonovate.CodeTest.Repositories;

namespace Sonovate.CodeTest.Services
{
    public class SupplierPaymentService : IPaymentService
    {
        private readonly IInvoiceTransactionRepository _invoiceTransactionRepository;
        private readonly ICandidateRepository _candidateRepository;
        private const string NotAvailable = "NOT AVAILABLE";
        internal SupplierPaymentService(IInvoiceTransactionRepository invoiceTransactionRepository, ICandidateRepository candidateRepository)
        {
            _invoiceTransactionRepository = invoiceTransactionRepository ?? throw new ArgumentNullException(nameof(invoiceTransactionRepository));
            _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
        }

        public bool ArePaymentsEnabled()
        {
            return true;
        }

        public async Task<IEnumerable> GetPayments(DateTime startDate, DateTime endDate)
        {
            var candidateInvoiceTransactions = _invoiceTransactionRepository.GetBetweenDates(startDate, endDate);
            
            if (!candidateInvoiceTransactions.Any())
            {
                throw new InvalidOperationException(
                    $"No supplier invoice transactions found between dates {startDate} to {endDate}");
            }

            var candidateBacsExport = await CreateCandidateBacxExportFromSupplierPayments(candidateInvoiceTransactions);

            return candidateBacsExport.SupplierPayment;
        }

        private async Task<SupplierBacsExport> CreateCandidateBacxExportFromSupplierPayments(IList<InvoiceTransaction> supplierPayments)
        {
            var candidateBacsExport = new SupplierBacsExport
            {
                SupplierPayment = new List<SupplierBacs>()
            };

            candidateBacsExport.SupplierPayment = await BuildSupplierPayments(supplierPayments);
                
            return candidateBacsExport;
        }

        private async Task<List<SupplierBacs>>BuildSupplierPayments(IEnumerable<InvoiceTransaction> invoiceTransactions)
        {
            var results = new List<SupplierBacs>();

            var transactionsByCandidateAndInvoiceId = invoiceTransactions.GroupBy(transaction => new
            {
                transaction.InvoiceId,
                transaction.SupplierId
            });

            foreach (var transactionGroup in transactionsByCandidateAndInvoiceId)
            {
                var candidate = _candidateRepository.GetCandidateData();

                candidate.TryGetValue(transactionGroup.Key.SupplierId, out Candidate matchingCandidate);

                if (matchingCandidate == null)
                {
                    throw new InvalidOperationException(
                        $"Could not load candidate with Id {transactionGroup.Key.SupplierId}");
                }

                var bank = matchingCandidate?.BankDetails;

                var result = new SupplierBacs
                {
                    PaymentAmount = transactionGroup.Sum(invoiceTransaction => invoiceTransaction.Gross),
                    InvoiceReference = string.IsNullOrEmpty(transactionGroup.First().InvoiceRef)
                        ? NotAvailable
                        : transactionGroup.First().InvoiceRef,
                    PaymentReference = $"SONOVATE{transactionGroup.First().InvoiceDate.GetValueOrDefault():ddMMyyyy}",
                    AccountName = bank?.AccountName,
                    AccountNumber = bank?.AccountNumber,
                    SortCode = bank?.SortCode
                };

                results.Add(result);
            }

            //I had a go at changing this from the above to the below, is that roughly right?

            //I had  a look at the select  n + 1 problem, I see the problem with this if you had large oamounts of N it would be a big hit on the db. I did think it would be easier to get all the candidate data out in one hit a new method, but I think reading the instructions, I am not allowed to do that. Also it makes more difficult to unit test. 

            //var result = (from transaction in transactionsByCandidateAndInvoiceId
            //    let candidate = _candidateRepository.GetById(transaction.Key.SupplierId)
            //    where candidate != null
            //    select new SupplierBacs()
            //    {
            //        PaymentAmount = transaction.Sum(invoiceTransaction => invoiceTransaction.Gross),
            //        InvoiceReference = string.IsNullOrEmpty(transaction.First().InvoiceRef)
            //            ? NotAvailable
            //            : transaction.First().InvoiceRef,
            //        PaymentReference = $"SONOVATE{transaction.First().InvoiceDate.GetValueOrDefault():ddMMyyyy}",
            //        AccountName = candidate.BankDetails.AccountName,
            //        AccountNumber = candidate.BankDetails.AccountNumber,
            //        SortCode = candidate.BankDetails.SortCode

            //    });
           
            return results;
        }
    }
}
