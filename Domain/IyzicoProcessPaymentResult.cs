namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using Nop.Services.Payments;

    public class IyzicoProcessPaymentResult : ProcessPaymentResult
    {
        public string HtmlContent { get; set; }
    }
}
