﻿namespace Nop.Plugin.Payments.Iyzico.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Nop.Core;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Orders;
    using Nop.Core.Domain.Payments;
    using Nop.Core.Http.Extensions;
    using Nop.Plugin.Payments.Iyzico.Domain;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Plugin.Payments.Iyzico.Services;
    using Nop.Plugin.Payments.Iyzico.Validators;
    using Nop.Services.Common;
    using Nop.Services.Localization;
    using Nop.Services.Logging;
    using Nop.Services.Orders;
    using Nop.Services.Payments;
    using Nop.Web.Framework.Controllers;

    public class IyzicoPaymentController : BasePluginController
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IIyzicoPaymentService _iyzicoPaymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderService _orderService;
        private readonly PaymentSettings _paymentSettings;
        #endregion

        #region Ctor
        public IyzicoPaymentController(ILogger logger,
            IIyzicoPaymentService iyzicoPaymentService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            OrderSettings orderSettings,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IOrderService orderService, 
            PaymentSettings paymentSettings)
        {
            _logger = logger;
            _iyzicoPaymentService = iyzicoPaymentService;
            _localizationService = localizationService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _orderSettings = orderSettings;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _orderService = orderService;
            _paymentSettings = paymentSettings;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Enter Payment Info
        /// </summary>
        /// <param name="form">IFormCollection</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [FormValueRequired("nextstep")]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult PaymentInfo(IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentPluginManager
                .LoadPluginBySystemName(paymentMethodSystemName, _workContext.CurrentCustomer,  _storeContext.CurrentStore.Id);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            var paymentInfoModel = _iyzicoPaymentService.ValidatePaymentForm(form);
            foreach (var warning in paymentInfoModel.Warnings)
                ModelState.AddModelError("", warning);

            if (ModelState.IsValid)
            {
                //get payment info
                var processPaymentRequest = _iyzicoPaymentService.GetPaymentInfo(form);
                //set previous order GUID (if exists)
                GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = paymentMethodSystemName;

                try
                {
                    var iyzicoProcessPaymentResult = (IyzicoProcessPaymentResult)_iyzicoPaymentService.ProcessPaymentThreedsInitialize(processPaymentRequest);
                    if (iyzicoProcessPaymentResult.Success)
                    {
                        //session save
                        _iyzicoPaymentService.SetCheckoutCookie("OrderPaymentInfo", processPaymentRequest);
                        return Content(iyzicoProcessPaymentResult.HtmlContent, "text/html; charset=utf-8");
                    }

                    paymentInfoModel.Warnings = iyzicoProcessPaymentResult.Errors;
                }
                catch (Exception ex)
                {
                    paymentInfoModel.Warnings.Add(ex.Message);
                    _logger.Error(ex.Message, ex);
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

            return View("~/Plugins/Payments.Iyzico/Views/Iyzico/CallbackConfirm.cshtml", threedsCallbackResource);
        }

        /// <summary>
        /// Order confirm action
        /// </summary>
        /// <param name="threedsCallbackResource">ThreedsCallbackResource</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [FormValueRequired("nextstep")]
        public virtual IActionResult OrderConfirm(ThreedsCallbackResource threedsCallbackResource)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //model
            var model = new PaymentInfoModel();
            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (! IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = _iyzicoPaymentService.GetCheckoutCookie<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (_orderProcessingService.IsPaymentWorkflowRequired(cart))
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

                GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                
                HttpContext.Session.Set("OrderPaymentInfo", processPaymentRequest);
                HttpContext.Session.Set(nameof(ThreedsCallbackResource), threedsCallbackResource);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    HttpContext.Session.Set<ThreedsCallbackResource>(nameof(ThreedsCallbackResource), null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content(_localizationService.GetResource("Checkout.RedirectMessage"));
                    }

                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            HttpContext.Session.Set(IyzicoDefaults.PAYMENT_INFO_MODEL_SESSION, model);
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        /// <summary>
        /// Get iyzico installment by BIN number
        /// </summary>
        /// <param name="binNumber">BinNumber:Bank Identification Number</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult GetInstallment(string binNumber)
        {
            if (string.IsNullOrEmpty(binNumber))
                return Json(new List<Installment>());

            try
            {
                return Json(_iyzicoPaymentService.GetInstallment(binNumber));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return Json(new List<Installment>());
        }

        #endregion

        #region Utilities

        protected virtual IList<string> ValidationThreedsCallbackResource(ThreedsCallbackResource threedsCallbackResource)
        {
            var warnings = new List<string>();

            var validator = new ThreedsCallbackResourceValidator(_localizationService);
            var validationResult = validator.Validate(threedsCallbackResource);

            if (validationResult.IsValid)
                return warnings;

            warnings.Add(validationResult.ToString(". "));
            return warnings;
        }

        protected virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id, customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        /// <summary>
        /// Generate an order GUID
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        protected virtual void GenerateOrderGuid(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (_paymentSettings.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest != null &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        #endregion

    }
}