using System;
using System.Net.Sockets;
using System.Text;

namespace rkeyboard
{
    public class Sender
    {
        private UdpClient _client;

        public Sender()
        {
            _client = new UdpClient();
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port);
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void Send(int key)
        {
            var bytes = BitConverter.GetBytes(key);
            _client.SendAsync(bytes, bytes.Length);
        }
    }
}