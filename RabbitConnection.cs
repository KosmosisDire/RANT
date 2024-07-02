using EasyNetQ.Management.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

public class RabbitConnection
{
    internal readonly IModel channel;
    internal readonly IConnection connection;
    internal readonly ManagementClient management;

    public RabbitConnection(string ip)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = ip };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            management = new ManagementClient(new Uri($"http://{ip}:15672"), "guest", "guest");
            
        }
        catch (Exception)
        {
            throw new ConnectFailureException("Could not connect to RabbitMQ at " + ip, null);
        }
    }

    public void Publish(string queueName, ReadOnlyMemory<byte> data)
    {
        channel.QueueDeclare(queueName);
        channel.BasicPublish(string.Empty, queueName, null, data);
    }

    public EventingBasicConsumer Subscribe(string queueName, Action<byte[]> callback)
    {
        var consumer = Subscribe(queueName);
        consumer.Received += (_, ea) => callback(ea.Body.ToArray());
        return consumer;
    }

    public EventingBasicConsumer Subscribe(string queueName)
    {
        var consumer = new EventingBasicConsumer(channel);
        channel.QueueDeclare(queueName);
        channel.BasicConsume(queueName, true, consumer);
        return consumer;
    }

    public void DestroyQueue(string queueName)
    {
        channel.QueueDelete(queueName, true, true);
    }

    public string[] GetQueueNames()
    {
        var channels = management.GetQueues();
        return channels.Select(x => x.Name).ToArray();
    }
}