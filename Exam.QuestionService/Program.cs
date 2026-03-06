using Exam.QuestionService.Consumers;
using MassTransit;
using Npgsql;
using System.Data;

var builder = Host.CreateApplicationBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PostgreSql");

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(connectionString));

builder.Services.AddMassTransit(x =>
{
    Console.WriteLine("Question Service başlayır");

    x.AddConsumer<PrepareQuestionsConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        Console.WriteLine($"RabbitMQ Hostuna qoşulur: {host}");

        cfg.ReceiveEndpoint("question-service-queue", e =>
        {
            e.ConfigureConsumer<PrepareQuestionsConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
