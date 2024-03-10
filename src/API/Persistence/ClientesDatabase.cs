using API.Model;
using Npgsql;

namespace API
{
    public class ClientesDatabase
    {
        private readonly NpgsqlConnection _connection;

        public ClientesDatabase(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<ResponseTransaction> ExecutarTransacao(int id, RequestTransaction request, CancellationToken cancellationToken)
        {
            await _connection.OpenAsync();

            await using (var cmd = new NpgsqlCommand($"SELECT * FROM ExecutarTransacao({id}, {request.valor}, '{request.tipo}', '{request.descricao}')", _connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync())
                    {
                        return new(reader.GetInt32(0), reader.GetInt32(1));
                    }
                }
            }
            return null;
        }

        public async Task<ResponseExtrato> ListarLogTransacao(int id, CancellationToken cancellationToken)
        {
            await _connection.OpenAsync();

            var responseExtratoDTO = new ResponseExtrato();
            responseExtratoDTO.saldo = new Saldo();
            responseExtratoDTO.ultimas_transacoes = new List<UltimasTransacoes>();

            await using (var cmd = new NpgsqlCommand($"select c.id, c.limite, c.saldo_inicial, hc.valor, hc.tipo, hc.descricao, hc.realizada_em from cliente c left join historico_cliente hc" +
                $" on c.id = hc.id_cliente where c.id = {id} order by hc.realizada_em DESC LIMIT 10", _connection))
            {
                await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        int cont = 0;
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            if (cont == 0)
                            {
                                responseExtratoDTO.saldo.total = reader.GetInt32(2);
                                responseExtratoDTO.saldo.limite = reader.GetInt32(1);
                                responseExtratoDTO.saldo.data_extrato = DateTime.UtcNow.AddHours(-3);
                            }

                            if (reader["descricao"] != DBNull.Value)
                            {
                                UltimasTransacoes ultimasTransacoesTemp = new UltimasTransacoes()
                                {
                                    valor = reader.GetInt32(3),
                                    descricao = reader.GetString(5),
                                    realizada_em = reader.GetDateTime(6).ToUniversalTime(),
                                    tipo = reader.GetString(4)
                                };
                                responseExtratoDTO.ultimas_transacoes.Add(ultimasTransacoesTemp);
                            }

                            cont++;
                        }
                    }
                }
            }
            return responseExtratoDTO;
        }
    }
}
