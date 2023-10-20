using Service_Billing.Controllers;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
namespace Service_Billing.HostedServices
{
    public class ChargePromotionService : BackgroundService
    {
        private readonly IBillRepository _billRepository;
        private readonly ILogger<ChargePromotionService> _logger;
        public IServiceProvider Services { get; }

        public ChargePromotionService(IServiceProvider services)
        {
            Services = services;
        }

        public async Task DoPromotions(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                        scope.ServiceProvider
                                .GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.DoPromotions(stoppingToken);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoPromotions(stoppingToken);
        }

        internal interface IScopedProcessingService
        {
            Task DoPromotions(CancellationToken stoppingToken);
        }

        internal class ScopedProcessingService : IScopedProcessingService
        {
            private int executionCount = 0;
            private readonly ILogger _logger;
            private readonly IBillRepository _chargeRepository;

            public ScopedProcessingService(ILogger<ScopedProcessingService> logger,
                IBillRepository chargeRepository)
            {
                _logger = logger;
                _chargeRepository = chargeRepository;
            }

            public async Task DoPromotions(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("do promotion");

                    await Task.Delay(5_000, stoppingToken);
                }
            }
        }
    }
 }
