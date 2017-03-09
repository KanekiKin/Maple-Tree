// Project: MapleLib
// File: TextLog.cs
// Updated By: Jared
// 

using System;
using System.Drawing;
using System.Text;

namespace MapleLib.Common
{
    public sealed class TextLog
    {
        public static TextLog MesgLog { get; set; } = new TextLog();
        public static TextLog ChatLog { get; set; } = new TextLog();
        public static TextLog StatusLog { get; set; } = new TextLog();

        private StringBuilder LogBuilder { get; } = new StringBuilder();

        public event EventHandler<NewLogEntryEvent> NewLogEntryEventHandler;

        private void NewLine(string text, Color color = default(Color))
        {
            LogBuilder.AppendFormat(text, color);
            NewLogEntryEventHandler?.Invoke(this, new NewLogEntryEvent(text, color));
        }

        public void WriteLog(string text, Color color = default(Color))
        {
            NewLine(text + Environment.NewLine, color);
        }

        public void WriteStatus(string text, Color color = default(Color))
        {
            NewLine(text, color);
        }

        public void WriteError(string text)
        {
            var color = Color.DarkRed;
            NewLine(text + Environment.NewLine, color);
        }
    }

    public class NewLogEntryEvent : EventArgs
    {
        public NewLogEntryEvent(string entry, Color color = default(Color))
        {
            Entry = $"[{TimeStamp()}] {entry}";
            Color = color;
        }

        public string Entry { get; private set; }

        public Color Color { get; private set; }

        private string TimeStamp()
        {
            return new DateTimeWithZone().TimeStamp();
        }
    }
}