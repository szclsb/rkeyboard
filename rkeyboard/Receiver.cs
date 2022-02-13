using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace rkeyboard
{
    public class Receiver
    {
        private volatile UdpClient _client;
        private Thread _thread;

        public void Listen(int port, Action<string> onReceive)
        {
            _thread = new Thread(() =>
            {
                _client = new UdpClient(port);
                var ep = new IPEndPoint(IPAddress.Any, port);
                try
                {
                    while (true)
                    {
                        var content = _client.Receive(ref ep);
                        if (content == null || content.Length == 0)
                        {
                            return;
                        }

                        var str = Encoding.ASCII.GetString(content);
                        onReceive?.Invoke(str);
                    }
                }
                catch (SocketException e)
                {
                    Console.Error.WriteLine(e);
                }
                finally
                {
                    _client.Close();
                }
            });
            _thread.Start();
        }

        public void Stop()
        {
            _client?.Close();
            _thread?.Join();
        }
    }
}