using System.Net;
using System.Text.Json;
using Atriis.Api.Products.Contracts;
using Polly;
using Polly.Retry;

namespace Atriis.Api.Products.Services.Implementations;

internal sealed class BbProductsClient : IBbProductsClient
{
    private static readonly Dictionary<string, string> DefaultQueryParams = new()
    {
        { "format", "json" },
        { "page", "1" },
        { "show", "sku,name,salePrice,thumbnailImage,type" },
        { "apiKey", "VEu4DRF1Wwgl54oI4TerpOTq" },
    };

    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _overQuotaPolicy;


    public BbProductsClient(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("best-buy-products");
        _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        _overQuotaPolicy = Policy.HandleResult<HttpResponseMessage>(res =>
        {
            if (res.StatusCode != HttpStatusCode.Forbidden) 
                return false;
            
            var error = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return error.Contains("The provided API Key has reached the per second limit allotted to it.");

        }).WaitAndRetryAsync(1, _ => TimeSpan.FromSeconds(1));
    }

    public async IAsyncEnumerable<BbProduct> GetAllBbProducts()
    {
        BbProductsResponse? bbProductsResponse = default;
        do
        {
            var currentPage = bbProductsResponse?.CurrentPage ?? 0;
            bbProductsResponse = await GetBbProductsPage(currentPage + 1);

            foreach (var product in bbProductsResponse.Products)
            {
                yield return product;
            }
        } while (
            bbProductsResponse.CurrentPage != bbProductsResponse.TotalPages 
            && bbProductsResponse.CurrentPage < 50 // [short-circuit] I'm adding a stop condition here so it can hand fairly quickly 
        );

        Console.WriteLine();
    }

    private async Task<BbProductsResponse> GetBbProductsPage(int page)
    {
        var updatedQuery = new Dictionary<string, string>(DefaultQueryParams)
        {
            ["page"] = $"{page}"
        };
        var queryString = string.Join("&", updatedQuery.Select(p => $"{p.Key}={p.Value}"));

        var response = await _overQuotaPolicy.ExecuteAsync(
            async () => await _http.GetAsync($"/v1/products?{queryString}")
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error while paging BestBuy's product list {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        var bbProductsResponse = JsonSerializer.Deserialize<BbProductsResponse>(content, _serializationOptions)
                                 ?? throw new Exception("Cannot deserialize products from best buy api");

        return bbProductsResponse;
    }
}