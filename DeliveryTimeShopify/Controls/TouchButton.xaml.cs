using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeliveryTimeShopify.Controls
{
    /// <summary>
    /// Interaction logic for TouchButton.xaml
    /// </summary>
    public partial class TouchButton : UserControl
    {
        public delegate void onClick(object sender, EventArgs e);
        public event onClick OnClick; 

        public string Text { get; set; }

        public TouchButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonExecute_Click(object sender, RoutedEventArgs e)
        {
            ButtonExecute.BeginStoryboard(FindResource("OnClickAnimation") as Storyboard);
            OnClick?.Invoke(this, e);
        }
    }
}
