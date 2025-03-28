using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Redis como servi�o de cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost"; // Substitua pelo seu endere�o Redis
    options.InstanceName = "CarrinhoDeComprasInstance"; // Nome da inst�ncia
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Rota para salvar o carrinho no Redis
app.MapPost("/carrinhos", async (Carrinho carrinho, IDistributedCache redis) =>
{
    // Serialize o objeto Carrinho para JSON e salva no Redis
    await redis.SetStringAsync(carrinho.UsuarioId, JsonSerializer.Serialize(carrinho));
    return Results.Ok(true);
});

// Rota para obter os dados do carrinho desse usu�rio
app.MapGet("/carrinhos/{usuarioId}", async (string usuarioId, IDistributedCache redis) =>
{
    // Verifica se o usu�rio existe
    var data = await redis.GetStringAsync(usuarioId);
    // Se n�o existir, retorna null
    if (string.IsNullOrEmpty(data)) return Results.NotFound();

    var carrinho = JsonSerializer.Deserialize<Carrinho>(data, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = false
    });

    return Results.Ok(carrinho);
});

app.Run();

// Classe usada apenas para grava��o com 2 atributos
record Carrinho(string UsuarioId, List<Produto> Produtos);

record Produto(string Nome, int Quantidade, decimal PrecoUnitario);
