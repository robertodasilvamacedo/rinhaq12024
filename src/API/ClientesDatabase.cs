using Npgsql;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Transactions;

namespace API
{
    public class ClientesDatabase : IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection _connection;

        public ClientesDatabase(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<ResponseTransacaoDTO> GetClientById(int id, NpgsqlTransaction transaction)
        {
            await using (var cmd = new NpgsqlCommand($"SELECT limite, saldo_inicial FROM cliente WHERE id = {id}", _connection, transaction))
            {
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        return new ResponseTransacaoDTO(reader.GetInt32(0), reader.GetInt32(1));
                    }
                }
            }
            return new ResponseTransacaoDTO(0,0);
        }

        public async Task<int> ExecutarTransacao(int id, int novo_saldo, NpgsqlTransaction transaction)
        {
            await using (var cmd = new NpgsqlCommand($"UPDATE cliente SET saldo_inicial = {novo_saldo} where id = {id}", _connection, transaction))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> InserirLogTransacao(int id, RequestTransacaoDTO request, NpgsqlTransaction transaction)
        {
            await using (var cmd = new NpgsqlCommand($"INSERT INTO historico_cliente (valor, tipo, descricao, realizada_em, id_cliente) VALUES ({request.valor},'{request.tipo}','{request.descricao}','{DateTime.Now.AddHours(-3).ToString("yyyy-MM-dd hh:mm:ss")}',{id})", _connection, transaction))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<UltimasTransacoes>> ListarLogTransacao(int id, NpgsqlTransaction transaction)
        {
            List<UltimasTransacoes> ultimasTransacoes = new List<UltimasTransacoes>();

            await using (var cmd = new NpgsqlCommand($"SELECT hc.valor, hc.tipo, hc.descricao, hc.realizada_em FROM historico_cliente hc" +
                $" WHERE id_cliente = {id} ORDER BY hc.realizada_em DESC LIMIT 10", _connection, transaction))
            {
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            UltimasTransacoes ultimasTransacoesTemp = new UltimasTransacoes()
                            {
                                valor = reader.GetInt32(0),
                                descricao = reader.GetString(2),
                                realizada_em = reader.GetDateTime(3),
                                tipo = reader.GetString(1)
                            };
                            ultimasTransacoes.Add(ultimasTransacoesTemp);
                        }
                    }
                }
            }
            return ultimasTransacoes;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
