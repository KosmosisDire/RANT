namespace RantCore;

public class FieldTopic<T>
{
    protected Topic<T> internalTopic;
    protected T? _lastValue;
    public T? Value
    {
        get => GetValue();
        set
        {
            if (value is null || Equals(value, default(T?))) return;
            SetValue(value);
        }
    }

    public FieldTopic(string name, T? defaultValue = default)
    {
        internalTopic = new Topic<T>(name);
        _lastValue = defaultValue;
        Value = defaultValue;
    }

    public void SetValue(T value)
    {
        internalTopic.Publish(value);
        _lastValue = value;
    }

    public T? GetValue()
    {
        var val = internalTopic.GetLatestMessage();
        if (val is null || Equals(val, default(T?)))
        {
            val = _lastValue ?? default;
        }

        return val;
    }

    public void BeginEcho()
    {
        internalTopic.BeginEcho((subInfo, x) => $"Rate: {subInfo.hz}Hz\nLatency: {subInfo.latencyMs}ms\nField Value Modified:\n{x}\n\n");
    }
}