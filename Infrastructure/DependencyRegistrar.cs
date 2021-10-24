namespace Nop.Plugin.Payments.Iyzico.Infrastructure
{
    using Autofac;
    using Autofac.Core;
    using Nop.Core.Configuration;
    using Nop.Core.Data;
    using Nop.Core.Infrastructure;
    using Nop.Core.Infrastructure.DependencyManagement;
    using Nop.Data;
    using Nop.Plugin.Payments.Iyzico.Data;
    using Nop.Plugin.Payments.Iyzico.Domain;
    using Nop.Plugin.Payments.Iyzico.Services;
    using Nop.Web.Framework.Infrastructure.Extensions;

    /// <summary>
    /// Represents a plugin dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<IyzicoPaymentService>().As<IIyzicoPaymentService>().InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<IyzicoPaymentObjectContext>("nop_object_context_iyzico_payment-payment");

            //override required repository with our custom context
            builder
                .RegisterType<EfRepository<IyzicoPayment>>().As<IRepository<IyzicoPayment>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_iyzico_payment-payment"))
                .InstancePerLifetimeScope();

            builder
                .RegisterType<EfRepository<IyzicoPaymentItem>>().As<IRepository<IyzicoPaymentItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_iyzico_payment-payment"))
                .InstancePerLifetimeScope();

            builder
                .RegisterType<EfRepository<IyzicoPaymentRefund>>().As<IRepository<IyzicoPaymentRefund>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_iyzico_payment-payment"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}