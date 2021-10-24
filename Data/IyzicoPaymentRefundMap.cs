namespace Nop.Plugin.Payments.Iyzico.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Nop.Data.Mapping;
    using Nop.Plugin.Payments.Iyzico.Domain;

    /// <summary>
    /// Represents the IyzicoPaymentRefund mapping class
    /// </summary>
    public class IyzicoPaymentRefundMap : NopEntityTypeConfiguration<IyzicoPaymentRefund>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<IyzicoPaymentRefund> builder)
        {
            builder.ToTable(nameof(IyzicoPaymentRefund), IyzicoDefaults.DB_PAYMENT_SCHEMA);
            builder.HasKey(ipi => ipi.Id);
            builder.Property(ipi => ipi.CustomerId).IsRequired();
            builder.Property(ipi => ipi.OrderId).IsRequired();

            //TODO:...
            /**
            builder.HasOne(ipi => ipi.Customer)
                .WithMany()
                .HasForeignKey(ipi => ipi.CustomerId);
            builder.HasOne(ipi => ipi.Order)
                .WithMany()
                .HasForeignKey(ipi => ipi.OrderId);
            */

            builder.Property(ipi => ipi.PaymentTransactionId).HasMaxLength(50).IsRequired();
            builder.Property(ipi => ipi.PaymentId).HasMaxLength(50).IsRequired();
            builder.Property(ipi => ipi.Amount).HasColumnType("decimal(18, 4)").IsRequired();
            builder.Property(ipi => ipi.CreatedOnUtc).HasDefaultValueSql("GETUTCDATE()").IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}
