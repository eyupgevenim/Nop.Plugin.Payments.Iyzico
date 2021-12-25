namespace Nop.Plugin.Payments.Iyzico.Components
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Nop.Core.Http.Extensions;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Web.Framework.Components;

    [ViewComponent(Name = IyzicoDefaults.OPC_PAYMENT_INFO_VIEW_COMPONENT_NAME)]
    public class IyzicoOpcPaymentViewComponent : NopViewComponent
    {
        private readonly IyzicoSettings _iyzicoSettings;
        public IyzicoOpcPaymentViewComponent(IyzicoSettings iyzicoSettings)
        {
            _iyzicoSettings = iyzicoSettings;
        } 

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            //prepare payment model
            var paymentInfoModel = GetPaymentInfoModel();
            paymentInfoModel.Enable3DSecure = _iyzicoSettings.Enable3DSecure;

            var currentYear = DateTime.Now.Year;

            //prepare years
            var expireYears = Enumerable.Range(0, 14).Select(i => $"{currentYear + i}").Select(y => new SelectListItem { Text = y, Value = y });
            paymentInfoModel.ExpireYears.AddRange(expireYears);

            //prepare months
            var expireMonths = Enumerable.Range(1, 12).Select(month => new SelectListItem { Text = $"{month:D2}", Value = $"{month}" });
            paymentInfoModel.ExpireMonths.AddRange(expireMonths);

            return View("~/Plugins/Payments.Iyzico/Views/Iyzico/OpcPaymentInfo.cshtml", paymentInfoModel);
        }

        private PaymentInfoModel GetPaymentInfoModel()
        {
            var paymentInfoModel = HttpContext.Session.Get<PaymentInfoModel>(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION);
            if (paymentInfoModel == null)
            {
                paymentInfoModel = new PaymentInfoModel();
            }
            else
            {
                HttpContext.Session.Remove(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION);
            }

            return paymentInfoModel;
        }

    }
}