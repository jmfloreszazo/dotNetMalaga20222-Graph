using Microsoft.AspNetCore.Mvc;
using Neo4J.Sample;

namespace Neo4J.DriverDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepository _personRepository;

    public PersonsController(ILogger<PersonsController> logger, IPersonRepository personRepository,
        INeo4jDataAccess neo4JDataAccess)
    {
        _ = logger;
        _personRepository = personRepository;
        _ = neo4JDataAccess;
    }

    [HttpGet(Name = "GetPersons")]
    public Task<bool> Get()
    {
        //Tremenda burrada, pero es para que veais el funcionamiento

        var person = new Person
        {
            Name = "Jose Maria",
        };
        return _personRepository.AddPerson(person);
    }
}