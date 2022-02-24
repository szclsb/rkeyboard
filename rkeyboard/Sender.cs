using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace rkeyboard {
    public class Sender {
        private CancellationTokenSource _tokenSource;
        private BlockingCollection<int> _blockingCollection;

        public Sender() {
            _blockingCollection = new BlockingCollection<int>(255);
        }

        public void Connect(string host, int port) {
            _tokenSource = new CancellationTokenSource();
            var client = new TcpClient();
            var bytes = new byte[4];
            Task.Run(() => {
                while (!_tokenSource.IsCancellationRequested) {
                    try {
                        client.Connect(host, port);
                        var stream = client.GetStream();
                        while (!_tokenSource.IsCancellationRequested) {
                            var key = _blockingCollection.Take(_tokenSource.Token);
                            bytes = BitConverter.GetBytes(key);
                            stream.WriteAsync(bytes);
                        }
                    } catch (Exception e) {
                        
                    } finally {
                        client.Close();
                    }
                }
            });
        }

        public void Disconnect() {
            _tokenSource.Cancel();
            while (_blockingCollection.TryTake(out _)) {
            }
        }

        public void Send(int key) {
            _blockingCollection.TryAdd(key);
        }
    }
}