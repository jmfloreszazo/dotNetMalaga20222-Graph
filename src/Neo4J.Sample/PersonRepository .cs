namespace Neo4J.Sample;

public class PersonRepository : IPersonRepository
{
    private readonly INeo4jDataAccess _neo4JDataAccess;

    private ILogger<PersonRepository> _logger;

    public PersonRepository(INeo4jDataAccess neo4JDataAccess, ILogger<PersonRepository> logger)
    {
        _neo4JDataAccess = neo4JDataAccess;
        _logger = logger;
    }

    public async Task<List<Dictionary<string, object>>> SearchPersonsByName(string searchString)
    {
        var query = @"MATCH (p:Person) WHERE toUpper(p.name) CONTAINS toUpper($searchString) 
                                RETURN p{ name: p.name, born: p.born } ORDER BY p.Name LIMIT 5";

        IDictionary<string, object> parameters = new Dictionary<string, object> { { "searchString", searchString } };

        var persons = await _neo4JDataAccess.ExecuteReadDictionaryAsync(query, "p", parameters);

        return persons;
    }

    public async Task<bool> AddPerson(Person? person)
    {
        if (person != null && !string.IsNullOrWhiteSpace(person.Name))
        {
            var query = @"MERGE (p:Person {name: $name}) RETURN true";
            IDictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "name", person.Name }
                };
            return await _neo4JDataAccess.ExecuteWriteTransactionAsync<bool>(query, parameters);
        }
        else
        {
            throw new System.ArgumentNullException(nameof(person), "Person must not be null");
        }
    }

    public async Task<long> GetPersonCount()
    {
        var query = @"Match (p:Person) RETURN count(p) as personCount";
        var count = await _neo4JDataAccess.ExecuteReadScalarAsync<long>(query);
        return count;
    }
}