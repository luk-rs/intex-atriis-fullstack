using System.Net;
using System.Text.RegularExpressions;
using Atriis.Api.Products.Contracts;
using Atriis.Api.Products.Domain;
using Atriis.Api.Products.Services.HostedServices;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Atriis.Api.Products.Api;

[ApiController]
[Route("[controller]")]
#if DEBUG
[EnableCors]
#endif
public sealed class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IMemoryCache _cache;


    public ProductsController(ILogger<ProductsController> logger,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    [HttpGet(Name = "GetProducts")]
    public IActionResult Get([FromQuery] string? name, [FromQuery(Name = "type")] string[] types, [FromQuery] bool? sortPriceAscending,
        [FromQuery] bool? sortNameAscending)
    {
        if (!_cache.TryGetValue(BbProductsSynchronizer.SynchronizedProducts, out var fromCache))
            return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                "Still Synchronizing info from BestBuy, please try again later");

        if (fromCache is not IEnumerable<BbProduct> allProducts)
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Invalid set of BestBuy's product, please contact system administrator");

        var filtered = allProducts
            .Where(bbProduct =>
            {
                var nameRegex = new Regex($".*{name}.*", RegexOptions.IgnoreCase);
                return name == default || nameRegex.IsMatch(bbProduct.Name ?? string.Empty);
            })
            .Where(bbProduct => !types.Any() || types.Contains(bbProduct.Type))
            .Select(bbProduct => new Product(bbProduct));

        var sorted = Sort(
            Sort(filtered, sortNameAscending, e => e.Name),
            sortPriceAscending,
            e => e.Price
        );
        
        return Ok(sorted);

        IEnumerable<T> Sort<T,TS>(IEnumerable<T> source, bool? control, Func<T, TS> keySelector) => control switch
        {
            true => source.OrderBy(keySelector),
            false => source.OrderByDescending(keySelector),
            _ => source
        };
    }

    [HttpGet("types", Name = "GetSupportedTypes")]
    public IActionResult GetBbProductTypes()
        => Ok(Enum
            .GetValues<KnownBbProductType>()
            .Select(v => $"{v}")
        );
}