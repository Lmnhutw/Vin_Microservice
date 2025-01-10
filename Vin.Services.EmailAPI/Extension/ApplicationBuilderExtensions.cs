using Vin.Services.EmailAPI.Messaging;

namespace Vin.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            /*var lifeTime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            lifeTime.ApplicationStarted.Register(async () => { await serviceBusConsumer.Start(); });
            lifeTime.ApplicationStopping.Register(async () => { await serviceBusConsumer.Stop(); });
            return app;*/

            var hostApplicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);

            return app;
        }

        private static void OnStopping()
        {
            ServiceBusConsumer.Stop();
        }

        private static void OnStarted()
        {
            ServiceBusConsumer.Start();
        }
    }
}
