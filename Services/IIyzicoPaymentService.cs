namespace Nop.Plugin.Payments.Iyzico.Services
{
    using Microsoft.AspNetCore.Http;
    using Nop.Core.Domain.Orders;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Services.Payments;
    using System.Collections.Generic;

    public interface IIyzicoPaymentService
    {
        #region Methods

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        PaymentInfoModel ValidatePaymentForm(IFormCollection form);

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>ProcessPaymentRequest</returns>
        ProcessPaymentRequest GetPaymentInfo(IFormCollection form);

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// ProcessPaymentResult
        /// </returns>
        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// RefundPaymentResult
        /// </returns>
        RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <returns>Result</returns>
        bool IsConfigured();

        /// <summary>
        /// Process a payment
        /// Initialize (3D) three d secure
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// ProcessPaymentResult
        /// </returns>
        ProcessPaymentResult ProcessPaymentThreedsInitialize(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Get Installments
        /// </summary>
        /// <param name="binNumber">Bank Identification Number</param>
        /// <returns>
        /// IList<Installment> 
        /// </returns>
        IList<Installment> GetInstallment(string binNumber);

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
