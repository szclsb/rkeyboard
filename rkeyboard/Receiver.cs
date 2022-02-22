using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace rkeyboard {
    public class Receiver {
        private volatile UdpClient _client;
        private Thread _thread;

        public void Listen(int port, Action<int> onReceive) {
            _thread = new Thread(() => {
                _client = new UdpClient(port);
                var ep = new IPEndPoint(IPAddress.Any, port);
                try {
                    while (true) {
                        var bytes = _client.Receive(ref ep);
                        if (bytes == null || bytes.Length == 0) {
                            return;
                        }

                        var key = BitConverter.ToInt32(bytes, 0);
                        onReceive?.Invoke(key);
                    }
                }
                catch (SocketException e) {
                    Console.Error.WriteLine(e);
                }
                finally {
                    _client.Close();
                }
            });
            _thread.Start();
        }

        public void Stop() {
            _client?.Close();
            _thread?.Join();
        }
    }
}