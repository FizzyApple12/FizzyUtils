using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace FizzyUtils {
    public class UsageTracker {
        private string url;

        private WebSocket webSocket = null;

        private bool reconnect = true;

        private List<UsageTrackerUser> usageTrackerUsers = new List<UsageTrackerUser>();

        internal UsageTracker(string url, bool secure) {
            this.url = url;

            if (!Plugin.track) return;

            webSocket = new WebSocket($"{(secure ? "wss" : "ws")}://{url}/usagetracker");
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

            webSocket.SendAsync(stringBuilder.ToString(), (bool completed) => { });
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
            webSocket.Close();
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
