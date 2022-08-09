using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryTimeShopify.Model
{
    public class OutgoingMailAuth
    {
        [JsonPropertyName("smtp_server")]
        public string SmtpServer { get; set; }

        [JsonPropertyName("smtp_port")]
        public int SmtpPort { get; set; }

        [JsonPropertyName("mail_address")]
        public string MailAddress { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        // Should be encrypted
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(SmtpServer) &&
                               !string.IsNullOrEmpty(MailAddress) &&
                               !string.IsNullOrEmpty(Password) &&
                               !string.IsNullOrEmpty(DisplayName) &&
                               SmtpPort > 0;

        public static OutgoingMailAuth FromJsonElement(JsonElement element)
        {
            OutgoingMailAuth outgoingMailAuth = new OutgoingMailAuth();

            outgoingMailAuth.SmtpServer = element.GetProperty("smtp_server").GetString();
            outgoingMailAuth.SmtpPort = element.GetProperty("smtp_port").GetInt32();
            outgoingMailAuth.MailAddress = element.GetProperty("mail_address").GetString();
            outgoingMailAuth.Password = element.GetProperty("password").GetString();
            outgoingMailAuth.DisplayName = element.GetProperty("display_name").GetString();

            return outgoingMailAuth;
        }
    }
}
