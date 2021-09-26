using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FizzyUtils.WebSockets {
    class WebSocket {

        private Uri uri;

        private ClientWebSocket webSocket = null;

        private CancellationTokenSource cancellationTokenSource;

        public delegate void OnOpenDelegate(object sender, EventArgs e);
        public event OnOpenDelegate OnOpen;

        public delegate void OnMessageDelegate(object sender, MessageEventArgs e);
        public event OnMessageDelegate OnMessage;

        public delegate void OnCloseDelegate(object sender, CloseEventArgs e);
        public event OnCloseDelegate OnClose;

        public delegate void OnErrorDelegate(object sender, ErrorEventArgs e);
        public event OnErrorDelegate OnError;

        private Task recieveTask = null;

        public WebSocket(Uri uri) {
            this.uri = uri;

            cancellationTokenSource = new CancellationTokenSource();

            webSocket = new ClientWebSocket();
        }

        public void Connect() {
            recieveTask = Task.Run(this.WebSocketTask);
        }

        protected async void WebSocketTask() {
            await webSocket.ConnectAsync(this.uri, cancellationTokenSource.Token);

            OnOpen?.Invoke(this, new EventArgs());

            var recieveBytes = new byte[65536];
            var recieveBuffer = new ArraySegment<byte>(recieveBytes);

            while (webSocket.State == WebSocketState.Open) {
                try {
                    WebSocketReceiveResult recieveResult = await webSocket.ReceiveAsync(recieveBuffer, cancellationTokenSource.Token);

                    byte[] messageBytes = recieveBuffer.Skip(recieveBuffer.Offset).Take(recieveResult.Count).ToArray();
                    string messageString = Encoding.UTF8.GetString(messageBytes);

                    if (recieveResult.MessageType == WebSocketMessageType.Close) {
                        OnClose?.Invoke(this, new CloseEventArgs() {
                            Code = (int) recieveResult.CloseStatus,
                            Reason = recieveResult.CloseStatusDescription
                        });

                        return;
                    }

                    OnMessage?.Invoke(this, new MessageEventArgs() {
                        Data = messageString,
                        RawData = messageBytes,
                        IsText = recieveResult.MessageType == WebSocketMessageType.Text,
                        IsBinary = recieveResult.MessageType == WebSocketMessageType.Binary,
                    });
                } catch (Exception e) {
                    OnError?.Invoke(this, new ErrorEventArgs() {
                        Exception = e,
                        Message = e.Message
                    });
                }
            }
        }

        public void Disconnect(WebSocketCloseStatus status, string description) {
            webSocket.CloseAsync(status, description, cancellationTokenSource.Token);
        }

        public void Send(byte[] buffer, WebSocketMessageType messageType) {
            webSocket.SendAsync(new ArraySegment<byte>(buffer), messageType, true, cancellationTokenSource.Token);
        }

        public void Send(string message, WebSocketMessageType messageType) {
            webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToCharArray())), messageType, true, cancellationTokenSource.Token);
        }
    }

    public class MessageEventArgs : EventArgs {
        public string Data { get; set; }
        public bool IsBinary { get; set; }
        public bool IsText { get; set; }
        public byte[] RawData { get; set; }
    }

    public class CloseEventArgs : EventArgs {
        public int Code { get; set; }
        public string Reason { get; set; }
    }

    public class ErrorEventArgs : EventArgs {
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
}
