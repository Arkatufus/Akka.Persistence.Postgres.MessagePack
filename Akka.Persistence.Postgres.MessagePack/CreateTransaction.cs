namespace Akka.Persistence.Postgres.MessagePack;

[MessagePackObject]
public record CreateTransaction: IWithClientId
{
    [Key(0)]
    public int Value { get; set; }
    
    [Key(1)]
    public TransactionType Type { get; set; }
    
    [Key(2)]
    public string Description { get; set; } = string.Empty;
    
    [Key(3)]
    public string ClientId { get; init; } = null!;
    
    [Key(4)]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}