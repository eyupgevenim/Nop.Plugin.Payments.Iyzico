namespace Nop.Plugin.Payments.Iyzico.Domain
{
    using System;
    using Nop.Core;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Orders;

    public class IyzicoPaymentRefund : BaseEntity
    {
        /// <summary>
        /// Gets or sets a customer identifier
        /// </summary>
        public int CustomerId { get; set; }
        //TODO:...
        /**
        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }
        */

        /// <summary>
        /// Order id
        /// </summary>
        public int OrderId { get; set; }
        //TODO:...
        /**
        /// <summary>
        /// Gets or sets the order
        /// </summary>
        public virtual Order Order { get; set; }
        */

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