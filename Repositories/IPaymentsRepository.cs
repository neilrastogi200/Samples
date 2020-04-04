using System;
using System.Collections.Generic;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    internal interface IPaymentsRepository
    {
        IList<Payment> GetBetweenDates(DateTime start, DateTime end);
    }
}