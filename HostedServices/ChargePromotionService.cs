
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
            private bool shouldRun = true; // stops update from running more than once per day of new quarter

            public ScopedProcessingService(ILogger<ScopedProcessingService> logger,
                IBillRepository chargeRepository)
            {
                _logger = logger;
                _chargeRepository = chargeRepository;
            }

            /* Only handles periodically run logic to decide if charges should be promoted to next quarter.
             * All logic of *how* charges are promoted is handled through IBillRepository interface. 
             * There is also controller endpoint (BillsController.PromoteChargesToNewQuarter to accomplish this
             * if we decide to do this through a CronJob or some other scheme. */
            public async Task DoPromotions(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    /* Logic for deciding whether or not promoting charges to next quarter is appropriate */
                    DateTime now = DateTime.UtcNow; //Note that this is server time
                    ////if (now.Day == 1) //is it the first of the month?
                    ////{
                    ////    switch (now.Month)
                    ////    {
                    ////        case 1:
                    ////        case 4:
                    ////        case 7:
                    ////        case 10:
                    ////            break;
                    ////        default: 
                    ////            return;
                    ////    }
                    ////    if(shouldRun == true)
                    ////    {
                            _logger.LogInformation("It's a new quarter! Promoting fixed charges in repository to new quarter...");
                            shouldRun = false; //only do this once, on first day of quarter
                            await _chargeRepository.PromoteChargesToNewQuarter();
                    //    }
                        
                    //}
                    //else
                    //{   //if it's not the first of the month, just get ready for next time.
                    //    shouldRun = true;
                    //}
                    
                    _logger.LogInformation($"A quick check to see if quarterly charges need to be updated was done by the app hosted service at {DateTime.Now} (server time)");

                    await Task.Delay(5400_000, stoppingToken);
                }
            }
        }
    }
 }
