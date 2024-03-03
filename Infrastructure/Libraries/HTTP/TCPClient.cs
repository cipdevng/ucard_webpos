using Core.Application.Exceptions;
using MySqlX.XDevAPI;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.HTTP {
    public class TCPClient : IDisposable {
        private TcpClient _client;
        private ConcurrentQueue<string> _responses;
        private Task _continualRead;
        private CancellationTokenSource _readCancellation;
        private Stream _stream;
        private int readTimeout, writeTimeout;
        private string _ip; int _port;
        private byte[] ret;

        public TCPClient(string ip, int port, int readTimeout = 60000, int writeTimeout = 60000) {
            _ip = ip;
            _port = port;
            this._responses = new ConcurrentQueue<string>();
            this._readCancellation = new CancellationTokenSource();
            this.readTimeout = readTimeout;
            this.writeTimeout = writeTimeout;
        }

        public async void connect(string ip, int port) {
            this._client = new TcpClient(ip, port) {
                /*ReceiveTimeout = readTimeout, 
                SendTimeout = writeTimeout,*/
            };
        }

        public async Task streamWrite(byte[] messageBytes) {
            try {
                _client = new TcpClient(_ip, _port);
                _client.ReceiveTimeout = this.readTimeout;
                _client.SendTimeout = this.writeTimeout;
                _stream = this._client.GetStream();
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            } catch {
                Dispose();
                throw;
            }
            
        }
        public async Task streamWrite(byte[] messageBytes, bool ssl) {
            try {
                if (!ssl) {
                    await streamWrite(messageBytes);
                    return;
                }
                _client = new TcpClient(_ip, _port);
                _client.ReceiveTimeout = this.readTimeout;
                _client.SendTimeout = this.writeTimeout;
                var stream = new SslStream(_client.GetStream());
                stream.AuthenticateAsClient(_ip);
                _stream = stream;
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            } catch {
                Dispose();
                throw;
            }            
        }

        public async Task<byte[]> getResult() {
            Task task = readFully();
            if (await awaitTimeoutTask(task, readTimeout))
                return ret;
            var err = new IOException("System.IO.IOException : Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond", new IOException(" System.Net.Sockets.SocketException : A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond."));
            throw err;
        }
        public async Task readFully(int initialLength = 0) {
            ret = new byte[] { };
            try {
                if (initialLength < 1) {
                    initialLength = 32768;
                }
                int timeout = _client.ReceiveTimeout;
                byte[] buffer = new byte[initialLength];
                int chunk;
                int read = 0;
                while ((chunk = await _stream.ReadAsync(buffer, read, buffer.Length - read)) > 0) {
                    read += chunk;
                    if (read == buffer.Length) {
                        int nextByte = _stream.ReadByte();
                        if (nextByte == -1) {
                            ret = buffer;
                            return;
                        }
                        byte[] newBuffer = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }
                }
                ret = new byte[read];
                Array.Copy(buffer, ret, read);
            } catch {
                throw;
            } finally {
                Dispose();
            }
        }

        private async Task<bool> awaitTimeoutTask(Task task, int timeout) {
            return await Task.WhenAny(task, Task.Delay(timeout)) == task;
        }

        public void getResponses() {
            //Do a TryDequeue etc... on this._responses.
        }
        public void Dispose() {
            _client.Dispose();
            _stream.Dispose();
        }
    }
}
