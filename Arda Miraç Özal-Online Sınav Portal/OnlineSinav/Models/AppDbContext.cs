using OnlineSinav.Models;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt.Extensions;
using System.Reflection.Emit;

namespace OnlineSinav.Models
{
    public class AppDbContext : IdentityDbContext <AppUser, IdentityRole, string>
    {
        private readonly IConfiguration _config;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }


        public DbSet<Option> Options { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }

        public DbSet<Classroom> Classrooms { get; set; }

        public DbSet<StudentAnswer> StudentAnswers { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>()
            .HasIndex(u => u.TCNo)
            .IsUnique(); // TCNo'nun benzersiz olması sağlanır
            var adminId = Guid.NewGuid().ToString();

            builder.Entity<Quiz>()
            .HasOne(q => q.Classroom)  // A Quiz is related to one Classroom
            .WithMany() // No reverse navigation property in Classroom
            .HasForeignKey(q => q.ClassroomId)  // The foreign key in Quiz
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete


            builder.Entity<StudentQuiz>()
           .HasOne(sq => sq.Quiz)  // A StudentQuiz is related to one Quiz
           .WithMany()  // No reverse navigation property in Quiz
           .HasForeignKey(sq => sq.QuizId)  // The foreign key in StudentQuiz
           .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading delete

            // Optionally, you can also apply it to the Student entity if necessary
            builder.Entity<StudentQuiz>()
                .HasOne(sq => sq.Student)  // A StudentQuiz is related to one Student
                .WithMany()  // No reverse navigation property in AppUser (Student)
                .HasForeignKey(sq => sq.StudentId)  // The foreign key in StudentQuiz
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading delete


            builder.Entity<StudentAnswer>()
          .HasOne(sa => sa.Question)  // A StudentAnswer is related to one Question
          .WithMany()  // No reverse navigation property in Question
          .HasForeignKey(sa => sa.QuestionId)  // The foreign key in StudentAnswer
          .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading delete

            // Optionally, configure cascading delete behavior for other relationships
            builder.Entity<StudentAnswer>()
             .HasOne(sa => sa.StudentQuiz)
             .WithMany(sq => sq.Answers) // StudentQuiz modelinde Answers koleksiyonu olmalı
             .HasForeignKey(sa => sa.StudentQuizId)
             .OnDelete(DeleteBehavior.Cascade); // İlişkiyi tanımlayın


            builder.Entity<AppUser>()
              .HasOne(u => u.Classroom)
              .WithMany(c => c.Users)
              .HasForeignKey(u => u.ClassroomId)
              .OnDelete(DeleteBehavior.Cascade); // ON DELETE SET NULL
            var adminRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                NormalizedName = "ADMİN"
            };
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Name = "Teacher",
                    NormalizedName = "TEACHER"
                }
                );
            builder.Entity<Classroom>().HasData(
                new Classroom
                {
                    Id = 1,
                    Name = "SinifBelirlenmemiş",
                }

                );
            var adminUser = new AppUser
            {
                Id = adminId,
                TCNo = "12345678901",
                UserName = "ADMIN",
                LastName ="ADMIN",
                FirstName = "ADMİN",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow,

            };
            var passwordHasher = new PasswordHasher<AppUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "ADMIN");
            builder.Entity<IdentityRole>().HasData(adminRole);
            builder.Entity<AppUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRole.Id,
                UserId = adminId
            });
            base.OnModelCreating(builder);


        }
    }
}
