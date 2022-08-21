namespace Nop.Plugin.Payments.Iyzico.Data
{
    using FluentMigrator;
    using Nop.Data.Extensions;
    using Nop.Data.Migrations;
    using Nop.Plugin.Payments.Iyzico.Domain;

    [NopMigration("2021/10/10 10:02:04:1687541", "Nop.Plugin.Payments.Iyzico schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            //$"IF (NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{IyzicoDefaults.DB_PAYMENT_SCHEMA}')) BEGIN EXEC ('CREATE SCHEMA [{IyzicoDefaults.DB_PAYMENT_SCHEMA}] AUTHORIZATION [dbo]') END"
            if (!Schema.Schema(IyzicoDefaults.DB_PAYMENT_SCHEMA).Exists())
                Create.Schema(IyzicoDefaults.DB_PAYMENT_SCHEMA);

            Create.TableFor<IyzicoPayment>();
            Create.TableFor<IyzicoPaymentItem>();
            Create.TableFor<IyzicoPaymentRefund>();
        }
    }
}
