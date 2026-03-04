using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using WEBAPP_FitMatch.Models;

namespace WEBAPP_FitMatch.Data
{
    public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Member> Members {get;set;}
    public DbSet<Comment> Comments {get;set;}

    public DbSet<Notification> Notifications{get;set;}
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Mission> Missions { get; set; } 

    public DbSet<History> Histories {get;set;}
}
}
