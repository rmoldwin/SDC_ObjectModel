using CSharpVitamins;
using SDC.Schema;
using static SDC.Schema.SdcUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SDC.Schema.Extensions
{
    public static class IdentifiedExtensionTypeExtensions
    {

        /// <summary>
        /// Set the ID property on any IdentifiedExtensionType node only if it does not already exist in the current TopNode's _UniqueIdentifiers hashable.
        /// Will return false if ID already already exists in _UniqueIdentifiers.
        /// </summary>
        /// <returns></returns>
        public static bool TrySetID(this IdentifiedExtensionType iet, string newID)
        {
            if (iet.TopNode is null || newID == "") return false;

            var tn = (_ITopNode)iet.TopNode;
            if (tn._UniqueIdentifiers.TryGetValue(iet.name, out string? curID)
                && iet.name == curID) return false;
            iet.name = newID;
            return true;
        }
    }
}