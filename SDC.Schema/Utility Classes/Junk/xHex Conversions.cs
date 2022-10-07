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

    public class SetSdcNumeric<T> where T: struct

        {

        public SetSdcNumeric(BaseType dt, T val = default, T minInclusive = default, T maxInclusive = default, T minExclusive = default, T maxExclusive = default, byte totalDigits = default, string? mask = null
            ,bool allowGT = false, bool allowGTE = false, bool allowLT = false, bool allowLTE = false, bool allowAPPROX = false)
    {

    }

        public SetSdcNumeric()
        {
        }

    }

    public class SetSdcDateTime<T> where T : struct

    {

        public SetSdcDateTime(BaseType dt, T val = default, T minInclusive = default, T maxInclusive = default, T minExclusive = default, T maxExclusive = default, byte totalDigits = default, string? mask = null
            , bool allowGT = false, bool allowGTE = false, bool allowLT = false, bool allowLTE = false, bool allowAPPROX = false)
        {

        }

        public SetSdcDateTime()
        {
        }

    }
    public class SetSdcDateDuration<T> where T : struct

    {

        public SetSdcDateDuration(BaseType dt, T val = default, T minInclusive = default, T maxInclusive = default, T minExclusive = default, T maxExclusive = default, byte totalDigits = default, string? mask = null
            , bool allowGT = false, bool allowGTE = false, bool allowLT = false, bool allowLTE = false, bool allowAPPROX = false)
        {

        }

        public SetSdcDateDuration()
        {
        }
    }
    public class SetSdcDateXml<T> where T : struct

    {

        public SetSdcDateXml(BaseType dt, T val = default, T minInclusive = default, T maxInclusive = default, T minExclusive = default, T maxExclusive = default, byte totalDigits = default, string? mask = null
            , bool allowGT = false, bool allowGTE = false, bool allowLT = false, bool allowLTE = false, bool allowAPPROX = false)
        {

        }

        public SetSdcDateXml()
        {
        }
    }

}
