using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace rkeyboard {
    public class Receiver {
        private readonly BlockingCollection<int> _blockingQueue;
        private CancellationTokenSource _tokenSource;

        public Receiver() {
            _blockingQueue = new BlockingCollection<int>(128);
        }

        public void Connect(string host, int port, Action<int> onReceive) {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => {
                foreach (var key in _blockingQueue.GetConsumingEnumerable(_tokenSource.Token)) {
                    onReceive?.Invoke(key);
                }
            });
            Task.Run(() => {
                while (!_tokenSource.IsCancellationRequested) {
                    var client = new TcpClient();
                    try {
                        client.Connect(host, port);
                        using var stream = client.GetStream();
                        _tokenSource.Token.Register(() => stream.Close());
                        var bytes = new byte[4];
                        while (stream.Read(bytes, 0, bytes.Length) != 0 && !_tokenSource.IsCancellationRequested) {
                            var key = BitConverter.ToInt32(bytes, 0);
                            _blockingQueue.TryAdd(key);
                        }
                    } catch (Exception e) {
                        Console.Error.WriteLine(e);
                        Thread.Sleep(2000);
                    } finally {
                        client.Close();
                    }
                }
            });
        }

        public void Disconnect() {
            _tokenSource.Cancel();
        }
    }
}