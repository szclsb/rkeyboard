using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;

namespace rkeyboard {
    public enum Mode {
        SEND,
        RECEIVE
    }

    public class Configuration : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool _running;

        public bool Running {
            get => _running;
            set {
                _running = value;
                OnPropertyChanged();
            }
        }

        private Mode? _mode;

        public Mode? Mode {
            get => _mode;
            set {
                _mode = value;
                OnPropertyChanged();
            }
        }

        private int? _port;

        public int? Port {
            get => _port;
            set {
                _port = value;
                OnPropertyChanged();
            }
        }

        private string _ipAddress;

        public string IpAddress {
            get => _ipAddress;
            set {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }
    }
}