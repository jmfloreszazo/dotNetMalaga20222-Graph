namespace Neo4J.Sample;

public interface IPersonRepository
{
    Task<List<Dictionary<string, object>>> SearchPersonsByName(string searchString);
    Task<bool> AddPerson(Person? person);
    Task<long> GetPersonCount();
}