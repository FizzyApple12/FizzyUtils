using System;
using System.Collections.Generic;
using System;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using FizzyUtils.WebSockets;

namespace FizzyUtils {
    public class UsageTracker {
        private Uri uri;

        private WebSocket webSocket = null;

        private bool reconnect = true;

        private List<UsageTrackerUser> usageTrackerUsers = new List<UsageTrackerUser>();

        internal UsageTracker(string url, bool secure) {
            this.uri = new Uri($"{(secure ? "wss" : "ws")}://{url}/usagetracker");

            if (!Plugin.track) return;

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

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            JsonWriter writer = new JsonTextWriter(stringWriter);

            writer.WriteStartObject();

            writer.WritePropertyName("event");
            writer.WriteValue("ping");

            writer.WritePropertyName("mods");
            writer.WriteStartArray();

            foreach (UsageTrackerUser usageTrackerUser in usageTrackerUsers) {
                writer.WriteValue(usageTrackerUser.modName);
            }

            writer.WriteEnd();
            writer.WriteEndObject();

            webSocket.Send(stringBuilder.ToString(), System.Net.WebSockets.WebSocketMessageType.Text);
        }

        public UsageTrackerUser AddUser(string modName) {
            UsageTrackerUser usageTrackerUser = new UsageTrackerUser(modName);

            usageTrackerUsers.Add(usageTrackerUser);

            return usageTrackerUser;
        }

        internal void RemoveUser(Guid uuid) {
            for (int i = 0; i < usageTrackerUsers.Count; i++) {
                if (usageTrackerUsers[i].uuid.Equals(uuid)) {
                    usageTrackerUsers.RemoveAt(i);
                    i--;
                }
            } 
        }

        internal void Stop() {
            reconnect = false;
            webSocket.Disconnect(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Going away");
        } 

        public class UsageTrackerUser {
            internal string modName;
            internal Guid uuid;
            internal UsageTracker parent;

            internal UsageTrackerUser(string modName) {
                uuid = Guid.NewGuid();
                this.modName = modName;
            }

            public void Remove() {
                parent.RemoveUser(uuid);
            }
        }
    }
}
