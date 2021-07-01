using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace FizzyUtils {
    public class LeaderBoarder {
        private string url;

        private WebSocket webSocket = null;

        private bool reconnect = true;

        internal LeaderBoarder(string url, bool secure) {
            this.url = url;

            webSocket = new WebSocket($"{(secure ? "wss" : "ws")}://{url}/leaderboards");
            webSocket.Log.Level = LogLevel.Info;
            webSocket.OnMessage += OnMessage;
            webSocket.OnClose += OnClose;
            webSocket.ConnectAsync();
        }

        private void OnClose(object sender, CloseEventArgs e) {
            if (reconnect) webSocket.ConnectAsync();
        }

        private void OnMessage(object sender, MessageEventArgs e) {
            if (!e.IsText) return;
        }

        internal void Stop() {
            reconnect = false;
            webSocket.Close();
        }
    }
}
