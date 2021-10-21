namespace Nop.Plugin.Payments.Iyzico.Validators
{
    using FluentValidation;
    using Iyzipay.Model;
    using Nop.Plugin.Payments.Iyzico.Models;
    using Nop.Services.Localization;
    using Nop.Web.Framework.Validators;

    /// <summary>
    /// Represents ThreedsCallback Resource validator
    /// </summary>
    public class ThreedsCallbackResourceValidator : BaseNopValidator<ThreedsCallbackResource>
    {
        public ThreedsCallbackResourceValidator(ILocalizationService localizationService)
        {
            //set validation rules
            RuleFor(x => x.Status).Must((x, context) => x.Status == $"{Status.SUCCESS}").WithMessage(x => localizationService.GetResource($"Plugins.Payments.Iyzico.ErrorMessage.MdStatus.{x.MdStatus}"));
            RuleFor(x => x.PaymentId).NotEmpty();
            //TODO:.....
            //RuleFor(x => x.ConversationId).NotEmpty();
            //RuleFor(x => x.ConversationData).NotEmpty();
                

        }
    }
}
