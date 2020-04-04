using System.Collections;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest.Services
{
    public interface ICsvWriterWrapper
    {
       void WriteRecords(IEnumerable records, string fileName);
    }
}
