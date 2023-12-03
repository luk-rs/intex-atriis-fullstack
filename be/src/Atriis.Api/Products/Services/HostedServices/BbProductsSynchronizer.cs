using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Atriis.Api.Products.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Atriis.Api.Products.Services.HostedServices;

internal sealed class BbProductsSynchronizer : IHostedService
{
    public const string SynchronizedProducts = nameof(SynchronizedProducts);

    private readonly ILogger<BbProductsSynchronizer> _logger;
    private readonly IBbProductsClient _bbClient;
    private readonly IMemoryCache _cache;
    private IDisposable? _subscription;


    public BbProductsSynchronizer(ILogger<BbProductsSynchronizer> logger, IBbProductsClient bbClient, IMemoryCache cache)
    {
        _logger = logger;
        _bbClient = bbClient;
        _cache = cache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscription = Observable
            .Timer(TimeSpan.Zero, 
                //uncomment for a realistic wait, dependant upon removing [short-circuit] on IBbProductsClient GetAllBbProducts() method
                // retryTimeSpan.FromHours(2),
                TimeSpan.FromMinutes(5), 
                Scheduler.Default)
            .Select(_ => TryRefreshCacheAsync().ToObservable())
            .Concat()
            .Subscribe(
                success =>
                {
                    if (success)
                        _logger.LogInformation("Refreshed best buy products list successfully at {}", DateTime.UtcNow);
                    else
                        _logger.LogWarning("Could not refresh best buy products list at {}", DateTime.UtcNow);
                }
            );

        return Task.CompletedTask;
    }

    private async Task<bool> TryRefreshCacheAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            ICollection<BbProduct> accumulated = new List<BbProduct>();
            await foreach (var bbProduct in _bbClient.GetAllBbProducts())
                accumulated.Add(bbProduct);
            sw.Stop();
            _logger.LogInformation("BestBuy synchronized in {}", sw.Elapsed);
            
            _cache.Set(SynchronizedProducts, accumulated);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while trying to reconcile BestBuy's available products");
            return false;
        }

        return true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        return Task.CompletedTask;
    }
}