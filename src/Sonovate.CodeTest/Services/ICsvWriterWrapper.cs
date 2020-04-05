using System.Collections;

namespace Sonovate.CodeTest.Services
{
    public interface ICsvWriterWrapper
    {
       void WriteRecords(IEnumerable records, string fileName);
    }
}
