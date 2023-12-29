using api.Domaine.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Domaine.Data
{
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
