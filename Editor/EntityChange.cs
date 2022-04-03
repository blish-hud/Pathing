using System;
using LiteDB;

namespace BhModule.Community.Pathing.Editor {
    public readonly struct EntityChange {

        public ObjectId Id { get; }

        public Guid PathableId { get; }

        public DateTime TimeStamp { get; }

        public string Attribute { get; }

        public string OldValue { get; }

        public string NewValue { get; }

    }
}
