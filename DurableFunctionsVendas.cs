using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Company.Function
{

    public class Order
    {
        public string OrderID { get; set; }
        public decimal Amount { get; set; }

        public String Status { get; set; }

    }

    public static class DurableFunctionsVendas
    {
        [FunctionName("DurableFunctionsVendas")]
        public static async Task<Order> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            

            // Replace "hello" with the name of your Durable Activity Function.
            
            var a = await context.CallActivityAsync<Order>("PedidoEnviado", null);
            var b = await context.CallActivityAsync<Order>("EfetuarPagamento", a);
            var c = await context.CallActivityAsync<Order>("RealizarEntrega", b);

            return c;
        }

        [FunctionName("PedidoEnviado")]
        public static Order PedidoEnviado([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            Order order = new Order();
            order.OrderID = "PED10001290232";
            order.Amount = 120012;
            order.Status = "pedido enviado";
            log.LogInformation($"Pedido {order.OrderID} Status {order.Status}.");
            return order;
        }

        [FunctionName("EfetuarPagamento")]
        public static Order EfetuarPagamento([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var input = context.GetInput<Order>();
            input.Status="pagamento efetuado";
            log.LogInformation($"Pedido {input.OrderID} Status {input.Status}.");
            return input;
        }

         [FunctionName("RealizarEntrega")]
        public static Order RealizarEntrega([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var input = context.GetInput<Order>();
            input.Status="pagamento efetuado";
            log.LogInformation($"Pedido {input.OrderID} Status {input.Status}.");
            return input;
        }

        [FunctionName("DurableFunctionsVendas_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableFunctionsVendas", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}