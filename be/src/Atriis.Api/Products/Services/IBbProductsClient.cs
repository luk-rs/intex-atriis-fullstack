using Atriis.Api.Products.Contracts;

namespace Atriis.Api.Products.Services;

internal interface IBbProductsClient
{
    IAsyncEnumerable<BbProduct> GetAllBbProducts();
}