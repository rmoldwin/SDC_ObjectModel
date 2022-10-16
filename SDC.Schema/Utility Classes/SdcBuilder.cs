using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema;

public class TopNodeBuilder<T> where T : class, ITopNode, new()
{
	BaseType? _parentNode;
	ITopNode _tempTopNode;
	
	readonly private T _newTopNode;
	private ITopNode? _ancestorTopNode;		
	private string? _id;

	public TopNodeBuilder()
	{
		_newTopNode = new();
		_tempTopNode = _newTopNode;
		_ancestorTopNode = _newTopNode;
	}

	/// <summary>
	/// Get the nearest ITopNode ancestor object to the ITopNode created in this class. <br/>
	/// If there is no ITopNode ancestor object, then ParentTopNode is a reference to TopNode
	/// </summary>
	public ITopNode? ParentTopNode { get => _ancestorTopNode; }

	/// <summary>
	/// Get the ID of the new ITopNode object
	/// </summary>
	public string? ID { get => _id; }

	/// <summary>
	/// The node that will be assigned to each SDC BaseType node's TopNode property, pointing to the closest ITopNode ancestor.<br/>
	/// The property is set during BaseType object construction.
	/// </summary>
	internal ITopNode TempTopNode
	{
		get => _tempTopNode??_newTopNode;
		set => _tempTopNode = value;			
	}

	/// <summary>
	/// Sets the nearest ITopNode ancestor object to the new ITopNode built in this class.<br/>
	/// If there is no ITopNode ancestor object, then ParentTopNode will be set to refer to the new ITopNode object
	/// </summary>
	/// <param name="ancestorTopNode"></param>
	/// <returns></returns>
	public TopNodeBuilder<T> SetAncestorTopNode(ITopNode ancestorTopNode)
	{
		if (ancestorTopNode is null) _ancestorTopNode = _newTopNode;
		else _ancestorTopNode = ancestorTopNode;
		return this;
	}
	/// <summary>
	/// Sets the parent node of the new ITopNode object, if present.  The node may be null;<br/>
	/// For new FormDesign nodes, parentNode must be either XMLPackageType or InjectFormType
	/// For RetrieveFormPackageType nodes, parentNode must be RetrieveFormPackageType
	/// For other new ITopNode nodes, parentNode must be XMLPackageType
	/// </summary>
	/// <param name="parentNode"></param>
	/// <returns></returns>
	public TopNodeBuilder<T> SetParentNode(BaseType parentNode)
	{
		if (parentNode is null) throw new NullReferenceException("parentNode cannot be null");
		switch (_newTopNode)
		{
			case RetrieveFormPackageType rfpt:
				//parentNode must be RetrieveFormPackageType
				switch (parentNode)
				{
					case RetrieveFormPackageType parRfpt:
						parRfpt.Items.Add(rfpt);
						break;
					default:
						throw new InvalidDataException("parentNode must be XMLPackageType");
				}
				break;
			case MappingType mt:
				//parentNode must be XMLPackageType
				switch (parentNode)
				{
					case XMLPackageType xpt:
						xpt.MapTemplate.Add(mt);
						break;
					default:
						throw new InvalidDataException("parentNode must be XMLPackageType");
				}
				break;
			case DataElementType det:
				//parentNode must be XMLPackageType
				switch (parentNode)
				{
					case XMLPackageType xpt:
						xpt.DataElement.Add(det);
						break;
					default:
						throw new InvalidDataException("parentNode must be XMLPackageType");
				}
				break;
			case DemogFormDesignType dfdt:
				//parentNode must be XMLPackageType
				switch (parentNode)
				{
					case XMLPackageType xpt:
						xpt.Item = dfdt;
						break;
					default:
						throw new InvalidDataException("parentNode must be XMLPackageType");
				}
				break;
			case FormDesignType fdt:
				//parentNode must be XMLPackageType or InjectFormType
				switch (parentNode)
				{
					case XMLPackageType xpt:
						xpt.FormDesign.Add(fdt);
						break;
					case InjectFormType ift:
						ift.Item = fdt;
						break;
					default:
						throw new InvalidDataException("parentNode must be either XMLPackageType or InjectFormType");
				}
				break;


		}
		_parentNode = parentNode;


		return this;
	}
	/// <summary>
	/// Set the SDC ID for the new TopNode, if it is a subtype of IdentifiedExtensionType.  
	/// <br/>If not set, then an id will be created for it.<br/>
	/// If the new TopNode is not a subtype of IdentifiedExtensionType, an exception will be thrown.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public TopNodeBuilder<T> SetID(string id)
	{
		if (_newTopNode is not IdentifiedExtensionType) throw new InvalidOperationException("The id property is only applicable to IdentifiedExtensionType subtypes");
		_id = id;
		return this;
	}

	/// <summary>
	/// Build and return the new ITopNode object
	/// </summary>
	/// <returns></returns>
	public T Build ()
	{
		var ntn = _newTopNode as IdentifiedExtensionType;
		if (ntn is not null) ntn.ID = _id;
		TempTopNode = _tempTopNode;

		//(_newTopNode as BaseType)!.TopNode = _ancestorTopNode!;
		return _newTopNode;
	}

	public T Init(ITopNode parentTopNode, ITopNode? parentNode = null, string id = "")
	{
		if (parentTopNode is null) parentTopNode = _newTopNode;
		return _newTopNode;
	}
}
