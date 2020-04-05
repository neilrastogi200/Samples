using Sonovate.CodeTest.Domain;
using Sonovate.CodeTest.Services;

namespace Sonovate.CodeTest.Factory
{
    public interface IPaymentServiceFactory
    {
        IPaymentService GetPaymentTypeService(BacsExportType bacsExportType);
    }
}
