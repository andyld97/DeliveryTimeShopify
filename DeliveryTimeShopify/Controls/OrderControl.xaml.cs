using DeliveryTimeShopify.Model;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DeliveryTimeShopify.Controls
{
    /// <summary>
    /// Interaction logic for OrderControl.xaml
    /// </summary>
    public partial class OrderControl : UserControl
    {
        public Order Order { get; set; }

        public OrderControl(Order order)
        {
            InitializeComponent();

            Order = order;
            DataContext = order;

            if (order.IsShipping)
                TextShipping.Text = "Zu liefern!";
            else 
                TextShipping.Text = "Wird abgeholt";
        }
    }

    #region Converter
    public class SKUConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Order order)
                return string.Join(", ", order.SKUs);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MarkedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || (value is string str && string.IsNullOrWhiteSpace(str)))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
