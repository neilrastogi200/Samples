using System.Collections;
using System.IO;
using CsvHelper;

namespace Sonovate.CodeTest.Services
{
    public class CsvWriterWrapper : ICsvWriterWrapper
    {
        public void WriteRecords(IEnumerable records, string fileName)
        {
            using (var csv = new CsvWriter(new StreamWriter(new FileStream(fileName, FileMode.Create))))
            {
                csv.WriteRecords(records);
            }
        }
    }
}
