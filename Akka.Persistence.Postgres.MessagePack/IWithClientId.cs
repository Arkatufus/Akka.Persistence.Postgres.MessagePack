namespace Akka.Persistence.Postgres.MessagePack;

public interface IWithClientId
{
    public string ClientId { get; init; }
}