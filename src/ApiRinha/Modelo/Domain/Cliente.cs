namespace ApiRinha.Modelo.Domain
{
    public class Cliente
    {
        public int Id { get; set; }
        public int Limite { get; set; }
        public int SaldoInicial { get; set; }
        public virtual IEnumerable<HistoricoCliente> HistoricoClientes { get; private set; }
    }
}
