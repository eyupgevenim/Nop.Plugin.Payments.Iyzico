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
