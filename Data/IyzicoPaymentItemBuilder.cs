namespace Nop.Plugin.Payments.Iyzico.Data
{
    using System.Data;
    using FluentMigrator;
    using FluentMigrator.Builders.Create.Table;
    using Nop.Data.Mapping.Builders;
    using Nop.Plugin.Payments.Iyzico.Domain;

    public class IyzicoPaymentItemBuilder : NopEntityBuilder<IyzicoPaymentItem>
    {
        /// <summary>
        /// Apply entity IyzicoPaymentItem
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .InSchema(IyzicoDefaults.DB_PAYMENT_SCHEMA)
                //map the primary key (not necessary if it is Id field)
                .WithColumn(nameof(IyzicoPaymentItem.Id)).AsInt32().Identity().PrimaryKey()
                //https://github.com/fluentmigrator/fluentmigrator/issues/748
                //map the additional properties as foreign keys
                .WithColumn(nameof(IyzicoPaymentItem.IyzicoPaymentId)).AsInt32()
                    .ForeignKey(
                        foreignKeyName: $"FK_{nameof(IyzicoPayment)}_{nameof(IyzicoPaymentItem)}_{nameof(IyzicoPaymentItem.IyzicoPaymentId)}",
                        primaryTableSchema: IyzicoDefaults.DB_PAYMENT_SCHEMA,
                        primaryTableName: nameof(IyzicoPayment),
                        primaryColumnName: nameof(IyzicoPayment.Id)
                    ).OnDelete(Rule.Cascade)
                .WithColumn(nameof(IyzicoPaymentItem.PaymentTransactionId)).AsString(50).NotNullable()
                .WithColumn(nameof(IyzicoPaymentItem.ProductId)).AsString(50).NotNullable()
                .WithColumn(nameof(IyzicoPaymentItem.Amount)).AsDecimal().NotNullable()
                .WithColumn(nameof(IyzicoPaymentItem.Type)).AsString(50).NotNullable()
                .WithColumn(nameof(IyzicoPaymentItem.CreatedOnUtc)).AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            ;
        }
    }
}
