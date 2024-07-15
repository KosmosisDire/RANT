namespace RantCore.Messages;

public record struct Empty : IMessage
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Empty(){}
}
