namespace Akka.Persistence.Postgres.MessagePack;

public class MessageExtractor: HashCodeMessageExtractor
{
    public MessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string? EntityId(object message)
    {
        return message is IWithClientId msg ? msg.ClientId : null;
    }
}