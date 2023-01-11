using CSharpVitamins;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel; //contains ReadOnly collections
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace SDC.Schema.Tests.Utils
{
	public class CompareTrees<T> where T : ITopNode
	{
		private T _prevVersion;
		private T _newVersion;

		//Key for the SortedList is IET sGuid, Key for internal Dict is subNode sGuid; holds serializable attribute List for individual subNodes
		private SortedList<string, Dictionary<string, List<AttributeInfo>>> _slAttPrev;
		private SortedList<string, Dictionary<string, List<AttributeInfo>>> _slAttNew;

		private List<IdentifiedExtensionType> _IETnodesRemovedInNew;
		private List<IdentifiedExtensionType> _IETnodesAddedInNew;
		private readonly ConcurrentDictionary<string, DifNodeIET> dDifNodeIET = new(); //the key is the IET node sGuid. Holds attribute changes in all IET and subNodes
																					   //foreach(var kv2 in slAttNew)
		#region     ctor   
		public CompareTrees(T prevVersion, T newVersion)
		{
			CtorCompareTrees(prevVersion, newVersion);
		}
		public CompareTrees(string prevXml, string newXml)
		{
			try
			{
				_newVersion = ITopNodeDeserialize<T>.DeserializeFromXml(newXml);
			}
			catch (Exception ex)
			{
				//C# 11 syntax
				//ex.Data.Add("message", $"""
				//The XML parameter "{nameof(newXml)}" resulted in an error upon deserialization as type <T>.  
				//Check the following possible sources of error:
				//1) Does {nameof(newXml)} use an ITopNode element (e.g., <FormDesign>) as the root SDC XML element?  
				//   Try validating {nameof(newXml)} against the SDC Schema.
				//2) Does {nameof(newXml)} result in the correct <T> ITopNode type (e.g., <T> is <FormDesignType>)
				//"""
				//);
				ex.Data.Add("message",
					$"The XML parameter \"{nameof(newXml)}\" resulted in an error upon deserialization as type <T>.\r\n" +
					$"\tCheck the following possible sources of error:\r\n" +
					$"1) Does {nameof(newXml)} use an ITopNode element (e.g., <FormDesign>) as the root SDC XML element?\r\n" +
					$"\tTry validating {nameof(newXml)} against the SDC Schema.\r\n" +
					$"2) Does {nameof(newXml)} result in the correct <T> ITopNode type (e.g., <T> is <FormDesignType>)"
				);
				throw;
			}
			try
			{
				_prevVersion = ITopNodeDeserialize<T>.DeserializeFromXml(newXml);
			}
			catch (Exception ex)
			{
				//C# 11 syntax
				//ex.Data.Add("message", $"""
				//The XML parameter "{nameof(prevXml)}" resulted in an error upon deserialization as type <T>.  
				//Check the following possible sources of error:
				//1) Does {nameof(prevXml)} use an ITopNode element (e.g., <FormDesign>) as the root SDC XML element?  
				//   Try validating {nameof(prevXml)} against the SDC Schema.
				//2) Does {nameof(prevXml)} result in the correct <T> ITopNode type (e.g., <T> is <FormDesignType>)
				//"""
				//);
				ex.Data.Add("message",
					$"The XML parameter \"{nameof(prevXml)}\" resulted in an error upon deserialization as type <T>.\r\n" +
					$"\tCheck the following possible sources of error:\r\n" +
					$"1) Does {nameof(prevXml)} use an ITopNode element (e.g., <FormDesign>) as the root SDC XML element?\r\n" +
					$"\tTry validating {nameof(prevXml)} against the SDC Schema.\r\n" +
					$"2) Does {nameof(prevXml)} result in the correct <T> ITopNode type (e.g., <T> is <FormDesignType>)"
);
				throw;
			}

			CtorCompareTrees(_prevVersion, _newVersion);
		}
		private void CtorCompareTrees(T prevVersion, T newVersion)
		{
			_prevVersion = prevVersion;
			_newVersion = newVersion;

			_slAttPrev = GetSerializedXmlAttributesFromTree(_prevVersion);
			_slAttNew = GetSerializedXmlAttributesFromTree(_newVersion);

			ComputeAddedRemovedNodes();
		}

		#endregion

		public CompareTrees<T> ChangePrevVersion(T prevVersion)
		{
			_prevVersion = prevVersion;
			_slAttPrev = GetSerializedXmlAttributesFromTree(_prevVersion);
			ComputeAddedRemovedNodes();
			CompareVersions();
			return this;
		}
		public CompareTrees<T> ChangeNewVersion(T newVersion)
		{
			_newVersion = newVersion;
			_slAttNew = GetSerializedXmlAttributesFromTree(_newVersion);
			ComputeAddedRemovedNodes();
			CompareVersions();
			return this;
		}

		private void ComputeAddedRemovedNodes()
		{
			_IETnodesRemovedInNew = _prevVersion.IETnodes.Except(_newVersion.IETnodes).ToList(); //Prev nodes no longer found in New
			_IETnodesAddedInNew = _newVersion.IETnodes.Except(_prevVersion.IETnodes).ToList(); //New nodes that were not present in Prev
		}
		private ConcurrentDictionary<string, DifNodeIET>? CompareVersions()
		{
			var eqAttCompare = new SdcSerializedAttComparer(); //should be thread-safe
			var locker = new object();
			if (_slAttNew is null || _slAttPrev is null) return null;

			_slAttNew.AsParallel().ForAll(kv2 =>
			//slAttNew.All(kv2 =>
			{
				//Setup IET node data;
				string sGuidIET = kv2.Key;
				Guid GuidIET = ShortGuid.Decode(sGuidIET); //may need locking here - check source code
				bool isParChangedIET = false;
				bool isMovedIET = false;
				bool isNewIET = false;
				bool isRemovedIET = false;
				bool isAttListChanged = false;


				List<AttInfoDif> laiDif = new(); //For each IET node, there is one laiDif per subnode (including the IET node)
				Dictionary<string, List<AttInfoDif>> dlaiDif = new();  //the key is the IET sGuid; dlaiDif will be added later to difNodeIET, which will then be added to **d**DifNodeIET
				dlaiDif.Add(sGuidIET, laiDif); //add the laiDif to its dictionary; later we will stuff this laiDiff List object with attribute change data for the IET node and all of its subNodes.

				//we now have to populate laiDif with with AttInfoDif structs for each changed attribute
				//We also have to set all the above bool settings for difNodeIET
				//Then finally, we need to add one new dDifNodeIET struct entry (difNodeIET) for each New IET.
				////We can also add difNodeIET structs for Prev IET nodes Prev that were not present in New

				//holds the List<AttributeInfo> where the attributes differ from Prev to New; part of dDiffNodeIET; the key of the IET node sGuid.
				//laiDif will become the value part of dlaiDifIET

				IdentifiedExtensionType? ietPrev;
				if (_prevVersion.Nodes.TryGetValue(GuidIET, out BaseType? value))
					ietPrev = value as IdentifiedExtensionType;
				else ietPrev = null;


				if (ietPrev is not null)
				{
					var ietNew = _newVersion.Nodes[GuidIET] as IdentifiedExtensionType;

					//If New IET parent node is not the same as Prev parent node, mark as PARENT CHANGED
					if (ietPrev.ParentNode?.sGuid != ietNew?.ParentNode?.sGuid)
					{ isParChangedIET = true; }

					//If New IET prev sib node is not the same as Prev prev sib, mark as POSITION CHANGED (isMovedIET = true;)


					//TODO: see if we can add prev sib to the ai struct, to perhaps avoid this lookup
					//TODO: use a non-static thread-safe version of GetNodePreviousSib to avoid locking;  Thus it could not be an extension method
					//!- Tried to create thread-safe version unsuccessfully, so we still need a lock when looking up previous sib nodes,
					//! and potentially incurring the need for sorting of ChildNodes entries
					//var util = new SdcUtilParallel();
					//lock(locker) 	if (util.GetPrevSibElement(ietPrev)?.sGuid != util.GetPrevSibElement(ietNew)?.sGuid) //thread safe instance (?) method hierarchy with (hopefully) no shared state

					lock (locker) if (ietPrev.GetNodePreviousSib()?.sGuid != ietNew!.GetNodePreviousSib()?.sGuid)  //static extension method needs locking
						{ isMovedIET = true; }

					//Look for match in slAttPrev
					if (_slAttPrev.TryGetValue(kv2.Key, out var dlaiPrev))  //retrieve attribute dictionary for each New IET node
					{
						//Get the Prev IET attribute dictionary.  The first entry contains the New IET node data					
						List<AttributeInfo>? laiPrevIET = dlaiPrev[sGuidIET]; //could simply use dlaiPrev[0] instead
						AttributeInfo aiPrevIET = laiPrevIET[0];

						//loop through attributes of each New node under the current IET, test for a mismatched value
						var dlaiNew = kv2.Value; //dict contains List<AttributeInfo> for iet node and all non IET descendant nodes.

						foreach (var sGuidNew in dlaiNew.Keys) //loop through IET subNodes
						{

							var aiHashPrevIET = new HashSet<SdcSerializedAtt>(eqAttCompare);
							var aiHashNewIET = new HashSet<SdcSerializedAtt>(eqAttCompare);

							dlaiPrev.TryGetValue(sGuidNew, out var laiPrev); //Find matching subNode in Prev (using sGuidNew), and retrieve its serializable attributes (laiPrev)

							foreach (var aiNew in dlaiNew[sGuidNew]) //Loop through New **attributes** in the currrent subNode (with subNode key: sGuidNew)
							{
								aiHashNewIET.Add(new(sGuidNew, aiNew)); //document that the serializable attribute exists in New

								if (laiPrev is not null)
								{   //look for Prev subNode attribute match in laiPrev
									var aiPrev = laiPrev.FirstOrDefault(aiPrev => aiPrev.Name == aiNew.Name);  //TODO: can be optimized to remove Linq

									if (aiPrev != default) //matching serialized attributes were found on the Prev subNode
									{
										aiHashPrevIET.Add(new(sGuidNew, aiPrev)); //document that the serializable attribute exists in Prev

										if (aiPrev.ValueString != aiNew.ValueString) //See if the attribute values match;
																					 //TODO: could perhaps make this more efficient by doing direct compare of value types, instead of using ToString()
										{
											laiDif.Add(new AttInfoDif(sGuidNew, aiPrev, aiNew));
											isAttListChanged = true;
										}
									}
									else //if (aiPrev == default) //a matching serialized attribute was NOT found on the Prev subNode
									{
										//The Prev subNode does exist here.
										//aiPrev has default value -  so, the aiPrev attribute on the Prev subNode is not serialized (i.e., it's missing or at default tvalue) 
										laiDif.Add(new AttInfoDif(sGuidNew, default, aiNew));
										isAttListChanged = true;
									}
								}
								else //sGuidNew does not match a list of filled Prev attributes (laiPrev) on a matching Prev subNode (if it exists);
									 //we probably have a new New **sub**Node here, without a matching Prev **sub**Node
									 // (i.e., a New-matching Prev subNode does not exist),
									 // or maybe? it's a subNode with all default attributes (this should not happen here, as laiPrev should still exist, but with no values)
								{
									laiDif.Add(new AttInfoDif(sGuidNew, default, aiNew));
									isAttListChanged = true;
									//at present, there is no way to document that the Prev subNode did not exist, but that subNode info is not currently needed.
									//However, the (non-)existance of the Prev node can be determined easily from Prev Nodes dictionary.
								}
							}

							var attsRemovedInNew = aiHashPrevIET.Except(aiHashNewIET, eqAttCompare); //Add IEqualityComparer to only look at sGuid and Name; ai.Value is an object, which requires special handling (convert to string before comparing)

							//Document the New removed attributes in the laiDif List:
							//The missing attribute name/value can be found by querying on AttInfoDif.sGuidSubnode, and looking in AttInfoDif.aiPrev.Name and AttInfoDif.aiPrev.Value
							foreach (var rem in attsRemovedInNew)
							{
								laiDif.Add(new AttInfoDif(rem.sGuid,  rem.ai, default));  //note that aiNew is **default**; this indicates that **all** Prev serialized attribute were removed in New
								isAttListChanged = true;
							}

						}//looping through IET subNodes ends here


					}//retrieve a Prev attribute dictionary for each IET node ends here
					else //could not retrieve a Prev attribute dictionary (dlaiPrev) matching a New IET node, even though the Prev IET node exists;
						 //It should be present even if it has no Key/Value entries.
						 //If the Prev subNode was null, we would not be here.  See label **PrevSubNodeIsNull**
					{
						Debugger.Break();
						//throw error here?
					}
				}//Find matching Prev IET node ends here			
				else //matching ietPrev is not found in Prev Nodes
				{
				PrevSubNodeIsNull: isNewIET = true;
				}
				//finished looking for subNodes with attribute differences, as well as missing subnodes
				//Construct difNodeIET and add to dDifNodeIET for each IET node 

				DifNodeIET difNodeIET = new(sGuidIET, isParChangedIET, isMovedIET, isNewIET, isRemovedIET, isAttListChanged, dlaiDif);
				dDifNodeIET.AddOrUpdate(sGuidIET, difNodeIET, (sGuidIET, difNodeIET) => difNodeIET);

				//We could also use a ConcurrentBag<(string, DifNodeIET)>, and add nodes to a dictionary after this method completes 
				//We could also try a regular dictionary with a lock, but that might be slower if there are many Add contentions on the lock - needs testing 

				//TODO: Should we add isRemoved DifNodeIET entries, for IETs in Prev but not in New?  This is not strictly necessary 
				//!We could fill a hash table with all Prev matching nodes in this loop; the New nodes (or sGuids) not in the Prev-match hashtable were removed in New

				//return true;
			}//END of each New IET node loop processing in lambda
				);
			//Add Prev nodes that are not in New
			return dDifNodeIET;

			void CompareNodes()
			{
			}

			//  ------------------------------------------------------------------------------------
			void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				//char gt = ">"[0];
				const char gt = '>';
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {string.Empty.PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? string.Empty).PadRight(13)}| {ai.Value?.ToString()}");
			}
		}

		public SortedList<string, Dictionary<string, List<AttributeInfo>>> GetSerializedXmlAttributesFromTree(ITopNode topNode)
		{
			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------

			foreach (IdentifiedExtensionType iet in topNode.IETnodes)
			{
				var en = iet.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				//Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");

				Dictionary<string, List<AttributeInfo>> dlai = new();

				//process iet's child nodes and their attributes
				var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0, false);
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						var lai = SdcUtil.ReflectChildXmlAttributes(subNode);
						//Log(subNode, lai);
						dlai.Add(subNode.sGuid, lai);
					}
					dictAttr.Add(iet.sGuid, dlai);
				}
			}
			return dictAttr;
			//  ------------------------------------------------------------------------------------
			void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {string.Empty.PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? string.Empty).PadRight(13)}| {ai.Value?.ToString()}");
			}
		}

		public ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesRemovedInNew
		{ get => new (_IETnodesRemovedInNew); }
		public ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesAddedInNew
		{ get => new (_IETnodesRemovedInNew); }
		public DifNodeIET? GetIETattributes(IdentifiedExtensionType IETnode)
		{ return dDifNodeIET[IETnode.sGuid]; }
		public DifNodeIET? GetIETattributes(string sGuidIET)
		{ return dDifNodeIET[sGuidIET]; }
		public ReadOnlyDictionary<string, DifNodeIET>? GetIETattDiffs { get => new(dDifNodeIET); }//new(dDifNodeIET);} //C# 11 only: dDifNodeIET.AsReadOnly(); 
		public bool IsNewNodeAdded(BaseType nodeNew, out BaseType? NodePrev)
		=> _prevVersion.Nodes.TryGetValue(nodeNew.ObjectGUID, out NodePrev);
		public bool IsPrevNodeRemoved(BaseType prevNode, out BaseType? newNode)
		=> _newVersion.Nodes.TryGetValue(prevNode.ObjectGUID, out newNode);

		void test()
		{
			var t = GetIETnodesRemovedInNew.Append(new SectionItemType(null));
			//var d = GetIETattDiffs.Values.Append(new(GetIETattributes("A")));


		}
	}
}