using CalendarApp.MessageConsumer.Services;
using CalendarApp.MessageConsumer.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Register EmailService as singleton
builder.Services.AddSingleton<EmailService>();

// Register workers
builder.Services.AddHostedService<UserRegisteredWorker>();
builder.Services.AddHostedService<CalendarSharedWorker>();

var host = builder.Build();
host.Run();
