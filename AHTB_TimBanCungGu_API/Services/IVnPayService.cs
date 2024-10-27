using Microsoft.AspNetCore.Http;
using System.Net.Http;
using AHTB_TimBanCungGu_API.ViewModels;

namespace AHTB_TimBanCungGu_MVC.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
