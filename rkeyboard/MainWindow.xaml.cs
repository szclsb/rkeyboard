using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using rkeyboard.WinHook;
using rkeyboard.WinUser;
using InputType = rkeyboard.WinUser.InputType;

namespace rkeyboard {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Configuration _configuration;
        private Receiver _receiver;
        private Sender _sender;
        private IntPtr _hook;

        public MainWindow() {
            InitializeComponent();
            _receiver = new Receiver();
            _sender = new Sender();
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
                        _hook = Interceptor.InstallHook(HookCallback);
                        break;
                    }
                    case Mode.RECEIVE: {
                        ValidateElement("AddressTextBox", "Invalid address");
                        _receiver.Connect(_configuration.IpAddress, _configuration.Port.Value, key => {
                            var input = key > 0 ? CreateKeyDownInput(key) : CreateKeyUpInput(-key);
                            // MessageBox.Show(key.ToString());
                            WinInput.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Input)));
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
                        Interceptor.UninstallHook(_hook);
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

        private static Input CreateKeyDownInput(int key) {
            return new Input {
                type = (uint) InputType.Keyboard,
                input = new InputUnion {
                    ki = new KeyboardInput {
                        wVk = (ushort) key,
                        dwFlags = (uint) KeyEventFlags.KeyDown,
                        dwExtraInfo = WinInput.GetMessageExtraInfo()
                    }
                }
            };
        }
        
        private static Input CreateKeyUpInput(int key) {
            return new Input {
                type = (uint) InputType.Keyboard,
                input = new InputUnion {
                    ki = new KeyboardInput {
                        wVk = (ushort) key,
                        dwFlags = (uint) KeyEventFlags.KeyUp,
                        dwExtraInfo = WinInput.GetMessageExtraInfo()
                    }
                }
            };
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var key = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr) Interceptor.KEY_DOWN || wParam == (IntPtr) Interceptor.WM_SYSKEY_DOWN) {
                    _sender.Send(key);
                } else if (wParam == (IntPtr) Interceptor.KEY_UP || wParam == (IntPtr) Interceptor.WM_SYSKEY_UP) {
                    _sender.Send(-key);
                }
            }
            // return Interceptor.CallNextHookEx(_hook, nCode, wParam, lParam);
            return (IntPtr) 1;
        }

        private void OnWindowDeactivated(object? sender, EventArgs e) {
            if (_sender.Running()) {
                Interceptor.UninstallHook(_hook);
            }
        }

        private void OnWindowActivated(object? sender, EventArgs e) {
            if (_sender.Running()) {
                _hook = Interceptor.InstallHook(HookCallback);
            }
        }
    }
}