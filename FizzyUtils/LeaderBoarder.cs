using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzyUtils.WebSockets;

namespace FizzyUtils {
    public class LeaderBoarder {
        private Uri uri;

        private WebSocket webSocket = null;

        private bool reconnect = true;

        internal LeaderBoarder(string url, bool secure) {
            this.uri = new Uri($"{(secure ? "wss" : "ws")}://{url}/leaderboards");

            webSocket = new WebSocket(this.uri);
            webSocket.OnMessage += OnMessage;
            webSocket.OnClose += OnClose;
            webSocket.Connect();
        }

        private void OnClose(object sender, CloseEventArgs e) {
            if (reconnect) webSocket.Connect();
        }

        private void OnMessage(object sender, MessageEventArgs e) {
            if (!e.IsText) return;
        }

        internal void Stop() {
            reconnect = false;
            webSocket.Disconnect(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Going away");
        }
    }
}
