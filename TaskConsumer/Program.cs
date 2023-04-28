using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var config = new ConfigurationBuilder()
.AddJsonFile($"appsettings.development.json")
.Build();

var rabbitMQConfig = config.GetSection("RabbitMQ");

var factory = new ConnectionFactory
{
  HostName = "localhost",
  Port = int.Parse(rabbitMQConfig.GetSection("Port").Value ?? string.Empty),
  UserName = rabbitMQConfig.GetSection("Username").Value,
  Password = rabbitMQConfig.GetSection("Password").Value
};

using var conn = factory.CreateConnection();
using var chan = conn.CreateModel();

var queueName = rabbitMQConfig.GetSection("MessageQueue").Value;

chan.QueueDeclare(
  queue: queueName,
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: null
);

chan.BasicQos(
  prefetchSize: 0,
  prefetchCount: 1,
  global: false
);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(chan);

consumer.Received += (model, ea) =>
{
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);

  Console.WriteLine($" [x] Received {message}");

  int dots = message.Split('.').Length - 1;

  Thread.Sleep(dots * 1000);

  Console.WriteLine(" [x] Done");

  chan.BasicAck(
    deliveryTag: ea.DeliveryTag,
    multiple: false
  );
};

chan.BasicConsume(
  queue: queueName,
  autoAck: false,
  consumer: consumer
  );

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();