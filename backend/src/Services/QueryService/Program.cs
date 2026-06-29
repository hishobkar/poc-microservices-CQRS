using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.QueryService.Data;
using RealWorldApp.QueryService.Consumers;
using RealWorldApp.QueryService.Handlers.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework for read database
builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReadDb")));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register query handlers
//builder.Services.AddScoped<IRequestHandler<GetArticlesQuery, List<ArticleDto>>, GetArticlesHandler>();
//builder.Services.AddScoped<IRequestHandler<GetArticleByIdQuery, ArticleDto?>, GetArticleByIdHandler>();

// Register Kafka consumers as hosted services
builder.Services.AddHostedService<ArticleCreatedConsumer>();
//builder.Services.AddHostedService<ArticleUpdatedConsumer>();
//builder.Services.AddHostedService<ArticleDeletedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database schema exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();