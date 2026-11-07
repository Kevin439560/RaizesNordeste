using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesDoNordeste.Aplicacao.Service;
using RaizesNordeste.Infra.Data;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<PedidoService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensagens = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var erro = new ErroPadraoDto(
                Status: StatusCodes.Status422UnprocessableEntity,
                Erro: "Erro de Validação do Contrato",
                Mensagem: string.Join(" | ", mensagens),
                Path: context.HttpContext.Request.Path,
                Timestamp: DateTime.UtcNow
            );

            return new UnprocessableEntityObjectResult(erro);
        };
    });

var chaveSecreta = builder.Configuration["Jwt:Key"] ?? "ChavePadraoEmCasosEspecificos!@#";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "RaizesNordeste.Api",
            ValidAudience = "RaizesNordeste.Clientes",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var erro = new ErroPadraoDto(
                    Status: 401,
                    Erro: "Não Autenticado",
                    Mensagem: "Token JWT ausente, inválido ou expirado.",
                    Path: context.Request.Path,
                    Timestamp: DateTime.UtcNow
                );
                await context.Response.WriteAsJsonAsync(erro);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var erro = new ErroPadraoDto(
                    Status: 403,
                    Erro: "Acesso Negado",
                    Mensagem: "Seu perfil de usuário não possui permissão para acessar este recurso.",
                    Path: context.Request.Path,
                    Timestamp: DateTime.UtcNow
                );
                await context.Response.WriteAsJsonAsync(erro);
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Raizes do Nordeste API",
        Version = "V1",
        Description = "Api de pedidos, estoque, fidelidade e pagamentos mockados sendo multicanal"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token jwt que e gerado no endpoint /api/v1/auth/login"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (status, titulo) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não Autenticado"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Não Encontrado"),
            ArgumentException => (StatusCodes.Status422UnprocessableEntity, "Erro de Validação"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflito de Regra de Negocio"),
            _ => (StatusCodes.Status500InternalServerError, "Erro Interno do Servidor")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var erroDto = new ErroPadraoDto(
            Status: status,
            Erro: titulo,
            Mensagem: exception?.Message ?? "Ocorreu um erro inesperado.",
            Path: context.Request.Path,
            Timestamp: DateTime.UtcNow
        );

        await context.Response.WriteAsJsonAsync(erroDto);
    });
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
