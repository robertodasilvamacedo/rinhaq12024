using ApiRinha.Modelo;
using ApiRinha.Modelo.Domain;
using ApiRinha.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiRinha.Controllers
{
    [ApiController]
    [Route("/clientes/")]
    public class RinhaController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public RinhaController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("{id}/transacoes")]
        public async Task<IResult> Transacao(int id, [FromBody] RequestTransaction request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.valor.GetType() != typeof(Int32) ||
                request.descricao.Length > 10 ||
                (!request.tipo.Equals("c") && !request.tipo.Equals("d")))
                {
                    return Results.UnprocessableEntity();
                }

                if (request is null)
                {
                    return Results.UnprocessableEntity();
                }

                if (request.descricao is null || request.tipo is null)
                {
                    return Results.UnprocessableEntity();
                }

                if(id < 0 || id > 5)
                {
                    return Results.NotFound();
                }
                string sql = $"SELECT * FROM ExecutarTransacao({id}, {request.valor}, '{request.tipo}', '{request.descricao}')";
                var result = await _dbContext.SPFuncaoPostgres.FromSqlRaw(sql).FirstOrDefaultAsync();

                if(result is not null)
                {
                    ResponseTransaction ret = new ResponseTransaction(result!.Limite, result.Saldo);
                    return Results.Ok(ret);
                }
                else
                {
                    Results.UnprocessableEntity();
                }
            }
            catch (Exception ex)
            {
                return Results.UnprocessableEntity();
            }

            return Results.UnprocessableEntity();
        }

        [HttpGet("{id}/extrato")]
        public async Task<IResult> Extrato(int id, CancellationToken cancellationToken)
        {
            if (id.GetType() != typeof(Int32))
            {
                return Results.UnprocessableEntity();
            }

            
            var cliente = await _dbContext.Cliente.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (cliente is null)
            {
                return Results.NotFound();
            }

            var historicoCliente = await _dbContext.HistoricoCliente.Where(p => p.ClienteId == id).OrderByDescending(p => p.RealizadaEm).Take(10).AsNoTracking().ToListAsync(cancellationToken);

            var responseExtratoDTO = new ResponseExtrato();
            responseExtratoDTO.saldo = new Saldo();
            responseExtratoDTO.saldo.total = cliente.SaldoInicial;
            responseExtratoDTO.saldo.data_extrato = DateTime.UtcNow.AddHours(-3);
            responseExtratoDTO.saldo.limite = cliente.Limite;
            responseExtratoDTO.ultimas_transacoes = new List<UltimasTransacoes>();

            if (historicoCliente.Count > 0)
            {
                foreach (var historico in historicoCliente)
                {
                    UltimasTransacoes ultimasTransacoesTemp = new UltimasTransacoes()
                    {
                        valor = historico.Valor,
                        descricao = historico.Descricao,
                        realizada_em = historico.RealizadaEm.ToUniversalTime(),
                        tipo = historico.Tipo
                    };
                    responseExtratoDTO.ultimas_transacoes.Add(ultimasTransacoesTemp);
                }
            }

            return Results.Ok(responseExtratoDTO);
        }

        [HttpGet()]
        public IResult Get(int id)
        {
            return Results.Ok();
        }
    }
}
