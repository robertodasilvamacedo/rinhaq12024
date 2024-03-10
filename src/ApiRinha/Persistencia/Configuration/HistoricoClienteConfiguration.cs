using ApiRinha.Modelo.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiRinha.Persistencia.Configuration
{
    public static class HistoricoClienteConfiguration
    {
        public static ModelBuilder ConfigureHistoricoCliente(this ModelBuilder builder)
        {
            builder.Entity<HistoricoCliente>().ToTable("historico_cliente");
            builder.Entity<HistoricoCliente>().HasKey(x => x.Id);
            builder.Entity<HistoricoCliente>().Property(x => x.Id).HasColumnName("id");
            builder.Entity<HistoricoCliente>().Property(x => x.Valor).HasColumnName("valor");
            builder.Entity<HistoricoCliente>().Property(x => x.Tipo).HasColumnName("tipo");
            builder.Entity<HistoricoCliente>().Property(x => x.Descricao).HasColumnName("descricao");
            builder.Entity<HistoricoCliente>().Property(x => x.RealizadaEm).HasColumnName("realizada_em");
            builder.Entity<HistoricoCliente>().Property(x => x.ClienteId).HasColumnName("id_cliente");
            return builder;
        }
    }
}
