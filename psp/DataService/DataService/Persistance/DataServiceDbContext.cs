using DataService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataService.Persistance;

public class DataServiceDbContext : DbContext
{
    public DataServiceDbContext(DbContextOptions<DataServiceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}
