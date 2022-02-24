﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void OnKeyDown(object sender, KeyEventArgs e) {
            var vKey = KeyInterop.VirtualKeyFromKey(e.Key);
            // MessageBox.Show(vKey.ToString());
            _sender.Send(vKey);
            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            var vKey = KeyInterop.VirtualKeyFromKey(e.Key);
            // MessageBox.Show(vKey.ToString());
            _sender.Send(-vKey);
            e.Handled = true;
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
                        ValidateElement("AddressTextBox", "Invalid address");
                        _sender.Connect(_configuration.IpAddress, _configuration.Port.Value);
                        break;
                    }
                    case Mode.RECEIVE: {
                        _receiver.Listen(_configuration.Port.Value, key => {
                            var input = key > 0 ? CreateKeyDownInput(key) : CreateKeyUpInput(-key);
                            // MessageBox.Show(input.ToString());
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
                        _sender.Disconnect();
                        break;
                    }
                    case Mode.RECEIVE: {
                        _receiver.Stop();
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
    }
}