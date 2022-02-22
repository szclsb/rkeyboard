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
        private Receiver _receiver;
        private Sender _sender;
        
        public MainWindow()
        {
            InitializeComponent();
            _receiver = new Receiver();
            _sender = new Sender();
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            _configuration = FindResource("Config") as Configuration;
            if (_configuration == null) throw new Exception("Unable to load resource Config");
            _configuration.Mode = Mode.RECEIVE;
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
            switch (_configuration.Mode)
            {
                case Mode.SEND:
                {
                    //FIXME
                    _sender.Connect(_configuration.IpAddress, _configuration.Port.Value);
                    break;
                }
                case Mode.RECEIVE:
                {
                    //FIXME
                    _receiver.Listen(_configuration.Port.Value, key =>
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            MessageBox.Show(key.ToString());
                        });
                    });
                    break;
                }
                default:
                {
                    throw new ArgumentException("Invalid mode");
                }
            }
            _configuration.Running = true;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            switch (_configuration.Mode)
            {
                case Mode.SEND:
                {
                    _sender.Disconnect();
                    break;
                }
                case Mode.RECEIVE:
                {
                    _receiver.Stop();
                    break;
                }
                default:
                {
                    throw new ArgumentException("Invalid mode");
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _sender.Send((int) e.Key);
            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _sender.Send(-(int) e.Key);
            e.Handled = true;
        }
    }
}