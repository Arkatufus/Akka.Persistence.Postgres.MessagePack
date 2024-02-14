// -----------------------------------------------------------------------
//   <copyright file="TransactionType.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2024 .NET Petabridge, LLC
//   </copyright>
// -----------------------------------------------------------------------

namespace Akka.Persistence.Postgres.MessagePack;

public enum TransactionType
{
    Debit,
    Credit
}