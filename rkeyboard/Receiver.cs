using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace rkeyboard {
    public class Receiver {
        private CancellationTokenSource _tokenSource;
        private BlockingCollection<byte[]> _blockingQueue;

        public Receiver() {
            _blockingQueue = new BlockingCollection<byte[]>(128);
        }

        public void Listen(int port, Action<int> onReceive) {
            var localAddress = IPAddress.Any;
            var listener = new TcpListener(localAddress, port);
            listener.Start();
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => {
                foreach (var bytes in _blockingQueue.GetConsumingEnumerable(_tokenSource.Token)) {
                    var key = BitConverter.ToInt32(bytes, 0);
                    onReceive?.Invoke(key);
                }
            });
            Task.Run(() => {
                try {
                    while (!_tokenSource.IsCancellationRequested) {
                        var client = listener.AcceptTcpClient();
                        var bytes = new byte[4];
                        var stream = client.GetStream(); // use only one connection
                        _tokenSource.Token.Register(() => stream.Close());
                        while (stream.Read(bytes, 0, bytes.Length) != 0 && !_tokenSource.IsCancellationRequested) {
                            _blockingQueue.TryAdd(bytes);
                        }
                        client.Close();
                    }
                } catch (Exception e) {
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