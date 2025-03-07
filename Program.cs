// Program.cs
using LlamaWebApi.Helpers;
using LlamaWebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Semantic Kernel with LLaMA
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IChatCompletionService>(sp =>
    new LlamaChatCompletionService(sp.GetRequiredService<HttpClient>(), "http://localhost:8080/")
);


builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.Services.AddSingleton(sp.GetRequiredService<IChatCompletionService>());
    var kernel = kernelBuilder.Build();
    kernel.ImportPluginFromObject(new AppFunctions(), "App");
    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();