using ApiRinha.Modelo.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiRinha.Persistencia.Configuration
{
    public static class ClienteConfiguration
    {
        public static ModelBuilder ConfigureCliente(this ModelBuilder builder)
        {
            builder.Entity<Cliente>().ToTable("cliente");
            builder.Entity<Cliente>().HasKey(x => x.Id);
            builder.Entity<Cliente>().Property(x => x.Id).HasColumnName("id");
            builder.Entity<Cliente>().Property(x => x.Limite).HasColumnName("limite");
            builder.Entity<Cliente>().Property(x => x.SaldoInicial).HasColumnName("saldo_inicial");
            return builder;
        }
    }
}
