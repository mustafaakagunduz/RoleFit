using RoleFit.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));
builder.Services.AddHttpClient<ILlmClient, OpenAiLlmClient>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IFitAnalyzer, FitAnalyzer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
