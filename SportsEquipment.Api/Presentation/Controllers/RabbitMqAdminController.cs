using System.Net;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using SportsEquipment.Application.DTOs.Rabbit;

namespace SportsEquipment.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/rabbitmq")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [SwaggerTag("Administração do RabbitMQ (Apenas para Desenvolvimento/Debug)")]
    public class RabbitMqAdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqAdminController> _logger;

        public RabbitMqAdminController(IConfiguration configuration, ILogger<RabbitMqAdminController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Limpa as filas relacionadas a pedidos (ordem e erro)
        /// </summary>
        [HttpPost("queues/orders/purge")]
        [Authorize(Roles = "Administrator")]
        [SwaggerOperation(Summary = "Limpa todas as filas relacionadas a pedidos")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Filas de pedidos limpas com sucesso")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão")]
        public async Task<IActionResult> PurgeOrderQueuesAsync()
        {
            var orderQueues = new[] { "order-created-queue", "order-created-queue_error" };

            try
            {
                var factory = CreateConnectionFactory();
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                var results = new List<QueuePurgeResultDto>();

                foreach (var queueName in orderQueues)
                {
                    try
                    {
                        uint purgedCount = 0;
                        string statusMessage;

                        try
                        {
                            // Tenta verificar se a fila existe
                            var queueDeclareOk = await channel.QueueDeclarePassiveAsync(queueName);
                            var messageCount = (uint)queueDeclareOk.MessageCount;

                            if (messageCount > 0)
                            {
                                purgedCount = await channel.QueuePurgeAsync(queueName);
                                statusMessage = $"{purgedCount} mensagens removidas";
                                _logger.LogWarning("Fila de pedidos {QueueName} limpa. {PurgedCount} mensagens removidas",
                                    queueName, purgedCount);
                            }
                            else
                            {
                                statusMessage = "Fila já está vazia";
                            }
                        }
                        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
                        {
                            statusMessage = "Fila não existe (nunca foi criada)";
                            _logger.LogInformation("Fila {QueueName} não existe ainda", queueName);
                        }
                        catch
                        {
                            statusMessage = "Erro ao verificar fila";
                        }

                        results.Add(new QueuePurgeResultDto
                        {
                            QueueName = queueName,
                            Success = true,
                            PurgedMessages = purgedCount,
                            Message = statusMessage
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new QueuePurgeResultDto
                        {
                            QueueName = queueName,
                            Success = false,
                            PurgedMessages = 0,
                            Message = $"Erro: {ex.Message}"
                        });
                        _logger.LogError(ex, "Erro ao limpar fila de pedidos {QueueName}", queueName);
                    }
                }

                return Ok(new
                {
                    Message = "Filas de pedidos processadas",
                    Results = results,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar filas de pedidos");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        Error = "Erro ao conectar ao RabbitMQ",
                        Details = ex.Message,
                        Help = "Verifique se o serviço RabbitMQ está rodando e acessível"
                    });
            }
        }

        /// <summary>
        /// Verifica a saúde da conexão com o RabbitMQ
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Verifica a saúde da conexão com RabbitMQ")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Conexão saudável")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Conexão com problemas")]
        public async Task<IActionResult> HealthCheckAsync()
        {
            try
            {
                var factory = CreateConnectionFactory();
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                // Criar e deletar uma fila temporária
                var testQueueName = $"health-check-{Guid.NewGuid():N}";

                await channel.QueueDeclareAsync(
                    queue: testQueueName,
                    durable: false,
                    exclusive: true,
                    autoDelete: true,
                    arguments: null);

                await channel.QueueDeleteAsync(testQueueName);

                return Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Message = "Conexão com RabbitMQ está funcionando corretamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no health check do RabbitMQ");
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    new
                    {
                        Status = "Unhealthy",
                        Error = ex.Message,
                        Timestamp = DateTime.UtcNow
                    });
            }
        }

        /// <summary>
        /// Obtém informações sobre as filas de pedidos
        /// </summary>
        [HttpGet("queues/orders")]
        [Authorize(Roles = "Administrator,Seller")]
        [SwaggerOperation(Summary = "Obtém informações sobre as filas de pedidos")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Informações obtidas com sucesso")]
        public async Task<IActionResult> GetOrderQueuesInfoAsync()
        {
            try
            {
                var factory = CreateConnectionFactory();
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                var queueInfos = new List<QueueInfoDto>();

                foreach (var queueName in new[] { "order-created-queue", "order-created-queue_error" })
                {
                    try
                    {
                        // Usar QueueDeclare com passive: true para verificar se existe
                        var queueDeclareOk = await channel.QueueDeclarePassiveAsync(queueName);
                        queueInfos.Add(new QueueInfoDto
                        {
                            Name = queueName,
                            MessageCount = (uint)queueDeclareOk.MessageCount,
                            ConsumerCount = (uint)queueDeclareOk.ConsumerCount,
                            Exists = true
                        });
                    }
                    catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
                    {
                        // Fila não existe (código 404) - isso é normal
                        queueInfos.Add(new QueueInfoDto
                        {
                            Name = queueName,
                            MessageCount = 0,
                            ConsumerCount = 0,
                            Exists = false
                        });
                    }
                    catch (Exception ex)
                    {
                        // Outro tipo de erro
                        _logger.LogWarning(ex, "Erro ao verificar fila {QueueName}", queueName);
                        queueInfos.Add(new QueueInfoDto
                        {
                            Name = queueName,
                            MessageCount = 0,
                            ConsumerCount = 0,
                            Exists = false,
                            Error = ex.Message
                        });
                    }
                }

                return Ok(new
                {
                    Queues = queueInfos,
                    Timestamp = DateTime.UtcNow,
                    Note = "Fila de erro só é criada quando ocorre um erro no processamento"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações das filas");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        Error = "Erro ao obter informações das filas",
                        Details = ex.Message
                    });
            }
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            var rabbitMqUri = _configuration["RabbitMq:Uri"] ?? "rabbitmq://localhost";
            var rabbitMqUser = _configuration["RabbitMq:User"] ?? "guest";
            var rabbitMqPassword = _configuration["RabbitMq:Password"] ?? "guest";

            var uri = new Uri(rabbitMqUri);
            var host = uri.Host;
            var port = uri.Port == -1 ? 5672 : uri.Port;

            return new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = rabbitMqUser,
                Password = rabbitMqPassword,
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                SocketReadTimeout = TimeSpan.FromSeconds(30),
                SocketWriteTimeout = TimeSpan.FromSeconds(30)
            };
        }
    }
}