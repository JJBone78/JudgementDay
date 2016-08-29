using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectedManager : MonoBehaviour, ISelectedManager {
	
	private List<IOrderable> SelectedActiveObjects = new List<IOrderable>();	
	private List<RTSObject> SelectedObjects = new List<RTSObject>();

	private List<List<RTSObject>> Group = new List<List<RTSObject>>();
	
	public static SelectedManager main;
	
	public int OverlayWidth
	{
		get;
		private set;
	}
		
	public void Awake()
	{
		for (int i=0; i<10; i++)
		{
			Group.Add (new List<RTSObject>());
		}
		
		main = this;
		
		OverlayWidth = 80;
	}
	
	public void AddObject (RTSObject obj)
	{
		if (!SelectedObjects.Contains (obj))
		{
			if (obj is IOrderable && obj.gameObject.layer == 8)
			{
				SelectedActiveObjects.Add ((IOrderable)obj);
			}
			SelectedObjects.Add (obj);
			obj.SetSelected ();
		}
	}
	
	public void DeselectAll()
	{
		foreach (RTSObject obj in SelectedObjects)
		{
			obj.SetDeselected ();
		}
		
		SelectedObjects.Clear ();
		SelectedActiveObjects.Clear ();
	}
	
	public void DeselectObject(RTSObject obj)
	{	
		if (obj is IOrderable)
		{
			SelectedActiveObjects.Remove ((IOrderable)obj);
		}
		obj.SetDeselected ();
		SelectedObjects.Remove (obj);		
	}
	
	public void GiveOrder(Order order)
	{
		foreach (IOrderable unit in SelectedActiveObjects)
		{
			unit.GiveOrder(order);
		}
	}
	
	public void AddUnitsToGroup(int groupNumber)
	{
		Group[groupNumber].Clear ();
		foreach (RTSObject obj in SelectedObjects)
		{
			Group[groupNumber].Add (obj);
			
		}		
	}
	
	public void SelectGroup(int groupNumber)
	{
		DeselectAll ();
		foreach (RTSObject obj in Group[groupNumber])
		{
			AddObject (obj);
		}
	}
	
	public int ActiveObjectsCount()
	{
		return SelectedActiveObjects.Count;
	}
	
	public IOrderable FirstActiveObject()
	{
		return SelectedActiveObjects[0];
	}
	
	public List<IOrderable> ActiveObjectList()
	{
		return SelectedActiveObjects;
	}
	
	public bool IsObjectSelected(GameObject obj)
	{
		return SelectedObjects.Contains (obj.GetComponent<RTSObject>());
	}
}
