using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CsvHelper;
using Raven.Client.Documents;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Factory;
using Sonovate.CodeTest.Repositories;

namespace Sonovate.CodeTest.Services
{
    public class BacsExportService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ICsvWriterWrapper _csvWriterWrapper;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
     
        public BacsExportService()
        {
           _dateTimeService = new DateTimeService();
           _paymentServiceFactory = new PaymentServiceFactory();
           _csvWriterWrapper = new CsvWriterWrapper();
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