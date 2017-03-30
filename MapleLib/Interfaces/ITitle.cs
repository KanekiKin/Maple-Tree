// Project: MapleLib
// File: ITitle.cs
// Updated By: Jared
// 
namespace MapleLib.Interfaces
{
    public interface ITitle
    {
        /// <summary>
        /// The title ID
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// The title name
        /// </summary>
        string Name { get; set; }
    }
}