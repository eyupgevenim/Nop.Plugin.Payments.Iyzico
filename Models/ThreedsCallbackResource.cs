namespace Nop.Plugin.Payments.Iyzico.Models
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// https://dev.iyzipay.com/tr/api/3d-ile-odeme
    /// </summary>
    public class ThreedsCallbackResource
    {
        /// <summary>
        /// Service response result (success / failure)
        /// </summary>
        [BindProperty(Name = "status")]
        public string Status { get; set; }

        /// <summary>
        /// If verification is successful, iyzico will return a paymentid. It must be set in Auth request
        /// </summary>
        [BindProperty(Name = "paymentId")]
        public string PaymentId { get; set; }

        /// <summary>
        /// If verification is successful, iyzico might return. If returns, it must be set in Auth request
        /// </summary>
        [BindProperty(Name = "conversationData")]
        public string ConversationData { get; set; }

        /// <summary>
        /// If set, conversation ID to match request and response
        /// </summary>
        [BindProperty(Name = "conversationId")]
        public long ConversationId { get; set; }

        /// <summary>
        /// 1 for successful payment, 0,2,3,4,5,6,7,8 for failure payments
        ///     mdStatus = 0	Invalid 3D Secure signature or verification
        ///     mdStatus = 2	Card holder or Issuer not registered to 3D Secure network
        ///     mdStatus = 3	Issuer is not registered to 3D secure network
        ///     mdStatus = 4	Verification is not possible, card holder chosen to register later on system
        ///     mdStatus = 5    Verification is not possbile
        ///     mdStatus = 6	3D Secure error
        ///     mdStatus = 7	System error
        ///     mdStatus = 8	Unknown card
        /// </summary>
        [BindProperty(Name = "mdStatus")]
        public string MdStatus { get; set; }
    }
}
