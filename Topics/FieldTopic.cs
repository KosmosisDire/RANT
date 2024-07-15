namespace RantCore.Topics;

public class FieldTopic<T> : SubTopicType<T>
{
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

    public FieldTopic(string name, T? defaultValue = default) : base(name)
    {
        _lastValue = defaultValue;
        Value = defaultValue;
    }

    public void SetValue(T value)
    {
        InternalTopic.Publish(value);
        _lastValue = value;
    }

    public T? GetValue()
    {
        var val = InternalTopic.GetLatestMessage();
        if (val is null || Equals(val, default(T?)))
        {
            val = _lastValue ?? default;
        }

        return val;
    }

}