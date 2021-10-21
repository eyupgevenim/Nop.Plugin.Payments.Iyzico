namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using System;
    using Nop.Core;

    public class IyzicoPayment : BaseEntity
    {
        /// <summary>
        /// Gets or sets a customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Merchant's basket ID
        /// </summary>
        public string BasketId { get; set; }

        /// <summary>
        /// If verification is successful, iyzico will return a paymentid. It must be set in Auth request
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Final amount (including installment fee) that will be charged to customer’s card
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Installment value. For single installment payments it should be 1 (valid values: 1, 2, 3, 6, 9, 12)
        /// </summary>
        public int? Installment { get; set; }

        /// <summary>
        /// Gets or sets the date and time of IyzicoPayment creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /**
        /// <summary>
        /// Gets or sets an payment status identifier (initialize, callback_post, confirmed, failed, voided)
        /// </summary>
        public int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status (initialize, callback_post, confirmed, failed, voided)
        /// </summary>
        public IyzicoPaymentStatus PaymentStatus
        {
            get => (IyzicoPaymentStatus)PaymentStatusId;
            set => PaymentStatusId = (int)value;
        }
        */

    }
}