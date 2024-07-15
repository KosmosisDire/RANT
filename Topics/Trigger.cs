using RantCore.Messages;

namespace RantCore.Topics;

public class Trigger : SubTopicType<Empty>
{
    public event Action<DateTime> OnTrigger;

    public Trigger(string name) : base(name)
    {
        InternalTopic.Subscribe((msg) => OnTrigger?.Invoke(msg.Timestamp));
    }

    public void Invoke()
    {
        InternalTopic.Publish(new Empty());
    }
    
}