using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace rkeyboard {
    public class Sender {
        private readonly BlockingCollection<int> _blockingQueue;
        private CancellationTokenSource _tokenSource;

        public Sender() {
            _blockingQueue = new BlockingCollection<int>(128);
        }

        public void Listen(int port) {
            var localAddress = IPAddress.Any;
            var listener = new TcpListener(localAddress, port);
            listener.Start();
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => {
                try {
                    while (!_tokenSource.IsCancellationRequested) {
                        using var client = listener.AcceptTcpClient();
                        using var stream = client.GetStream();
                        _tokenSource.Token.Register(() => stream.Close());
                        foreach (var key in _blockingQueue.GetConsumingEnumerable(_tokenSource.Token)) {
                            var bytes = BitConverter.GetBytes(key);
                            stream.Write(bytes);
                        }
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine(e);
                } finally {
                    listener.Stop();
                }
            });
        }

        public bool Running() {
            return _tokenSource is { IsCancellationRequested: false };
        }

        public void Stop() {
            _tokenSource.Cancel();
        }

        public void Send(int key) {
            _blockingQueue.TryAdd(key);
        }
    }
}