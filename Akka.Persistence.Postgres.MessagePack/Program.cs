namespace Akka.Persistence.Postgres.MessagePack;

public static class Program
{
    public static async Task Main(string[] args)
    {
        const string connString = 
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres;";
        
        var host = new HostBuilder()
            .ConfigureLogging(builder =>
            {
                builder.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddAkka("test", (builder, provider) =>
                {
                    builder
                        .WithClustering()
                        .AddHocon("""
                                  akka {
                                    actor {
                                      serializers {
                                        messagepack = "Akka.Serialization.MessagePack.MsgPackSerializer, Akka.Serialization.MessagePack"
                                      }
                                      serialization-bindings {
                                        "System.Object" = messagepack
                                      }
                                    }
                                  }
                                  """, HoconAddMode.Prepend)
                        .AddHocon(ClusterSingletonManager.DefaultConfig(), HoconAddMode.Append)
                        .WithPostgreSqlPersistence(
                            journal =>
                            {
                                journal.ConnectionString = connString;
                                journal.Serializer = "messagepack";
                                journal.AutoInitialize = true;
                            },
                            snapshot =>
                            {
                                snapshot.ConnectionString = connString;
                                snapshot.Serializer = "messagepack";
                                snapshot.AutoInitialize = true;
                            })
                        .WithShardRegion<TestActor>(
                            "TestActor", 
                            id => Props.Create(() => new TestActor(id)),
                            new MessageExtractor(10), 
                            new ShardOptions())
                        .AddStartup((sys, registry) =>
                        {
                            var cluster = Cluster.Cluster.Get(sys);
                            cluster.Join(cluster.SelfAddress);
                        });
                });
            }).Build();
        
        Console.WriteLine("Starting up");
        await host.StartAsync();

        var rnd = new Random();
        var sys = host.Services.GetRequiredService<ActorSystem>();
        
        Console.WriteLine("Acquiring shard region");
        var registry = ActorRegistry.For(sys);
        var actorRef = await registry.GetAsync<TestActor>();

        var running = true;
        Console.WriteLine("Running. X to exit, D to kill persistence actor, any key to persist a message.");
        while (running)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.D:
                    actorRef.Tell(new Die
                    {
                        ClientId = "test"
                    });
                    break;
                case ConsoleKey.X:
                    running = false;
                    break;
                default:
                    var message = new CreateTransaction
                    {
                        ClientId = "test",
                        Description = "blah",
                        Type = rnd.NextDouble() >= 0.5 ? TransactionType.Debit : TransactionType.Credit,
                        Value = rnd.Next(1, 1000)
                    };
                    actorRef.Tell(message);
                    break;
            }
        }

        await host.StopAsync();
    }
}