namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using System.ComponentModel;

    public enum IyzicoPaymentStatus
    {
        [Description("initialize")]
        INITIALIZE = 0,
        [Description("callback_post")]
        CALLBACKPOST = 1,
        [Description("confirmed")]
        CONFIRMED = 3,
        [Description("failed")]
        FAILED = 4,
        [Description("voided")]
        VOIDED = 5
    }
}
