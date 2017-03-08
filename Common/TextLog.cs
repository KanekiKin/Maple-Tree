// Project: MapleLib
// File: TextLog.cs
// Updated By: Jared
// 

using System;
using System.Drawing;
using System.Text;

namespace MapleLib
{
    public sealed class TextLog
    {
        public static TextLog MesgLog { get; set; } = new TextLog();

        private StringBuilder LogBuilder { get; set; } = new StringBuilder();

        public event EventHandler<OnNewLogEntry> NewLogEntryEventHandler;

        public void NewLine(string text, Color color = default(Color))
        {
            LogBuilder.AppendFormat(text, color);
            NewLogEntryEventHandler?.Invoke(this, new OnNewLogEntry(this, text, color));
        }
    }

    public class OnNewLogEntry : EventArgs
    {
        public string Entry { get; private set; }

        public Color Color { get; private set; }

        public object Sender { get; set; }

        public OnNewLogEntry(object sender, string entry, Color color = default(Color))
        {
            Sender = sender;
            Entry = entry;
            Color = color;
        }
    }
}