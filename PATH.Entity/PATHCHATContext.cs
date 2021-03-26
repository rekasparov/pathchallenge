using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace PATH.Entity
{
    public partial class PATHCHATContext : DbContext
    {
        public PATHCHATContext()
        {
        }

        public PATHCHATContext(DbContextOptions<PATHCHATContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatRoomLog> ChatRoomLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;Database=PATHCHAT;User Id=postgres;Password=07081983Konya;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Turkish_Turkey.utf8");

            modelBuilder.Entity<ChatRoomLog>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ChatRoomName)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.Message).HasMaxLength(250);

                entity.Property(e => e.MessageDate)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
