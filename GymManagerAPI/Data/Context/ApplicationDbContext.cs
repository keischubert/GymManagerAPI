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

            base.OnModelCreating(modelBuilder);
        }
    }
}
