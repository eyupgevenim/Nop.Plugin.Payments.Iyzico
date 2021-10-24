namespace Nop.Plugin.Payments.Iyzico.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Nop.Data.Mapping;
    using Nop.Plugin.Payments.Iyzico.Domain;

    /// <summary>
    /// Represents the IyzicoPaymentItem mapping class
    /// </summary>
    public class IyzicoPaymentItemMap : NopEntityTypeConfiguration<IyzicoPaymentItem>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<IyzicoPaymentItem> builder)
        {
            builder.ToTable(nameof(IyzicoPaymentItem), IyzicoDefaults.DB_PAYMENT_SCHEMA);
            builder.HasKey(ipi => ipi.Id);
            builder.Property(ipi => ipi.IyzicoPaymentId).IsRequired();

            //TODO:...
            /**
            builder
                .HasOne(ipi => ipi.IyzicoPayment)
                .WithMany()
                .HasForeignKey(ipi => ipi.IyzicoPaymentId);
            */

            builder.Property(ipi => ipi.PaymentTransactionId).HasMaxLength(50).IsRequired();
            builder.Property(ipi => ipi.ProductId).HasMaxLength(50).IsRequired();
            builder.Property(ipi => ipi.Amount).HasColumnType("decimal(18, 4)").IsRequired();
            builder.Property(ipi => ipi.Type).HasMaxLength(50).IsRequired();
            builder.Property(ipi => ipi.CreatedOnUtc).HasDefaultValueSql("GETUTCDATE()").IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}
