namespace Nop.Plugin.Payments.Iyzico.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    public class OpcCallbackConfirmModel
    {
        public OpcCallbackConfirmModel()
        {
            Warnings = new List<string>();
        }

        public IList<string> Warnings { get; set; }
        public string SuccessUrl { get; set; }

        public string GetWarningsAsJson()
        {
            if (Warnings.Any())
            {
                return JsonSerializer.Serialize(Warnings);
            }

            return string.Empty;
        }
    }
}
