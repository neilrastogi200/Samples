using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Services;
using Xunit;

namespace Sonovate.Tests
{
    public class TestDataBuilder
    {
        private string _filenameResult;
        private IEnumerable _recordsResult;
        public TestDataBuilder()
        {
            //var csvWriter = GetMockedCsvWriterWrapper();
            //csvWriter.Setup(x => x.WriteRecords(It.IsAny<IEnumerable>(), It.IsAny<string>()))
            //    .Callback<IEnumerable, string>((x, y) =>
            //    {
            //        recordsResult = x;
            //        filenameResult = y;
            //    });
        }

        public void VerifyCsvRecords<T>(BacsExportType bacsExportType, Action<T[]> verify)
        {
            var filename = BacsExportService.GetFilename(bacsExportType);
            Assert.Equal(filename, _filenameResult);
            var records = Assert.IsAssignableFrom<IEnumerable<T>>(_recordsResult);
            verify(records.ToArray());
        }

        public Mock<ICsvWriterWrapper> GetMockedCsvWriterWrapper()
        {
            var csvWriter = new Mock<ICsvWriterWrapper>();
            csvWriter.Setup(x => x.WriteRecords(It.IsAny<IEnumerable>(), It.IsAny<string>()))
                .Callback<IEnumerable, string>((x, y) =>
                {
                    _recordsResult = x;
                    _filenameResult = y;
                });
            return csvWriter;

        }
    }
}
