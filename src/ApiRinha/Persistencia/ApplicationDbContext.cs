using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApiRinha.Modelo.Domain;
using ApiRinha.Persistencia.Configuration;
using System.Reflection.Emit;

namespace ApiRinha.Persistencia
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //public ApplicationDbContext(string connectionString) : base(GetOptions(connectionString))
        //{

        //}

        private static DbContextOptions GetOptions(string connectionString)
        {
            return NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql(new DbContextOptionsBuilder(), connectionString).Options;
        }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<HistoricoCliente> HistoricoCliente { get; set; }
        public DbSet<SPFuncaoPostgres> SPFuncaoPostgres { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ConfigureCliente();
            builder.ConfigureHistoricoCliente();

            builder.Entity<SPFuncaoPostgres>(e =>
            {
                e.HasNoKey();
                e.Property(x => x.Limite).HasColumnName("limite");
                e.Property(x => x.Saldo).HasColumnName("saldo_inicial");
            });
        }
    }
}
