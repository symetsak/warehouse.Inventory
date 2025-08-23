namespace BlazorClient.Services
{
    public interface IBarcodeService
    {
        Task<bool> CreateAsync(int productId, string code, string type = "CODE128", bool isPrimary = true);
    }

}
