﻿using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Services;

namespace Sonovate.CodeTest
{
    public static class Application
    {
        public static void Main()
        {
            var builder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "EnableAgencyPayments", "rrrr" }
            });

            Settings = builder.Build();

            new BacsExportService().ExportZip(BacsExportType.Agency).Wait();
            new BacsExportService().ExportZip(BacsExportType.Supplier).Wait();
        }

        public static IConfigurationRoot Settings { get; private set; }
    }
}