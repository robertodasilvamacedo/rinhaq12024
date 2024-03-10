namespace ApiRinha.Modelo.Domain
{
    public class HistoricoCliente
    {
        public int Id { get; set; }
        public int Valor { get; set; }
        public string Tipo { get; set; }
        public string Descricao { get; set;}
        public DateTime RealizadaEm { get; set; }
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; private set; }
    }
}
