using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryTimeShopify.Model
{
    public class Order
    {
        public string Id { get; set; }

        public string Mail { get; set; }

        public bool IsShipping { get; set; }

        public Address BillingAddress { get; set; }

        public Address ShippingAdress { get; set; }    

        public string TotalPrice { get; set; }

        public List<int> SKUs { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; }

        public string AdditionalShippingInfo { get; set; }

        public string AdditionalNote { get; set; }

        public override string ToString()
        {
            if (IsShipping)
                return ShippingAdress?.ToString();
            else
                return BillingAddress?.ToString();
        }
    }

    public class Address
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string StreetAndNr { get; set; }

        public string Zip { get; set; }

        public string City { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsEmpty
        {
            get
            {
                // Trim() because StreetAndNr is combined of 2 values likes this "value1 value2".
                // So if both values are null, it turns to " " and that is not string.Empty but actually it's empty, so Trim()
                // would remove this whitespace then (if necessary ofc)
                return string.IsNullOrEmpty(FirstName) &&
                       string.IsNullOrEmpty(LastName) &&
                       string.IsNullOrEmpty(StreetAndNr.Trim()) &&
                       string.IsNullOrEmpty(Zip) &&
                       string.IsNullOrEmpty(City);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FirstName);
            sb.Append(' ');
            sb.Append(LastName);
            sb.AppendLine();
            sb.Append(StreetAndNr);
            sb.AppendLine();
            sb.Append(City);
            sb.Append(' ');
            sb.Append(Zip);

            return sb.ToString().Trim();
        }
    }
}
