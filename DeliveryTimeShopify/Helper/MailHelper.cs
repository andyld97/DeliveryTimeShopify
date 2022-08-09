using DeliveryTimeShopify.Model;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace DeliveryTimeShopify.Helper
{
    public static class MailHelper
    {
        public static async Task SendMailAsync(TimeSpan timeSpan, Order order, OutgoingMailAuth outgoingMailAuth)
        {
            var stream = typeof(Order).Assembly.GetManifestResourceStream("DeliveryTimeShopify.resources.template.message.html");
            string htmlTemplate = string.Empty;

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                htmlTemplate = await reader.ReadToEndAsync();
            }

            htmlTemplate = htmlTemplate.Replace("{{ shop.email }}", outgoingMailAuth.MailAddress);

            string messageContent;
            string messageSubject;
            string timeText = string.Empty;

            if (timeSpan.TotalMinutes >= 60)
                timeText = $"{FormatTimeNumber(timeSpan.Hours)}:{FormatTimeNumber(timeSpan.Minutes)}h";
            else
                timeText = $"{FormatTimeNumber((int)timeSpan.TotalMinutes)} Minuten";

            if (order.IsShipping)
            {
                messageContent = $"Ihre Bestellung wird in {timeText} (ca. {order.CreatedAt.Add(timeSpan):t} Uhr) geliefert.";
                messageSubject = $"Ihre Bestellung von {outgoingMailAuth.DisplayName} wird in {timeText} geliefert.";
            }
            else
            {
                messageContent = $"Ihre Bestellung kann in {timeText} (ca. {order.CreatedAt.Add(timeSpan):t} Uhr) abgeholt werden.";
                messageSubject = $"Ihre Bestellung von {outgoingMailAuth.DisplayName} kann in {timeText} abgeholt werden.";
            }

            htmlTemplate = htmlTemplate.Replace("{{ shipping_time }}", messageContent);

            using (SmtpClient client = new SmtpClient())
            {
                if (outgoingMailAuth.SmtpPort == 25)
                    client.Connect(outgoingMailAuth.SmtpServer, 25, options: MailKit.Security.SecureSocketOptions.None);
                else
                    client.Connect(outgoingMailAuth.SmtpServer, outgoingMailAuth.SmtpPort);

                client.Authenticate(outgoingMailAuth.MailAddress, outgoingMailAuth.Password);

                var messageToSend = new MimeMessage
                {
                    Sender = new MailboxAddress(outgoingMailAuth.DisplayName, outgoingMailAuth.MailAddress),
                    Subject = messageSubject
                };

                BodyBuilder bodyBuilder = new BodyBuilder { HtmlBody = htmlTemplate };

                messageToSend.Body = bodyBuilder.ToMessageBody();


                // test recepiants
#if DEBUG
                messageToSend.To.Add(new MailboxAddress("Floris John", "floris.john97@yahoo.de"));
                messageToSend.To.Add(new MailboxAddress("Andreas Leopold", "andreasleopold97@gmail.com"));
#else
                messageToSend.To.Add(new MailboxAddress((invoice.IsShipping ? invoice.ShippingAdress.FullName : invoice.BillingAddress.FullName), invoice.Mail));
#endif

                messageToSend.From.Add(new MailboxAddress(outgoingMailAuth.DisplayName, outgoingMailAuth.MailAddress));
                messageToSend.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:91.0) Gecko/20100101 Thunderbird/91.11.0");

                await client.SendAsync(messageToSend);
            }
        }

        public static Order ParseMail(string json)
        {
            try
            {
                var d = JObject.Parse(json);

                Order order = new Order();
                order.AdditionalNote = d["note"].Value<string>();
                order.CreatedAt = DateTime.Parse($"{d["current_date"].Value<string>()} {d["current_time"]}");
                order.Mail = d["email"].Value<string>();
                order.Id = d["id"].Value<string>();
                order.TotalPrice = d["total_price"].Value<string>();

                bool requires_shipping = bool.Parse(d["requires_shipping"].Value<string>());
                order.IsShipping = requires_shipping;

                if (!requires_shipping)
                    order.BillingAddress = new Address() { FirstName = d["customer.name"].Value<string>() };
                else
                {
                    order.ShippingAdress = new Address()
                    {
                        FirstName = d["customer.name"].Value<string>(),
                        StreetAndNr = d["shipping_address.street"].Value<string>(),
                        City = d["shipping_address.city"].Value<string>(),
                        Zip = d["shipping_address.zip"].Value<string>(),
                    };
                }

                if (d.ContainsKey("skus"))
                {
                    string skus = d["skus"].Value<string>();

                    foreach (var sku in skus.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(sku, out int skuNumber))
                            order.SKUs.Add(skuNumber);
                    }
                }

                return order;
            }
            catch
            {
                // ToDo: *** Log
                return null;
            }
        }

        private static string FormatTimeNumber(int number)
        {
            if (number < 10)
                return $"0{number}";
            else
                return number.ToString();
        }
    }
}