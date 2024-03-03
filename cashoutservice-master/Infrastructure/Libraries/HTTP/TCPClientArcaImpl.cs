using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MySqlX.XDevAPI.Common;
using System.Diagnostics;
using Core.Shared;
using Azure;

namespace Infrastructure.Libraries.HTTP {
    public class TCPClientArcaImpl {
    }
    public class TCPConnector {
        private readonly string _server;
        private readonly int _port;
        private readonly int _readTimeout;
        private readonly int _writeTimeout;
        private byte[] result = null;
        public TCPConnector(string server, int port, int readTimeout = 12000 , int writeTimeout= 12000) {
            _server = server;
            _port = port;
            _readTimeout = readTimeout;
            _writeTimeout = writeTimeout;
        }

        private async Task<bool> awaitTimeoutTask(Task task, int timeout) {
            return await Task.WhenAny(task, Task.Delay(timeout)) == task;
        }

        public async Task PostMessageAsync(string message, string server, int port) {
            // Create TCPClient 
            using TcpClient client = new TcpClient();

            // Connect to server endpoint
            await client.ConnectAsync(server, port);

            // Get Network Stream
            using NetworkStream stream = client.GetStream();

            // Convert message to bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Prepend length of message
            byte[] lenBytes = BitConverter.GetBytes(messageBytes.Length);
            byte[] sendBytes = lenBytes.Concat(messageBytes).ToArray();

            // Send message
            await stream.WriteAsync(sendBytes, 0, sendBytes.Length);

            // Close client
            client.Close();
        }



        public async Task<string> PostMessageAndGetResponseAsync(string message, string server, int port) {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(server, port);

            using NetworkStream stream = client.GetStream();

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lenBytes = BitConverter.GetBytes(messageBytes.Length);

            await stream.WriteAsync(lenBytes, 0, lenBytes.Length);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            // Read response length
            byte[] respLenBytes = new byte[4];
            await stream.ReadAsync(respLenBytes, 0, 4);
            int respLength = BitConverter.ToInt32(respLenBytes, 0);

            // Read response body
            byte[] respBytes = new byte[respLength];
            await stream.ReadAsync(respBytes, 0, respLength);

            string response = Encoding.UTF8.GetString(respBytes);

            return response;
        }


        public async Task PostMessageWithSSLAsync(byte[] messageBytes) {
            using TcpClient client = new();
            await client.ConnectAsync(_server, _port);

            using SslStream sslStream = new SslStream(client.GetStream());

            // Authenticate certificate
            await sslStream.AuthenticateAsClientAsync(_server, null,
              SslProtocols.Tls12, false);

            // Send message
            byte[] packet = PreparePacket(messageBytes);
            Debug.WriteLine("Original Packet:> " + BitConverter.ToString(messageBytes).Replace("-", ""));
            Debug.WriteLine("Prepared Packet:> " + BitConverter.ToString(packet).Replace("-", ""));
            await sslStream.WriteAsync(packet);

            // Read response

            byte[] header = new byte[2];
            await sslStream.ReadAsync(header.AsMemory(0, 2));

            int length = (header[0] << 8) | header[1];
            byte[] body = new byte[length];

            int bytesRead = await sslStream.ReadAsync(body.AsMemory(0, length));
            this.result = new byte[bytesRead];
            // Convert to string
            Array.Copy(body, 0, this.result, 0, bytesRead);
            Debug.Write("Data Raw Result:> "+Encoding.ASCII.GetString(this.result));
        }

        public async Task<byte[]> sendRequest(byte[] payload) {
            Task task = PostMessageWithSSLAsync(payload);
            if (await awaitTimeoutTask(task, this._readTimeout))
                return this.result;
            var err = new IOException("System.IO.IOException : Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond", new IOException(" System.Net.Sockets.SocketException : A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond."));
            throw err;
        }

        private static byte[] PreparePacket(byte[] data) {
            byte[] packet = new byte[data.Length + 2];
            packet[0] = (byte)(data.Length >> 8);
            packet[1] = (byte)data.Length;
            Array.Copy(data, 0, packet, 2, data.Length);
            return packet;
        }
    }
}
