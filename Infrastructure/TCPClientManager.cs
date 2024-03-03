using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure {
    public class TCPClientManager {
        public static async Task<byte[]> connect(string server, int port, byte[] data) {
            try {
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();
                int count = 0;
                await stream.WriteAsync(data, 0, data.Length);                
                Debug.WriteLine("Sent: {0}", BitConverter.ToString(data).Replace("-", ""));
                data = new Byte[256];
                String response = String.Empty;
                int bytes = await stream.ReadAsync(data, 0, data.Length);
                var readBytes = data[1..bytes];
                var rbs = BitConverter.ToString(readBytes);
                response = Encoding.ASCII.GetString(data, 0, bytes);
                Debug.WriteLine("Received: {0}", response);
                stream.Close();
                client.Close();
                return readBytes;
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
            }
            return new byte[] { };
        }
    }

    public class TCPClientManagerV2  {
        private TcpClient _client;
        private ConcurrentQueue<string> _responses;
        private Task _continualRead;
        private CancellationTokenSource _readCancellation;
        private NetworkStream _stream;

        public TCPClientManagerV2(string ip, int port) {
            connect(ip, port);
            this._responses = new ConcurrentQueue<string>();
            this._readCancellation = new CancellationTokenSource();
            //this._continualRead = Task.Factory.StartNew(this.continualReadOperation, this._readCancellation.Token, this._readCancellation.Token);

        }

        public async void connect(string ip, int port) {
            this._client = new TcpClient(ip, port) {
                ReceiveTimeout = 30000, // probably shouldn't be 3ms.
                SendTimeout = 30000,    // ^
            };
            //int timeout = 10000;
            //return await this.AwaitTimeoutTask(, timeout);
            //await this._client.ConnectAsync(ip, port);
            //return true;
        }

        public async Task streamWrite(byte[] messageBytes) {
            _stream = _stream ??  this._client.GetStream();
            Debug.WriteLine("Sent: {0}", BitConverter.ToString(messageBytes).Replace("-", ""));
            if (await this.AwaitTimeoutTask(_stream.WriteAsync(messageBytes, 0, messageBytes.Length), 10000)) {
                //write success
            } else {
                //write failure.
            }
            return;
        }
        public async Task<byte[]> readFully(int initialLength = 0) {
            //await Task.Delay(10000);
            _stream = _stream ?? _client.GetStream();
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1) {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = await _stream.ReadAsync(buffer, read, buffer.Length - read)) > 0) {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length) {
                    int nextByte = _stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1) {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        public async void continualReadOperation(object state) {
            var token = (CancellationToken)state;
            _stream = _stream ?? this._client.GetStream();
            var byteBuffer = new byte[4096];
            while (!token.IsCancellationRequested) {
                int bytesLastRead = 0;
                if (_stream.DataAvailable) {
                    bytesLastRead = await _stream.ReadAsync(byteBuffer, 0, byteBuffer.Length, token);
                }

                if (bytesLastRead > 0) {
                    var response = Encoding.ASCII.GetString(byteBuffer, 0, bytesLastRead);
                    this._responses.Enqueue(response);
                }
            }
        }

        private async Task<bool> AwaitTimeoutTask(Task task, int timeout) {
            return await Task.WhenAny(task, Task.Delay(timeout)) == task;
        }


        public void GetResponses() {
            //Do a TryDequeue etc... on this._responses.
        }
    }
}