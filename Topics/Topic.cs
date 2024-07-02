using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RantCore.Messages;
namespace RantCore;

public class TopicSubInfo()
{
    public float latencyMs = 0;
    public float hz = 0;
    public DateTime lastReceivedTime = DateTime.MinValue;
}

public class Topic<T>
{
    protected static readonly XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
    protected static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
    private static readonly XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
    protected static readonly XmlSerializer serializer = new XmlSerializer(typeof(T));
    public readonly string name;

    public Topic(string name)
    {
        ns.Add("", "");
        xmlWriterSettings.OmitXmlDeclaration = true;
        xmlWriterSettings.Indent = true;
        this.name = name;
    }

    public void Publish(T message)
    {
        try
        {
            var body = Serialize(message);
            Rant.Connection.Publish(name, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not publish message in topic " + name + ":\n" + ex.Message);
        }
    }

    protected byte[] Serialize(T message)
    {
        MemoryStream stream = new MemoryStream();
        var writer = XmlWriter.Create(stream, xmlWriterSettings);
        try
        {
            serializer.Serialize(writer, message, ns, "", "");
            writer.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not serialize message in topic " + name + ":\n Make sure the type is serializable.\n" + ex.Message);
        }
        return stream.ToArray().Take((int)stream.Position).ToArray();
    }

    protected T? Deserialize(byte[] data)
    {
        T? obj = default;
        try
        {
            var stream = new MemoryStream(data);
            var reader = XmlReader.Create(stream, xmlReaderSettings);
            obj = (T?) serializer.Deserialize(reader);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Could not deserialize message in topic {name}, \n{Encoding.UTF8.GetString(data)}\n{string.Join(',', data)}\n{ex}\n");
            Console.ResetColor();
            return default;
        }

        return obj;
    }

    public TopicSubInfo Subscribe(Action<T> callback)
    {
        var info = new TopicSubInfo();
        var consumer = Rant.Connection.Subscribe(name, (b) => 
        {
            var obj = Deserialize(b);
            if (obj is null || Equals(obj, default(T?)))
            {
                return;
            }

            if (info.lastReceivedTime != DateTime.MinValue)
            {
                info.hz = (float)(1f / (DateTime.UtcNow - info.lastReceivedTime).TotalSeconds);
            }

            if (obj is IMessage msg)
            {
                info.latencyMs = (float)(DateTime.UtcNow - msg.Timestamp).TotalMilliseconds;
            }

            info.lastReceivedTime = DateTime.UtcNow;
            callback(obj);
        });

        return info;
    }

    public TopicSubInfo SubscribeXML(Action<string> callback, bool measureLatency = false)
    {
        var info = new TopicSubInfo();
        Rant.Connection.Subscribe(name, (b) => 
        {
            if (info.lastReceivedTime != DateTime.MinValue)
            {
                info.hz = (float)(1f / (DateTime.UtcNow - info.lastReceivedTime).TotalSeconds);
            }

            if (measureLatency)
            {
                var obj = Deserialize(b);
                if (obj is IMessage msg)
                {
                    info.latencyMs = (float)(DateTime.UtcNow - msg.Timestamp).TotalMilliseconds;
                }
            }

            info.lastReceivedTime = DateTime.UtcNow;

            callback(Encoding.UTF8.GetString(b));
        });
        return info;
    }

    public T? GetLatestMessage()
    {
        var data = Rant.Connection.channel.BasicGet(name, true)?.Body.ToArray();
        if (data is null) return default(T?);
        return Deserialize(data);
    }

    public void BeginEcho(Func<TopicSubInfo, string, string> formatFunction)
    {
        var subInfo = new TopicSubInfo();
        subInfo = SubscribeXML(x => 
        {
            Console.WriteLine(formatFunction(subInfo, x));
        }, measureLatency: true);
    }

    public virtual void BeginEcho()
    {
        static string formatFunction(TopicSubInfo subInfo, string x) =>
                $"Rate: {subInfo.hz}Hz\nLatency: {subInfo.latencyMs}ms\nMessage:\n{x}\n\n";

        BeginEcho(formatFunction);
    }

    
}