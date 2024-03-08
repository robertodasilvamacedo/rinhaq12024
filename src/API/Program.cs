using API;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data.Common;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Transactions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=192.168.2.171; Username=postgres; Password=CampoDigital@123$; Database=rinha");
var dataSource = dataSourceBuilder.Build();
builder.Services.AddScoped<API.ClientesDatabase>();
builder.Services.AddScoped<NpgsqlConnection>((sp) => { return dataSource.OpenConnection(); });

var app = builder.Build();
app.UseStatusCodePages();

var clientesApi = app.MapGroup("/clientes");

clientesApi.MapPost("/{id}/transacoes", async (int id, NpgsqlConnection connection, ClientesDatabase db, [FromBody] RequestTransacaoDTO request) => {

        if (request.valor.GetType() != typeof(Int32) ||
        request.descricao.Length >= 10 ||
        (!request.tipo.Equals("c") && !request.tipo.Equals("d")))
        {
            return Results.UnprocessableEntity();
        }

        var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var cliente = await db.GetClientById(id, transaction);
        if (cliente is null) 
        { 
            return Results.NotFound();
        }

        
        int novo_saldo = request.tipo.Equals("d") ? cliente.saldo - request.valor : cliente.saldo + request.valor;
        ResponseTransacaoDTO ret = new ResponseTransacaoDTO(cliente.limite, novo_saldo);

        if (request.tipo.Equals("d") && (novo_saldo < 0 ? Math.Abs(novo_saldo) : novo_saldo) > cliente.limite)
        {
            return Results.UnprocessableEntity();
        }
        else
        {
            //atualizar as transações
            await db.ExecutarTransacao(id, novo_saldo, transaction);
            //inserir o log da transação
            await db.InserirLogTransacao(id, request, transaction);
        }

        try
        {
            await transaction.CommitAsync();
        }
        catch(Exception)
        {
            await transaction.RollbackAsync();
            return Results.StatusCode(500);
        }

        return Results.Ok(ret);

});

clientesApi.MapGet("/{id}/extrato", async (int id, NpgsqlConnection connection, ClientesDatabase db) =>
{

    if (id.GetType() != typeof(Int32))
    {
        return Results.UnprocessableEntity();
    }

    var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
    var cliente = await db.GetClientById(id, transaction);

    if (cliente is null)
    {
        return Results.NotFound();
    }

    var responseExtratoDTO = new ResponseExtratoDTO();
    responseExtratoDTO.saldo = new Saldo();
    responseExtratoDTO.saldo.total = cliente.saldo;
    responseExtratoDTO.saldo.data_extrato = DateTime.Now;
    responseExtratoDTO.saldo.limite = cliente.limite;
    responseExtratoDTO.ultimas_transacoes = await db.ListarLogTransacao(id, transaction);

    return Results.Ok(responseExtratoDTO);
});
app.Run();


#region "DTOs"
public record RequestTransacaoDTO(int valor, string tipo, string descricao);
public record ResponseTransacaoDTO(int limite, int saldo);

public record ResponseExtratoDTO
{
    public Saldo saldo { get; set; }
    public List<UltimasTransacoes> ultimas_transacoes { get; set; }
}

public record Saldo
{
    public int total { get; set; }
    public DateTime data_extrato { get; set; }
    public int limite { get; set; }
}

public record UltimasTransacoes
{
    public int valor { get; set; }
    public string tipo { get; set; }
    public string descricao { get; set; }
    public DateTime realizada_em { get; set; }
}


[JsonSerializable(typeof(RequestTransacaoDTO[]))]
[JsonSerializable(typeof(ResponseTransacaoDTO))]
[JsonSerializable(typeof(ResponseExtratoDTO))]
[JsonSerializable(typeof(Saldo))]
[JsonSerializable(typeof(UltimasTransacoes))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
#endregion

