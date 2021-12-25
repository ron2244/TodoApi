using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace Todoapi.Models
{
    public class TodoContext : DbContext
    {
        protected readonly IConfiguration Configuration; 
        public TodoContext(DbContextOptions<TodoContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options){
            options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
        }
        public DbSet<task> tasks { get; set; }
        public DbSet<people> peoples { get; set; }
   }
}