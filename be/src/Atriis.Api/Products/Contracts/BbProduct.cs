namespace Atriis.Api.Products.Contracts;

public sealed class BbProduct
{
    public int Sku { get; set; }
    public string? Name { get; set; }
    public double SalePrice { get; set; }
    public string? ThumbnailImage { get; set; }
    public string? Type { get; set; }
}