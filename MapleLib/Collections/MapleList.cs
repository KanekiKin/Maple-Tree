// Project: MapleLib
// File: MapleList.cs
// Updated By: Jared
// 

using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapleLib.Collections
{
    public class MapleList<T> : List<T>
    {
        public event EventHandler<OnAddItemEventArgs<T>> OnAddItem; 
        
        /// <inheritdoc />
        public void Add(T item, Color color = default(Color))
        {
            OnAddItem?.Invoke(this, new OnAddItemEventArgs<T>(this, item, color));
            base.Add(item);
        }
    }

    public class OnAddItemEventArgs<T> : EventArgs
    {
        public object sender;
        public readonly T item;
        public readonly Color color;

        public OnAddItemEventArgs(object sender, T item, Color color = default(Color))
        {
            this.sender = sender;
            this.item = item;
            this.color = color;
        }
    }
}