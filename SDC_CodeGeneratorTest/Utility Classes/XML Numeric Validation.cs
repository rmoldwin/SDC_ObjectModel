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

    public struct IsValidNumericDE<T> where T: INumericDE

        {

        public IsValidNumeric(BaseType dt, T val = default, T minInclusive = default, T maxInclusive = default, T minExclusive = default, T maxExclusive = default, byte totalDigits = default, string? mask = null
            ,bool allowGT = false, bool allowGTE = false, bool allowLT = false, bool allowLTE = false, bool allowAPPROX = false)
    {

    }

        public IsValidNumeric(T sdcNumDE)
        {

        }


        bool isValid = false;

        public bool IsValid
        {
            get => isValid;
            if()

        }
        

    }



}
