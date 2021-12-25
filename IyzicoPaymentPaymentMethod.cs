namespace Nop.Plugin.Payments.Iyzico
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Nop.Core.Caching;
    using Nop.Core.Data;
    using Nop.Core.Domain.Cms;
    using Nop.Core.Domain.Localization;
    using Nop.Core.Domain.Orders;
    using Nop.Core.Domain.Payments;
    using Nop.Plugin.Payments.Iyzico.Services;
    using Nop.Services.Configuration;
    using Nop.Services.Localization;
    using Nop.Services.Payments;
    using Nop.Services.Plugins;
    using System.Collections.Generic;
    using System.Linq;
    using Nop.Data;
    using System;
    using Nop.Plugin.Payments.Iyzico.Data;

    public class IyzicoPaymentPaymentMethod : BasePlugin, IPaymentMethod
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly IIyzicoPaymentService _iyzicoPaymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly PaymentSettings _paymentSettings;
        private readonly WidgetSettings _widgetSettings;
        private readonly ILanguageService _languageService;
        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly OrderSettings _orderSettings;
        private readonly IyzicoPaymentObjectContext _objectContext;

        #endregion

        #region Ctor
        public IyzicoPaymentPaymentMethod(ISettingService settingService,
            IIyzicoPaymentService iyzicoPaymentService,
            ILocalizationService localizationService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            PaymentSettings paymentSettings,
            WidgetSettings widgetSettings,
            ILanguageService languageService,
            IRepository<LocaleStringResource> lsrRepository,
            IStaticCacheManager staticCacheManager,
            OrderSettings orderSettings,
            IyzicoPaymentObjectContext objectContext)
        {
            _settingService = settingService;
            _iyzicoPaymentService = iyzicoPaymentService;
            _localizationService = localizationService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _paymentSettings = paymentSettings;
            _widgetSettings = widgetSettings;
            _languageService = languageService;
            _lsrRepository = lsrRepository;
            _staticCacheManager = staticCacheManager;
            _orderSettings = orderSettings;
            _objectContext = objectContext;
        }
        #endregion

        #region Properies
        /// <summary>
        /// Gets a value indicating whether capture is not supported
        /// </summary>
        public bool SupportCapture => false;
        /// <summary>
        /// Gets a value indicating whether capture is not supported
        /// </summary>
        public bool SupportPartiallyRefund => false;
        /// <summary>
        /// Gets a value indicating whether capture is not supported
        /// </summary>
        public bool SupportRefund => true;
        /// <summary>
        /// Gets a value indicating whether capture is not supported
        /// </summary>
        public bool SupportVoid => false;
        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;
        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.Iyzico.Admin.Fields.PaymentMethodDescription");

        #endregion

        #region Methods
        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// CancelRecurringPaymentResult
        /// </returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// bool
        /// </returns>
        public bool CanRePostProcessPayment(Order order)
        {
            return false;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// CapturePaymentResult
        /// </returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public  decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _iyzicoPaymentService.GetAdditionalHandlingFee(cart);
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public  ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return _iyzicoPaymentService.GetPaymentInfo(form);
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        public string GetPublicViewComponentName()
        {
            if (_orderSettings.OnePageCheckoutEnabled)
                return IyzicoDefaults.OPC_PAYMENT_INFO_VIEW_COMPONENT_NAME;

            return IyzicoDefaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>
        /// bool
        /// </returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            var notConfigured = _iyzicoPaymentService.IsConfigured();
            return !notConfigured;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)  { }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// ProcessPaymentResult
        /// </returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return _iyzicoPaymentService.ProcessPayment(processPaymentRequest);
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// ProcessPaymentResult
        /// </returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// RefundPaymentResult
        /// </returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return _iyzicoPaymentService.Refund(refundPaymentRequest);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public  IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var paymentInfoModel = _iyzicoPaymentService.ValidatePaymentForm(form);
            return paymentInfoModel.Warnings;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// VoidPaymentResult
        /// </returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(IyzicoDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override  void Install()
        {
            //settings
            _settingService.SaveSetting(new IyzicoSettings
            {
                ApiKey = "",
                SecretKey = "",
                BaseUrl = "",
                PaymentMethodDescription = "",
                CheckoutCookieExpires = 0,
                Enable3DSecure = false
            });

            if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains(IyzicoDefaults.SystemName))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Add(IyzicoDefaults.SystemName);
                _settingService.SaveSetting(_paymentSettings);
            }

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(IyzicoDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(IyzicoDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            //database objects
            _objectContext.Install();

            #region locales

            var defaultLocaleResources = new Dictionary<string, string>
            {
                ["Plugins.Payments.Iyzico.Admin.Fields.ApiKey"] = "Iyzico API Key",
                ["Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Hint"] = "Enter Iyzico API Key.",
                ["Plugins.Payments.Iyzico.Admin.Fields.ApiKey.Required"] = "API Key Is Required.",
                ["Plugins.Payments.Iyzico.Admin.Fields.SecretKey"] = "Iyzico Secret Key",
                ["Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Hint"] = "Enter Iyzico Secret Key.",
                ["Plugins.Payments.Iyzico.Admin.Fields.SecretKey.Required"] = "Secret Key Is Required.",
                ["Plugins.Payments.Iyzico.Admin.Fields.BaseUrl"] = "Iyzico Base URL",
                ["Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Hint"] = "Enter Iyzico Base URL.",
                ["Plugins.Payments.Iyzico.Admin.Fields.BaseUrl.Required"] = "Base URL Is Required.",
                ["Plugins.Payments.Iyzico.Admin.Fields.PaymentMethodDescription"] = "Pay by credit / debit card using Iyzico payment service",
                ["Plugins.Payments.Iyzico.Admin.Fields.CheckoutCookieExpires"] = "Checkout Cookie Expires (seconds)",
                ["Plugins.Payments.Iyzico.Admin.Fields.Enable3DSecure"] = "Enable 3D Secure",

                ["Plugins.Payments.Iyzico.Admin.IyzicoMessage.MessageCode"] = "Message Code",
                ["Plugins.Payments.Iyzico.Admin.IyzicoMessage.Message"] = "Message",
                ["Plugins.Payments.Iyzico.Admin.IyzicoMessage.Deleted"] = "Deleted",

                ["Plugins.Payments.Iyzico.CardHolderName"] = "Card Holder Name",
                ["Plugins.Payments.Iyzico.CardNumber"] = "Card Number",
                ["Plugins.Payments.Iyzico.ExpirationDate"] = "Expiration Date",
                ["Plugins.Payments.Iyzico.CardCode"] = "Card Code",
                ["Plugins.Payments.Iyzico.Installment"] = "Installment",
                ["Plugins.Payments.Iyzico.EmptyInstalment"] = "Empty Instalment"
            };

            foreach (var defaultLocaleResource in defaultLocaleResources)
                _localizationService.AddOrUpdatePluginLocaleResource(defaultLocaleResource.Key, defaultLocaleResource.Value);

            var allLanguages = _languageService.GetAllLanguages();

            #region locales-on-engilish
            if (allLanguages.Any(x => x.LanguageCulture == "en-US"))
            {
                var enLocaleResources = new Dictionary<string, string>
                {
                    ["Plugins.Payments.Iyzico.Installment"] = "Installment",
                    ["Plugins.Payments.Iyzico.Installments"] = "Installments",
                    ["Plugins.Payments.Iyzico.InstallmentRequired"] = "Installment Required",
                    ["Plugins.Payments.Iyzico.InstallmentCount"] = "Installment Count",
                    ["Plugins.Payments.Iyzico.MonthlyPayment"] = "Monthly Payment",
                    ["Plugins.Payments.Iyzico.Total"] = "Total",
                    ["Plugins.Payments.Iyzico.Installment1"] = "Single Payment",
                    ["Plugins.Payments.Iyzico.Installment2"] = "2 Installments",
                    ["Plugins.Payments.Iyzico.Installment3"] = "3 Installments",
                    ["Plugins.Payments.Iyzico.Installment6"] = "6 Installments",
                    ["Plugins.Payments.Iyzico.Installment9"] = "9 Installments",
                    ["Plugins.Payments.Iyzico.Installment12"] = "12 Installments",

                    //locales-error-messages
                    ["Plugins.Payments.Iyzico.ErrorMessage.8"] = "IdentityNumber must be sent!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.12"] = "Invalid card number!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.15"] = "Cvc is invalid!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.17"] = "ExpireYear and expireMonth are invalid!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.5152"] = "Cannot pay with test credit cards!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10005"] = "Transaction not confirmed!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10012"] = "Invalid transaction!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10034"] = "Scam Suspect",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10041"] = "Lost card, confiscate card",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10043"] = "Stolen card, confiscate card",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10051"] = "Insufficient card limit, insufficient balance!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10054"] = "Incorrect expiration date!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10057"] = "Card owner cannot do this!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10058"] = "Terminal is not authorized to perform this operation",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10084"] = "CVC2 information is incorrect",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10093"] = "Your card is not available for online shopping. To open it, you can write ONAY and send the last 6 digits of the card to 3340.",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10201"] = "Card did not allow transaction",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10204"] = "A general error occurred during the payment process",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10206"] = "CVC length is invalid",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10207"] = "Get approval from your bank",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10208"] = "Merchant category code is incorrect",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10209"] = "Card with blocked status",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10210"] = "Incorrect CAVV information",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10211"] = "Incorrect ECI information",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10213"] = "BIN not found",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10214"] = "Communication or system error",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10215"] = "Invalid card number!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10216"] = "Bank not found",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10217"] = "Bank cards can only be used for 3D Secure transaction",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10219"] = "Request to bank timed out",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10222"] = "Terminal installment processing disabled",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10223"] = "End of day must be done",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10225"] = "Restricted card",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10226"] = "Exceeded allowed number of PIN entries!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10227"] = "Invalid PIN",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10228"] = "Bank or terminal cannot process",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10229"] = "Invalid expiration date",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10232"] = "Invalid amount",
                    ["Plugins.Payments.Iyzico.Installment.Wrong"] = "Invalid installment!",

                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.0"] = "Invalid 3D Secure signature or verification",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.2"] = "Card holder or Issuer not registered to 3D Secure network",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.3"] = "Issuer is not registered to 3D secure network",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.4"] = "Verification is not possible, card holder chosen to register later on system",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.5"] = "Verification is not possbile",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.6"] = "Verification is not possbile",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.7"] = "System error",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.8"] = "Unknown card",

                    ["Plugins.Payments.Iyzico.ErrorMessage.UnauthenticatedPayment"] = "Unauthenticated Payment",
                    ["Plugins.Payments.Iyzico.ErrorMessage.Not3DCallbackResource"] = "Unknown resource",
                };

                foreach (var enLocaleResource in enLocaleResources)
                    _localizationService.AddOrUpdatePluginLocaleResource(enLocaleResource.Key, enLocaleResource.Value, "en-US");
            }

            #endregion

            #region locales-on-turkish
            if (allLanguages.Any(x => x.LanguageCulture == "tr-TR"))
            {
                var trLocaleResources = new Dictionary<string, string>
                {
                    ["Plugins.Payments.Iyzico.CardHolderName"] = "Kart Sahibi",
                    ["Plugins.Payments.Iyzico.CardNumber"] = "Kart Numarası",
                    ["Plugins.Payments.Iyzico.ExpirationDate"] = "Geçerlilik Tarihi",
                    ["Plugins.Payments.Iyzico.CardCode"] = "Güvenlik Kodu",
                    ["Plugins.Payments.Iyzico.Installment"] = "Taksit Sayısı",
                    ["Plugins.Payments.Iyzico.Installments"] = "Taksit Seçenekleri",
                    ["Plugins.Payments.Iyzicon.EmptyInstalment"] = "Taksit seçeneklerini görüntülemek için kart bilgilerinizi giriniz.",
                    ["Plugins.Payments.Iyzico.InstallmentRequired"] = "Taksit Gerekli",
                    ["Plugins.Payments.Iyzico.InstallmentNumber"] = "Taksit",
                    ["Plugins.Payments.Iyzico.Price"] = "Taksit Tutarı",
                    ["Plugins.Payments.Iyzico.TotalPrice"] = "Toplam Tutar",
                    ["Plugins.Payments.Iyzico.Installment1"] = "Tek Çekim",
                    ["Plugins.Payments.Iyzico.Installment2"] = "2 Taksit",
                    ["Plugins.Payments.Iyzico.Installment3"] = "3 Taksit",
                    ["Plugins.Payments.Iyzico.Installment6"] = "6 Taksit",
                    ["Plugins.Payments.Iyzico.Installment9"] = "9 Taksit",
                    ["Plugins.Payments.Iyzico.Installment12"] = "12 Taksit",

                    //locales-error-messages
                    ["Plugins.Payments.Iyzico.ErrorMessage.8"] = "IdentityNumber gönderilmesi zorunludur!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.12"] = "Kart numarası geçersizdir!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.15"] = "Cvc geçersizdir!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.17"] = "ExpireYear ve expireMonth geçersizdir!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.5152"] = "Test kredi kartları ile ödeme yapılamaz!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10005"] = "İşlem onaylanmadı!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10012"] = "Geçersiz işlem!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10034"] = "Dolandırıcılık şüphesi",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10041"] = "Kayıp kart, karta el koyunuz",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10043"] = "Çalıntı kart, karta el koyunuz",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10051"] = "Kart limiti yetersiz, yetersiz bakiye!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10054"] = "Son kullanma tarihi hatalı!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10057"] = "Kart sahibi bu işlemi yapamaz!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10058"] = "Terminalin bu işlemi yapmaya yetkisi yok",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10084"] = "CVC2 bilgisi hatalı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10093"] = "Kartınız internetten alışverişe kapalıdır. Açtırmak için ONAY yazıp kart son 6 haneyi 3340’a gönderebilirsiniz.",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10201"] = "Kart, işleme izin vermedi",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10204"] = "Ödeme işlemi esnasında genel bir hata oluştu",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10206"] = "CVC uzunluğu geçersiz",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10207"] = "Bankanızdan onay alınız",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10208"] = "Üye işyeri kategori kodu hatalı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10209"] = "Bloke statülü kart",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10210"] = "Hatalı CAVV bilgisi",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10211"] = "Hatalı ECI bilgisi",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10213"] = "BIN bulunamadı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10214"] = "İletişim veya sistem hatası",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10215"] = "Geçersiz kart numarası!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10216"] = "Bankası bulunamadı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10217"] = "Banka kartları sadece 3D Secure işleminde kullanılabilir",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10219"] = "Bankaya gönderilen istek zaman aşımına uğradı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10222"] = "Terminal taksitli işleme kapalı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10223"] = "Gün sonu yapılmalı",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10225"] = "Kısıtlı kart",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10226"] = "İzin verilen PIN giriş sayısı aşılmış!",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10227"] = "Geçersiz PIN",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10228"] = "Banka veya terminal işlem yapamıyor",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10229"] = "Son kullanma tarihi geçersiz",
                    ["Plugins.Payments.Iyzico.ErrorMessage.10232"] = "Geçersiz tutar",
                    ["Plugins.Payments.Iyzico.Installment.Wrong"] = "Geçersiz taksit!",

                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.0"] = "3-D Secure imzası geçersiz veya doğrulama",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.2"] = "Kart sahibi veya bankası sisteme kayıtlı değil",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.3"] = "Kartın bankası sisteme kayıtlı değil",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.4"] = "Doğrulama denemesi, kart sahibi sisteme daha sonra kayıt olmayı seçmiş",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.5"] = "Doğrulama yapılamıyor",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.6"] = "3-D Secure hatası",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.7"] = "Sistem hatası",
                    ["Plugins.Payments.Iyzico.ErrorMessage.MdStatus.8"] = "Bilinmeyen kart no",

                    ["Plugins.Payments.Iyzico.ErrorMessage.UnauthenticatedPayment"] = "Doğrulanamayan ödeme",
                    ["Plugins.Payments.Iyzico.ErrorMessage.Not3DCallbackResource"] = "Bilinmeyen kaynak",

                };

                foreach (var trLocaleResource in trLocaleResources)
                    _localizationService.AddOrUpdatePluginLocaleResource(trLocaleResource.Key, trLocaleResource.Value, "tr-TR");
            }
            #endregion 

            #endregion

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(IyzicoDefaults.SystemName))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Remove(IyzicoDefaults.SystemName);
                _settingService.SaveSetting(_paymentSettings);
            }

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(IyzicoDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(IyzicoDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _settingService.DeleteSetting<IyzicoSettings>();

            //locales
            DeletePluginLocaleResources("Plugins.Payments.Iyzico");

            //database objects
            _objectContext.Uninstall();

            base.Uninstall();
        }

        /// <summary>
        /// Delete locale resources by the passed name prefix
        /// </summary>
        /// <param name="resourceNamePrefix">Resource name prefix</param>
        protected virtual void DeletePluginLocaleResources(string resourceNamePrefix)
        {
            var locales = _lsrRepository.Table.Where(locale => !string.IsNullOrEmpty(locale.ResourceName) && locale.ResourceName.StartsWith(resourceNamePrefix, StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var locale in locales)
                _lsrRepository.Delete(locale);

            //clear cache
            _staticCacheManager.RemoveByPrefix(NopLocalizationDefaults.LocaleStringResourcesPrefixCacheKey);
        }


        #endregion

    }
}