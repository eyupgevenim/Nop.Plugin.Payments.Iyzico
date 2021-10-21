namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using System;
    using Nop.Core;

    public class IyzicoPaymentRefund : BaseEntity
    {
        /// <summary>
        /// Gets or sets a customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Order id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// ID of the basket item that needs to be refunded
        /// </summary>
        public string PaymentTransactionId { get; set; }

        /// <summary>
        /// ID of refunded payment
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Amount that needs to be refunded
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the date and time of IyzicoPayment creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}