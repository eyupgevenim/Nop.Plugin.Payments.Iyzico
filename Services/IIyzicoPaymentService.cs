namespace Nop.Plugin.Payments.Iyzico.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Nop.Core.Domain.Orders;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Services.Payments;

    public interface IIyzicoPaymentService
    {
        #region Methods

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        Task<PaymentInfoModel> ValidatePaymentFormAsync(IFormCollection form);

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form);

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest);

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <returns>Result</returns>
        Task<bool> IsConfiguredAsync();

        /// <summary>
        /// Process a payment
        /// Initialize (3D) three d secure
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        Task<ProcessPaymentResult> ProcessPaymentThreedsInitializeAsync(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Get Installments
        /// </summary>
        /// <param name="binNumber">Bank Identification Number</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<IList<Installment>> GetInstallmentAsync(string binNumber);

        /// <summary>
        /// Set iyzico checkout cookie
        /// </summary>
        /// <param name="key">key:cookie name</param>
        /// <param name="value">value of the cookie</param>
        void SetCheckoutCookie<T>(string key, T value);

        /// <summary>
        /// Get value from cookie
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        T GetCheckoutCookie<T>(string key);

        #endregion
    }
}
