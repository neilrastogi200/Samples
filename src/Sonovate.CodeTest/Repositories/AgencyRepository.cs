using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    public class AgencyRepository : IAgencyRepository
    {
        private readonly IDocumentStore _documentStore;

        public AgencyRepository(IDocumentStore documentStore)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
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
