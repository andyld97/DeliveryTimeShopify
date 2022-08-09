﻿using MailKit.Net.Imap;
using MailKit;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MailKit.Search;
using HtmlAgilityPack;
using System.Web;
using Newtonsoft.Json.Linq;
using DeliveryTimeShopify.Model;
using System.Collections.Generic;
using DeliveryTimeShopify.Helper;

namespace DeliveryTimeShopify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private static ImapClient client = new ImapClient();
        private static IMailFolder inbox;

        private static object sync = new object();

        public static readonly List<Order> Orders = new List<Order>();
        public static readonly List<string> FinishedIDs = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            SetState(false);


#if DEBUG
            Orders.Add(new Order() { AdditionalNote = "", IsShipping = false });
            Orders.Add(new Order() { AdditionalNote = "", IsShipping = false });
            Orders.Add(new Order() { AdditionalNote = "", IsShipping = false });
            Orders.Add(new Order() { AdditionalNote = "A note 1", IsShipping = true });
            Orders.Add(new Order() { AdditionalNote = "A note 2", IsShipping = true });
#endif

            Refresh();

            dispatcherTimer.Interval = TimeSpan.FromSeconds(60);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();

            DispatcherTimer_Tick(this, null);

            // Move window to the right of the screen
            var dpi = VisualTreeHelper.GetDpi(this);
            Top = 0;
            Left = (WpfScreen.Primary.WorkingArea.Width / dpi.DpiScaleX) - Width;
            Height = WpfScreen.Primary.WorkingArea.Height / dpi.DpiScaleY;
        }

        private async void DispatcherTimer_Tick(object? sender, EventArgs e)
        {           
            try
            {
                lock (sync)
                {
                    // Logger.LogInfo(Properties.Resources.strLookingForUnreadMails);
                    bool found = false;

                    if (!client.IsAuthenticated || !client.IsConnected)
                    {
                        //   Logger.LogWarning(Properties.Resources.strMailClientIsNotConnectedAnymore);

                        try
                        {
                            client = new ImapClient();
                            client.Connect(Config.Instance.IngoingMailAuth.ImapServer, Config.Instance.IngoingMailAuth.ImapPort, true);
                            client.Authenticate(Config.Instance.IngoingMailAuth.MailAddress, Config.Instance.IngoingMailAuth.Password);

                            // The Inbox folder is always available on all IMAP servers...
                            inbox = client.Inbox;

                            // Logger.LogInfo(Properties.Resources.strConnectionEstablishedSuccess, sendWebHook: true);
                        }
                        catch (Exception ex)
                        {
                            //Logger.LogError(Properties.Resources.strFailedToConnect, ex);
                            return;
                        }

                    }


                    inbox.Open(FolderAccess.ReadWrite);

                    bool exponge = false;

                    foreach (var uid in inbox.Search(SearchQuery.NotSeen))
                    {
                        var message = inbox.GetMessage(uid);
                        string subject = message.Subject.ToLower();

                        if (Config.Instance.Filter.Any(f => subject.Contains(f.ToLower())))
                        {
                            string htmlContent = message.HtmlBody;

                            HtmlDocument htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(htmlContent);

                            var node = htmlDocument.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]").FirstOrDefault();

                            if (node == null)
                            {
                                inbox.SetFlags(uid, MessageFlags.Deleted, true);
                                exponge = true;
                                continue;
                            }

                            string json = HttpUtility.HtmlDecode(node.InnerText).Trim();
                            var order = MailHelper.ParseMail(json);
                            if (order == null)
                                continue;

                            // Check if this order should be added or not ...
                            lock (Orders)
                            {
                                if (!Orders.Any(p => p.Id == order.Id) && !FinishedIDs.Any(i => i == order.Id))
                                {
                                    var now = DateTime.Now;
                                    if (order.CreatedAt.Day == now.Day)
                                        Orders.Add(order);
                                }
                            }

                            // ... but in anyway the mail can be marked as deleted now
                            inbox.SetFlags(uid, MessageFlags.Deleted, true);
                            exponge = true;

                        }
                    }

                    if (exponge)
                    {
                        inbox.Expunge();

                        // Order orders :)
                        lock (Orders)
                        {
                            var ordersWithoutNode = Orders.Where(p => !string.IsNullOrEmpty(p.AdditionalNote)).ToList();
                            var oderedOrders = Orders.Where(p => string.IsNullOrEmpty(p.AdditionalNote)).OrderBy(p => p.CreatedAt).ToList();

                            Orders.Clear();
                            Orders.AddRange(oderedOrders);
                            Orders.AddRange(ordersWithoutNode);
                        }
                        
                        Refresh();
                    }
                }
            }
            catch
            {
                // ToDo: *** Log
            }
        }

        private void Refresh()
        {
            // Remember old selected item
            string oldSelectedId = string.Empty;
            if (ListOrders.SelectedItem is Order invoice)
                oldSelectedId = invoice.Id;

            // Assign item source
            ListOrders.ItemsSource = null;
            ListOrders.ItemsSource = Orders;
            SetState(ListOrders.Items.Count > 0);

            // Re-assign old selected item (if necessary)
            if (!string.IsNullOrEmpty(oldSelectedId) && Orders.Any(x => x.Id == oldSelectedId))
                ListOrders.SelectedItem = Orders.FirstOrDefault(x => x.Id == oldSelectedId);

            if (Orders.Count > 0)
                ListOrders.ScrollIntoView(ListOrders.Items[ListOrders.Items.Count - 1]);
        }

        private void SetState(bool state)
        {
            if (state)
            {
                TextNoInvoices.Visibility = Visibility.Hidden;
                ListOrders.Visibility = Visibility.Visible;
            }
            else
            {
                TextNoInvoices.Visibility = Visibility.Visible;
                ListOrders.Visibility = Visibility.Hidden;
            }
        }   

        private string FormatNote(Order order)
        {
            if (string.IsNullOrEmpty(order.AdditionalNote) && string.IsNullOrEmpty(order.AdditionalShippingInfo))
                return "Keine";

            if (string.IsNullOrEmpty(order.AdditionalNote) && !string.IsNullOrEmpty(order.AdditionalShippingInfo))
                return order.AdditionalShippingInfo;

            if (!string.IsNullOrEmpty(order.AdditionalNote) && string.IsNullOrEmpty(order.AdditionalShippingInfo))
                return order.AdditionalNote;

            return $"{order.AdditionalNote}{Environment.NewLine}{order.AdditionalShippingInfo}";
        }

        private void ListOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOrders.SelectedItem != null && ListOrders.SelectedItem is Order inv)
            {
                TextAdditionalInvoiceInfo.Text = FormatNote(inv);
                ButtonPanel.IsEnabled = true;
            }
            else
            {
                ButtonPanel.IsEnabled = false;
                TextAdditionalInvoiceInfo.Text = string.Empty;
            }
        }

        private async Task HandlePanelInputAsync(int minutes)
        {
            if (ListOrders.SelectedItem == null)
                return;

            var order = ListOrders.SelectedItem as Order;

            // on successs
            lock (Orders)
            {
                FinishedIDs.Add(order.Id);
                Orders.Remove(order);
                Refresh();
            }

            Dispatcher.Invoke(new Action(() =>
            {
                ButtonPanel.IsEnabled = false;
                TextAdditionalInvoiceInfo.Text = string.Empty;
                SetState(ListOrders.Items.Count > 0);
            }));

            try
            {
                await MailHelper.SendMailAsync(TimeSpan.FromMinutes(minutes), order, Config.Instance.OutgoingMailAuth).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Senden der E-Mail: {ex.Message}", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        #region Number Buttons

        private async void Button15_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(15);
        }

        private async void Button20_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(20);
        }

        private async void Button25_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(25);
        }

        private async void Button30_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(30);
        }

        private async void Button45_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(45);
        }

        private async void Button50_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(50);
        }

        private async void Button60_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(60);
        }

        private async void Button90_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(90);
        }

        private async void Button120_OnClick(object sender, EventArgs e)
        {
            await HandlePanelInputAsync(120);
        }

        #endregion
    }

    #region Converter

    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && string.IsNullOrEmpty(str))
                return new SolidColorBrush(Colors.Transparent);

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AC58FA"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Order invoice)
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                string icon = (invoice.IsShipping ? "delivery" : "user");
                bi.UriSource = new Uri($"pack://application:,,,/DeliveryTimeShopify;component/resources/icons/{icon}.png");
                bi.EndInit();

                return bi;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SKUConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Order invoice)
                return string.Join(", ", invoice.SKUs);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlternationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i && (i % 2 != 0))
                return new SolidColorBrush(Colors.LightGray);

            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}