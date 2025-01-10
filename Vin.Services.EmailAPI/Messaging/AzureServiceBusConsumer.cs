using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Vin.Services.EmailAPI.Models.Dto;

namespace Vin.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {

        private readonly string serviceBusConnnectionString;
        private readonly string emailCartQueue;
        //private readonly ILogger<AzureServiceBusConsumer> logger;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _emailCartProcessor;

        public AzureServiceBusConsumer(/*ILogger<AzureServiceBusConsumer> logger,*/ IConfiguration configuration)
        {
            //this.logger = logger;
            _configuration = configuration;
            serviceBusConnnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            emailCartQueue = _configuration.GetValue<string>("EmailCartQueue");
            var client = new ServiceBusClient(serviceBusConnnectionString);
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDTO objMessage = JsonConvert.DeserializeObject<CartDTO>(body);
            try
            {
                //TODO - try to log eamil
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task Stop()
        {

            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
