using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostUser> PostUsers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
}