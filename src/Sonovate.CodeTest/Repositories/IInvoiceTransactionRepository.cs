using System;
using System.Collections.Generic;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    internal interface IInvoiceTransactionRepository
    {
        List<InvoiceTransaction> GetBetweenDates(DateTime startDate, DateTime endDate);
    }
}