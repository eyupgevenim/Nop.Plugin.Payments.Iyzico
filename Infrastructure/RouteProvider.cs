namespace Nop.Plugin.Payments.Iyzico.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Nop.Web.Framework;
    using Nop.Web.Framework.Mvc.Routing;

    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.ConfigurationRouteName, "Plugins/Iyzico/Configure",
                new { controller = "IyzicoSettings", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.PaymentInfoRouteName, "Iyzico/PaymentInfo",
                new { controller = "IyzicoPayment", action = "PaymentInfo" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.CallbackConfirmRouteName, "Iyzico/CallbackConfirm",
                new { controller = "IyzicoPayment", action = "CallbackConfirm" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.OrderConfirmRouteName, "Iyzico/OrderConfirm",
                new { controller = "IyzicoPayment", action = "OrderConfirm" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.OpcCallbackConfirmRouteName, "Iyzico/OpcCallbackConfirm",
                new { controller = "IyzicoPayment", action = "OpcCallbackConfirm" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.OpcOrderConfirmRouteName, "Iyzico/OpcOrderConfirm",
                new { controller = "IyzicoPayment", action = "OpcOrderConfirm" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.OpcSavePaymentInfoRouteName, "Iyzico/OpcSavePaymentInfo",
                new { controller = "IyzicoPayment", action = "OpcSavePaymentInfo" });

            endpointRouteBuilder.MapControllerRoute(IyzicoDefaults.GetInstallmentRouteName, "Iyzico/GetInstallment",
                new { controller = "IyzicoPayment", action = "GetInstallment" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1;

        #endregion
    }
}