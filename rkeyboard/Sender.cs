using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace rkeyboard {
    public class Sender {
        private CancellationTokenSource _tokenSource;
        private BlockingCollection<byte[]> _blockingQueue;

        public Sender() {
            _blockingQueue = new BlockingCollection<byte[]>(128);
        }

        public void Connect(string host, int port) {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => {
                while (!_tokenSource.IsCancellationRequested) {
                    var client = new TcpClient();
                    try {
                        client.Connect(host, port);
                        var stream = client.GetStream();
                        _tokenSource.Token.Register(() => stream.Close());
                        foreach (var bytes in _blockingQueue.GetConsumingEnumerable(_tokenSource.Token)) {
                            stream.WriteAsync(bytes);
                        }
                    } catch (Exception e) {
                        Console.Error.WriteLine(e);
                    } finally {
                        client.Close();
                    }
                }
            });
        }

        public void Disconnect() {
            _tokenSource.Cancel();
        }

        public void Send(int key) {
            var bytes = BitConverter.GetBytes(key);
            _blockingQueue.TryAdd(bytes);
        }
    }
}