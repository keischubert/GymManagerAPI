using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DeletedSubscription> DeletedSubscriptions { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; } 
        public DbSet<PaymentDetail> PaymentDetails { get; set; }


        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //setting a relation one to many between Genders and Members
            modelBuilder.Entity<Gender>()
                .HasMany(g => g.Members)
                .WithOne(m => m.Gender)
                .HasForeignKey(m => m.GenderId);

            //setting a relation one to many between Members and Suscriptions
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Subscriptions)
                .WithOne(s => s.Member)
                .HasForeignKey(s => s.MemberId);

            //setting a relation one to one between Subscriptions and Payments
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Payment)
                .WithOne(p => p.Subscription)
                .HasForeignKey<Payment>(p => p.SubscriptionId);

            //setting a relation one to many between Plans and Payments
            modelBuilder.Entity<Plan>()
                .HasMany(p => p.Payments)
                .WithOne(p => p.Plan)
                .HasForeignKey(p => p.PlanId);

            //setting a relation one to one between Subscriptions and DeletedSubscriptions
            modelBuilder.Entity<DeletedSubscription>()
                .HasOne(ds => ds.Subscription)
                .WithOne(s => s.DeletedSubscription)
                .HasForeignKey<DeletedSubscription>(ds => ds.SubscriptionId);

            //Setting a relation one to many between PaymentDetails and Payments
            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Payment)
                .WithMany(p => p.PaymentDetails)
                .HasForeignKey(pd => pd.PaymentId);

            //Setting a relation one to many between PaymentDetails and PaymentMethods
            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.PaymentMethod)
                .WithMany(pm => pm.PaymentDetails)
                .HasForeignKey(pd => pd.PaymentMethodId);

            //setting a global filter
            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(s => !s.IsDeleted);

            modelBuilder.Entity<Payment>()
                .HasQueryFilter(p => !p.Subscription.IsDeleted);

            modelBuilder.Entity<DeletedSubscription>()
                .HasQueryFilter(ds => ds.Subscription.IsDeleted);

            base.OnModelCreating(modelBuilder);
        }
    }
}
