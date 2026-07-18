namespace IslaNova.Core.Application.Features.Property.Events
{
    public enum VectorEventType
    {
        Created,
        Updated,
        Deleted
    }

    /// <summary>
    /// Represents a property lifecycle event that triggers a vector store sync.
    /// Published by Command Handlers and consumed by PropertyVectorSyncService (BackgroundService).
    /// </summary>
    public class PropertyVectorEvent
    {
        public VectorEventType EventType { get; set; }
        public int PropertyId { get; set; }

        /// <summary>Timestamp when the event was published.</summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
