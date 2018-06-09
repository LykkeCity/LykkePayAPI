namespace Lykke.Service.PayAPI.Core.Services
{
    public interface IAuthService
    {
        string CreateToken(string email, string employeeId, string merchantId);
    }
}
