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

            //setting a relation one to many between Subscriptions and Payments
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Payment)
                .WithMany(p => p.PaymentDetails)
                .HasForeignKey(pd => pd.PaymentId);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Plan)
                .WithMany(p => p.PaymentDetails)
                .HasForeignKey(pd => pd.PlanId);


            base.OnModelCreating(modelBuilder);
        }
    }
}
