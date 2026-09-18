using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Messaging;


// Controlador de Broker ActiveMQ para enviar mensajes a colas específicas. Se utiliza para la comunicación entre servicios en un sistema distribuido.
// Solo se registra cuando Messaging:ActiveMqBrokerUri está configurado (p. ej. tcp://activemq:61616).
public class ActiveMqProducer(string brokerUri, ILogger<ActiveMqProducer> logger)
{
    private readonly string _brokerUri = brokerUri;

    // Envia un mensaje persistente a una cola de ActiveMQ. 
    // name="queueName">Nombre de la cola.
    // name="message">Contenido del mensaje.
    public void Send(string queueName, string message)
    {
        var factory = new ConnectionFactory(_brokerUri);

        using IConnection connection = factory.CreateConnection();

        connection.Start();

        using Apache.NMS.ISession session = connection.CreateSession();

        IDestination destination = session.GetQueue(queueName);

        using IMessageProducer producer = session.CreateProducer(destination);

        producer.DeliveryMode = MsgDeliveryMode.Persistent;

        ITextMessage textMessage = producer.CreateTextMessage(message);

        producer.Send(textMessage);

        logger.LogInformation("[ActiveMQ] Message sent to queue: {QueueName}", queueName);
    }
}
