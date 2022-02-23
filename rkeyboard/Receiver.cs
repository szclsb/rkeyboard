using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace rkeyboard {
    public class Receiver {
        private CancellationTokenSource _tokenSource;

        public void Listen(int port, Action<int> onReceive) {
            var localAddress = IPAddress.Parse("127.0.0.1");
            var listener = new TcpListener(localAddress, port);
            listener.Start();
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => {
                var bytes = new byte[4];
                try {
                    while (!_tokenSource.IsCancellationRequested) {
                        var client = listener.AcceptTcpClient();
                        var stream = client.GetStream(); // use only one connection
                        while (!_tokenSource.IsCancellationRequested) {
                            var i = stream.ReadAsync(bytes, 0, bytes.Length, _tokenSource.Token)
                                .GetAwaiter().GetResult();
                            if (i != 0 && !_tokenSource.IsCancellationRequested) {
                                var key = BitConverter.ToInt32(bytes, 0);
                                Task.Run(() => onReceive?.Invoke(key));
                            }
                        }
                        client.Close();
                    }
                } catch (SocketException e) {
                    Console.Error.WriteLine(e);
                } finally {
                    listener.Stop();
                }
            });
        }

        public void Stop() {
            _tokenSource.Cancel();
        }
    }
}