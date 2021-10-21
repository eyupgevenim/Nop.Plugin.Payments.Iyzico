namespace Nop.Plugin.Payments.Iyzico.Helpers
{
    using System.Collections.Generic;
    using Iyzipay;
    using Iyzipay.Model;

    public static class IyzicoHelper
    {
        public static Options GetIyzicoOptions(IyzicoSettings iyzicoPaymentSettings)
        {
            return new Options
            {
                ApiKey = iyzicoPaymentSettings.ApiKey,
                SecretKey = iyzicoPaymentSettings.SecretKey,
                BaseUrl = iyzicoPaymentSettings.BaseUrl
            };
        }

        public static string GetIyzicoCurrency(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                return string.Empty;

            var currencyCodes = new List<string>
            {
                $"{Currency.TRY}",
                $"{Currency.EUR}",
                $"{Currency.USD}",
                $"{Currency.GBP}",
                $"{Currency.IRR}",
                $"{Currency.NOK}",
                $"{Currency.RUB}",
                $"{Currency.CHF}"
            };

            var currencyCodeToUpper = currencyCode.ToUpper();
            if (currencyCodes.Contains(currencyCodeToUpper))
                return currencyCodeToUpper;

            return $"{Currency.TRY}";
        }


        public static string GetLocale(string uniqueSeoCode)
        {
            if ((uniqueSeoCode ?? string.Empty).Equals($"{Locale.EN}", System.StringComparison.OrdinalIgnoreCase))
            {
                return $"{Locale.EN}";
            }

            return $"{Locale.TR}";
        }
    }
}
