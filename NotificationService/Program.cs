using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ExamFinishedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    b => b.AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()
          .WithOrigins("https://localhost:7043")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");

app.MapHub<ExamHub>("/examHub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
