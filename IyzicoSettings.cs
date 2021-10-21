namespace Nop.Plugin.Payments.Iyzico
{
    using Nop.Core.Configuration;

    /// <summary>
    /// Represents settings of manual payment plugin
    /// </summary>
    public class IyzicoSettings : ISettings
    {
        /// <summary>
        /// Api key
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Secret key
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// Api base url
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Iyzico payment method description
        /// </summary>
        public string PaymentMethodDescription { get; set; }
        /// <summary>
        /// Expiration time on seconds for the "Iyzico checkout" cookie
        /// </summary>
        public int CheckoutCookieExpires { get; set; }
        /// <summary>
        /// Enable 3D Secure
        /// </summary>
        public bool Enable3DSecure { get; set; }
    }
}
