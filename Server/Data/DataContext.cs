using blazorSBIFS.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace blazorSBIFS.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Activity> Activities { get; set; }
    }
}
