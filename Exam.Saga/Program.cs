using Exam.Saga;
using Exam.Saga.Persistence;
using Exam.Saga.StateMachines;
using MassTransit;
using MassTransit.QuartzIntegration;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSql");

builder.Services.AddDbContext<ExamSagaDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    q.UsePersistentStore(s =>
    {
        s.UsePostgres(connectionString);
        s.UseJsonSerializer();           
        s.UseClustering();              
    });
});
builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

builder.Services.AddMassTransit(x =>
{
    x.AddPublishMessageScheduler();

    x.AddQuartzConsumers();

    x.AddSagaStateMachine<ExamStateMachine, ExamState>()
    .EntityFrameworkRepository(r =>
    {
        r.ExistingDbContext<ExamSagaDbContext>();
        r.UsePostgres();

        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
    });


    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost");

        cfg.UsePublishMessageScheduler();

        cfg.ReceiveEndpoint("exam-saga-queue", e =>
        {
            e.UseInMemoryOutbox(context);
            e.ConfigureSaga<ExamState>(context);
        });

        cfg.ReceiveEndpoint("quartz", e =>
        {
            context.ConfigureConsumer<ScheduleMessageConsumer>(e);
            context.ConfigureConsumer<CancelScheduledMessageConsumer>(e);
        });

        /*cfg.ReceiveEndpoint("quartz", e =>
        {
            e.ConfigureConsumeTopology = false;
            context.ConfigureConsumer<ScheduleMessageConsumer>(e);
            context.ConfigureConsumer<CancelScheduledMessageConsumer>(e);
        });

        cfg.ReceiveEndpoint("exam-saga-queue", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromMilliseconds(200)));
            e.UseInMemoryOutbox(context);
            e.ConfigureSaga<ExamState>(context);
        });*/
    });

});

var host = builder.Build();
host.Run();
