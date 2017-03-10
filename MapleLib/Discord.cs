using System.Threading.Tasks;
using Discord;

namespace MapleLib
{
    internal class Discord
    {
        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient();

            _client.MessageReceived += async (s, e) => {
                if (!e.Message.IsAuthor)
                    await e.Channel.SendMessage(e.Message.Text);
            };

            _client.ExecuteAndWait(async () => await Connect());
        }

        private async Task Connect()
        {
            //await _client.Connect("aaaaabbbbbbcccccdddddeeeeefffffggggg", TokenType.User);
            await _client.Connect("jaredt741@gmail.com", "admins");
        }
    }
}