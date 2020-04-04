using System.Collections.Generic;
using System.Threading.Tasks;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    public interface IAgencyRepository
    {
        Task<List<Agency>> GetAgenciesForPayments(IEnumerable<string> agencyIds);
    }
}
