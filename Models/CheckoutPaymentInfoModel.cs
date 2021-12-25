namespace Nop.Plugin.Payments.Iyzico.Models
{
    using Nop.Web.Framework.Models;

    public partial class CheckoutPaymentInfoModel : BaseNopModel
    {
        public string PaymentViewComponentName { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool DisplayOrderTotals { get; set; }
    }
}
