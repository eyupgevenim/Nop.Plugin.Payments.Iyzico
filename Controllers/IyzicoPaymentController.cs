namespace Nop.Plugin.Payments.Iyzico.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Nop.Core;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Orders;
    using Nop.Core.Http.Extensions;
    using Nop.Plugin.Payments.Iyzico.Domain;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Plugin.Payments.Iyzico.Services;
    using Nop.Plugin.Payments.Iyzico.Validators;
    using Nop.Services.Common;
    using Nop.Services.Customers;
    using Nop.Services.Localization;
    using Nop.Services.Logging;
    using Nop.Services.Orders;
    using Nop.Services.Payments;
    using Nop.Web.Framework.Controllers;
    using Nop.Web.Models.Checkout;

    public class IyzicoPaymentController : BasePluginController
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IIyzicoPaymentService _iyzicoPaymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor
        public IyzicoPaymentController(ILogger logger,
            IIyzicoPaymentService iyzicoPaymentService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            OrderSettings orderSettings,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IOrderService orderService)
        {
            _logger = logger;
            _iyzicoPaymentService = iyzicoPaymentService;
            _localizationService = localizationService;
            _workContext = workContext;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _orderSettings = orderSettings;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _orderService = orderService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Enter Payment Info
        /// </summary>
        /// <param name="form">IFormCollection</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        [HttpPost]
        [FormValueRequired("nextstep")]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> PaymentInfo(IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            var paymentInfoModel = await _iyzicoPaymentService.ValidatePaymentFormAsync(form);
            foreach (var warning in paymentInfoModel.Warnings)
                ModelState.AddModelError("", warning);
            if (ModelState.IsValid)
            {
                //get payment info
                var processPaymentRequest = await _iyzicoPaymentService.GetPaymentInfoAsync(form);
                //set previous order GUID (if exists)
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = paymentMethodSystemName;

                try
                {
                    var iyzicoProcessPaymentResult = (IyzicoProcessPaymentResult)await _iyzicoPaymentService.ProcessPaymentThreedsInitializeAsync(processPaymentRequest);
                    if (iyzicoProcessPaymentResult.Success)
                    {
                        //session save
                        _iyzicoPaymentService.SetCheckoutCookie("OrderPaymentInfo", processPaymentRequest);
                        //return new ContentResult { Content = iyzicoProcessPaymentResult.HtmlContent, ContentType = "text/html; charset=utf-8",  StatusCode = (int)HttpStatusCode.OK };
                        return Content(iyzicoProcessPaymentResult.HtmlContent, "text/html; charset=utf-8");
                    }

                    paymentInfoModel.Warnings = iyzicoProcessPaymentResult.Errors;
                }
                catch (Exception ex)
                {
                    paymentInfoModel.Warnings.Add(ex.Message);
                    await _logger.ErrorAsync(ex.Message, ex);
                }
            }

            HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, paymentInfoModel);
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        /// <summary>
        /// Iyzico callback action
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        public virtual IActionResult CallbackConfirm(ThreedsCallbackResource threedsCallbackResource)
        {
            //validate
            var warnings = ValidationThreedsCallbackResource(threedsCallbackResource);
            if (warnings.Any())
            {
                var paymentInfoModel = new PaymentInfoModel { Warnings = warnings };
                HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, paymentInfoModel);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/CallbackConfirm.cshtml", threedsCallbackResource);
        }

        /// <summary>
        /// Order confirm action
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        [HttpPost]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> OrderConfirm(ThreedsCallbackResource threedsCallbackResource)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //model
            var model = new PaymentInfoModel();
            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(await _workContext.GetCurrentCustomerAsync()))
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = _iyzicoPaymentService.GetCheckoutCookie<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                        return RedirectToRoute("CheckoutPaymentInfo");

                    processPaymentRequest = new ProcessPaymentRequest();
                }

                var warnings = ValidationThreedsCallbackResource(threedsCallbackResource);
                if (warnings.Any())
                {
                    model.Warnings = warnings;
                    HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, model);
                    return RedirectToRoute("CheckoutPaymentInfo");
                }

                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                
                HttpContext.Session.Set("OrderPaymentInfo", processPaymentRequest);
                HttpContext.Session.Set(nameof(ThreedsCallbackResource), threedsCallbackResource);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    HttpContext.Session.Set<ThreedsCallbackResource>(nameof(ThreedsCallbackResource), null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
                    }

                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, model);
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        /// <summary>
        /// Get iyzico installment by BIN number
        /// </summary>
        /// <param name="binNumber">BinNumber:Bank Identification Number</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetInstallment(string binNumber)
        {
            if (string.IsNullOrEmpty(binNumber))
                return Json(new List<Installment>());

            try
            {
                return Json(await _iyzicoPaymentService.GetInstallmentAsync(binNumber));
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return Json(new List<Installment>());
        }

        #endregion

        #region Methods (one page checkout)

        /// <returns>A task that represents the asynchronous operation</returns>
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> OpcSavePaymentInfo(IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new NopException(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new NopException("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new NopException("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new NopException("Anonymous checkout is not allowed");

                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id)
                    ?? throw new NopException("Payment method is not selected");

                var paymentInfoModel = await _iyzicoPaymentService.ValidatePaymentFormAsync(form);
                foreach (var warning in paymentInfoModel.Warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    //get payment info
                    var processPaymentRequest = await paymentMethod.GetPaymentInfoAsync(form);
                    //set previous order GUID (if exists)
                    _paymentService.GenerateOrderGuid(processPaymentRequest);
                    processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                    processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                    processPaymentRequest.PaymentMethodSystemName = paymentMethodSystemName;

                    try
                    {
                        var iyzicoProcessPaymentResult = (IyzicoProcessPaymentResult)await _iyzicoPaymentService.ProcessPaymentThreedsInitializeAsync(processPaymentRequest);
                        if (iyzicoProcessPaymentResult.Success)
                        {
                            //session save
                            _iyzicoPaymentService.SetCheckoutCookie("OrderPaymentInfo", processPaymentRequest);
                            var htmlContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(iyzicoProcessPaymentResult.HtmlContent));
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel
                                {
                                    name = "payment-info",
                                    html = htmlContentBase64,
                                },
                                goto_threeds = true
                            });
                        }

                        paymentInfoModel.Warnings = iyzicoProcessPaymentResult.Errors;
                    }
                    catch (Exception ex)
                    {
                        await _logger.WarningAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                        return Json(new { error = 1, message = ex.Message });
                    }
                }

                //If we got this far, something failed, redisplay form
                HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, paymentInfoModel);
                var checkoutPaymentInfoModel = new CheckoutPaymentInfoModel
                {
                    PaymentViewComponentName = paymentMethod.GetPublicViewComponentName(),
                    DisplayOrderTotals = _orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab
                };
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = await RenderPartialViewToStringAsync("~/Views/Checkout/OpcPaymentInfo.cshtml", checkoutPaymentInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        /// <summary>
        /// Iyzico opc callback action
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        public virtual IActionResult OpcCallbackConfirm(ThreedsCallbackResource threedsCallbackResource)
        {
            //validate
            var warnings = ValidationThreedsCallbackResource(threedsCallbackResource);
            if (warnings.Any())
            {
                var model = new OpcCallbackConfirmModel { Warnings = warnings };
                return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcOrderConfirm.cshtml", model);
            }

            return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcCallbackConfirm.cshtml", threedsCallbackResource);
        }

        /// <summary>
        /// Iyzico opc order confirm action
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        public virtual async Task<IActionResult> OpcOrderConfirm(ThreedsCallbackResource threedsCallbackResource)
        {
            var model = new OpcCallbackConfirmModel();
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(await _workContext.GetCurrentCustomerAsync()))
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = _iyzicoPaymentService.GetCheckoutCookie<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }

                var warnings = ValidationThreedsCallbackResource(threedsCallbackResource);
                if (warnings.Any())
                {
                    throw new Exception(string.Join(" \n ", warnings));
                }

                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                HttpContext.Session.Set(nameof(ThreedsCallbackResource), threedsCallbackResource);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    model.SuccessUrl = _iyzicoPaymentService.GetRouteUrl("CheckoutCompleted");
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    HttpContext.Session.Set<ThreedsCallbackResource>(nameof(ThreedsCallbackResource), null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(),
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (paymentMethod == null)
                    {
                        //payment method could be null if order total is 0
                        //success
                        return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcOrderConfirm.cshtml", model);
                    }

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        model.SuccessUrl = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment";
                        return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcOrderConfirm.cshtml", model);
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    //success
                    return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcOrderConfirm.cshtml", model);
                }

                //error
                foreach (var error in placeOrderResult.Errors)
                {
                    model.Warnings.Add(error);
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                model.Warnings.Add(exc.Message);
            }

            return View($"{IyzicoDefaults.IYZICO_VIEWS_PATH}/OpcOrderConfirm.cshtml", model);
        }

        #endregion

        #region Utilities

        protected virtual IList<string> ValidationThreedsCallbackResource(ThreedsCallbackResource threedsCallbackResource)
        {
            var warnings = new List<string>();

            var validator = new ThreedsCallbackResourceValidator(_localizationService);
            var validationResult = validator.Validate(threedsCallbackResource);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    warnings.Add(error.ErrorMessage);
                }
            }

            return warnings;
        }

        protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                    customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1)
                ).FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        #endregion

    }
}
