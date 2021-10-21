namespace Nop.Plugin.Payments.Iyzico.Data
{
    using FluentMigrator;
    using Nop.Data.Migrations;
    using Nop.Plugin.Payments.Iyzico.Domain;

    [SkipMigrationOnUpdate]
    [NopMigration("2021/10/10 10:02:04:1687541", "Nop.Plugin.Payments.Iyzico schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            //$"IF (NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{IyzicoDefaults.DB_PAYMENT_SCHEMA}')) BEGIN EXEC ('CREATE SCHEMA [{IyzicoDefaults.DB_PAYMENT_SCHEMA}] AUTHORIZATION [dbo]') END"
            if (!Schema.Schema(IyzicoDefaults.DB_PAYMENT_SCHEMA).Exists())
                Create.Schema(IyzicoDefaults.DB_PAYMENT_SCHEMA);

            _migrationManager.BuildTable<IyzicoPayment>(Create);
            _migrationManager.BuildTable<IyzicoPaymentItem>(Create);
            _migrationManager.BuildTable<IyzicoPaymentRefund>(Create);
        }
    }
}
