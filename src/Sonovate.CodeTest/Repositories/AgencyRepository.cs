using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    public class AgencyRepository : IAgencyRepository
    {
        private readonly IDocumentStore _documentStore;

        public AgencyRepository()
        {
            _documentStore = new DocumentStore {Urls = new[]{"http://localhost"}, Database = "Export"};
            _documentStore.Initialize();
        }

        public async Task<List<Agency>> GetAgenciesForPayments(IEnumerable<string> agencyIds)
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                return (await session.LoadAsync<Agency>(agencyIds)).Values.ToList();
            }
        }
    }
}
