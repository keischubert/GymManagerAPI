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
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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

            modelBuilder.Entity<PaymentDetail>()
                .HasQueryFilter(pd => !pd.Payment.Subscription.IsDeleted);

            modelBuilder.Entity<DeletedSubscription>()
                .HasQueryFilter(ds => ds.Subscription.IsDeleted);

            // Definiendo una clave primaria compuesta en UserRoles.
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            //many-to-many relationship between User and Role.
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            //setting a relation one to many between Users and Subscriptions
            //setting a relation one to many between Users and DeletedSubscriptions
            //create an index
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Subscriptions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId);

                entity.HasMany(u => u.DeletedSubscriptions)
                      .WithOne(ds => ds.User)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasForeignKey(ds => ds.UserId);

                entity.HasIndex(x => x.UserName).IsUnique();
            });

            //setting a relation one to many between Users and RefreshTokens
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId);

                entity.Property(rt => rt.Token).IsRequired();
            });
                

            base.OnModelCreating(modelBuilder);
        }
    }
}