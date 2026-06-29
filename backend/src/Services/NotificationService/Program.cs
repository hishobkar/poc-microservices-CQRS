using RealWorldApp.NotificationService.Consumers;
using RealWorldApp.NotificationService.Contracts;
using RealWorldApp.NotificationService.Services;
using RealWorldApp.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register Kafka consumers
builder.Services.AddHostedService<ArticleCreatedNotificationConsumer>();
builder.Services.AddHostedService<ArticleUpdatedNotificationConsumer>();

var app = builder.Build();
app.Run();