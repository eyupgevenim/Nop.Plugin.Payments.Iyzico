namespace Nop.Plugin.Payments.Iyzico.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Iyzipay.Model;
    using Iyzipay.Request;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using Nop.Core;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Localization;
    using Nop.Core.Domain.Orders;
    using Nop.Core.Domain.Payments;
    using Nop.Core.Domain.Tax;
    using Nop.Core.Http.Extensions;
    using Nop.Data;
    using Nop.Plugin.Payments.Iyzico.Domain;
    using Nop.Plugin.Payments.Iyzico.Helpers;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Plugin.Payments.Iyzico.Validators;
    using Nop.Services.Catalog;
    using Nop.Services.Common;
    using Nop.Services.Customers;
    using Nop.Services.Directory;
    using Nop.Services.Localization;
    using Nop.Services.Orders;
    using Nop.Services.Payments;
    using Nop.Services.Tax;

    public class IyzicoPaymentService : IIyzicoPaymentService
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IyzicoSettings _iyzicoSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly TaxSettings _taxSettings;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<IyzicoPayment> _iyzicoPaymentRepository;
        private readonly IRepository<IyzicoPaymentItem> _iyzicoPaymentItemRepository;
        private readonly IRepository<IyzicoPaymentRefund> _iyzicoPaymentRefundRepository;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly OrderSettings _orderSettings;
        #endregion

        #region Ctor
        public IyzicoPaymentService(ICustomerService customerService,
            IyzicoSettings iyzicoPaymentSettings,
            IGenericAttributeService genericAttributeService,
            IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IProductService productService,
            ICategoryService categoryService,
            IShoppingCartService shoppingCartService,
            IPriceCalculationService priceCalculationService,
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService,
            IWorkContext workContext,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            TaxSettings taxSettings,
            IWebHelper webHelper,
            IRepository<IyzicoPayment> iyzicoPaymentRepository,
            IRepository<IyzicoPaymentItem> iyzicoPaymentItemRepository,
            IRepository<IyzicoPaymentRefund> iyzicoPaymentRefundRepository,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory, 
            OrderSettings orderSettings)
        {
            _customerService = customerService;
            _addressService = addressService;
            _countryService = countryService;
            _iyzicoSettings = iyzicoPaymentSettings;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _productService = productService;
            _categoryService = categoryService;
            _shoppingCartService = shoppingCartService;
            _priceCalculationService = priceCalculationService;
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderService = orderService;
            _workContext = workContext;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _taxSettings = taxSettings;
            _webHelper = webHelper;
            _iyzicoPaymentRepository = iyzicoPaymentRepository;
            _iyzicoPaymentItemRepository = iyzicoPaymentItemRepository;
            _iyzicoPaymentRefundRepository = iyzicoPaymentRefundRepository;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _orderSettings = orderSettings;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public virtual async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            var processPaymentRequest = _httpContextAccessor.HttpContext?.Session?.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest != null)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, shoppingCartType: ShoppingCartType.ShoppingCart);

                var(shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);

                var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
                var retrieveInstallmentInfoRequest = new RetrieveInstallmentInfoRequest()
                {
                    BinNumber = processPaymentRequest.CreditCardNumber.Substring(0, 6),
                    Locale = locale,
                    Price = await RoundAndFormatPriceAsync(shoppingCartTotal ?? 0),
                    ConversationId = string.Empty
                };

                var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
                var installmentInfo = InstallmentInfo.Retrieve(retrieveInstallmentInfoRequest, options);

                if (installmentInfo.Status == $"{Status.SUCCESS}" && installmentInfo.InstallmentDetails.Any())
                {
                    var installmentKey = await _localizationService.GetResourceAsync(IyzicoDefaults.INSTALLMENT_KEY);
                    var installmentValue = (string)processPaymentRequest.CustomValues.GetValueOrDefault(installmentKey);

                    int.TryParse(installmentValue, out int formInstallment);

                    var installmentTotalPrice = installmentInfo.InstallmentDetails.FirstOrDefault()
                        .InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == formInstallment).TotalPrice;

                    var fee = DecimalParse(installmentTotalPrice) - (shoppingCartTotal ?? 0);

                    return await _paymentService.CalculateAdditionalFeeAsync(cart, fee, false);
                }
            }

            return decimal.Zero;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public virtual Task<PaymentInfoModel> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var model = new PaymentInfoModel
            {
                CardholderName = form[nameof(PaymentInfoModel.CardholderName)],
                CardNumber = form[nameof(PaymentInfoModel.CardNumber)],
                ExpireMonth = form[nameof(PaymentInfoModel.ExpireMonth)],
                ExpireYear = form[nameof(PaymentInfoModel.ExpireYear)],
                CardCode = form[nameof(PaymentInfoModel.CardCode)],
                Installment = form[nameof(PaymentInfoModel.Installment)]
            };

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var validationResult = validator.Validate(model);

            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    model.Warnings.Add(error.ErrorMessage);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <returns>Result</returns>
        public virtual Task<bool> IsConfiguredAsync()
        {
            //client id and secret are required to request services
            return Task.FromResult(!string.IsNullOrEmpty(_iyzicoSettings?.ApiKey) || !string.IsNullOrEmpty(_iyzicoSettings?.SecretKey));
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public virtual Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //set card details
            var paymentRequest = new ProcessPaymentRequest
            {
                CreditCardName = form[nameof(PaymentInfoModel.CardholderName)],
                CreditCardNumber = form[nameof(PaymentInfoModel.CardNumber)],
                CreditCardExpireMonth = int.Parse(form[nameof(PaymentInfoModel.ExpireMonth)]),
                CreditCardExpireYear = int.Parse(form[nameof(PaymentInfoModel.ExpireYear)]),
                CreditCardCvv2 = form[nameof(PaymentInfoModel.CardCode)],
            };

            var installment = form[nameof(PaymentInfoModel.Installment)];
            if (!StringValues.IsNullOrEmpty(installment) && !installment.FirstOrDefault().Equals(Guid.Empty.ToString()))
            {
                var installmentKey = _localizationService.GetResourceAsync(IyzicoDefaults.INSTALLMENT_KEY).Result;
                paymentRequest.CustomValues.Add(installmentKey, installment.FirstOrDefault());
            }

            return Task.FromResult(paymentRequest);
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public virtual async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            try
            {
                if (_iyzicoSettings.Enable3DSecure)
                {
                    var threedsCallbackResource = _httpContextAccessor.HttpContext.Session.Get<ThreedsCallbackResource>(nameof(ThreedsCallbackResource));
                    if (threedsCallbackResource != null)
                    {
                        result = await CreateThreedsPaymentAsync(threedsCallbackResource);
                    }
                    else
                    {
                        string errorMessage = await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.Not3DCallbackResource");
                        result.AddError(errorMessage);
                    }
                }
                else
                {
                    var createPaymentRequest = await GetCreatePaymentRequestAsync(processPaymentRequest);
                    var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
                    var payment = Payment.Create(createPaymentRequest, options);

                    if (payment.Status == $"{Status.SUCCESS}")
                    {
                        await AddIyzicoPaymentAsync(payment);
                        result.NewPaymentStatus = PaymentStatus.Pending;
                    }
                    else
                    {
                        string errorMessage = await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.{payment.ErrorCode}");
                        result.AddError(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;
            if (await ExistsIyzicoPaymentRefundAsync(order.Id))
                throw new NopException("Previously returned");

            var customerId = order.CustomerId;
            var iyzicoPayment = await GetIyzicoPaymentByBasketIdAsync($"{order.OrderGuid}") ?? throw new NopException("The payment was not.");
            var iyzicoPaymentItems = await GetIyzicoPaymentItemsByIyzicoPaymentIdAsync(iyzicoPayment.Id) ?? throw new NopException("The payment was not.");

            if (refundPaymentRequest.AmountToRefund > iyzicoPaymentItems.Sum(x => x.Amount))
                throw new NopException($"Invalid 'amount to refund':{refundPaymentRequest.AmountToRefund}");

            var ip = _webHelper.GetCurrentIpAddress();
            var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
            
            var createAmountBasedRefundRequest = new CreateAmountBasedRefundRequest
            {
                ConversationId = $"{order.CaptureTransactionId}",
                Price = await RoundAndFormatPriceAsync(refundPaymentRequest.AmountToRefund),
                PaymentId = iyzicoPayment.PaymentId,
                Ip = ip,
                Locale = locale,
            };

            var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
            Refund refund = Refund.CreateAmountBasedRefundRequest(createAmountBasedRefundRequest, options);
            if (refund.Status == $"{Status.FAILURE}")
            {
                string errorMessage = await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.{refund.ErrorCode}") ?? refund.ErrorMessage;
                return new RefundPaymentResult { Errors = new[] { errorMessage } };
            }

            await _iyzicoPaymentRefundRepository.InsertAsync(new IyzicoPaymentRefund
            {
                CustomerId = customerId,
                OrderId = order.Id,
                PaymentTransactionId = string.IsNullOrEmpty(refund.PaymentTransactionId) ? "0" : refund.PaymentTransactionId,
                Amount = DecimalParse(refund.Price),
                PaymentId = refund.PaymentId,
                CreatedOnUtc = DateTime.UtcNow
            });

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        /// <summary>
        /// Process a payment
        /// Initialize (3D) three d secure
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public virtual async Task<ProcessPaymentResult> ProcessPaymentThreedsInitializeAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var processPaymentResult = new IyzicoProcessPaymentResult();
            
            var createPaymentRequest = await GetCreatePaymentRequestAsync(processPaymentRequest);
            var callbackConfirmRouteValue = _orderSettings.OnePageCheckoutEnabled
                ? GetRouteUrl(IyzicoDefaults.OpcCallbackConfirmRouteName)
                : GetRouteUrl(IyzicoDefaults.CallbackConfirmRouteName);
            createPaymentRequest.CallbackUrl = $"{GetBaseUrl()}{callbackConfirmRouteValue}";

            var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
            var threedsInitialize = ThreedsInitialize.Create(createPaymentRequest, options);
            if (threedsInitialize.Status == $"{Status.FAILURE}")
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.{threedsInitialize.ErrorCode}") 
                    ?? threedsInitialize.ErrorMessage);

                return processPaymentResult;
            }

            processPaymentResult.HtmlContent = threedsInitialize.HtmlContent;
            return processPaymentResult;
        }

        /// <summary>
        /// Get Installments
        /// </summary>
        /// <param name="binNumber">Bank Identification Number</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<IList<Installment>> GetInstallmentAsync(string binNumber)
        {
            IList<Installment> installments = new List<Installment>();

            var priceIncludesTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax
                && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

            var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCart);

            var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
            var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
            var retrieveInstallmentInfoRequest = new RetrieveInstallmentInfoRequest()
            {
                BinNumber = binNumber,
                Locale = locale,
                Price = await RoundAndFormatPriceAsync(shoppingCartTotal.Value),
                ConversationId = string.Empty
            };

            var installmentInfo = InstallmentInfo.Retrieve(retrieveInstallmentInfoRequest, options);
            if (installmentInfo.Status == $"{Status.SUCCESS}" && installmentInfo.InstallmentDetails.Any())
            {
                foreach (var installmentDetail in installmentInfo.InstallmentDetails.FirstOrDefault().InstallmentPrices)
                {
                    var installment = new Installment();

                    installment.DisplayName = await _localizationService.GetResourceAsync($"{IyzicoDefaults.INSTALLMENT_KEY}{installmentDetail.InstallmentNumber}");
                    installment.InstallmentNumber = installmentDetail.InstallmentNumber ?? 0;

                    decimal price = DecimalParse(installmentDetail.Price);
                    installment.Price = await FormatPriceAndShowCurrencyAsync(price, priceIncludesTax);

                    decimal totalPrice = DecimalParse(installmentDetail.TotalPrice);
                    installment.TotalPrice = await FormatPriceAndShowCurrencyAsync(totalPrice, priceIncludesTax);

                    installments.Add(installment);
                }

                return installments;
            }
           
            var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotal ?? 0, Currency);
            installments.Add(new Installment
            {
                DisplayName = await _localizationService.GetResourceAsync($"{IyzicoDefaults.INSTALLMENT_KEY}1"),
                InstallmentNumber = 1,
                Price = await FormatPriceAndShowCurrencyAsync(subtotal, priceIncludesTax),
                TotalPrice = await FormatPriceAndShowCurrencyAsync(subtotal, priceIncludesTax)
            });

            return installments;
        }

        /// <summary>
        /// Set iyzico checkout cookie
        /// </summary>
        /// <param name="key">key:cookie name</param>
        /// <param name="value">value of the cookie</param>
        public virtual void SetCheckoutCookie<T>(string key, T value)
        {
            if (_httpContextAccessor.HttpContext?.Response?.HasStarted ?? true)
                return;

            //delete current cookie value
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);

            //get date of cookie expiration
            var cookieExpires = _iyzicoSettings.CheckoutCookieExpires;
            var cookieExpiresDate = DateTime.Now.AddSeconds(cookieExpires);

            //if passed value is empty set cookie as expired
            if (value == null)
                cookieExpiresDate = DateTime.Now.AddMonths(-1);

            //set new cookie value
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate,
                Secure = _webHelper.IsCurrentConnectionSecured(),
                SameSite = SameSiteMode.None
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, JsonConvert.SerializeObject(value), options);
        }

        /// <summary>
        /// Get value from cookie
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public virtual T GetCheckoutCookie<T>(string key)
        {
            var value = _httpContextAccessor.HttpContext?.Request?.Cookies[key];
            if (value == null)
                return default;

            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Generates a URL with an absolute path for the specified routeName.
        /// </summary>
        /// <param name="routeName">The name of the route that is used to generate URL.</param>
        /// <returns>The generated URL.</returns>
        public virtual string GetRouteUrl(string routeName)
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(routeName);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets or sets current user working currency
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual Core.Domain.Directory.Currency Currency => _workContext.GetWorkingCurrencyAsync().Result;

        /// <summary>
        /// Gets current user working language
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual Language Language => _workContext.GetWorkingLanguageAsync().Result;

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="priceIncludesTax">A value indicating whether price includes tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation 
        /// The task result contains the price
        /// </returns>
        protected virtual async Task<string> FormatPriceAndShowCurrencyAsync(decimal price, bool priceIncludesTax)
        {
            return await _priceFormatter.FormatPriceAsync(price, true, Currency, Language.Id, priceIncludesTax);
        }

        /// <summary>
        /// Gets base server url
        /// </summary>
        /// <returns>base server url</returns>
        protected virtual string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase.ToUriComponent()}";
        }

        /// <summary>
        /// Converts the string representation of a number to its System.Decimal equivalent using the specified culture-specific format information.
        /// CultureInfo("en-US") that supplies culture-specific parsing information about s.
        /// </summary>
        /// <param name="value">s represents a number less than System.Decimal.MinValue or greater than System.Decimal.MaxValue.</param>
        /// <returns>
        /// The System.Decimal number equivalent to the number contained in s as specified by provider.
        /// </returns>
        protected virtual decimal DecimalParse(string value)
        {
            return decimal.Parse(value, new CultureInfo("en-US"));
        }

        /// <summary>
        /// Gets address
        /// </summary>
        /// <param name="address"> customer address</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a buyer address      
        /// </returns>
        protected virtual async Task<Address> GetAddressAsync(Core.Domain.Common.Address address)
        {
            var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            if (country == null)
                throw new NopException("Billing address country not set");

            return new Address
            {
                ContactName = $"{address.FirstName} {address.LastName}",
                City = address.City,
                Country = country.Name,
                Description = address.Address1,
                ZipCode = address.ZipPostalCode
            };
        }

        /// <summary>
        /// Get installment
        /// </summary>
        /// <param name="processPaymentRequest">Process Payment Request</param>
        /// <param name="paymentCard">Payment Card</param>
        /// <returns>installment</returns>
        protected virtual async Task<Installment> GetInstallmentAsync(ProcessPaymentRequest processPaymentRequest, PaymentCard paymentCard)
        {
            var installment = new Installment()
            {
                InstallmentNumber = 1,
                TotalPrice = await RoundAndFormatPriceAsync(processPaymentRequest.OrderTotal)
            };

            var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
            var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
            var retrieveInstallmentInfoRequest = new RetrieveInstallmentInfoRequest()
            {
                BinNumber = paymentCard.CardNumber,
                Locale = locale,
                Price = await RoundAndFormatPriceAsync(processPaymentRequest.OrderTotal),
                ConversationId = string.Empty
            };
            var installmentInfo = InstallmentInfo.Retrieve(retrieveInstallmentInfoRequest, options);

            if (installmentInfo.Status == $"{Status.SUCCESS}" && installmentInfo.InstallmentDetails.Any())
            {
                var installmentKey = await _localizationService.GetResourceAsync(IyzicoDefaults.INSTALLMENT_KEY);
                var installmentValue = (string)processPaymentRequest.CustomValues.GetValueOrDefault(installmentKey);

                int.TryParse(installmentValue, out int formInstallment);

                var installmentDetail = installmentInfo.InstallmentDetails.FirstOrDefault()
                    .InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == formInstallment);

                installment.InstallmentNumber = installmentDetail.InstallmentNumber ?? 1;
                installment.TotalPrice = installmentDetail.TotalPrice;
            }

            return installment;
        }

        /// <summary>
        /// Get transaction line items
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>List of transaction items</returns>
        protected virtual async Task<List<BasketItem>> GetItemsAsync(Customer customer, int storeId)
        {
            //get current shopping cart            
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
            var basketItems = shoppingCart.Select(async shoppingCartItem =>
            {
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                var (unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, includeDiscounts: true);
                var (price, _) = await _taxService.GetProductPriceAsync(product, unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(price, await _workContext.GetWorkingCurrencyAsync());

                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(shoppingCartItem.ProductId);
                var categoriesName = categories.Select(c => _categoryService.GetCategoryByIdAsync(c.CategoryId).Result.Name).Aggregate((p, n) => $"{p},{n}");

                return new BasketItem
                {
                    Id = $"{product.Id}",
                    Name = product.Name,
                    Category1 = categoriesName,
                    Price = await RoundAndFormatPriceAsync(shoppingCartUnitPriceWithDiscount * shoppingCartItem.Quantity),
                    ItemType = $"{BasketItemType.PHYSICAL}",
                };

            })?.Select(i => i.Result)?.ToList() ?? new List<BasketItem>();

            //shipping without tax
            var (shippingTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart, false);
            if (shippingTotal.HasValue && shippingTotal.Value != 0)
            {
                basketItems.Add(new BasketItem
                {
                    Id = $"{Guid.NewGuid()}",
                    Name = "Shipping",
                    Category1 = "Shipping",
                    Price = await RoundAndFormatPriceAsync(shippingTotal ?? 0),
                    ItemType = $"{BasketItemType.VIRTUAL}",
                });
            }

            return basketItems;
        }

        /// <summary>
        /// decimal data convert string and format
        /// </summary>
        /// <param name="value">decimal data</param>
        /// <returns>
        /// Formated decimal
        /// </returns>
        protected virtual async Task<string> RoundAndFormatPriceAsync(decimal value)
        {
            return (await _priceCalculationService.RoundPriceAsync(value)).ToString("f2", new CultureInfo("en-US"));
            //return (await _priceCalculationService.RoundPriceAsync(value)).ToString("f2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets buyer information
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a buyer
        /// </returns>
        protected virtual async Task<Buyer> GetBuyerAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            var customerEmail = customer.Email;
            var customerName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            var customerSurName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
            var customerIdentityNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, "IdentityNumber");
            if (string.IsNullOrEmpty(customerIdentityNumber))
                customerIdentityNumber = "11111111111";
            var customerGsmNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);

            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            if (billingAddress == null)
                throw new NopException("Customer billing address not set");

            var country = await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0);
            if (country == null)
                throw new NopException("Billing address country not set");

            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = billingAddress.FirstName;
            }

            if (string.IsNullOrWhiteSpace(customerSurName))
            {
                customerSurName = billingAddress.LastName;
            }

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                customerEmail = billingAddress.Email;
            }

            if (string.IsNullOrWhiteSpace(customerGsmNumber))
            {
                customerGsmNumber = billingAddress.PhoneNumber;
            }

            return new Buyer
            {
                Id = customer.CustomerGuid.ToString(),
                Name = customerName,
                Surname = customerSurName,
                Email = customerEmail,
                IdentityNumber = customerIdentityNumber,
                RegistrationAddress = billingAddress.Address1,
                Ip = customer.LastIpAddress,
                City = billingAddress.City,
                Country = country.Name,
                ZipCode = billingAddress.ZipPostalCode,
                GsmNumber = customerGsmNumber,
            };
        }

        /// <summary>
        /// Gets Iyzico CreatePaymentRequest model
        /// </summary>
        /// <param name="processPaymentRequest">ProcessPaymentRequest</param>
        /// <returns>
        /// A task that represents the asynchronous operation      
        /// </returns>
        protected virtual async Task<CreatePaymentRequest> GetCreatePaymentRequestAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var billingAddressId = (await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId))?.BillingAddressId;
            var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId ?? 0);
            if (billingAddress == null)
                throw new NopException("Customer billing address not set");

            var shippingAddressId = (await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId))?.ShippingAddressId;
            var shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId ?? 0);

            var billingAddressModel = await GetAddressAsync(billingAddress);
            var shippingAddressModel = shippingAddress != null ? await GetAddressAsync(shippingAddress) : billingAddressModel;

            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

            var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);

            if (processPaymentRequest.OrderTotal == 0)
                processPaymentRequest.OrderTotal = shoppingCartTotal.Value;

            var paymentCard = new PaymentCard()
            {
                CardHolderName = processPaymentRequest.CreditCardName,
                CardNumber = processPaymentRequest.CreditCardNumber,
                ExpireMonth = processPaymentRequest.CreditCardExpireMonth.ToString(),
                ExpireYear = processPaymentRequest.CreditCardExpireYear.ToString(),
                Cvc = processPaymentRequest.CreditCardCvv2
            };

            var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
            var currencyCode = IyzicoHelper.GetIyzicoCurrency((await _workContext.GetWorkingCurrencyAsync()).CurrencyCode);
            var installment = await GetInstallmentAsync(processPaymentRequest, paymentCard);
           
            return new CreatePaymentRequest
            {
                Locale = locale,
                PaymentChannel = $"{PaymentChannel.WEB}",
                PaymentGroup = $"{PaymentGroup.PRODUCT}",
                Price = await RoundAndFormatPriceAsync(shoppingCartTotal ?? 0),
                PaidPrice = installment.TotalPrice,
                Currency = currencyCode,
                Installment = installment.InstallmentNumber,
                BasketId = $"{processPaymentRequest.OrderGuid}",
                PaymentCard = paymentCard,
                Buyer = await GetBuyerAsync(processPaymentRequest.CustomerId),
                ShippingAddress = shippingAddressModel,
                BillingAddress = billingAddressModel,
                BasketItems = await GetItemsAsync(customer, processPaymentRequest.StoreId),
            };
        }

        /// <summary>
        /// Creates (3D) three d secure payment
        /// Completing (3D) three d secure transactions
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        protected virtual async Task<ProcessPaymentResult> CreateThreedsPaymentAsync(ThreedsCallbackResource threedsCallbackResource)
        {
            var processPaymentResult = new ProcessPaymentResult();

            if (threedsCallbackResource.Status == $"{Status.SUCCESS}")
            {
                var locale = IyzicoHelper.GetLocale((await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode);
                var options = IyzicoHelper.GetIyzicoOptions(_iyzicoSettings);
                var createThreedsPaymentRequest = new CreateThreedsPaymentRequest
                {
                    Locale = locale,
                    ConversationId = $"{threedsCallbackResource.ConversationId}",
                    PaymentId = threedsCallbackResource.PaymentId,
                    ConversationData = threedsCallbackResource.ConversationData
                };
                var threedsPayment = ThreedsPayment.Create(createThreedsPaymentRequest, options);
                if (threedsPayment.Status == $"{Status.SUCCESS}")
                {
                    await AddIyzicoPaymentAsync(threedsPayment);
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;

                    return processPaymentResult;
                }

                //string errorMessage = await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.{threedsPayment.ErrorCode}");
                processPaymentResult.AddError(threedsPayment.ErrorMessage);//TODO:... get mappin errors
                return processPaymentResult;
            }
            
            var errorMessage = await _localizationService.GetResourceAsync($"Plugins.Payments.Iyzico.ErrorMessage.MdStatus.{threedsCallbackResource.MdStatus}");
            processPaymentResult.AddError(errorMessage);

            return processPaymentResult;
        }

        /// <summary>
        /// Add IyzicoPayment
        /// </summary>
        /// <param name="paymentResource">PaymentResource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task AddIyzicoPaymentAsync(PaymentResource paymentResource)
        {
            var customerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var iyzicoPayment = new IyzicoPayment
            {
                CustomerId = customerId,
                BasketId = paymentResource.BasketId,
                PaymentId = paymentResource.PaymentId,
                Amount = DecimalParse(paymentResource.PaidPrice),
                Installment = paymentResource.Installment,
                CreatedOnUtc = DateTime.Now,
            };
            await _iyzicoPaymentRepository.InsertAsync(iyzicoPayment);
            foreach (var paymentItem in paymentResource.PaymentItems)
            {
                await _iyzicoPaymentItemRepository.InsertAsync(new IyzicoPaymentItem
                {
                    IyzicoPaymentId = iyzicoPayment.Id,
                    PaymentTransactionId = paymentItem.PaymentTransactionId,
                    ProductId = paymentItem.ItemId,
                    Amount = DecimalParse(paymentItem.PaidPrice),
                    Type = "", //TODO:...BasketItemType.PHYSICAL, BasketItemType.VIRTUAL
                    CreatedOnUtc = DateTime.Now,
                });
            }
        }

        /// <summary>
        /// Get IyzicoPayment by basket id
        /// </summary>
        /// <param name="basketId">Iyzico BasketId : e-commerce OrderGuid</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<IyzicoPayment> GetIyzicoPaymentByBasketIdAsync(string basketId)
        {
            var query = _iyzicoPaymentRepository.Table;
            return await query.FirstOrDefaultAsync(q => q.BasketId == basketId);
        }

        /// <summary>
        /// Get IyzicoPaymentItems by IyzicoPaymentId
        /// </summary>
        /// <param name="iyzicoPaymentId">IyzicoPaymentId</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<IList<IyzicoPaymentItem>> GetIyzicoPaymentItemsByIyzicoPaymentIdAsync(int iyzicoPaymentId)
        {
            var query = _iyzicoPaymentItemRepository.Table;
            query = query.Where(q => q.IyzicoPaymentId == iyzicoPaymentId);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Exists IyzicoPaymentRefund
        /// </summary>
        /// <param name="orderId">OrderId</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<bool> ExistsIyzicoPaymentRefundAsync(int orderId)
        {
            var query = _iyzicoPaymentRefundRepository.Table;
            query = query.Where(q => q.OrderId == orderId);

            return await query.AnyAsync();
        }

        #endregion

    }
}
