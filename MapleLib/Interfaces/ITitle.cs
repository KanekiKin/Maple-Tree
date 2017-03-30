// Project: MapleLib
// File: ITitle.cs
// Updated By: Jared
// 
namespace MapleLib.Interfaces
{
    public interface ITitle
    {
        /// <summary>
        /// Used to identify a game title
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Required for content decryption
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Human readable string for the game title
        /// </summary>
        string Name { get; set; }
    }
}