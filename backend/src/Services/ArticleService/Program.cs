using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Shared.Contracts;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.ArticleService.Data;
using RealWorldApp.ArticleService.Handlers.Commands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add Entity Framework
builder.Services.AddDbContext<ArticleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ArticleDb")));

// Add Kafka Producer as Singleton
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Register handlers
builder.Services.AddScoped<IRequestHandler<CreateArticleCommand, ArticleDto>, CreateArticleHandler>();
builder.Services.AddScoped<IRequestHandler<UpdateArticleCommand, bool>, UpdateArticleHandler>();
// builder.Services.AddScoped<IRequestHandler<DeleteArticleCommand, bool>, DeleteArticleHandler>();

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
    var db = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();