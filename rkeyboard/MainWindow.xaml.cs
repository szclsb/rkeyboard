using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace rkeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Configuration _configuration;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            _configuration = FindResource("Config") as Configuration;
            if (_configuration == null) throw new Exception("Unable to load resource Config");
        }

        private void OnSetSend(object sender, RoutedEventArgs e)
        {
            _configuration.Mode = Mode.SEND;
        }
        
        private void OnSetReceive(object sender, RoutedEventArgs e)
        {
            _configuration.Mode = Mode.RECEIVE;
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_configuration.Mode + "\n" + _configuration.IpAddress + "\n" + _configuration.Port);
        }
    }
}