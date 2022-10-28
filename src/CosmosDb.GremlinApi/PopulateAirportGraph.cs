using System.Diagnostics;
using Gremlin.Net.Driver;
using Microsoft.Extensions.Configuration;

namespace CosmosDb.GremlinApi
{
	public static class PopulateAirportGraph
	{
		private const string DatabaseName = "graphdb";
		private const string GraphName = "airport";

		public async static Task Run()
		{
			Debugger.Break();

			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var hostname = config["CosmosHostName"];
			var masterKey = config["CosmosMasterKey"];

			var username = $"/dbs/{DatabaseName}/colls/{GraphName}";

			var gremlinServer = new GremlinServer(hostname, 443, true, username, masterKey);

			using (var client = new GremlinClient(gremlinServer, mimeType: GremlinClient.GraphSON2MimeType))
			{
				await ClearGraph(client);

				// --- Terminal 1 ---

				// V: Terminal 1
				await CreateTerminal(client, "Terminal 1");

				// V: Gates in terminal 1
				await CreateGate(client, "Gate T1-1");
				await CreateGate(client, "Gate T1-2");
				await CreateGate(client, "Gate T1-3");

				// V: Restaurants in terminal 2
				await CreateRestaurant(client, "Wendys", 4, 9);
				await CreateRestaurant(client, "McDonalds", 3, 8);
				await CreateRestaurant(client, "Chipotle", 6, 12);

				// E: TerminalToGate (cyan)
				await CreateTerminalToGate(client, "Terminal 1", "Gate T1-1", 3);
				await CreateTerminalToGate(client, "Terminal 1", "Gate T1-2", 5);
				await CreateTerminalToGate(client, "Terminal 1", "Gate T1-3", 7);

				// E: TerminalToRestaurant (purple)
				await CreateTerminalToRestaurant(client, "Terminal 1", "Wendys", 5);
				await CreateTerminalToRestaurant(client, "Terminal 1", "McDonalds", 7);
				await CreateTerminalToRestaurant(client, "Terminal 1", "Chipotle", 10);

				// E: GateToNextGate / GateToPrevGate (cyan dashed)
				await CreateGateToGate(client, "Gate T1-1", "Gate T1-2", 2);
				await CreateGateToGate(client, "Gate T1-2", "Gate T1-3", 2);

				// E: GateToRestaurant (purple dashed)
				await CreateGateToRestaurant(client, "Gate T1-1", "Wendys", 2);
				await CreateGateToRestaurant(client, "Gate T1-1", "McDonalds", 4);
				await CreateGateToRestaurant(client, "Gate T1-1", "Chipotle", 6);
				await CreateGateToRestaurant(client, "Gate T1-2", "Wendys", 2);
				await CreateGateToRestaurant(client, "Gate T1-2", "McDonalds", 4);
				await CreateGateToRestaurant(client, "Gate T1-2", "Chipotle", 6);
				await CreateGateToRestaurant(client, "Gate T1-3", "Wendys", 6);
				await CreateGateToRestaurant(client, "Gate T1-3", "McDonalds", 4);
				await CreateGateToRestaurant(client, "Gate T1-3", "Chipotle", 2);

				// --- Terminal 2 ---

				// V: Terminal 2
				await CreateTerminal(client, "Terminal 2");

				// V: Gates in terminal 2
				await CreateGate(client, "Gate T2-1");
				await CreateGate(client, "Gate T2-2");
				await CreateGate(client, "Gate T2-3");

				// V: Restaurants in terminal 2
				await CreateRestaurant(client, "Jack in the Box", 3, 3);
				await CreateRestaurant(client, "Kentucky Fried Chicken", 4, 7);
				await CreateRestaurant(client, "Burger King", 2, 7);

				// E: TerminalToGate
				await CreateTerminalToGate(client, "Terminal 2", "Gate T2-1", 3);
				await CreateTerminalToGate(client, "Terminal 2", "Gate T2-2", 5);
				await CreateTerminalToGate(client, "Terminal 2", "Gate T2-3", 7);

				// E: TerminalToRestaurant
				await CreateTerminalToRestaurant(client, "Terminal 2", "Jack in the Box", 5);
				await CreateTerminalToRestaurant(client, "Terminal 2", "Kentucky Fried Chicken", 7);
				await CreateTerminalToRestaurant(client, "Terminal 2", "Burger King", 10);

				// E: GateToNextGate / GateToPrevGate
				await CreateGateToGate(client, "Gate T2-1", "Gate T2-2", 2);
				await CreateGateToGate(client, "Gate T2-2", "Gate T2-3", 2);

				// E: GateToRestaurant
				await CreateGateToRestaurant(client, "Gate T2-1", "Jack in the Box", 2);
				await CreateGateToRestaurant(client, "Gate T2-1", "Kentucky Fried Chicken", 4);
				await CreateGateToRestaurant(client, "Gate T2-1", "Burger King", 6);
				await CreateGateToRestaurant(client, "Gate T2-2", "Jack in the Box", 2);
				await CreateGateToRestaurant(client, "Gate T2-2", "Kentucky Fried Chicken", 4);
				await CreateGateToRestaurant(client, "Gate T2-2", "Burger King", 6);
				await CreateGateToRestaurant(client, "Gate T2-3", "Jack in the Box", 6);
				await CreateGateToRestaurant(client, "Gate T2-3", "Kentucky Fried Chicken", 4);
				await CreateGateToRestaurant(client, "Gate T2-3", "Burger King", 2);

				// --- Terminal to Terminal ---

				// E: TerminalToNextTerminal / TerminalToPrevTerminal
				await CreateTerminalToTerminal(client, "Terminal 1", "Terminal 2", 10);
			}
		}

		private static async Task ClearGraph(GremlinClient client)
		{
			var gremlinCode = $@"
				g.V()
					.drop()
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine("Graph has been cleared");
		}

		private static async Task CreateTerminal(GremlinClient client, string id)
		{
			var gremlinCode = $@"
				g.addV('terminal')
					.property('id', '{id}')
					.property('city', 'LA')
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created vertex: Terminal '{id}'");
		}

		private static async Task CreateGate(GremlinClient client, string id)
		{
			var gremlinCode = $@"
				g.addV('gate')
					.property('id', '{id}')
					.property('city', 'LA')
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created vertex: Gate '{id}'");
		}

		private static async Task CreateRestaurant(GremlinClient client, string id, decimal rating, decimal averagePrice)
		{
			var gremlinCode = $@"
				g.addV('restaurant')
					.property('id', '{id}')
					.property('city', 'LA')
					.property('rating', {rating})
					.property('averagePrice', {averagePrice})
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created vertex: Restaurant '{id}'");
		}

		private static async Task CreateTerminalToGate(GremlinClient client, string terminal, string gate, int distanceInMinutes)
		{
			var gremlinCode = $@"
				g.V()
					.has('id', '{terminal}')
					.addE('terminalToGate')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{gate}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: TerminalToGate '{terminal}' > '{gate}'");

			gremlinCode = $@"
				g.V()
					.has('id', '{gate}')
					.addE('gateToTerminal')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{terminal}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: GateToTerminal '{gate}' > '{terminal}'");
		}

		private static async Task CreateTerminalToRestaurant(GremlinClient client, string terminal, string restaurant, int distanceInMinutes)
		{
			var gremlinCode = $@"
				g.V()
					.has('id', '{terminal}')
					.addE('terminalToRestaurant')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{restaurant}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: TerminalToRestaurant '{terminal}' > '{restaurant}'");

			gremlinCode = $@"
				g.V()
					.has('id', '{restaurant}')
					.addE('restaurantToTerminal')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{terminal}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: RestaurantToTerminal '{restaurant}' > '{terminal}'");
		}

		private static async Task CreateGateToGate(GremlinClient client, string gate1, string gate2, int distanceInMinutes)
		{
			var gremlinCode = $@"
				g.V()
					.has('id', '{gate1}')
					.addE('gateToNextGate')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{gate2}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: GateToNextGate '{gate1}' > '{gate2}'");

			gremlinCode = $@"
				g.V()
					.has('id', '{gate2}')
					.addE('gateToPrevGate')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{gate1}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: GateToPrevGate '{gate2}' > '{gate1}'");
		}

		private static async Task CreateGateToRestaurant(GremlinClient client, string gate, string restaurant, int distanceInMinutes)
		{
			var gremlinCode = $@"
				g.V()
					.has('id', '{gate}')
					.addE('gateToRestaurant')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{restaurant}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: GateToRestaurant '{gate}' > '{restaurant}'");

			gremlinCode = $@"
				g.V()
					.has('id', '{restaurant}')
					.addE('restaurantToGate')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{gate}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: RestaurantToGate '{restaurant}' > '{gate}'");
		}

		private static async Task CreateTerminalToTerminal(GremlinClient client, string terminal1, string terminal2, int distanceInMinutes)
		{
			var gremlinCode = $@"
				g.V()
					.has('id', '{terminal1}')
					.addE('terminalToNextTerminal')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{terminal2}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: TerminalToNextTerminal '{terminal1}' > '{terminal2}'");

			gremlinCode = $@"
				g.V()
					.has('id', '{terminal2}')
					.addE('terminalToPrevTerminal')
					.property('distanceInMinutes', {distanceInMinutes})
					.to(
						g.V()
							.has('id', '{terminal1}'))
			";

			await client.SubmitAsync(gremlinCode);
			Console.WriteLine($"Created edge: TerminalToPrevTerminal '{terminal2}' > '{terminal1}'");
		}

	}
}
