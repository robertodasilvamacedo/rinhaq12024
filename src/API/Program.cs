using API;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES")))
{
    builder.Configuration["ConnectionStrings:Default"] = Environment.GetEnvironmentVariable("POSTGRES");
}

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("Default"));

builder.Services.AddScoped<API.ClientesDatabase>();

var app = builder.Build();
app.UseStatusCodePages();

var clientesApi = app.MapGroup("/clientes");

clientesApi.MapPost("/{id}/transacoes", async (int id, ClientesDatabase db, [FromBody] RequestTransaction request, CancellationToken cancellationToken) => {

    try
    {
        if(request is null)
        {
            return Results.UnprocessableEntity();
        }

        if (request.valor.GetType() != typeof(Int32) ||
        request.descricao.Length > 10 || String.IsNullOrEmpty(request.descricao) ||
        (!request.tipo.Equals("c") && !request.tipo.Equals("d")))
        {
            return Results.UnprocessableEntity();
        }

        if (id < 0 || id > 5)
        {
            return Results.NotFound();
        }

        ResponseTransaction ret = await db.ExecutarTransacao(id, request, cancellationToken);

        if (ret != null)
        {
            return Results.Ok(ret);
        }
        else
        {
            return Results.UnprocessableEntity();
        }
    }
    catch (Exception)
    {
        return Results.UnprocessableEntity();
    } 
});

clientesApi.MapGet("/{id}/extrato", async (int id, ClientesDatabase db, CancellationToken cancellationToken) =>
{
    try
    {
        if (id.GetType() != typeof(Int32))
        {
            return Results.UnprocessableEntity();
        }

        if (id < 0 || id > 5)
        {
            return Results.NotFound();
        }

        return Results.Ok(await db.ListarLogTransacao(id, cancellationToken));
    }
    catch (Exception)
    {
        return Results.UnprocessableEntity();
    }
});
app.Run();



[JsonSerializable(typeof(RequestTransaction[]))]
[JsonSerializable(typeof(ResponseTransaction))]
[JsonSerializable(typeof(ResponseExtrato))]
[JsonSerializable(typeof(Saldo))]
[JsonSerializable(typeof(UltimasTransacoes))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

