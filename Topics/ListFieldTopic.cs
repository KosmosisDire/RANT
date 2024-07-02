namespace RantCore;

public class ListFieldTopic<T> : FieldTopic<T>
{
    public ListFieldTopic(string name, T? defaultValue = default) : base(name, defaultValue) 
    {

    }
}