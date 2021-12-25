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
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute(IyzicoDefaults.ConfigurationRouteName, "Plugins/Iyzico/Configure",
                new { controller = "IyzicoSettings", action = "Configure", area = AreaNames.Admin });

            routeBuilder.MapRoute(IyzicoDefaults.PaymentInfoRouteName, "Iyzico/PaymentInfo",
                new { controller = "IyzicoPayment", action = "PaymentInfo" });

            routeBuilder.MapRoute(IyzicoDefaults.CallbackConfirmRouteName, "Iyzico/CallbackConfirm",
                new { controller = "IyzicoPayment", action = "CallbackConfirm" });

            routeBuilder.MapRoute(IyzicoDefaults.OrderConfirmRouteName, "Iyzico/OrderConfirm",
                new { controller = "IyzicoPayment", action = "OrderConfirm" });

            routeBuilder.MapRoute(IyzicoDefaults.OpcCallbackConfirmRouteName, "Iyzico/OpcCallbackConfirm",
                new { controller = "IyzicoPayment", action = "OpcCallbackConfirm" });

            routeBuilder.MapRoute(IyzicoDefaults.OpcOrderConfirmRouteName, "Iyzico/OpcOrderConfirm",
                new { controller = "IyzicoPayment", action = "OpcOrderConfirm" });

            routeBuilder.MapRoute(IyzicoDefaults.OpcSavePaymentInfoRouteName, "Iyzico/OpcSavePaymentInfo",
                new { controller = "IyzicoPayment", action = "OpcSavePaymentInfo" });

            routeBuilder.MapRoute(IyzicoDefaults.GetInstallmentRouteName, "Iyzico/GetInstallment",
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