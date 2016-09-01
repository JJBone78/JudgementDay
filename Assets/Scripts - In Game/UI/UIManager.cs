using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour, IUIManager {
	
	//Singleton
	public static UIManager main;
	
	//Width of GUI menu
	private float m_GuiWidth;
	
	//Action Variables
	private HoverOver hoverOver = HoverOver.Land;
	private GameObject currentObject;
	
	//Mode Variables
	private Mode m_Mode = Mode.Normal;
	
	//Interface variables the UI needs to deal with
	private ISelectedManager m_SelectedManager;
	private ICamera m_Camera;
	private IGUIManager m_GuiManager;
	private IMiniMapController m_MiniMapController;
	
	//Building Placement variables
	private Action m_CallBackFunction;
	private Item m_ItemBeingPlaced;
	private GameObject m_ObjectBeingPlaced;
	private bool m_PositionValid = true;
	private bool m_Placed = false;
	
	public bool IsShiftDown
	{
		get;
		set;
	}
	
	public bool IsControlDown
	{
		get;
		set;
	}
	
	public Mode CurrentMode
	{
		get
		{
			return m_Mode;
		}
	}
	
	void Awake()
	{
		main = this;
	}

	// Use this for initialization
	void Start () 
	{
		//Resolve interface variables
		m_SelectedManager = ManagerResolver.Resolve<ISelectedManager>();
		m_Camera = ManagerResolver.Resolve<ICamera>();	
		m_GuiManager = ManagerResolver.Resolve<IGUIManager>();
		m_MiniMapController = ManagerResolver.Resolve<IMiniMapController>();
		
		//Attach Event Handlers
		IEventsManager eventsManager = ManagerResolver.Resolve<IEventsManager>();
		eventsManager.MouseClick += ButtonClickedHandler;
		eventsManager.MouseScrollWheel += ScrollWheelHandler;
		eventsManager.KeyAction += KeyBoardPressedHandler;
		eventsManager.ScreenEdgeMousePosition += MouseAtScreenEdgeHandler;
		
		//Attach gui width changed event	
		GUIEvents.MenuWidthChanged += MenuWidthChanged;
		
		//Loader.main.FinishedLoading (this);
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch (m_Mode)
		{
		case Mode.Normal:
			ModeNormalBehaviour ();
			break;
			
		case Mode.Menu:
			
			break;
			
		case Mode.PlaceBuilding:
			ModePlaceBuildingBehaviour();
			break;
		}
	}
	
	private void ModeNormalBehaviour()
	{
		//Handle all non event, and non gui UI elements here
		hoverOver = HoverOver.Land;
		InteractionState interactionState = InteractionState.Nothing;
		
		//Are we hovering over the GUI or the main screen?
		if (Input.mousePosition.x < Screen.width-m_GuiWidth)
		{
			//We're over the main screen, let's raycast
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;		
			
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, ~(1 << 16)))
			{
				currentObject = hit.collider.gameObject;
				switch (hit.collider.gameObject.layer)
				{
				case 8:
					//Friendly unit
					hoverOver = HoverOver.FriendlyUnit;
					break;
					
				case 9:
					//Enemy Unit
					hoverOver = HoverOver.EnemyUnit;
					break;
					
				case 11:
				case 17:
					//Terrain or shroud
					hoverOver = HoverOver.Land;
					break;
					
				case 12:
					//Friendly Building
					hoverOver = HoverOver.FriendlyBuilding;
					break;
					
				case 13:
					//Enemy building
					hoverOver = HoverOver.EnemyBuilding;
					break;
				}				
			}
		}
		else
		{
			//Mouse is over GUI
			hoverOver = HoverOver.Menu;
		}
		
		if (hoverOver == HoverOver.Menu || m_SelectedManager.ActiveObjectsCount() == 0 || m_GuiManager.GetSupportSelected != 0)
		{
			//Nothing orderable Selected or mouse is over menu or support is selected
			CalculateInteraction (hoverOver, ref interactionState);
		}
		else if (m_SelectedManager.ActiveObjectsCount() == 1)
		{
			//One object selected
			CalculateInteraction (m_SelectedManager.FirstActiveObject (), hoverOver, ref interactionState);
		}
		else
		{
			//Multiple objects selected, need to find which order takes precedence									
			CalculateInteraction (m_SelectedManager.ActiveObjectList(), hoverOver, ref interactionState);
		}
				
		//Tell the cursor manager to update itself based on the interactionstate
		CursorManager.main.UpdateCursor (interactionState);	
	}
	
	private void CalculateInteraction(HoverOver hoveringOver, ref InteractionState interactionState)
	{
		switch (hoveringOver)
		{
		case HoverOver.Menu:			
		case HoverOver.Land:
			//Normal Interaction
			interactionState = InteractionState.Nothing;
			break;
			
		case HoverOver.FriendlyBuilding:
			//Select interaction
			interactionState = InteractionState.Select;
			
			//Unless a support item is selected
			if (m_GuiManager.GetSupportSelected == Const.MAINTENANCE_Sell)
			{
				//Sell
				if (currentObject.GetComponent<Building>().CanSell ())
				{
					interactionState = InteractionState.Sell;
				}
				else
				{
					interactionState = InteractionState.CantSell;
				}
			}
			else if (m_GuiManager.GetSupportSelected == Const.MAINTENANCE_Fix)
			{
				//Fix
				if (currentObject.GetComponent<Building>().GetHealthRatio () < 1)
				{
					interactionState = InteractionState.Fix;
				}
				else
				{
					interactionState = InteractionState.CantFix;
				}
			}	
			else if (m_GuiManager.GetSupportSelected == Const.MAINTENANCE_Disable)
			{
				//Disable
			}
			break;
			
			
		case HoverOver.FriendlyUnit:		
		case HoverOver.EnemyUnit:
		case HoverOver.EnemyBuilding:
			//Select Interaction
			interactionState = InteractionState.Select;
			break;
			
		}
	}
	
	private void CalculateInteraction(IOrderable obj, HoverOver hoveringOver, ref InteractionState interactionState)
	{	
		if (obj.IsAttackable())
		{
			if (hoverOver == HoverOver.EnemyUnit || hoverOver == HoverOver.EnemyBuilding)
			{
				//Attack Interaction
				interactionState = InteractionState.Attack;
				return;
			}
		}
		
		if (obj.IsDeployable())
		{
			if (((RTSObject)obj).gameObject.Equals (currentObject))
			{
				//Deploy Interaction
				interactionState = InteractionState.Deploy;
				return;
			}
		}
		
		if (obj.IsInteractable ())
		{
			if (hoverOver == HoverOver.FriendlyUnit)
			{
				//Check if object can interact with unit (carry all for example)
				if (((IInteractable)obj).InteractWith(currentObject))
				{
					//Interact Interaction
					interactionState = InteractionState.Interact;
					return;
				}	
			}
		}
		
		if (obj.IsMoveable())
		{
			if (hoverOver == HoverOver.Land)
			{
				//Move Interaction
				interactionState = InteractionState.Move;
				return;
			}
		}
		
		if (hoverOver == HoverOver.FriendlyBuilding)
		{
			//Check if building can interact with object (repair building for example)
			if (currentObject.GetComponent<Building>().InteractWith (obj))
			{
				//Interact Interaction
					interactionState = InteractionState.Interact;
					return;
			}
		}
		
		if (hoverOver == HoverOver.FriendlyUnit || hoverOver == HoverOver.FriendlyBuilding || hoverOver == HoverOver.EnemyUnit || hoverOver == HoverOver.EnemyBuilding)
		{
			//Select Interaction
			interactionState = InteractionState.Select;
			return;
		}
		
		//Invalid interaction
		interactionState = InteractionState.Invalid;
	}
	
	private void CalculateInteraction(List<IOrderable> list, HoverOver hoveringOver, ref InteractionState interactionState)
	{
		foreach (IOrderable obj in list)
		{
			if (obj.ShouldInteract (hoveringOver))
			{
				CalculateInteraction (obj, hoveringOver, ref interactionState);
				return;
			}
		}
		
		CalculateInteraction (hoveringOver, ref interactionState);
	}
	
	private void ModePlaceBuildingBehaviour()
	{
		//Get current location and place building on that location
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, Mathf.Infinity, 1 << 11))
		{
			m_ObjectBeingPlaced.transform.position = hit.point;
		}
		
		//Check validity of current position
		if (Input.GetKeyDown ("v"))
		{
			m_PositionValid = !m_PositionValid;
			
			if (m_PositionValid)
			{
				m_ObjectBeingPlaced.GetComponent<BuildingBeingPlaced>().SetToValid();
			}
			else
			{
				m_ObjectBeingPlaced.GetComponent<BuildingBeingPlaced>().SetToInvalid();
			}
		}
	}
	
	//----------------------Mouse Button Handler------------------------------------
	private void ButtonClickedHandler(object sender, MouseEventArgs e)
	{
		//If mouse is over GUI then we don't want to process the button clicks
		if (e.X < Screen.width-m_GuiWidth)
		{
			e.Command ();
		}
	}
	//-----------------------------------------------------------------------------
	
	//------------------------Mouse Button Commands--------------------------------------------
	public void LeftButton_SingleClickDown(MouseEventArgs e)
	{
		switch (m_Mode)
		{
		case Mode.Normal:
			//We've left clicked, what have we left clicked on?
			int currentObjLayer = currentObject.layer;
			
			if (currentObjLayer == 8)
			{
				//Friendly Unit, is the unit selected?
				if (m_SelectedManager.IsObjectSelected(currentObject))
				{
					//Is the unit deployable?
					if (currentObject.GetComponent<Unit>().IsDeployable())
					{
						currentObject.GetComponent<Unit>().GiveOrder (Orders.CreateDeployOrder());
					}
				}
			}
			break;
			
		case Mode.PlaceBuilding:
			//We've left clicked, if we're valid place the building
			if (m_PositionValid)
			{
				GameObject newObject = (GameObject)Instantiate (m_ItemBeingPlaced.Prefab, m_ObjectBeingPlaced.transform.position, m_ItemBeingPlaced.Prefab.transform.rotation);
				UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(newObject, "Assets/Scripts - In Game/UI/UIManager.cs (376,5)", m_ItemBeingPlaced.ObjectType.ToString ());
				newObject.layer = 12;
				newObject.tag = "Player";
				
				BoxCollider tempCollider = newObject.GetComponent<BoxCollider>();
				
				if (tempCollider == null)
				{
					tempCollider = newObject.AddComponent<BoxCollider>();
				}
				
				tempCollider.center = m_ObjectBeingPlaced.GetComponent<BuildingBeingPlaced>().ColliderCenter;
				tempCollider.size = m_ObjectBeingPlaced.GetComponent<BuildingBeingPlaced>().ColliderSize;
				tempCollider.isTrigger = true;
				
				m_ItemBeingPlaced.FinishBuild ();
				m_CallBackFunction.Invoke ();
				m_Placed = true;
				SwitchToModeNormal ();
			}
			break;
		}
		
	}
	
	public void LeftButton_DoubleClickDown(MouseEventArgs e)
	{
		if (currentObject.layer == 8)
		{
			//Select all units of that type on screen
			
		}
	}
	
	public void LeftButton_SingleClickUp(MouseEventArgs e)
	{
		switch (m_Mode)
		{
		case Mode.Normal:
			//If we've just switched from another mode, don't execute
			if (m_Placed)
			{
				m_Placed = false;
				return;
			}
			
			//We've left clicked, have we left clicked on a unit?
			int currentObjLayer = currentObject.layer;
			if (!m_GuiManager.Dragging && (currentObjLayer == 8 || currentObjLayer == 9 || currentObjLayer == 12 || currentObjLayer == 13))
			{
				if (!IsShiftDown)
				{
					m_SelectedManager.DeselectAll ();
				}
				
				m_SelectedManager.AddObject (currentObject.GetComponent<RTSObject>());
			}
			else if (!m_GuiManager.Dragging)
			{
				m_SelectedManager.DeselectAll ();
			}
			break;
			
		case Mode.PlaceBuilding:
			if (m_Placed)
			{
				m_Placed = false;
			}
			break;
		}
	}
	
	public void RightButton_SingleClick(MouseEventArgs e)
	{
		switch (m_Mode)
		{
		case Mode.Normal:
			//We've right clicked, have we right clicked on ground, interactable object or enemy?
			int currentObjLayer = currentObject.layer;
			
			if (currentObjLayer == 11 || currentObjLayer == 17 || currentObjLayer == 18)
			{
				//Terrain -> Move Command
				m_SelectedManager.GiveOrder (Orders.CreateMoveOrder (e.WorldPosClick));
			}
			else if (currentObjLayer == 8 || currentObjLayer == 14)
			{
				//Friendly Unit -> Interact (if applicable)
			}
			else if (currentObjLayer == 9 || currentObjLayer == 15)
			{
				//Enenmy Unit -> Attack
			}
			else if (currentObjLayer == 12)
			{
				//Friendly Building -> Interact (if applicable)
			}
			else if (currentObjLayer == 13)
			{
				//Enemy Building -> Attack
			}
			break;
			
		case Mode.PlaceBuilding:
			
			//Cancel building placement
			
			
			SwitchToModeNormal();
			break;
		}
		
	}
	
	public void RightButton_DoubleClick(MouseEventArgs e)
	{
		
	}
	
	public void MiddleButton_SingleClick(MouseEventArgs e)
	{
		
	}
	
	public void MiddleButton_DoubleClick(MouseEventArgs e)
	{
		
	}
	//------------------------------------------------------------------------------------------
	
	private void ScrollWheelHandler(object sender, ScrollWheelEventArgs e)
	{
		//Zoom In/Out
		m_Camera.Zoom (sender, e);
		m_MiniMapController.ReCalculateViewRect ();
	}
	
	private void MouseAtScreenEdgeHandler(object sender, ScreenEdgeEventArgs e)
	{
		//Pan
		m_Camera.Pan (sender, e);
		m_MiniMapController.ReCalculateViewRect ();
	}
	
	//-----------------------------------KeyBoard Handler---------------------------------
	private void KeyBoardPressedHandler(object sender, KeyBoardEventArgs e)
	{
		e.Command();
	}
	//-------------------------------------------------------------------------------------
	
	public bool IsCurrentUnit(RTSObject obj)
	{
		return currentObject == obj.gameObject;
	}
	
	public void MenuWidthChanged(float newWidth)
	{
		m_GuiWidth = newWidth;
	}
	
	public void UserPlacingBuilding(Item item, Action callbackFunction)
	{
		SwitchToModePlacingBuilding(item, callbackFunction);
	}
	
	public void SwitchMode(Mode mode)
	{
		switch (mode)
		{
		case Mode.Normal:
			SwitchToModeNormal ();
			break;
			
		case Mode.Menu:
			
			break;
			
		case Mode.Disabled:
			
			break;
		}
	}
	
	private void SwitchToModeNormal()
	{
		if (m_ObjectBeingPlaced)
		{
			Destroy (m_ObjectBeingPlaced);
		}
		m_CallBackFunction = null;
		m_ItemBeingPlaced = null;
		m_Mode = Mode.Normal;
	}
	
	private void SwitchToModePlacingBuilding(Item item, Action callBackFunction)
	{
		m_Mode = Mode.PlaceBuilding;
		m_CallBackFunction = callBackFunction;
		m_ItemBeingPlaced = item;
		m_ObjectBeingPlaced = (GameObject)Instantiate (item.Prefab);
		m_ObjectBeingPlaced.AddComponent<BuildingBeingPlaced>();
	}
}

public enum HoverOver
{
	Menu,
	Land,
	FriendlyUnit,
	EnemyUnit,
	FriendlyBuilding,
	EnemyBuilding,
}

public enum InteractionState
{
	Nothing = 0,
	Invalid = 1,
	Move = 2,
	Attack = 3,
	Select = 4,
	Deploy = 5,
	Interact = 6,
	Sell = 7,
	CantSell = 8,
	Fix = 9,
	CantFix = 10,
	Disable = 11,
	CantDisable = 12,
}

public enum Mode
{
	Normal,
	Menu,
	PlaceBuilding,
	Disabled,
}
