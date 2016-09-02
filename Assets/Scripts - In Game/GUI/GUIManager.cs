using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GUIManager : MonoBehaviour, IGUIManager
{
    public static float _global_displayed_hour = 7.0F;
    public static float _global_displayed_minute = 0;
    public static float _global_game_speed = 1.0F;
    public static string _global_game_time = "";
    public static DayNightShift _global_day_night_shift = DayNightShift.Day;
    public Font _digital_font;
    private GUIStyle _global_clock_style = new GUIStyle();




    //Singleton
    public static GUIManager main;
	
	//Member Variables
	private Rect m_MiniMapRect;
	private float m_MainMenuWidth;
	
	private Rect m_RightMiniMapBG;
	private Rect m_LeftMiniMapBG;
	private Rect m_AboveMiniMapBG;
	private Rect m_BelowMiniMapBG;
	
	private Texture2D m_MainMenuBGColor;
	
	private ITypeButton[] m_TypeButtons = new TypeButton[5];
	private IMaintenanceButtons[] m_MaintenanceButtons = new IMaintenanceButtons[3];
	private IManager m_Manager;


    public enum DayNightShift
    {
        Day = 0,
        Night = 1
    };

    //Properties
    public float MainMenuWidth
	{
		get
		{
			return m_MainMenuWidth;
		}
		private set
		{
			if (Equals (m_MainMenuWidth, value))
			{
				return;
			}
			
			m_MainMenuWidth = value;
			
			GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
		}
	}
	
	public bool Dragging
	{
		get;
		set;
	}
	
	public Rect DragArea
	{
		get;
		set;
	}
	
	public int GetSupportSelected
	{
		get
		{
			return 0;
		}
	}
	
	void Awake()
	{
		//Set singleton
		main = this;
		
		//Set Textures
		m_MainMenuBGColor = TextureGenerator.MakeTexture (Color.black);
	}

	// Use this for initialization
	void Start () 
	{
		//Load the mini map and assign the menu width and mini map rect
		IMiniMapController miniMap = ManagerResolver.Resolve<IMiniMapController>();		
		float tempWidth;
		miniMap.LoadMiniMap(out tempWidth, out m_MiniMapRect);
		MainMenuWidth = tempWidth;
		
		//Build Borders around the map
		float sideBorderWidth = (m_MainMenuWidth-(m_MiniMapRect.width*Screen.width))/2;
		float topBorderHeight = (1-m_MiniMapRect.yMax)*Screen.height;
		
		m_LeftMiniMapBG = new Rect();
		m_LeftMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_LeftMiniMapBG.xMax = m_LeftMiniMapBG.xMin + sideBorderWidth;
		m_LeftMiniMapBG.yMin = 0;
		m_LeftMiniMapBG.yMax = (1-m_MiniMapRect.yMin)*Screen.height;
		
		m_RightMiniMapBG = new Rect();
		m_RightMiniMapBG.xMin = m_MiniMapRect.xMax*Screen.width;
		m_RightMiniMapBG.xMax = Screen.width;
		m_RightMiniMapBG.yMin = 0;
		m_RightMiniMapBG.yMax = (1-m_MiniMapRect.yMin)*Screen.height;
		
		m_AboveMiniMapBG = new Rect();
		m_AboveMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_AboveMiniMapBG.xMax = Screen.width;
		m_AboveMiniMapBG.yMin = 0;
		m_AboveMiniMapBG.yMax = topBorderHeight;
		
		m_BelowMiniMapBG = new Rect();
		m_BelowMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_BelowMiniMapBG.xMax = Screen.width;
		m_BelowMiniMapBG.yMin = ((1-m_MiniMapRect.yMin)*Screen.height)-1;
		m_BelowMiniMapBG.yMax = Screen.height;
		
		//Create viewable area rect
		Rect menuArea = new Rect();
		menuArea.xMin = m_LeftMiniMapBG.xMin;
		menuArea.xMax = m_RightMiniMapBG.xMax;
		menuArea.yMin = m_BelowMiniMapBG.yMin;
		menuArea.yMax = Screen.height;
		
		//Create type buttons
		m_TypeButtons[0] = new TypeButton(ButtonType.Building, menuArea);
		m_TypeButtons[1] = new TypeButton(ButtonType.Support, menuArea);
		m_TypeButtons[2] = new TypeButton(ButtonType.Infantry, menuArea);
		m_TypeButtons[3] = new TypeButton(ButtonType.Vehicle, menuArea);
		m_TypeButtons[4] = new TypeButton(ButtonType.Air, menuArea);
		
		//Calcualte Maintenace button rects
		float size = m_RightMiniMapBG.width-4;
		float totalHeight = size*3;
		float offSet = (m_RightMiniMapBG.height-totalHeight)/2;
		float x = m_RightMiniMapBG.xMin;
		float y = m_RightMiniMapBG.yMin+offSet;
		
		Rect rect1 = new Rect(x, y, size, size);
		Rect rect2 = new Rect(x, y+size, size, size);
		Rect rect3 = new Rect(x, y+(size*2), size, size);
		
		//Assign maintenance buttons
		m_MaintenanceButtons[0] = new Maintenance_Sell(rect1);
		m_MaintenanceButtons[1] = new Maintenance_Fix(rect2);
		m_MaintenanceButtons[2] = new Maintanance_Disable(rect3);
		
		//Resolve Manager
		m_Manager = ManagerResolver.Resolve<IManager>();
	}

  	void Update () 
	{
        if (Input.GetKeyUp(KeyCode.KeypadPlus)) //increase or decrease game speed based on numpad +/-
        {
            if (_global_game_speed == 0.1F)
                _global_game_speed = 0.3F;
            else if (_global_game_speed == 0.3F)
                _global_game_speed = 0.5F;
            else if (_global_game_speed == 0.5F)
                _global_game_speed = 0.7F;
            else if (_global_game_speed == 0.7F)
                _global_game_speed = 0.9F;
            else if (_global_game_speed == 0.9F)
                _global_game_speed = 1.0F;
            else if (_global_game_speed == 1.0F)
                _global_game_speed = 2.0F;
            else if (_global_game_speed == 2.0F)
                _global_game_speed = 3.0F;
            else if (_global_game_speed == 3.0F)
                _global_game_speed = 4.0F;
            else if (_global_game_speed == 4.0F)
                _global_game_speed = 5.0F;
        }
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            if (_global_game_speed == 0.3F)
                _global_game_speed = 0.1F;
            else if (_global_game_speed == 0.5F)
                _global_game_speed = 0.3F;
            else if (_global_game_speed == 0.7F)
                _global_game_speed = 0.5F;
            else if (_global_game_speed == 0.9F)
                _global_game_speed = 0.7F;
            else if (_global_game_speed == 1.0F)
                _global_game_speed = 0.9F;
            else if (_global_game_speed == 2.0F)
                _global_game_speed = 1.0F;
            else if (_global_game_speed == 3.0F)
                _global_game_speed = 2.0F;
            else if (_global_game_speed == 4.0F)
                _global_game_speed = 3.0F;
            else if (_global_game_speed == 5.0F)
                _global_game_speed = 4.0F;
        }
        Time.timeScale = _global_game_speed; //game speed

        //_timer_count = (Time.time - _timer_start) / 2;

        

        //if (((Mathf.Floor(_timer_count % 6)) * 10) == 0 && _hour_changed)
        //{
        //    if (_global_displayed_hour > 22)
        //        _global_displayed_hour = 0;
        //    else
        //        _global_displayed_hour += 1;
        //    _hour_changed = false;
        //}
        //if (((Mathf.Floor(_timer_count % 6)) * 10) == 10)
        //    _hour_changed = true;
        //_global_displayed_minute = (Mathf.Floor(_timer_count % 6)) * 10;




        _global_game_time = string.Format("{0:00}:{1:00}", _global_displayed_hour, _global_displayed_minute);
      

        //Tell all items that are being built to update themselves
        GUIEvents.TellItemsToUpdate(Time.deltaTime);
		
		if (Input.GetKeyDown ("r"))
		{
			Resize ();
		}
	}
	
	void OnGUI()
	{
		//Draw Menu Backgrounds
		GUI.DrawTexture (m_LeftMiniMapBG, m_MainMenuBGColor);
		GUI.DrawTexture (m_RightMiniMapBG, m_MainMenuBGColor);
		GUI.DrawTexture (m_AboveMiniMapBG, m_MainMenuBGColor);
		GUI.DrawTexture (m_BelowMiniMapBG, m_MainMenuBGColor);
		
		//Draw Type Buttons
		foreach (IButton typeButton in m_TypeButtons)
		{
			typeButton.Execute ();
		}
		
		//Draw maintenance buttons
		foreach (IMaintenanceButtons button in m_MaintenanceButtons)
		{
			button.Execute ();
		}
		
		//Draw Money Label
		GUI.Label(m_AboveMiniMapBG, m_Manager.Money.ToString (), GUIStyles.MoneyLabel);
        //draw in-game clock
        _global_clock_style.normal.textColor = Color.green;
        _global_clock_style.fontStyle = FontStyle.Bold;
        _global_clock_style.fontSize = 18;
        if (_digital_font != null)
            _global_clock_style.font = _digital_font;
        GUI.Label(new Rect(0, 0, 200, 50), _global_game_time, _global_clock_style);
    }

    public bool IsWithin(Vector3 worldPos)
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint (worldPos);
		Vector3 realScreenPos = new Vector3(screenPos.x, Screen.height-screenPos.y, screenPos.z);
		
		if (DragArea.Contains (realScreenPos))
		{
			return true;
		}
		
		return false;
	}
	
	public void UpdateQueueContents(List<Item> availableItems)
	{
		foreach (TypeButton typeButton in m_TypeButtons)
		{
			typeButton.UpdateQueueContents (availableItems);
		}
	}

	public void AddConstructor (Building building)
	{
		switch (building.ID)
		{
		case Const.BUILDING_ConYard:
			m_TypeButtons[0].AddNewQueue (building);
			m_TypeButtons[1].AddNewQueue (building);
			break;
			
		case Const.BUILDING_Barracks:
			m_TypeButtons[2].AddNewQueue (building);
			break;
		}
	}

	public void RemoveConstructor (Building building)
	{
		
	}

	public void Resize ()
	{
		//Resolution has changed, re-size all GUI elements
		//Mini map first
		ManagerResolver.Resolve<IMiniMapController>().LoadMiniMap (out m_MainMenuWidth, out m_MiniMapRect);
		
		//Build Borders around the map
		float sideBorderWidth = (m_MainMenuWidth-(m_MiniMapRect.width*Screen.width))/2;
		float topBorderHeight = (1-m_MiniMapRect.yMax)*Screen.height;
		
		m_LeftMiniMapBG = new Rect();
		m_LeftMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_LeftMiniMapBG.xMax = m_LeftMiniMapBG.xMin + sideBorderWidth;
		m_LeftMiniMapBG.yMin = 0;
		m_LeftMiniMapBG.yMax = (1-m_MiniMapRect.yMin)*Screen.height;
		
		m_RightMiniMapBG = new Rect();
		m_RightMiniMapBG.xMin = m_MiniMapRect.xMax*Screen.width;
		m_RightMiniMapBG.xMax = Screen.width;
		m_RightMiniMapBG.yMin = 0;
		m_RightMiniMapBG.yMax = (1-m_MiniMapRect.yMin)*Screen.height;
		
		m_AboveMiniMapBG = new Rect();
		m_AboveMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_AboveMiniMapBG.xMax = Screen.width;
		m_AboveMiniMapBG.yMin = 0;
		m_AboveMiniMapBG.yMax = topBorderHeight;
		
		m_BelowMiniMapBG = new Rect();
		m_BelowMiniMapBG.xMin = Screen.width-m_MainMenuWidth;
		m_BelowMiniMapBG.xMax = Screen.width;
		m_BelowMiniMapBG.yMin = ((1-m_MiniMapRect.yMin)*Screen.height)-1;
		m_BelowMiniMapBG.yMax = Screen.height;
		
		//Create viewable area rect
		Rect menuArea = new Rect();
		menuArea.xMin = m_LeftMiniMapBG.xMin;
		menuArea.xMax = m_RightMiniMapBG.xMax;
		menuArea.yMin = m_BelowMiniMapBG.yMin;
		menuArea.yMax = Screen.height;
		
		//Update type buttons with new viewable area
		foreach (ITypeButton button in m_TypeButtons)
		{
			button.Resize(menuArea);
		}
		
		//Calcualte Maintenace button rects
		float size = m_RightMiniMapBG.width-4;
		float totalHeight = size*3;
		float offSet = (m_RightMiniMapBG.height-totalHeight)/2;
		float x = m_RightMiniMapBG.xMin;
		float y = m_RightMiniMapBG.yMin+offSet;
		
		Rect rect1 = new Rect(x, y, size, size);
		Rect rect2 = new Rect(x, y+size, size, size);
		Rect rect3 = new Rect(x, y+(size*2), size, size);
		
		//Update Maintenance Buttons
		m_MaintenanceButtons[0].Resize (rect1);
		m_MaintenanceButtons[1].Resize (rect2);
		m_MaintenanceButtons[2].Resize (rect3);
	}
}
