using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace Neo4J.Sample;

public class Neo4JDataAccess : INeo4jDataAccess
{
    private readonly IAsyncSession _session;

    private readonly ILogger<Neo4JDataAccess> _logger;

    private readonly string _database;

    public Neo4JDataAccess(IDriver driver, ILogger<Neo4JDataAccess> logger,
        IOptions<ApplicationSettings> appSettingsOptions)
    {
        _logger = logger;
        _database = appSettingsOptions.Value.Neo4jDatabase ?? "neo4j";
        _session = driver.AsyncSession(o => o.WithDatabase(_database));
    }

    public async Task<List<string>> ExecuteReadListAsync(string query, string returnObjectKey,
        IDictionary<string, object>? parameters = null)
    {
        return await ExecuteReadTransactionAsync<string>(query, returnObjectKey, parameters);
    }

    public async Task<List<Dictionary<string, object>>> ExecuteReadDictionaryAsync(string query, string returnObjectKey,
        IDictionary<string, object>? parameters = null)
    {
        return await ExecuteReadTransactionAsync<Dictionary<string, object>>(query, returnObjectKey, parameters);
    }

    public async Task<T> ExecuteReadScalarAsync<T>(string query, IDictionary<string, object>? parameters = null)
    {
        try
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var result = await _session.ReadTransactionAsync(async tx =>
            {
                var scalar = default(T);

                var res = await tx.RunAsync(query, parameters);

                scalar = (await res.SingleAsync())[0].As<T>();

                return scalar;
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a problem while executing database query");
            throw;
        }
    }

    public async Task<T> ExecuteWriteTransactionAsync<T>(string query, IDictionary<string, object>? parameters = null)
    {
        try
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var result = await _session.WriteTransactionAsync(async tx =>
            {
                var scalar = default(T);

                var res = await tx.RunAsync(query, parameters);

                scalar = (await res.SingleAsync())[0].As<T>();

                return scalar;
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a problem while executing database query");
            throw;
        }
    }

    private async Task<List<T>> ExecuteReadTransactionAsync<T>(string query, string returnObjectKey,
        IDictionary<string, object>? parameters)
    {
        try
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var result = await _session.ReadTransactionAsync(async tx =>
            {
                var data = new List<T>();

                var res = await tx.RunAsync(query, parameters);

                var records = await res.ToListAsync();

                data = records.Select(x => (T) x.Values[returnObjectKey]).ToList();

                return data;
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a problem while executing database query");
            throw;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _session.CloseAsync();
    }
}