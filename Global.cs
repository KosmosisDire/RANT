namespace RantCore;

public static class Rant
{
    internal static readonly string IP = Environment.GetEnvironmentVariable("RantIP") ?? "localhost";
    internal static readonly RabbitConnection Connection = new RabbitConnection(IP);

    public static string[] GetAllTopicNames()
    {
        return Connection.GetQueueNames();
    }
}


