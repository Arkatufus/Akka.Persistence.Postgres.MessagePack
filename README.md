# Akka.Persistence Sample

Technologies used in this example project:
* Akka.Hosting
* Akka.Cluster.Sharding
* Akka.Persistence.PostgreSql
* Akka.Serialization.MessagePack

Make sure that you've started the PostgreSql docker container before running this sample project:
```
docker run --name postgres-sample -p 5432:5432 -e POSTGRES_PASSWORD=postgres -d postgres
```

Once the example is running, you can control it by using these keys:
* D: Stops the persistence actor.
* X: Exits the demo application.
* Any other keys: Tell the persistence actor to persist a message.
