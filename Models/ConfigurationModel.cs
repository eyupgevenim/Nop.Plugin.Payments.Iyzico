namespace Nop.Plugin.Payments.Iyzico.Models
{
    using System;
    using Nop.Web.Framework.Models;
    using Nop.Web.Framework.Mvc.ModelBinding;

    public record ConfigurationModel : BaseNopModel, ISettingsModel, IEquatable<ConfigurationModel>
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.BaseUrl")]
        public string BaseUrl { get; set; }
        public bool BaseUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.CheckoutCookieExpires")]
        public int CheckoutCookieExpires { get; set; }
        public bool CheckoutCookieExpires_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.Enable3DSecure")]
        public bool Enable3DSecure { get; set; }
        public bool Enable3DSecure_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Admin.Fields.PaymentMethodDescription")]
        public string PaymentMethodDescription { get; set; }
    }
}
