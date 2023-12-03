using Atriis.Api.Products.Contracts;

namespace Atriis.Api.Products.Domain;

public sealed class Product
{
    private readonly BbProduct _bbProduct;

    public Product(BbProduct bbProduct)
    {
        _bbProduct = bbProduct;
    }


    public int Sku => _bbProduct.Sku;
    public string? Name => _bbProduct.Name;
    public double Price => _bbProduct.SalePrice;
    public string? ThumbnailImage => _bbProduct.ThumbnailImage;
    public string? Type => _bbProduct.Type;
}