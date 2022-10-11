using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using rkeyboard.Input;
using Keyboard = rkeyboard.Input.Keyboard;

namespace rkeyboard {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Configuration _configuration;
        private Receiver _receiver;
        private Sender _sender;
        private IKeyboard _keyboard;

        public MainWindow() {
            InitializeComponent();
            _receiver = new Receiver();
            _sender = new Sender();
            _keyboard = new Keyboard(key => _sender.Send(key), key => _sender.Send(-key));
        }

        private void OnInitialized(object? sender, EventArgs e) {
            _configuration = FindResource("Config") as Configuration;
            if (_configuration == null) throw new Exception("Unable to load resource Config");
            _configuration.Mode = Mode.RECEIVE;
        }

        private void OnSetSend(object sender, RoutedEventArgs e) {
            _configuration.Mode = Mode.SEND;
        }

        private void OnSetReceive(object sender, RoutedEventArgs e) {
            _configuration.Mode = Mode.RECEIVE;
        }

        private void OnBtnPressed(object sender, RoutedEventArgs e) {
            if (!_configuration.Running) {
                Start();
            } else {
                Stop();
            }
        }

        private void OnClosed(object? sender, EventArgs e) {
            if (_configuration.Running) {
                Stop();
            }
        }

        private void ValidateElement(string name, string errorMessage) {
            var obj = FindName(name) as DependencyObject;
            if (obj == null || Validation.GetHasError(obj)) {
                throw new ArgumentException(errorMessage);
            }
        }

        private void Start() {
            try {
                ValidateElement("PortTextBox", "Invalid port");
                switch (_configuration.Mode) {
                    case Mode.SEND: {
                        _sender.Listen(_configuration.Port.Value);
                        _keyboard.StartScan();
                        break;
                    }
                    case Mode.RECEIVE: {
                        ValidateElement("AddressTextBox", "Invalid address");
                        _receiver.Connect(_configuration.IpAddress, _configuration.Port.Value, key => {
                            if (key > 0) {
                                _keyboard.EmulateKeyDown(key);
                            } else {
                                _keyboard.EmulateKeyUp(-key);
                            }
                            // MessageBox.Show(key.ToString());
                        });
                        break;
                    }
                    default: {
                        throw new ArgumentException("Invalid mode");
                    }
                }
                _configuration.Running = true;
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stop() {
            try {
                switch (_configuration.Mode) {
                    case Mode.SEND: {
                        _keyboard.StopScan();
                        _sender.Stop();
                        break;
                    }
                    case Mode.RECEIVE: {
                        _receiver.Disconnect();
                        break;
                    }
                    default: {
                        throw new ArgumentException("Invalid mode");
                    }
                }
                _configuration.Running = false;
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnWindowDeactivated(object? sender, EventArgs e) {
            if (_sender.Running()) {
                _keyboard.PauseScan();
            }
        }

        private void OnWindowActivated(object? sender, EventArgs e) {
            if (_sender.Running()) {
                _keyboard.ResumeScan();
            }
        }
    }
}