namespace Nop.Plugin.Payments.Iyzico.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Nop.Data.Mapping;
    using Nop.Plugin.Payments.Iyzico.Domain;

    /// <summary>
    /// Represents the IyzicoPayment mapping class
    /// </summary>
    public class IyzicoPaymentMap : NopEntityTypeConfiguration<IyzicoPayment>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<IyzicoPayment> builder)
        {
            builder.ToTable(nameof(IyzicoPayment), IyzicoDefaults.DB_PAYMENT_SCHEMA);
            builder.HasKey(ip => ip.Id);
            builder.Property(ip => ip.CustomerId).IsRequired();

            //TODO:...
            /**
            builder.HasOne(ip => ip.Customer)
                .WithMany()
                .HasForeignKey(ip => ip.CustomerId);
            */

            builder.Property(ip => ip.BasketId).HasMaxLength(50).IsRequired();
            builder.Property(ip => ip.PaymentId).HasMaxLength(50).IsRequired();
            builder.Property(ip => ip.Amount).HasColumnType("decimal(18, 4)").IsRequired();
            builder.Property(ip => ip.Installment);
            builder.Property(ip => ip.CreatedOnUtc).HasDefaultValueSql("GETUTCDATE()").IsRequired();

            base.Configure(builder);
        }

        #endregion
    }

}
