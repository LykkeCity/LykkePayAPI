namespace Lykke.Service.PayAPI.Models
{
    public class AuthResponseModel
    {
        public string Token { get; set; }
        public bool ForcePasswordUpdate { get; set; }
    }
}
