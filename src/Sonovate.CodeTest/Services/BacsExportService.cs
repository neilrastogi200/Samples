using System;
using System.Threading.Tasks;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Factory;

namespace Sonovate.CodeTest.Services
{
    public class BacsExportService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ICsvWriterWrapper _csvWriterWrapper;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
     
        public BacsExportService() : this (new PaymentServiceFactory(),new DateTimeService(), new CsvWriterWrapper() )
        {
           //_dateTimeService = new DateTimeService();
           //_paymentServiceFactory = new PaymentServiceFactory();
           //_csvWriterWrapper = new CsvWriterWrapper();
        }

        public BacsExportService(IPaymentServiceFactory paymentServiceFactory, IDateTimeService dateTimeService, ICsvWriterWrapper csvWriterWrapper)
        {
            _paymentServiceFactory = paymentServiceFactory;
            _dateTimeService = dateTimeService;
            _csvWriterWrapper = csvWriterWrapper;
        }

        public async Task ExportZip(BacsExportType bacsExportType)
        {
            if (bacsExportType == BacsExportType.None)
            {
                const string invalidExportTypeMessage = "No export type provided.";
                throw new Exception(invalidExportTypeMessage);
            }

            var startDate = _dateTimeService.GetStartDateTime();
            var endDate = _dateTimeService.GetCurrentDateTime();

            var service = _paymentServiceFactory.GetPaymentTypeService(bacsExportType);

            if (!service.ArePaymentsEnabled())
            {
                //return;
                throw new Exception("Invalid BACS Export Type.");
            }

            var payments = await service.GetPayments(startDate, endDate);
            var filename = GetFilename(bacsExportType);
            _csvWriterWrapper.WriteRecords(payments,filename);
        }

        public static string GetFilename(BacsExportType type)
        {
            return $"{type}_BACSExport.csv";;
        }
    }
}