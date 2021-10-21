namespace Nop.Plugin.Payments.Iyzico.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Plugin.Payments.Iyzico.Validators;
    using Nop.Services.Configuration;
    using Nop.Services.Localization;
    using Nop.Services.Messages;
    using Nop.Services.Security;
    using Nop.Web.Framework;
    using Nop.Web.Framework.Controllers;
    using Nop.Web.Framework.Mvc.Filters;
    using System.Threading.Tasks;

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin(false)]
    public class IyzicoSettingsController : BasePluginController
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IyzicoSettings _iyzicoSettings;
        #endregion

        #region Ctor
        public IyzicoSettingsController(ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INotificationService notificationService,
            IyzicoSettings iyzicoSettings)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _iyzicoSettings = iyzicoSettings;
        }
        #endregion

        #region Methods

        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ConfigurationModel()
            {
                ApiKey = _iyzicoSettings.ApiKey,
                SecretKey = _iyzicoSettings.SecretKey,
                BaseUrl = _iyzicoSettings.BaseUrl,
                PaymentMethodDescription = _iyzicoSettings.PaymentMethodDescription,
                CheckoutCookieExpires = _iyzicoSettings.CheckoutCookieExpires,
                Enable3DSecure = _iyzicoSettings.Enable3DSecure
            };

            return View("~/Plugins/Payments.Iyzico/Views/Configure/Configure.cshtml", model);
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //validate configuration (custom validation)
            var validationResult = new ConfigurationValidator(_localizationService).Validate(model);
            if (!validationResult.IsValid)
                return await Configure();

            _iyzicoSettings.ApiKey = model.ApiKey;
            _iyzicoSettings.SecretKey = model.SecretKey;
            _iyzicoSettings.BaseUrl = model.BaseUrl;
            _iyzicoSettings.PaymentMethodDescription = model.PaymentMethodDescription;
            _iyzicoSettings.CheckoutCookieExpires = model.CheckoutCookieExpires;
            _iyzicoSettings.Enable3DSecure = model.Enable3DSecure;

            _settingService.SaveSetting(_iyzicoSettings);
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return View("~/Plugins/Payments.Iyzico/Views/Configure/Configure.cshtml", model);
        }
        
        #endregion
    }
}
