using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryTimeShopify.Model
{
    public class Config
    {
        private static string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static Config Instance = LoadConfig();

        [JsonPropertyName("ingoing_mail_auth")]
        public IngoingMailAuth IngoingMailAuth { get; set; } = new IngoingMailAuth();

        [JsonPropertyName("outgoing_mail_auth")]
        public OutgoingMailAuth OutgoingMailAuth { get; set; } = new OutgoingMailAuth();

        [JsonPropertyName("filter")]
        public List<string> Filter { get; set; } = new List<string>();

        [JsonPropertyName("webhook_url")]
        public string WebHookUrl { get; set; }

        [JsonIgnore]
        public bool IsConfigValid => IngoingMailAuth != null && OutgoingMailAuth != null &&
                                     IngoingMailAuth.IsValid && OutgoingMailAuth.IsValid &&
                                     Filter.Count > 0;                                

        private static Config LoadConfig()
        {
            try
            {
                var configDocument = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(System.IO.File.ReadAllText(configPath));
                Config result = new Config();
                var filterProperty = configDocument.RootElement.GetProperty("filter");
                result.Filter = new List<string>(filterProperty.GetArrayLength());
                foreach (var word in filterProperty.EnumerateArray())
                    result.Filter.Add(word.GetString().ToLower());

                result.OutgoingMailAuth = OutgoingMailAuth.FromJsonElement(configDocument.RootElement.GetProperty("outgoing_mail_auth"));
                result.IngoingMailAuth = IngoingMailAuth.FromJsonElement(configDocument.RootElement.GetProperty("ingoing_mail_auth"));
                result.WebHookUrl = configDocument.RootElement.GetProperty("webhook_url").GetString();

                return result;
            }
            catch
            {
                return new Config();
            }
        }
    }
}