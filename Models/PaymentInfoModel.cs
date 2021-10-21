namespace Nop.Plugin.Payments.Iyzico.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Nop.Web.Framework.Models;
    using Nop.Web.Framework.Mvc.ModelBinding;

    public class PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
            Warnings = new List<string>();
        }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.CardHolderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.ExpirationDate")]
        public string ExpireMonth { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Iyzico.ExpirationDate")]
        public string ExpireYear { get; set; }
        public List<SelectListItem> ExpireMonths { get; set; }
        public List<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.CardCode")]
        public string CardCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Iyzico.Installment")]
        public string Installment { get; set; }

        public bool Enable3DSecure { get; set; }
        public IList<string> Warnings { get; set; }

        //TODO:....
        public bool OnePageCheckoutPlugin { get; set; }

    }

    public class Installment
    {
        public string DisplayName { get; set; }

        public int InstallmentNumber { get; set; }

        public string Price { get; set; }

        public string TotalPrice { get; set; }
    }
}
