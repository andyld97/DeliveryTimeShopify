using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryTimeShopify.Model
{
    public class IngoingMailAuth
    {
        [JsonPropertyName("imap_server")]
        public string ImapServer { get; set; }

        [JsonPropertyName("imap_port")]
        public int ImapPort { get; set; }

        [JsonPropertyName("mail_address")]
        public string MailAddress { get; set; }

        // Should be encrypted
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(ImapServer) &&
                               !string.IsNullOrEmpty(MailAddress) &&
                               !string.IsNullOrEmpty(Password) &&
                               ImapPort > 0; 

        public static IngoingMailAuth FromJsonElement(JsonElement element)
        {
            IngoingMailAuth ingoingMailAuth = new IngoingMailAuth();

            ingoingMailAuth.ImapServer = element.GetProperty("imap_server").GetString();
            ingoingMailAuth.ImapPort = element.GetProperty("imap_port").GetInt32();
            ingoingMailAuth.MailAddress = element.GetProperty("mail_address").GetString();
            ingoingMailAuth.Password = element.GetProperty("password").GetString();

            return ingoingMailAuth;
        }
    }
}