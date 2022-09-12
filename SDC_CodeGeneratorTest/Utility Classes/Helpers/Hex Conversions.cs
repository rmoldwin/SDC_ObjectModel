using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using System.Xml;
using Newtonsoft.Json;
using SDC;

namespace SDC.Schema
{

    public static class HexConversions

    {
        //https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
        //https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        //https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array


        //Convert hex string to byte array
        public static byte[] HexStringToByteArrayFastest(string hex) //orig name: StringToByteArrayFastest
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The input string cannot have an odd number of hex characters");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
            //---------------------------
            static int GetHexVal(char hex)
            {
                int val = (int)hex;
                //For uppercase A-F letters:
                //return val - (val < 58 ? 48 : 55);
                //For lowercase a-f letters:
                //return val - (val < 58 ? 48 : 87);
                //Or the two combined, but a bit slower:
                return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
            }
        }

        public static byte[] HexStringToByteArraySlow(string hex)
        {
            if (hex.Length % 2 == 1) throw new Exception("The input string cannot have an odd number of hex characters");
            return Enumerable.Range(0, hex.Length / 2).Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16)).ToArray();
        }
        public static byte[] HexStringToBytes(this string hexString)   //orig name: HexToBytes     
        {
        https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
            if (hexString.Length % 2 == 1) throw new Exception("The input string cannot have an odd number of hex characters");
            byte[] b = new byte[hexString.Length / 2];
            char c;
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                c = hexString[i * 2];
                b[i] = (byte)((c < 0x40 ? c - 0x30 : (c < 0x47 ? c - 0x37 : c - 0x57)) << 4);
                c = hexString[i * 2 + 1];
                b[i] += (byte)(c < 0x40 ? c - 0x30 : (c < 0x47 ? c - 0x37 : c - 0x57));
            }

            return b;
        }


    }
}
