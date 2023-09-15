using BlobTrigger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddScoped<EmailSenderService>();
    })
    .Build();

host.Run();
