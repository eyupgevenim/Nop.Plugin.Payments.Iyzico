namespace Nop.Plugin.Payments.Iyzico.Data
{
    using System.Data;
    using FluentMigrator;
    using FluentMigrator.Builders.Create.Table;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Orders;
    using Nop.Data.Extensions;
    using Nop.Data.Mapping.Builders;
    using Nop.Plugin.Payments.Iyzico.Domain;

    public class IyzicoPaymentRefundBuilder : NopEntityBuilder<IyzicoPaymentRefund>
    {
        /// <summary>
        /// Apply entity IyzicoPaymentRefund
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table

                ///nopCommerce - release --> 4.50.3, 4.50.2, 4.50.1, 4.50.0 için BUG'lı ilgli issue'daki gibi düzeltebilirsiniz....
                ///https://github.com/nopSolutions/nopCommerce/issues/6139
                ///https://github.com/nopSolutions/nopCommerce/pull/6140/files
                .InSchema(IyzicoDefaults.DB_PAYMENT_SCHEMA)

                //map the primary key (not necessary if it is Id field)
                .WithColumn(nameof(IyzicoPaymentRefund.Id)).AsInt32().Identity().PrimaryKey()
                //map the additional properties as foreign keys
                .WithColumn(nameof(IyzicoPaymentRefund.CustomerId)).AsInt32().ForeignKey<Customer>(onDelete: Rule.Cascade)
                .WithColumn(nameof(IyzicoPaymentRefund.PaymentTransactionId)).AsString(50).NotNullable()
                .WithColumn(nameof(IyzicoPaymentRefund.OrderId)).AsInt32().ForeignKey<Order>(onDelete: Rule.Cascade)
                .WithColumn(nameof(IyzicoPaymentRefund.PaymentId)).AsString(50).NotNullable()
                .WithColumn(nameof(IyzicoPaymentRefund.Amount)).AsDecimal().NotNullable()
                .WithColumn(nameof(IyzicoPaymentRefund.CreatedOnUtc)).AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            ;
        }
    }
}
