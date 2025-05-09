using Microsoft.EntityFrameworkCore;
using Bot.Models;

namespace Bot;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Query> Queries => Set<Query>();
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=data/database.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
        
            user.OwnsMany(u => u.Queries, query =>
            {
                query.HasKey(q => q.Id);
                query.Property(q => q.Id).ValueGeneratedOnAdd();
                
                query.WithOwner();
                query.Property(q => q.Text).IsRequired();
                query.OwnsMany(q => q.NewArticles, article =>
                {
                    article.HasKey(a => a.Id);
                    article.Property(a => a.Id).ValueGeneratedOnAdd(); 
                    
                    article.Property(a => a.Title);
                    article.Property(a => a.Doi);
                    article.Property(a => a.Author);
                    article.Property(a => a.Publisher);
                    article.Property(a => a.Issued);
                    article.Property(a => a.Url);
                    
                    article.OwnsOne(a => a.Journal, journal =>
                    {
                        journal.Property(j => j.Title);
                        journal.Property(j => j.PrintIssn);
                        journal.Property(j => j.ElectronicIssn);
                    });
                });
            });

            user.Property(u => u.ShownArticlesDois)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet()
                );
        });
    }

}