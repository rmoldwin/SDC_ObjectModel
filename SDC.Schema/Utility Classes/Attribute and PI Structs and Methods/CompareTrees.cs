using CSharpVitamins;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel; //contains ReadOnly collections
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

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
		private List<BaseType> _nodesRemovedInNew;
		private List<BaseType> _nodesAddedInNew;

		private readonly ConcurrentDictionary<string, DifNodeIET> _dDifNodeIET = new(); //the key is the IET node sGuid. Holds attribute changes in all IET and subNodes
																						//foreach(var kvNewIET in slAttNew)
		private SDCsGuidEqualityComparer<BaseType> _sGuidEqComparerBase = new();
		private SDCsGuidEqualityComparer<IdentifiedExtensionType> _sGuidEqComparerIET = new();
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

			_slAttPrev = FindSerializedXmlAttributesFromTree(_prevVersion);
			_slAttNew = FindSerializedXmlAttributesFromTree(_newVersion);

			ComputeAddedRemovedNodes();
			CompareVersionAttributes();
		}

		#endregion
		
		public T NewVersion
		{
			get => _newVersion;
			set => ChangeNewVersion(value);
		}
		public T PrevVersion
		{
			get => _prevVersion;
			set => ChangePrevVersion(value);
		}

		private CompareTrees<T> ChangePrevVersion(T prevVersion)
		{
			_prevVersion = prevVersion;
			_slAttPrev = FindSerializedXmlAttributesFromTree(_prevVersion);
			ComputeAddedRemovedNodes();
			CompareVersionAttributes();
			return this;
		}
		private CompareTrees<T> ChangeNewVersion(T newVersion)
		{
			_newVersion = newVersion;
			_slAttNew = FindSerializedXmlAttributesFromTree(_newVersion);
			ComputeAddedRemovedNodes();
			CompareVersionAttributes();
			return this;
		}

		private void ComputeAddedRemovedNodes()
		{
			_IETnodesRemovedInNew = _prevVersion.IETnodes.Except(_newVersion.IETnodes, _sGuidEqComparerIET).ToList(); //Prev nodes no longer found in New
			_IETnodesAddedInNew = _newVersion.IETnodes.Except(_prevVersion.IETnodes, _sGuidEqComparerIET).ToList(); //New nodes that were not present in Prev

			//Collection<BaseType> test = new();
			BaseType[] test = { };
			var prevNodes = _prevVersion.Nodes.Values;
			var newNodes = _newVersion.Nodes.Values;
			_nodesRemovedInNew = prevNodes.Except(newNodes, _sGuidEqComparerBase).ToList(); //Prev nodes no longer found in New
			_nodesAddedInNew = newNodes.Except(prevNodes, _sGuidEqComparerBase).ToList(); //New nodes that were not present in Prev

		}
		/// <summary>
		/// Fill and return _dDifNodeIET, a dictionary of all NewVersion IET nodes that are new in this version, 
		/// or have had XML attribute changes when compared to PrevVersion.  
		/// </summary>
		/// <returns></returns>
		private ConcurrentDictionary<string, DifNodeIET>? CompareVersionAttributes()
		{
			var eqAttCompare = new SdcSerializedAttComparer(); //should be thread-safe
			var locker = new object();
			if (_slAttNew is null || _slAttPrev is null) return null;
			_dDifNodeIET.Clear();

			//With _slAttNew and _slAttPrev, we are only looking at nodes that have attributes that will be serialized to XML.  All other nodes are skipped.
			//Since we are parellelizing the algorith, the results will not be in node order.  They can be sorted later

			_slAttNew.AsParallel().ForAll(kvNewIET =>
			//slAttNew.All(kvNewIET =>
			{
				//Setup IET node data;
				string sGuidNewIET = kvNewIET.Key;
				Dictionary<string, List<AttributeInfo>>? dlaiNewIET = kvNewIET.Value; //Contains List<AttributeInfo> for IET node and all non IET descendant nodes. Key is the IET & subnode sGuid
				Guid GuidIET = ShortGuid.Decode(sGuidNewIET); //may need locking here - check source code
				bool isParChangedIET = false;
				bool isMovedIET = false;
				bool isNewIET = false;
				bool isRemovedIET = false;
				bool isAttListChanged = false;
				bool hasAddedSubNodes = false;
				bool hasRemovedSubNodes = false;
				List<BaseType>? addedSubNodes = null;
				List<BaseType>? removedSubNodes = null;				

				List<AttInfoDif> laiDifSubNodes = new(); //For each IET node, there is one laiDifSubNodes per subnode (including the IET node)
				Dictionary<string, List<AttInfoDif>> dlaiDifIET = new();  //the key is the IET sGuid; dlaiDifIET will be added later to difNodeIET, which will then be added to **d**DifNodeIET
				dlaiDifIET.Add(sGuidNewIET, laiDifSubNodes); //add the laiDifSubNodes to its dictionary; later we will stuff this laiDiff List object with attribute change data for the IET node and all of its subNodes.


				//We now have to populate laiDifSubNodes with with AttInfoDif structs for each changed attribute
				//We also have to set all the above bool settings for difNodeIET
				//Then finally, we need to add one new _dDifNodeIET struct entry (difNodeIET) for each New IET.
				////We can also add difNodeIET structs for Prev IET nodes Prev that were not present in New

				//holds the List<AttributeInfo> where the attributes differ from Prev to New; part of dDiffNodeIET; the key of the IET node sGuid.
				//laiDifSubNodes will become the value part of dlaiDifIET

				IdentifiedExtensionType? ietPrev;
				if (_prevVersion.Nodes.TryGetValue(GuidIET, out BaseType? value))
					ietPrev = value as IdentifiedExtensionType;
				else ietPrev = null;

				if (ietPrev is not null)
				{
					//if (sGuidNewIET == "WhIrlfe5f0-ukxg8DOyZ7w") Debugger.Break(); //Why is this unchanged LI node flagged with new and removed subNodes?
					//if (sGuidNewIET == "lmxweaPWI0W5tUPPegM0Qw") Debugger.Break(); //LI Node with new/moved subnodes:  Property and LIRF subnodes from t6xPFRjcrkKxwMXRq7H4YA
					//if (sGuidNewIET == "t6xPFRjcrkKxwMXRq7H4YA") Debugger.Break(); //LI Node with removed subnodes: Property and LIRF subnodes

					//Check for added or removed subnodes, by comparing the the matching ietPrev node:
					lock (locker) removedSubNodes = FindRemovedIETsubNodes(sGuidNewIET);
					if (removedSubNodes is not null && removedSubNodes.Count > 0) hasRemovedSubNodes = true; 
					lock (locker) addedSubNodes = FindAddedIETsubNodes(sGuidNewIET);
					if (addedSubNodes is not null && addedSubNodes.Count > 0) hasAddedSubNodes = true;  //this step is required to flag possibly changed attributes on IET subNodes.																										

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
					if (_slAttPrev.TryGetValue(sGuidNewIET, out var dlaiPrevIET))  //retrieve serialized attribute dictionary for each New IET node
					{
						//Get the Prev IET attribute dictionary.  The first entry contains the New IET node data					
						//List<AttributeInfo> laiPrevIET = dlaiPrevIET[sGuidNewIET]; //could simply use dlaiPrevIET.Values[0] instead
						//AttributeInfo aiPrevIET = laiPrevIET[0];

						//loop through attributes of each New node under the current IET, test for a mismatched value

						foreach (var sGuidNewSubNode in dlaiNewIET.Keys) //loop through IET subNodes, whose sGuids are the keys to dlaiNewIET
						{
							//The first sub-node retrieved is the IET node itself
							//Check for matching Prev subnode here

							var aiHashPrevIET = new HashSet<SdcSerializedAtt>(eqAttCompare); //holds nodes with serialized attributes in Prev that match a node in New; This is not a complete collection of Prev nodes 
							var aiHashNewIET = new HashSet<SdcSerializedAtt>(eqAttCompare);  //holds nodes with serialized attributes in New

							dlaiPrevIET.TryGetValue(sGuidNewSubNode, out var laiPrevSubNode); //Find matching subNode in Prev (using sGuidNewSubNode), and retrieve its serializable attributes (laiPrevSubNode)

							foreach (var aiNewSubNode in dlaiNewIET[sGuidNewSubNode]) //Loop through New serialized **attributes** in the currrent New subNode (with subNode key: sGuidNewSubNode)
							{
								aiHashNewIET.Add(new(sGuidNewSubNode, aiNewSubNode)); //document that the serialized attribute exists in New

								if (laiPrevSubNode is not null)
								{   //look for Prev subNode serialized-attribute match in laiPrevSubNode
									var aiPrevSubNode = laiPrevSubNode.FirstOrDefault(aiPrevSubNode => aiPrevSubNode.Name == aiNewSubNode.Name);  //TODO: can be optimized to remove Linq

									//COMPARE ATTRIBUTES

									if (aiPrevSubNode != default) 
									//a matching serialized attribute (represented by aiPrevSubNode) was found on the Prev subNode.  It matches aiNewSubNode
									{
										aiHashPrevIET.Add(new(sGuidNewSubNode, aiPrevSubNode)); //document that the serializable attribute exists in the matching node from  Prev tree

										if (aiPrevSubNode.ValueString != aiNewSubNode.ValueString) //See if the attribute values match;  
										{
											laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, aiPrevSubNode, aiNewSubNode));
											isAttListChanged = true;
										}
									}
									else //if (aiPrevSubNode == default) //a matching serialized attribute was NOT found on the Prev subNode
									{
										//The Prev subNode does exist here, but all its attributes are at default values.
										//aiPrevSubNode has default value -  so, the aiPrevSubNode attribute on the Prev subNode is not serialized (i.e., it's missing or at default tvalue) 
										if (//aiNewSubNode.ValueString is not null 
										aiNewSubNode.ValueString != aiNewSubNode.DefaultValueString //TODO: Can probably shorten these comparisons to just this one line.
										//&& aiNewSubNode.Value != aiNewSubNode.DefaultValue
										)
										{
											laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, default, aiNewSubNode));
											isAttListChanged = true;
										}
									}
								}
								else //laiPrevSubNode is null here //sGuidNewSubNode does not match a list of filled Prev attributes (laiPrevSubNode) on a matching Prev subNode (if it exists);
									 //we probably have a new New **sub**Node here, without a matching Prev **sub**Node
									 // (i.e., a New-matching Prev subNode does not exist),
									 // or maybe? it's a subNode with all default attributes (this should not happen here, as laiPrevSubNode should still exist (not null), but with no values (count = 0))
								{
									laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, null, aiNewSubNode));									
									//isAttListChanged = true;
								}
							}
							//Uses SdcSerializedAttComparer eqAttCompare to only look at sGuid and Name;
							//ai.Value is an object, which requires special handling (convert to string before comparing)
							var attsRemovedInNew = aiHashPrevIET.Except(aiHashNewIET, eqAttCompare); 

							//Document the New removed attributes in the laiDifSubNodes List:
							//The missing attribute name/value can be found by querying on AttInfoDif.sGuidSubnode, and looking in AttInfoDif.aiPrevSubNode.Name and AttInfoDif.aiPrevSubNode.Value
							foreach (var rem in attsRemovedInNew)
							{
								laiDifSubNodes.Add(new AttInfoDif(rem.sGuid,  rem.ai, null));  //note that aiNewSubNode is null ; this indicates that Prev does not exist or  **all** Prev serialized attribute were removed in New
								isAttListChanged = true;
							}

						}//looping through IET subNodes ends here


					}//retrieve a Prev attribute dictionary for each IET node ends here
					else //could not retrieve a Prev serialized attribute dictionary (dlaiPrevIET) matching a New IET node, even though the Prev IET node exists;
						 //It should be present even if it has no Key/Value entries.
						 //If the Prev subNode was null, we would not be here.  See label **PrevSubNodeIsNull**
					{
						Debugger.Break();
						//throw error here?
					}
				}//Find matching Prev IET node ends here			
				else //matching ietPrev is not found in Prev Nodes
				{
					isNewIET = true;
				}
				//finished looking for subNodes with attribute differences, as well as missing subnodes
				//Construct difNodeIET and add to _dDifNodeIET for each IET node 

				DifNodeIET difNodeIET = new(sGuidNewIET, isParChangedIET, isMovedIET, isNewIET, isRemovedIET, isAttListChanged, 
					hasAddedSubNodes, hasRemovedSubNodes, addedSubNodes, removedSubNodes, dlaiDifIET);

				_dDifNodeIET.AddOrUpdate(sGuidNewIET, difNodeIET, (sGuidIET, difNodeIET) => difNodeIET);

				//We could also use a ConcurrentBag<(string, DifNodeIET)>, and add nodes to a dictionary after this method completes 
				//We could also try a regular dictionary with a lock, but that might be slower if there are many Add contentions on the lock - needs testing 

				//TODO: Should we add isRemoved DifNodeIET entries, for IETs in Prev but not in New?  This is not strictly necessary 
				//!We could fill a hash table with all Prev matching nodes in this loop; the New nodes (or sGuids) not in the Prev-match hashtable were removed in New

				//return true;
			}//END of each New IET node loop processing in lambda
				);
			//Add Prev nodes that are not in New
			return _dDifNodeIET;

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


		private DifNodeIET CompareIET(IdentifiedExtensionType iet)
		{
			var eqAttCompare = new SdcSerializedAttComparer(); //should be thread-safe
			var locker = new object();
			if (_slAttNew is null || _slAttPrev is null) throw new InvalidOperationException ("_slAttNew or  is null");

			var lai = SdcUtil.ReflectNodeXmlAttributes(iet, false);

				//Setup IET node data;
				string sGuidNewIET = iet.sGuid;
				Dictionary<string, List<AttributeInfo>> dlaiNewIET = FindSerializedXmlAttributesIET(iet); //Contains List<AttributeInfo> for IET node and all non IET descendant nodes. Key is the IET & subnode sGuid
				
				Guid GuidIET = ShortGuid.Decode(sGuidNewIET); //may need locking here - check source code
				bool isParChangedIET = false;
				bool isMovedIET = false;
				bool isNewIET = false;
				bool isRemovedIET = false;
				bool isAttListChanged = false;
				bool hasAddedSubNodes = false;
				bool hasRemovedSubNodes = false;
				List<BaseType>? addedSubNodes = null;
				List<BaseType>? removedSubNodes = null;

				List<AttInfoDif> laiDifSubNodes = new(); //For each IET node, there is one laiDifSubNodes per subnode (including the IET node)
				Dictionary<string, List<AttInfoDif>> dlaiDifIET = new();  //the key is the IET sGuid; dlaiDifIET will be added later to difNodeIET, which will then be added to **d**DifNodeIET
				dlaiDifIET.Add(sGuidNewIET, laiDifSubNodes); //add the laiDifSubNodes to its dictionary; later we will stuff this laiDiff List object with attribute change data for the IET node and all of its subNodes.


				//We now have to populate laiDifSubNodes with with AttInfoDif structs for each changed attribute
				//We also have to set all the above bool settings for difNodeIET
				//Then finally, we need to add one new _dDifNodeIET struct entry (difNodeIET) for each New IET.
				////We can also add difNodeIET structs for Prev IET nodes Prev that were not present in New


				IdentifiedExtensionType? ietPrev;
				if (_prevVersion.Nodes.TryGetValue(GuidIET, out BaseType? btPrev))
					ietPrev = btPrev as IdentifiedExtensionType;
				else ietPrev = null;

				if (ietPrev is not null)
				{

					//Check for added or removed subnodes, by comparing the the matching ietPrev node:
					lock (locker) removedSubNodes = FindRemovedIETsubNodes(sGuidNewIET);
					if (removedSubNodes is not null && removedSubNodes.Count > 0) hasRemovedSubNodes = true;
					lock (locker) addedSubNodes = FindAddedIETsubNodes(sGuidNewIET);
					if (addedSubNodes is not null && addedSubNodes.Count > 0) hasAddedSubNodes = true;  //this step is required to flag possibly changed attributes on IET subNodes.																										

					var ietNew = _newVersion.Nodes[GuidIET] as IdentifiedExtensionType;

					//If New IET parent node is not the same as Prev parent node, mark as PARENT CHANGED
					if (ietPrev.ParentNode?.sGuid != ietNew?.ParentNode?.sGuid)
					{ isParChangedIET = true; }


					lock (locker) if (ietPrev.GetNodePreviousSib()?.sGuid != ietNew!.GetNodePreviousSib()?.sGuid)  //static extension method needs locking
						{ isMovedIET = true; }

					//Look for match in slAttPrev
					if (_slAttPrev.TryGetValue(sGuidNewIET, out var dlaiPrevIET))  //retrieve serialized attribute dictionary for each New IET node
					{

						//loop through attributes of each New node under the current IET, test for a mismatched value

						foreach (var sGuidNewSubNode in dlaiNewIET.Keys) //loop through IET subNodes, whose sGuids are the keys to dlaiNewIET
						{
							//The first sub-node retrieved is the IET node itself
							//Check for matching Prev subnode here

							var aiHashPrevIET = new HashSet<SdcSerializedAtt>(eqAttCompare); //holds nodes with serialized attributes in Prev that match a node in New; This is not a complete collection of Prev nodes 
							var aiHashNewIET = new HashSet<SdcSerializedAtt>(eqAttCompare);  //holds nodes with serialized attributes in New

							dlaiPrevIET.TryGetValue(sGuidNewSubNode, out var laiPrevSubNode); //Find matching subNode in Prev (using sGuidNewSubNode), and retrieve its serializable attributes (laiPrevSubNode)

							foreach (var aiNewSubNode in dlaiNewIET[sGuidNewSubNode]) //Loop through New serialized **attributes** in the currrent New subNode (with subNode key: sGuidNewSubNode)
							{
								aiHashNewIET.Add(new(sGuidNewSubNode, aiNewSubNode)); //add the serialized AttributeInfo struct (ai) for NewSubNode
								if (laiPrevSubNode is not null)
								{   //look for Prev subNode serialized-attribute match in laiPrevSubNode
									var aiPrevSubNode = laiPrevSubNode.FirstOrDefault(aiPrevSubNode => aiPrevSubNode.Name == aiNewSubNode.Name);

								//COMPARE ATTRIBUTES

								if (aiPrevSubNode != default)
								//a matching serialized attribute (represented by aiPrevSubNode) was found on the Prev subNode.  It matches aiNewSubNode
								{
									aiHashPrevIET.Add(new(sGuidNewSubNode, aiPrevSubNode)); //add the serialized AttributeInfo struct (ai) for PrevSubNode

									if (aiPrevSubNode.ValueString != aiNewSubNode.ValueString) //See if the attribute values match;  
									{
										laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, aiPrevSubNode, aiNewSubNode));
										isAttListChanged = true;
									}
								}
								else //if (aiPrevSubNode == default) //a matching serialized attribute was NOT found on the Prev subNode
								{
									//The Prev subNode does exist here, but all its attributes are at default values.
									//aiPrevSubNode has default value -  so, the aiPrevSubNode attribute on the Prev subNode is not serialized (i.e., it's missing or at default tvalue) 
									if (
									//aiNewSubNode.ValueString is not null && 
									aiNewSubNode.ValueString != aiNewSubNode.DefaultValueString  //TODO: we probably only need this comparison
									//&& aiNewSubNode.Value != aiNewSubNode.DefaultValue
									)
									{
										laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, default, aiNewSubNode));
										isAttListChanged = true;
									}
								}
							}
							else //laiPrevSubNode is null here //sGuidNewSubNode does not match a list of filled Prev attributes (laiPrevSubNode) on a matching Prev subNode (if it exists);
								 //we probably have a new New **sub**Node here, without a matching Prev **sub**Node
								 // (i.e., a New-matching Prev subNode does not exist),
								 // or maybe? it's a subNode with all default attributes (this should not happen here, as laiPrevSubNode should still exist (not null), but with no values (count = 0))
							{
								laiDifSubNodes.Add(new AttInfoDif(sGuidNewSubNode, null, aiNewSubNode));
									//isAttListChanged = true;
								}
							}
							//Uses SdcSerializedAttComparer eqAttCompare to only look at sGuid and Name;
							//ai.Value is an object, which requires special handling (convert to string before comparing)
							var attsRemovedInNew = aiHashPrevIET.Except(aiHashNewIET, eqAttCompare);

							//Document the New removed attributes in the laiDifSubNodes List:
							//The missing attribute name/value can be found by querying on AttInfoDif.sGuidSubnode, and looking in AttInfoDif.aiPrevSubNode.Name and AttInfoDif.aiPrevSubNode.Value
							foreach (var rem in attsRemovedInNew)
							{
								laiDifSubNodes.Add(new AttInfoDif(rem.sGuid, rem.ai, null));  //note that aiNewSubNode is null ; this indicates that Prev does not exist or  **all** Prev serialized attribute were removed in New
								isAttListChanged = true;
							}

						}//looping through IET subNodes ends here


					}//retrieve a Prev attribute dictionary for each IET node ends here
					else //could not retrieve a Prev serialized attribute dictionary (dlaiPrevIET) matching a New IET node, even though the Prev IET node exists;
						 //It should be present even if it has no Key/Value entries.
						 //If the Prev subNode was null, we would not be here.  See label **PrevSubNodeIsNull**
					{
						Debugger.Break();
						//throw error here?
					}
				}//Find matching Prev IET node ends here			
					isNewIET = true;

				//finished looking for subNodes with attribute differences, as well as missing subnodes

				DifNodeIET difNodeIET = new(sGuidNewIET, isParChangedIET, isMovedIET, isNewIET, isRemovedIET, isAttListChanged,
					hasAddedSubNodes, hasRemovedSubNodes, addedSubNodes, removedSubNodes, dlaiDifIET);
			return difNodeIET;

		}

		public SortedList<string, Dictionary<string, List<AttributeInfo>>> FindSerializedXmlAttributesFromTree(ITopNode topNode)
		{
			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------

			foreach (IdentifiedExtensionType iet in topNode.IETnodes)
			{
				//var en = iet.ElementName;
				//int enLen = 36 - en.Length;
				//int pad = (enLen > 0) ? enLen : 0;
				//Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");
				

				{//DELETE THIS BLOCK
					//Dictionary<string, List<AttributeInfo>> dlai = new();

					////process iet's child nodes and their attributes
					//var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0, false);
					//if (sublist is not null)
					//{
					//	foreach (var subNode in sublist)
					//	{
					//		var lai = SdcUtil.ReflectNodeXmlAttributes(subNode, false);
					//		//Log(subNode, lai);
					//		dlai.Add(subNode.sGuid, lai);
					//	}
					//	dictAttr.Add(iet.sGuid, dlai);
					//}
				}

				//Log(subNode, lai);
				dictAttr.Add(iet.sGuid, FindSerializedXmlAttributesIET(iet));
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
		/// <summary>
		/// Retrieve all XML attributes that will be serialized to XML from the input IET node, <br/>
		/// and from all of its non-IET descendant nodes.
		/// </summary>
		/// <param name="iet">Input IET node</param>
		/// <returns>Dictionary of <see cref="AttributeInfo"/> structs, one struct per node, starting with the input IET node </returns>
		public Dictionary<string, List<AttributeInfo>> FindSerializedXmlAttributesIET(IdentifiedExtensionType iet)
		{
			Dictionary<string, List<AttributeInfo>> dlai = new();

			List<BaseType> sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0, false);
			foreach (var subNode in sublist)
			{
				List<AttributeInfo> lai = SdcUtil.ReflectNodeXmlAttributes(subNode, false);
				dlai.Add(subNode.sGuid, lai);
			}
			return dlai;
		}

		/// <summary>
		/// Find IET sub-nodes from <see cref="PrevVersion"/> that were added in <see cref="NewVersion"/>.<br/>
		/// The function expects an sGuid for an <see cref="IdentifiedExtensionType"/> node from <see cref="NewVersion"/>.<br/>
		/// If a matching IET node exists in <see cref="PrevVersion"/>, it returns a list of any of the IET's <br/>
		/// <see cref="NewVersion"/> sub-nodes that were added to (i.e., not found in) <see cref="PrevVersion"/>.
		/// </summary>
		/// <param name="sGuidIETnew">sGuid of the new IET node sub-tree to examine.</param>
		/// <returns>Null, if no matching previous IET node exists in <see cref="PrevVersion"/>.<br/>
		/// Empty List&lt;BaseType>, if a matching previous IET node exists in <see cref="PrevVersion"/>, but no added nodes were found.<br/>
		/// Otherwise, returns List&lt;BaseType> containing added IET sub-nodes that were not present in <see cref="PrevVersion"/>.
		/// </returns>
		public List<BaseType>? FindAddedIETsubNodes(ShortGuid sGuidIETnew)
		{
			//Get added nodes:
			//Get ietNew node from sGuidIETnew
			//Get list of ietNew descendants(ietNewSubNodes)
			//	For each newSubNode, look in _prevVersion.Nodes for matches
			//		For each missing prevSubNode, add newSubNode to the addedSubNodes

			if (_newVersion.Nodes.TryGetValue(ShortGuid.Decode(sGuidIETnew), out BaseType? btNew)
				&& btNew is IdentifiedExtensionType ietNew)
			{
				var addedSubNodes = new List<BaseType>();
				var ietNewSubNodes = SdcUtil.GetSortedNonIETsubtreeList(ietNew, -1, 0, false);
				for (int i = 1; i < ietNewSubNodes.Count; i++)//skip the first node, which is the IET node
				{
					var newSubNode = ietNewSubNodes[i];
					if (!_prevVersion.Nodes.TryGetValue(ShortGuid.Decode(newSubNode.sGuid), out BaseType? snPrev) //if newSubNode was not present in prevVersion
						|| snPrev.ParentIETnode?.sGuid != newSubNode.ParentIETnode?.sGuid  //or newSubNode/snPrev was present in PrevVersion, but does not share an IET parent																						   
						|| snPrev.ParentNode?.sGuid != newSubNode.ParentNode?.sGuid)       //or newSubNode/snPrev does not share a direct parent in the previous version
																						   //So, if newSubNode/snPrev was moved to a new IET parent or direct parent, we will consider it as "added" to that parent.
																						   //but newSubNode/snPrev may also be flagged as "removed" under the PreviousVersion node, if that node still exists in NewVersion 
						addedSubNodes.Add(newSubNode);
				}
				return addedSubNodes;
			}
			return null; //no matching ietPrev node was present in _prevVersion	
		}
		/// <summary>
		/// Find IET sub-nodes from <see cref="PrevVersion"/> that are no longer present in <see cref="NewVersion"/>.<br/>
		/// The function expects an sGuid for an <see cref="IdentifiedExtensionType"/> node from <see cref="NewVersion"/>.<br/>
		/// If a matching IET node exists in <see cref="PrevVersion"/>, it returns a list of any of the IET's <br/>
		/// <see cref="PrevVersion"/> sub-nodes that were removed from (i.e., not found in) <see cref="NewVersion"/>.
		/// </summary>
		/// <param name="sGuidIETnew">sGuid of the new IET node sub-tree to examine.</param>
		/// <returns>Null, if no matching previous IET node exists in <see cref="PrevVersion"/>.<br/>
		/// Empty List&lt;BaseType>, if a matching previous IET node exists in <see cref="PrevVersion"/>, but no removed nodes were found.<br/>
		/// Otherwise, returns List&lt;BaseType> containing removed IET sub-nodes that were present in <see cref="PrevVersion"/>.
		/// </returns>
		public List<BaseType>? FindRemovedIETsubNodes(ShortGuid sGuidIETnew)
		{
			//Get removed nodes:
			//Get matching ietPrev node from sGuidIETnew
			//Get list of ietPrev descendants(ietPrevSubNodes)
			//	For each prevSubNode, look in _newVersion.Nodes
			//		For each missing newSubNode, add prevSubNode to removedSubNodes

			if (_prevVersion.Nodes.TryGetValue(ShortGuid.Decode(sGuidIETnew), out BaseType? btPrev)
				&& btPrev is IdentifiedExtensionType ietPrev )
				{
					var removedSubNodes = new List<BaseType>();
					var ietPrevSubNodes = SdcUtil.GetSortedNonIETsubtreeList(ietPrev, -1, 0, false);
				for (int i = 1; i < ietPrevSubNodes.Count; i++) //skip the first node, which is the IET node
					{
					BaseType? prevSubNode = ietPrevSubNodes[i];
					if (!_newVersion.Nodes.TryGetValue(ShortGuid.Decode(prevSubNode.sGuid), out BaseType? snNew) //prevSubNode not found in NewVersion at all 
						 || snNew.ParentIETnode?.sGuid != prevSubNode.ParentIETnode?.sGuid //prevSubNode/snNew have different IET parents
						 || snNew.ParentNode?.sGuid != prevSubNode.ParentNode?.sGuid)      //prevSubNode/snNew have different direct parents
						//snNew is either not present in NewVersion, or has moved to a different parent, and has been removed from its PrevVersion parent
						removedSubNodes.Add(prevSubNode);
				}
					return removedSubNodes;
				}
			return null; //no matching ietPrev node was present in _prevVersion			
		}
		public ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesRemovedInNew
		{ get => new (_IETnodesRemovedInNew); }
		public ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesAddedInNew
		{ get => new (_IETnodesAddedInNew); }




		public ReadOnlyCollection<BaseType> GetNodesRemovedInNew
		{ get => new(_nodesRemovedInNew); }

		public ReadOnlyCollection<BaseType> GetNodesAddedInNew
		{ get => new(_nodesAddedInNew); }
		public DifNodeIET? GetIETattributes(IdentifiedExtensionType IETnode)
		{ return _dDifNodeIET.TryGetValue(IETnode.sGuid, out DifNodeIET dni) ? dni : null; }
		public DifNodeIET? GetIETattributes(ShortGuid sGuidIET)
		{ return _dDifNodeIET.TryGetValue(sGuidIET, out DifNodeIET dni )?dni:null ; }
		public ReadOnlyDictionary<string, DifNodeIET>? GetIETattDiffs { get => new(_dDifNodeIET); }
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