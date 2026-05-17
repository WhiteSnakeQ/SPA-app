namespace SPA_приложение.Data
{
    using Microsoft.EntityFrameworkCore;
    using SPA_приложение.DTOs;
    using SPA_приложение.Models;

    public class AppDbContext : DbContext
    {
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<CommentFile> CommentsFiles => Set<CommentFile>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Comment>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasIndex(x => x.RootId);
            builder.Entity<Comment>()
                .HasIndex(x => x.ParentId);
            builder.Entity<Comment>()
                .HasIndex(x => x.CreatedAt);
            builder.Entity<Comment>()
                .HasIndex(x => x.UserName);
            builder.Entity<Comment>()
                .HasIndex(x => x.Email);

            builder.Entity<CommentFile>()
                 .HasOne(x => x.Comment)
                 .WithMany(x => x.Files)
                 .HasForeignKey(x => x.CommentId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommentFile>()
                .Property(x => x.FileType)
                .HasConversion<string>();

            builder.Entity<CommentFile>()
                .HasIndex(x => x.CommentId);
        }
    }
}
