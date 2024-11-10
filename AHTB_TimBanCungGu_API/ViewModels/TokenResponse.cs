using System;
using System.Text.Json.Serialization;
namespace AHTB_TimBanCungGu_API.ViewModels
{


    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }  // Đảm bảo ánh xạ chính xác

        [JsonPropertyName("expiration")]
        public DateTime Expiration { get; set; }  // Đảm bảo ánh xạ chính xác
    }
}
