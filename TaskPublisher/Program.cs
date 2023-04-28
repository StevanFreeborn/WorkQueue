using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

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
  durable: false,
  exclusive: false,
  autoDelete: false,
  arguments: null
);

var message = (args.Length > 0)
? string.Join(" ", args)
: "Hello World!";

var body = Encoding.UTF8.GetBytes(message);

chan.BasicPublish(
  exchange: string.Empty,
  routingKey: queueName,
  basicProperties: null,
  body: body
);

Console.WriteLine($" [x] Sent {message}");