using RantCore.Messages;

namespace Examples.Messages;

public record struct Test : IMessage
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///  this is the data in the message
    /// </summary>
    public string data;

    public Test(){}
}