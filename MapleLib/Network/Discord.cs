using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using MapleLib.Common;
using Color = System.Drawing.Color;

namespace MapleLib.Network
{
    public class Discord
    {
        private readonly DiscordClient _client;
        private Channel _channel;
        private bool _connected;
        private ListBox _listBox;
        private Server _server;

        private Discord()
        {
            _client = new DiscordClient();
            _client.Ready += _client_Ready;
            _client.MessageReceived += _client_MessageReceived;
        }

        private static Discord Instance { get; } = new Discord();

        public static bool Connected => Instance._connected;

        private static Server Server => Instance?._server;

        public static string Nickname => Instance?._server?.CurrentUser?.ToString();

        private BindingList<string> Users {
            get {
                return _channel?.Users != null ? new BindingList<string>(_channel.Users.Select(user => user.Name).ToList()) : null;
            }
        }

        public static DiscordClient Client()
        {
            return Instance?._client;
        }

        public static List<string> GetChannelList()
        {
           return Server.TextChannels.Select(channel => channel.Name).ToList();
        }

        public static void SetChannel(string channel)
        {
            if (!Connected) return;

            Instance._channel = Instance?._server?.AllChannels.First(c => c.Name == channel);

            Instance._listBox.DataSource = Instance.Users;

            TextLog.ChatLog.WriteLog($"Current channel '{Instance._channel?.Name}'", Color.Green);
        }

        public static async void SendMessage(string text)
        {
            var sendMessage = Instance?._channel?.SendMessage(text);
            if (sendMessage != null) await sendMessage;
        }

        private void _client_Ready(object sender, EventArgs e)
        {
            _server = _client.Servers.FirstOrDefault(s => s.Id == 223863411539312641);

            TextLog.ChatLog.WriteLog($"Connected to Discord {_server?.Name}", Color.Green);
            _connected = true;
        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Channel != _channel) return;

            TextLog.ChatLog.WriteLog($"[{e.Channel}] {e.Message}");
        }

        public static async Task Connect()
        {
            var connectTask = Instance?.ConnectTask();
            if (connectTask != null) await connectTask;
            await Task.Delay(2000);
            SetChannel("warez");
        }

        private async Task ConnectTask()
        {
            if (string.IsNullOrEmpty(Settings.DiscordEmail) ||
                string.IsNullOrEmpty(Settings.DiscordPass)) {
                TextLog.ChatLog.WriteError(
                    "Chat feature disabled!! Go to the Settings tab, fill in your Discord login, then click 'Connect to Chat'.");
                return;
            }

            await _client.Connect(Settings.DiscordEmail, Settings.DiscordPass);
        }

        public static void UpdateUserlist(ListBox listBox)
        {
            Instance._listBox = listBox;
        }
    }
}