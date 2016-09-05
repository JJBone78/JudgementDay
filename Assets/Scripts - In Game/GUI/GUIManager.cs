using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class GUIManager : MonoBehaviour, IGUIManager
{
    public static float _global_displayed_hour = 7.0F;
    public static float _global_displayed_minute = 0;
    public static float _global_game_speed = 1.0F;
    public static string _global_game_time = "";
    public static string _global_map_name = "Map name placeholder";
    public static DayNightShift _global_day_night_shift = DayNightShift.Day;
    public Font _digital_font;
    private bool _is_minimap_shown = true;
    public static bool _is_menu_shown = false;
    private GUIStyle _global_clock_style = new GUIStyle();
    public GUISkin _global_engine_skin;

    private Texture _horizontal_borders_texture;
    private Texture _vertical_borders_texture;
    private Texture _gui_elements_texture;
    private Texture _map_name_background_texture;
    Color32[] pix;

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
            if (Equals(m_MainMenuWidth, value))
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
        m_MainMenuBGColor = TextureGenerator.MakeTexture(Color.black);
    }

    // Use this for initialization
    void Start()
    {
        //Load the mini map and assign the menu width and mini map rect
        IMiniMapController miniMap = ManagerResolver.Resolve<IMiniMapController>();
        float tempWidth;
        miniMap.LoadMiniMap(out tempWidth, out m_MiniMapRect);
        MainMenuWidth = tempWidth;

        //Build Borders around the map
        float sideBorderWidth = (m_MainMenuWidth - (m_MiniMapRect.width * Screen.width)) / 2;
        float topBorderHeight = (1 - m_MiniMapRect.yMax) * Screen.height;

        m_LeftMiniMapBG = new Rect();
        m_LeftMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_LeftMiniMapBG.xMax = m_LeftMiniMapBG.xMin + sideBorderWidth;
        m_LeftMiniMapBG.yMin = 0;
        m_LeftMiniMapBG.yMax = (1 - m_MiniMapRect.yMin) * Screen.height;

        m_RightMiniMapBG = new Rect();
        m_RightMiniMapBG.xMin = m_MiniMapRect.xMax * Screen.width;
        m_RightMiniMapBG.xMax = Screen.width;
        m_RightMiniMapBG.yMin = 0;
        m_RightMiniMapBG.yMax = (1 - m_MiniMapRect.yMin) * Screen.height;

        m_AboveMiniMapBG = new Rect();
        m_AboveMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_AboveMiniMapBG.xMax = Screen.width;
        m_AboveMiniMapBG.yMin = 0;
        m_AboveMiniMapBG.yMax = topBorderHeight;

        m_BelowMiniMapBG = new Rect();
        m_BelowMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_BelowMiniMapBG.xMax = Screen.width;
        m_BelowMiniMapBG.yMin = ((1 - m_MiniMapRect.yMin) * Screen.height) - 1;
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
        float size = m_RightMiniMapBG.width - 4;
        float totalHeight = size * 3;
        float offSet = (m_RightMiniMapBG.height - totalHeight) / 2;
        float x = m_RightMiniMapBG.xMin;
        float y = m_RightMiniMapBG.yMin + offSet;

        Rect rect1 = new Rect(x, y, size, size);
        Rect rect2 = new Rect(x, y + size, size, size);
        Rect rect3 = new Rect(x, y + (size * 2), size, size);

        //Assign maintenance buttons
        m_MaintenanceButtons[0] = new Maintenance_Sell(rect1);
        m_MaintenanceButtons[1] = new Maintenance_Fix(rect2);
        m_MaintenanceButtons[2] = new Maintanance_Disable(rect3);

        //Resolve Manager
        m_Manager = ManagerResolver.Resolve<IManager>();











        _horizontal_borders_texture = Resources.Load("Textures/GUI/HorizontalBorders") as Texture;
        _vertical_borders_texture = Resources.Load("Textures/GUI/VerticalBorders") as Texture;
        _gui_elements_texture = Resources.Load("Textures/GUI/horizontalUIelements") as Texture;
        _map_name_background_texture = Resources.Load("Textures/GUI/GenericBg") as Texture;
        //pix = sourceTex.GetPixels32();
        //Array.Reverse(pix);

    }

    void Update()
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
        _global_game_time = string.Format("{0:00}:{1:00}", _global_displayed_hour, _global_displayed_minute);
        //Tell all items that are being built to update themselves
        GUIEvents.TellItemsToUpdate(Time.deltaTime);
        if (Input.GetKeyDown("r"))
        {
            Resize();
        }
    }

    void OnGUI()
    {
        GUI.skin = _global_engine_skin; //override default game skin
        //Draw Menu Backgrounds
        GUI.DrawTexture(m_LeftMiniMapBG, m_MainMenuBGColor);
        GUI.DrawTexture(m_RightMiniMapBG, m_MainMenuBGColor);
        GUI.DrawTexture(m_AboveMiniMapBG, m_MainMenuBGColor);
        GUI.DrawTexture(m_BelowMiniMapBG, m_MainMenuBGColor);

        //Draw Type Buttons

        foreach (IButton typeButton in m_TypeButtons)
        {
            if (typeButton != null)
                typeButton.Execute();
        }
        //Draw maintenance buttons
        foreach (IMaintenanceButtons button in m_MaintenanceButtons)
        {
            if (button != null)
                button.Execute();
        }

        //draw in-game clock
        _global_clock_style.normal.textColor = Color.green;
        _global_clock_style.fontStyle = FontStyle.Bold;
        _global_clock_style.fontSize = 18;
        if (_digital_font != null)
            _global_clock_style.font = _digital_font;
        GUI.Label(new Rect(0, 0, 200, 50), _global_game_time, _global_clock_style);

        //start bottom GUI box
        //GUI.Box(new Rect(0, Screen.height - 100, Screen.width, 100), "Menu");
        _horizontal_borders_texture.wrapMode = TextureWrapMode.Repeat;

        #region SELECTION/CONSTRUCTION BARS DRAWING
        GUITextures._tile_texture(Resources.Load("Textures/GUI/GenericTile") as Texture, new Rect(0, 0, 64, 64), new Rect(0, Screen.height - 190, Screen.width, 32), ScaleMode.ScaleAndCrop); //draw menu tabs background
        GUITextures._tile_texture(Resources.Load("Textures/GUI/GenericTile") as Texture, new Rect(0, 0, 64, 64), new Rect(0, Screen.height - 158, Screen.width, 128), ScaleMode.ScaleAndCrop); //draw menu selection background
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 161, Screen.width, 2), _horizontal_borders_texture, new Rect(0, 0.1f, Screen.width / _horizontal_borders_texture.width, 0.01f)); //draw light border between construction buttons and selection menu
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 198, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.88f, Screen.width / _horizontal_borders_texture.width, 0.12f));//construction menu, top border
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 30, Screen.width, 3), _horizontal_borders_texture, new Rect(0, 0.53f, Screen.width / _horizontal_borders_texture.width, 0.1f)); //selection menu, bottom dark line
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 27, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.75f, Screen.width / _horizontal_borders_texture.width, 0.12f));//selection menu, bottom border
        #endregion

        #region MAIN MENU BAR DRAWING
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, Screen.width, 19), _gui_elements_texture, new Rect(0, 0.18f, Screen.width / _gui_elements_texture.width, 0.13f));//main menu background
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.88f, Screen.width / _horizontal_borders_texture.width, 0.12f));//main menu, top border
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 8, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.75f, Screen.width / _horizontal_borders_texture.width, 0.12f));//main menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, 10, 19), _vertical_borders_texture, new Rect(0.16f, 0.3f, 0.16f, 0.3f)); 
        GUITextures._tile_texture(Resources.Load("Textures/GUI/HorizontalBridge") as Texture, new Rect(0, 0, 10, 19), new Rect(10, Screen.height - 19, 10, 19), ScaleMode.ScaleAndCrop); //draw menu tabs background
        if (_is_minimap_shown) //draw lit/unlit minimap button
            GUI.DrawTexture(new Rect(22, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/MapToggle1") as Texture, ScaleMode.StretchToFill); //draw menu tabs background
        else
            GUI.DrawTexture(new Rect(22, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/MapToggle2") as Texture, ScaleMode.StretchToFill); //draw menu tabs background
        if (GUI.Button(new Rect(22, Screen.height - 19, 30, 19), "", new GUIStyle()))
            _is_minimap_shown = !_is_minimap_shown;


        GUI.DrawTextureWithTexCoords(new Rect(65, Screen.height - 19, 250, 19), Resources.Load("Textures/GUI/GenericBg") as Texture, new Rect(0, 0.75f, Screen.width / 64, 0.20f)); //map title background
        GUI.Label(new Rect(65, Screen.height - 19, 250, 19), _global_map_name, new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//map title
        GUI.DrawTextureWithTexCoords(new Rect(65, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f))); //begin box borders, map name
        GUI.DrawTextureWithTexCoords(new Rect(311, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f)));
        GUI.DrawTextureWithTexCoords(new Rect(66, Screen.height - 20, 246, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(67, Screen.height - 3, 244, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, map name

        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 142, Screen.height - 19, 120, 19), Resources.Load("Textures/GUI/GenericBg") as Texture, new Rect(0, 0.75f, Screen.width / 120, 0.20f)); //money background
        GUI.Label(new Rect(Screen.width - 142, Screen.height - 19, 120, 19), m_Manager.Money.ToString(), new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//money value
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 142, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f))); //begin box borders, money value
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 22, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f)));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 141, Screen.height - 20, 120, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 142, Screen.height - 3, 120, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, money value

        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 284, Screen.height - 19, 120, 19), Resources.Load("Textures/GUI/GenericBg") as Texture, new Rect(0, 0.75f, Screen.width / 120, 0.20f)); //energy background
        GUI.Label(new Rect(Screen.width - 284, Screen.height - 19, 120, 19), m_Manager.Money.ToString(), new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//energy value
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 284, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f))); //begin box borders, energy value
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 164, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f)));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 282, Screen.height - 20, 119, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 284, Screen.height - 3, 120, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, energy value

        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), Resources.Load("Textures/GUI/GenericBg") as Texture, new Rect(0, 0.75f, Screen.width / 120, 0.20f)); //energy background
        if (!_is_menu_shown)
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "Menu", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.32f, 0.39f, 0.28f) } });//menu text, closed
        else
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "Menu", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//menu text, opened
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f))); //begin box borders, menu button
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f)));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 20, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 3, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
        if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "", new GUIStyle()))
            _is_menu_shown = !_is_menu_shown;



        #endregion
        //GUITextures._tile_texture_with_coordinates(new Rect(0, Screen.height - 428, Screen.width, 64), Resources.Load("Textures/GUI/HorizontalBorders") as Texture2D, new Rect(0, 0, 1f, 1f));
        //Texture2D destTex = new Texture2D(sourceTex.width, sourceTex.height);
        //destTex.SetPixels32(0, 10, sourceTex.width, 10, pix);
        //destTex.Apply();
        //GUITextures._tile_texture(destTex, new Rect(0, 0, 64, 64), new Rect(0, Screen.height - 228, Screen.width, 10), ScaleMode.ScaleAndCrop); //draw menu selection background

    }

    public bool IsWithin(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector3 realScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, screenPos.z);

        if (DragArea.Contains(realScreenPos))
        {
            return true;
        }

        return false;
    }

    public void UpdateQueueContents(List<Item> availableItems)
    {
        foreach (TypeButton typeButton in m_TypeButtons)
        {
            typeButton.UpdateQueueContents(availableItems);
        }
    }

    public void AddConstructor(Building building)
    {
        switch (building.ID)
        {
            case Const.BUILDING_ConYard:
                m_TypeButtons[0].AddNewQueue(building);
                m_TypeButtons[1].AddNewQueue(building);
                break;

            case Const.BUILDING_Barracks:
                m_TypeButtons[2].AddNewQueue(building);
                break;
        }
    }

    public void RemoveConstructor(Building building)
    {

    }

    public void Resize()
    {
        //Resolution has changed, re-size all GUI elements
        //Mini map first
        ManagerResolver.Resolve<IMiniMapController>().LoadMiniMap(out m_MainMenuWidth, out m_MiniMapRect);

        //Build Borders around the map
        float sideBorderWidth = (m_MainMenuWidth - (m_MiniMapRect.width * Screen.width)) / 2;
        float topBorderHeight = (1 - m_MiniMapRect.yMax) * Screen.height;

        m_LeftMiniMapBG = new Rect();
        m_LeftMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_LeftMiniMapBG.xMax = m_LeftMiniMapBG.xMin + sideBorderWidth;
        m_LeftMiniMapBG.yMin = 0;
        m_LeftMiniMapBG.yMax = (1 - m_MiniMapRect.yMin) * Screen.height;

        m_RightMiniMapBG = new Rect();
        m_RightMiniMapBG.xMin = m_MiniMapRect.xMax * Screen.width;
        m_RightMiniMapBG.xMax = Screen.width;
        m_RightMiniMapBG.yMin = 0;
        m_RightMiniMapBG.yMax = (1 - m_MiniMapRect.yMin) * Screen.height;

        m_AboveMiniMapBG = new Rect();
        m_AboveMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_AboveMiniMapBG.xMax = Screen.width;
        m_AboveMiniMapBG.yMin = 0;
        m_AboveMiniMapBG.yMax = topBorderHeight;

        m_BelowMiniMapBG = new Rect();
        m_BelowMiniMapBG.xMin = Screen.width - m_MainMenuWidth;
        m_BelowMiniMapBG.xMax = Screen.width;
        m_BelowMiniMapBG.yMin = ((1 - m_MiniMapRect.yMin) * Screen.height) - 1;
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
        float size = m_RightMiniMapBG.width - 4;
        float totalHeight = size * 3;
        float offSet = (m_RightMiniMapBG.height - totalHeight) / 2;
        float x = m_RightMiniMapBG.xMin;
        float y = m_RightMiniMapBG.yMin + offSet;

        Rect rect1 = new Rect(x, y, size, size);
        Rect rect2 = new Rect(x, y + size, size, size);
        Rect rect3 = new Rect(x, y + (size * 2), size, size);

        //Update Maintenance Buttons
        m_MaintenanceButtons[0].Resize(rect1);
        m_MaintenanceButtons[1].Resize(rect2);
        m_MaintenanceButtons[2].Resize(rect3);
    }
}
