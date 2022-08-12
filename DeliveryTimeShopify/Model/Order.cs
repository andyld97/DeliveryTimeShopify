using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DeliveryTimeShopify.Model
{
    public class Order : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string id, mail, totalPrice, additionalNote;
        private bool isShipping;
        private Address billingAddress, shippingAddress;
        private DateTime createdAt;

        public string Id
        {
            get => id;
            set
            {
                if (value != id)
                {
                    id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Mail
        {
            get => mail;
            set
            {
                if (value != mail)
                {
                    mail = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsShipping
        {
            get => isShipping;
            set
            {
                if (value != isShipping)
                {
                    isShipping = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Address BillingAddress
        {
            get => billingAddress;
            set
            {
                if (value != billingAddress)
                {
                    billingAddress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Address ShippingAddress
        {
            get => shippingAddress;
            set
            {
                if (value != shippingAddress)
                {
                    shippingAddress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TotalPrice
        {
            get => totalPrice;
            set
            {
                if (value != totalPrice)
                {
                    totalPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<int> SKUs { get; set; } = new List<int>();

        public DateTime CreatedAt
        {
            get => createdAt;
            set
            {
                if (value != createdAt)
                {
                    createdAt = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string AdditionalNote
        {
            get => additionalNote;
            set
            {
                if (value != additionalNote)
                {
                    additionalNote = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public override string ToString()
        {
            if (IsShipping)
                return ShippingAddress?.ToString();
            else
                return BillingAddress?.ToString();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
