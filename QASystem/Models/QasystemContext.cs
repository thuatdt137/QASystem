using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QASystem.Models
{
    // Kế thừa từ IdentityDbContext để tích hợp ASP.NET Identity
    public partial class QasystemContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public QasystemContext()
        {
        }

        public QasystemContext(DbContextOptions<QasystemContext> options)
            : base(options)
        {
        }

        // Các DbSet cho các bảng trong cơ sở dữ liệu
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }

        // Cấu hình chuỗi kết nối cơ sở dữ liệu
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost;Database=QASystem;Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=true");
            }
        }

        // Cấu hình mô hình cơ sở dữ liệu
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Đảm bảo Identity hoạt động

            // Cấu hình bảng Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id).HasName("PK__Users__1788CCAC42C21221");

                entity.HasIndex(e => e.UserName, "UQ__Users__536C85E433516CC1").IsUnique();
                entity.HasIndex(e => e.Email, "UQ__Users__A9D1053408B07B86").IsUnique();

                entity.Property(e => e.Id).HasColumnName("UserID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Reputation).HasDefaultValue(0);
                entity.Property(e => e.UserName).HasMaxLength(50);

                // Ánh xạ các trường Identity
                entity.Property(e => e.Id).HasColumnName("UserID");
                entity.Property(e => e.UserName).HasColumnName("Username");
                entity.Property(e => e.Email).HasColumnName("Email");
                entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            });

            // Cấu hình bảng Answers
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.AnswerId).HasName("PK__Answers__D482502427F3B7B5");
                entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Answers__Questio__2F10007B");

                entity.HasOne(d => d.User).WithMany(p => p.Answers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Answers__UserID__300424B4");
            });

            // Cấu hình bảng Questions
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8C96711804");
                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User).WithMany(p => p.Questions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Questions__UserI__2B3F6F97");

                entity.HasMany(d => d.Tags).WithMany(p => p.Questions)
                    .UsingEntity<Dictionary<string, object>>(
                        "QuestionTag",
                        r => r.HasOne<Tag>().WithMany()
                            .HasForeignKey("TagId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__QuestionT__TagID__36B12243"),
                        l => l.HasOne<Question>().WithMany()
                            .HasForeignKey("QuestionId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__QuestionT__Quest__35BCFE0A"),
                        j =>
                        {
                            j.HasKey("QuestionId", "TagId").HasName("PK__Question__DB97A028F809BCBA");
                            j.ToTable("QuestionTags");
                            j.IndexerProperty<int>("QuestionId").HasColumnName("QuestionID");
                            j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                        });
            });

            // Cấu hình bảng Reports
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48E571976E59");
                entity.Property(e => e.ReportId).HasColumnName("ReportID");
                entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
                entity.Property(e => e.ReportedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Reason).HasMaxLength(255);
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Answer).WithMany(p => p.Reports)
                    .HasForeignKey(d => d.AnswerId)
                    .HasConstraintName("FK__Reports__AnswerI__4316F928");

                entity.HasOne(d => d.Question).WithMany(p => p.Reports)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("FK__Reports__Questio__4222D4EF");

                entity.HasOne(d => d.User).WithMany(p => p.Reports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Reports__UserID__412EB0B6");
            });

            // Cấu hình bảng Tags
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagId).HasName("PK__Tags__657CFA4CA22D5D0D");
                entity.HasIndex(e => e.Name, "UQ__Tags__737584F6BCA6F6F8").IsUnique();
                entity.Property(e => e.TagId).HasColumnName("TagID");
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            // Cấu hình bảng Votes
            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => e.VoteId).HasName("PK__Votes__52F015E20EF4C475");
                entity.Property(e => e.VoteId).HasColumnName("VoteID");
                entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Answer).WithMany(p => p.Votes)
                    .HasForeignKey(d => d.AnswerId)
                    .HasConstraintName("FK__Votes__AnswerID__3D5E1FD2");

                entity.HasOne(d => d.Question).WithMany(p => p.Votes)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("FK__Votes__QuestionI__3C69FB99");

                entity.HasOne(d => d.User).WithMany(p => p.Votes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Votes__UserID__3B75D760");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}