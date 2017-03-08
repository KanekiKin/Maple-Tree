using CefSharp;
using Cemu_UI;
using MapleLib;

namespace MapleSeed
{
    class Plugin
    {
        static bool loaded;

        public string Author => @"github.com/Tsume";

        public string Description => "Updates Wii U titles to the latest version.";

        public string Name => "Title Updater";

        public async void Initialize()
        {
            await Database.Initialize();

            Cemu_UI.Program.main.browser.RegisterJsObject("Updater", this);
            Cemu_UI.Program.events.MenuLoaded += Events_MenuLoaded;
            Cemu_UI.Program.events.GameSelected += Events_GameSelected;
            Network.DownloadProgressChangedEvent += Network_DownloadProgressChangedEvent;
            TextLog.MesgLog.NewLogEntryEventHandler += MesgLog_NewLogEntryEventHandler;

            Logger.log($@"Initializing Plugin: {Name}");
        }

        private void MesgLog_NewLogEntryEventHandler(object sender, OnNewLogEntry e)
        {
            if (loaded && !string.IsNullOrWhiteSpace(e.Entry))
                Cemu_UI.Program.main.browser.EvaluateScriptAsync($@"$('#update-container .info').val($('#update-container .info').val() + '{e.Entry.Replace("\\", "\\\\").Replace("'", "\\'")}\n');var area = $('#update-container .info');if(area.length)area.scrollTop(area[0].scrollHeight - area.height());");
        }

        private void Network_DownloadProgressChangedEvent(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Cemu_UI.Program.main.browser.EvaluateScriptAsync($@"$('.progress').text('{e.ProgressPercentage}% {(e.BytesReceived / 1024f) / 1024f} MB/{(e.TotalBytesToReceive / 1024f) / 1024f}')");
        }

        private void Events_MenuLoaded(object sender, System.EventArgs e)
        {
            loaded = true;
            Cemu_UI.Program.main.browser.EvaluateScriptAsync(@"$('.container').css({'transition':'1s'});$('#cemu-ui').append('<div id=""update-container"" style=""position: absolute; z-index: 10; top: 0; left: 1280px; width: 1280px; height: 720px; transition: 1s; background: rgba(30, 30, 30, .45); transition: 1s; opacity: 0;""> <h3>Updating</h3> <div class=""updating"" style=""position: absolute; top: 180px; left: 54px; color: #fff; font-size: 25px; width: 1180px;""> <img src="""" style=""width: 100px;""> <div class=""name"" style=""position: absolute; left: 120px; top: 30px;""></div> <div class=""progress"">Progress Coming Soon</div> <textarea class=""info"" style=""position: absolute; background: transparent; top: 150px; left: 0px; width: 500px; height: 200px; color: white; resize: none; outline-width: 0;"" readonly=""""></textarea> <a class=""leaveUpdate"" style=""position: absolute; font-size: 18px; display: block; width: 150px; padding: 10px 20px; cursor: pointer; transition: 1s; text-align: center; text-decoration: none; color: #fff; border-radius: 8px; background: #444; top: 450px;"" onclick=""$(\'.container\').css({\'opacity\':\'255\'}); $(\'#update-container\').css({\'left\':\'1280px\', \'opacity\':\'0\'});"">Back</a> </div> </div>');");

            Cemu_UI.Program.main.browser.EvaluateScriptAsync(@"$('#game-info').append('<a id=""update"" onclick=""$(\'#update-container .info\').val(\'\'); $(\'.container\').css({\'opacity\':\'0\'});$(\'#update-container\').css({\'left\':\'0px\', \'opacity\':\'255\'});$(\'#update-container .name\').text($(\'#game-info h2\').text());$(\'#update-container .updating img\').attr(\'src\', $(\'#game-icon\').css(\'background-image\').replace(\'url(\',\'\').replace(\')\',\'\').replace(/\\\&quot;/gi, &quot;&quot;));Updater.updateGame(cemu.selected.titleId, cemu.selected.game);"" style=""font-size: 18px; display: block; width: 200px; margin: 15px 0 0; padding: 5px 20px; cursor: pointer; transition: 1s; text-align: center; text-decoration: none; color: #fff; border-radius: 8px; background: #00acd2;"">Update Game</a>');");
        }

        public void UpdateGame(string titleId, string game)
        {

        }

        private void Events_GameSelected(object sender, Events.GameSelectedArgs e)
        {

        }
    }
}