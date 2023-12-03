namespace Atriis.Api.Products.Contracts;

public class BbProductsResponse
{
    public int From { get; set; }
    public int To { get; set; }
    public int CurrentPage { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public string? QueryTime { get; set; }
    public string? TotalTime { get; set; }
    public bool Partial { get; set; }
    public string? CanonicalUrl { get; set; }
    public BbProduct[] Products { get; set; } = Array.Empty<BbProduct>();
}