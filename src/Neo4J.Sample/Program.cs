using Neo4j.Driver;
using Neo4J.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

builder.Services.AddSingleton(GraphDatabase.Driver(settings.Neo4jConnection, AuthTokens.Basic(settings.Neo4jUser, settings.Neo4jPassword)));

builder.Services.AddScoped<INeo4jDataAccess, Neo4JDataAccess>();
builder.Services.AddTransient<IPersonRepository, PersonRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
