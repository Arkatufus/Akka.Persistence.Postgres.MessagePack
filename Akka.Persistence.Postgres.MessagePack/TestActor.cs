// -----------------------------------------------------------------------
//   <copyright file="TestActor.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2024 .NET Petabridge, LLC
//   </copyright>
// -----------------------------------------------------------------------

using Akka.Event;

namespace Akka.Persistence.Postgres.MessagePack;

public class TestActor: ReceivePersistentActor
{
    private static int _count;
    public override string PersistenceId { get; }
    private State _state = new ();
    private readonly ILoggingAdapter _log;
    
    public TestActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        _log = Context.GetLogger();

        Recover<SnapshotOffer>(snap =>
        {
            var state = (State) snap.Snapshot;
            _log.Info($"SnapshotOffer. Total: {state.Total}");
            _state = state;
        });
        
        Recover<CreateTransaction>(msg =>
        {
            Handle(msg);
            _log.Info($"Recover. Type: {msg.Type}, value: {msg.Value}, total: {_state.Total}");
        });
        
        Command<SaveSnapshotSuccess>(msg =>
        {
            _log.Info($"SaveSnapshot success. SeqNo: {msg.Metadata.SequenceNr}");
        });
        
        Command<CreateTransaction>(msg =>
        {
            Persist(msg, persisted =>
            {
                Handle(persisted);
                _log.Info($"Persisted. Type: {persisted.Type}, value: {persisted.Value}, total: {_state.Total}");
                _count++;
                if(_count % 10 == 0)
                {
                    _log.Info("Saving snapshot.");
                    SaveSnapshot(_state);
                }
            });
        });
        
        Command<Die>(_ =>
        {
            _log.Info($"Killing self: {Self.Path}");
            Context.Stop(Self);
        });
    }

    protected override void PreStart()
    {
        base.PreStart();
        _log.Info($"Actor started: {Self.Path}");
    }

    private void Handle(CreateTransaction transaction)
    {
        switch (transaction.Type)
        {
            case TransactionType.Credit:
                _state.Total -= transaction.Value;
                break;
            case TransactionType.Debit:
                _state.Total += transaction.Value;
                break;
            default:
                throw new IndexOutOfRangeException($"Unknown transaction type: {transaction.Type}");
        }
    }
}

[MessagePackObject]
public class State
{
    [Key(0)]
    public int Total { get; set; }
}

public class Die: IWithClientId
{
    public string ClientId { get; init; } = string.Empty;
}