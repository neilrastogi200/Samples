using System.Collections.Generic;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Repositories
{
    internal interface ICandidateRepository
    {
        Candidate GetById(string supplierId);
        IDictionary<string, Candidate> GetCandidateData();
    }
}