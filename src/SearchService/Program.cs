using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (System.Exception e)
    {
        Console.WriteLine(e);
    }
});



app.Run();

//if our auction service is down
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
//we are gonna handle the exception
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
//and we are gonna keep on trying every 3 seconds
//until the action service is back up
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
