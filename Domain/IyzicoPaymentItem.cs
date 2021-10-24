namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using System;
    using Nop.Core;

    /// <summary>
    /// Iyzico itemTransactions
    /// </summary>
    public class IyzicoPaymentItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets a IyzicoPayment identifier
        /// </summary>
        public int IyzicoPaymentId { get; set; }
        //TODO:...
        /**
        /// <summary>
        /// Gets or sets the IyzicoPayment
        /// </summary>
        public virtual IyzicoPayment IyzicoPayment { get; set; }
        */

        /// <summary>
        /// ID of basket item. Merchants should keep payment ID in their system (this ID will be used for cancel requests)
        /// </summary>
        public string PaymentTransactionId { get; set; }

        /// <summary>
        /// Item ID of each item in basket / nopCommerce ProductId
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Captured amount from card based on each item. Merchants should keep paidPrice in their system
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Item Type. Valid values are PHYSICAL,VIRTUAL
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the date and time of IyzicoPaymentItem creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}