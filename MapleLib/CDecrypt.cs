// Project: MapleLib
// File: CDecrypt.cs
// Updated By: Jared
// 

using System.IO;
using System.Security.Cryptography;
using MapleLib.Common;

namespace MapleLib
{
    public class CDecrypt
    {
        public static byte[] Decrypt(byte[] cipherData, ulong ContentID)
        {
            byte[] IV = new byte[16];
            IV[1] = (byte) ContentID;

            var key = WiiUCommenKey;

            try {
                byte[] buffer = new byte[cipherData.Length];
                using (var rm = new RijndaelManaged {Key = key, IV = IV, Mode = CipherMode.CBC}) {
                    using (var ms = new MemoryStream(cipherData)) {
                        using (var cs = new CryptoStream(ms, rm.CreateDecryptor(key, IV), CryptoStreamMode.Read)) {
                            cs.Read(buffer, 0, buffer.Length);
                            return buffer;
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                TextLog.StatusLog.WriteError($@"A Cryptographic error occurred: {e.Message}");
                return null;
            }
            // You may want to catch more exceptions here...
        }

        private static byte[] WiiUCommenDevKey =
        {
            0x2F, 0x5C, 0x1B, 0x29, 0x44, 0xE7, 0xFD, 0x6F, 0xC3, 0x97, 0x96, 0x4B, 0x05,
            0x76, 0x91, 0xFA
        };

        private static readonly byte[] WiiUCommenKey =
        {
            0xD7, 0xB0, 0x04, 0x02, 0x65, 0x9B, 0xA2, 0xAB, 0xD2, 0xCB, 0x0D, 0xB2, 0x7F,
            0xA2, 0xB6, 0x56
        };

        //AES_KEY key;
        byte[] enc_title_key = new byte[16];
        byte[] dec_title_key = new byte[16];
        byte[] title_id = new byte[16];
        byte[] dkey = new byte[16];

        ulong H0Count = 0;
        ulong H0Fail = 0;

        enum ContentType
        {
            CONTENT_REQUIRED = (1 << 0),    // not sure
            CONTENT_SHARED = (1 << 15),
            CONTENT_OPTIONAL = (1 << 14),
        };

        public class ContentInfo
        {
            ushort IndexOffset; //	0	 0x204
            ushort CommandCount; //	2	 0x206
            byte[] SHA2 = new byte[32]; //  12 0x208
        }

        public class Content
        {
            uint ID; //	0	 0xB04
            ushort Index; //	4  0xB08
            ushort Type; //	6	 0xB0A
            ulong Size; //	8	 0xB0C
            byte[] SHA2 = new byte[32]; //  16 0xB14
        }

        public class TitleMetaData
        {
            public uint SignatureType; // 0x000
            public byte[] Signature = new byte[0x100]; // 0x004

            public byte[] Padding0 = new byte[0x3C]; // 0x104
            public byte[] Issuer = new byte[0x40]; // 0x140

            public byte Version; // 0x180
            public byte CACRLVersion; // 0x181
            public byte SignerCRLVersion; // 0x182
            public byte Padding1; // 0x183

            public ulong SystemVersion; // 0x184
            public ulong TitleID; // 0x18C
            public uint TitleType; // 0x194
            public ushort GroupID; // 0x198
            public byte[] Reserved = new byte[62]; // 0x19A
            public uint AccessRights; // 0x1D8
            public ushort TitleVersion; // 0x1DC
            public ushort ContentCount; // 0x1DE
            public ushort BootIndex; // 0x1E0
            public byte[] Padding3 = new byte[2]; // 0x1E2
            public byte[] SHA2 = new byte[32]; // 0x1E4

            public ContentInfo[] ContentInfos;

            public Content[] Contents; // 0x1E4
        }

        public class FSTInfo
        {
            public uint Unknown;
            public uint Size;
            public uint UnknownB;
            public uint[] UnknownC = new uint[6];
        }

        public class FST
        {
            public uint MagicBytes;
            public uint Unknown;
            public uint EntryCount;

            public uint[] UnknownB = new uint[5];

            public FSTInfo[] FSTInfos;
        }


        /*struct FEntry
        {
        	union
        	{
        		struct
        		{
        			u32 Type				:8;
        			u32 NameOffset	:24;
        		};
        		u32 TypeName;
        	};
        	union
        	{
        		struct		// File Entry
        		{
        			u32 FileOffset;
        			u32 FileLength;
        		};
        		struct		// Dir Entry
        		{
        			u32 ParentOffset;
        			u32 NextOffset;
        		};
        		u32 entry[2];
        	};
        	unsigned short Flags;
        	unsigned short ContentID;
        };*/

        private uint bs24(uint i)
        {
            return ((i & 0xFF0000) >> 16) | ((i & 0xFF) << 16) | (i & 0x00FF00);
        }
        private ulong bs64(ulong i)
        {
            return ((ulong)((uint)((((i & 0xFFFFFFFF) & 0xFF0000) >> 8) | (((i & 0xFFFFFFFF) & 0xFF00) << 8) | ((i & 0xFFFFFFFF) >> 24) | ((i & 0xFFFFFFFF) << 24))) << 32) | ((uint)((((i >> 32) & 0xFF0000) >> 8) | (((i >> 32) & 0xFF00) << 8) | ((i >> 32) >> 24) | ((i >> 32) << 24)));
        }


    }
}
 