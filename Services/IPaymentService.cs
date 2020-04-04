using System;
using System.Collections;
using System.Threading.Tasks;

namespace Sonovate.CodeTest.Services
{
    public interface IPaymentService 
    {
        bool ArePaymentsEnabled();
        Task<IEnumerable> GetPayments(DateTime startDate, DateTime endDate); 
    }
}
