using RantCore.Topics;

public class SubTopicType<T>
{
    protected Topic<T> InternalTopic {get; private set;}

    public SubTopicType(string name)
    {
        InternalTopic = new Topic<T>(name);
    }

    public void BeginEcho()
    {
        InternalTopic.BeginEcho((subInfo, x) => $"Rate: {subInfo.hz}Hz\nLatency: {subInfo.latencyMs}ms\nValue:\n{x}\n\n");
    }
}