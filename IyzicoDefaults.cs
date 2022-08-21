namespace Nop.Plugin.Payments.Iyzico
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class IyzicoDefaults
    {
        #region Payments Iyzico

        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Iyzico";

        #endregion

        #region Routing

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Payments.Iyzico.Configure";

        /// <summary>
        /// Gets the get PaymentInfo route name
        /// </summary>
        public static string PaymentInfoRouteName => "Plugin.Payments.Iyzico.PaymentInfo";

        /// <summary>
        /// Gets the get CallbackConfirm route name
        /// </summary>
        public static string CallbackConfirmRouteName => "Plugin.Payments.Iyzico.CallbackConfirm";

        /// <summary>
        /// Gets the get OrderConfirm route name
        /// </summary>
        public static string OrderConfirmRouteName => "Plugin.Payments.Iyzico.OrderConfirm";

        /// <summary>
        /// Gets the get OpcCallbackConfirm route name
        /// </summary>
        public static string OpcCallbackConfirmRouteName => "Plugin.Payments.Iyzico.OpcCallbackConfirm";

        /// <summary>
        /// Gets the get OpcOrderConfirm route name
        /// </summary>
        public static string OpcOrderConfirmRouteName => "Plugin.Payments.Iyzico.OpcOrderConfirm";

        /// <summary>
        /// Gets the get OpcSavePaymentInfo route name
        /// </summary>
        public static string OpcSavePaymentInfoRouteName => "Plugin.Payments.Iyzico.OpcSavePaymentInfo";

        /// <summary>
        /// Gets the get installment route name
        /// </summary>
        public static string GetInstallmentRouteName => "Plugin.Payments.Iyzico.GetInstallment";

        #endregion

        #region consts

        /// <summary>
        /// The Payment Info model session key
        /// </summary>
        public const string PAYMENT_INFO_MODEL_SESSION = "Iyzico.PaymentInfo.Model";

        /// <summary>
        /// Gets a name of the view component to display payment info in public store
        /// </summary>
        public const string PAYMENT_INFO_VIEW_COMPONENT_NAME = "IyzicoPaymentInfo";

        /// <summary>
        /// Gets a name of the view component to display one page checkout(opc) payment info in public store
        /// </summary>
        public const string OPC_PAYMENT_INFO_VIEW_COMPONENT_NAME = "IyzicoOpcPaymentInfo";

        /// <summary>
        /// Installment key
        /// </summary>
        public const string INSTALLMENT_KEY = "Plugins.Payments.Iyzico.Installment";

        /// <summary>
        /// Database schema
        /// </summary>
        public const string DB_PAYMENT_SCHEMA = "Payment";

        /// <summary>
        /// Iyzico views path
        /// </summary>
        public const string IYZICO_VIEWS_PATH = "~/Plugins/Payments.Iyzico/Views/Iyzico";

        /// <summary>
        /// Iyzico settings views path
        /// </summary>
        public const string IYZICO_CONFIGURE_VIEWS_PATH = "~/Plugins/Payments.Iyzico/Views/Configure";

        #endregion

    }
}
