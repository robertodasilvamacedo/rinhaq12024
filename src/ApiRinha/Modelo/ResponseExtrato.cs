namespace ApiRinha.Modelo
{
    public class ResponseExtrato
    {
        public Saldo saldo { get; set; }
        public List<UltimasTransacoes> ultimas_transacoes { get; set; }
    }
    public class Saldo
    {
        public int total { get; set; }
        public DateTime data_extrato { get; set; }
        public int limite { get; set; }
    }

    public class UltimasTransacoes
    {
        public int valor { get; set; }
        public string tipo { get; set; }
        public string descricao { get; set; }
        public DateTime realizada_em { get; set; }
    }
}
