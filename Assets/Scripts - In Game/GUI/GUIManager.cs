using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour, IGUIManager
{
    public static float _global_displayed_hour = 7.0F;
    public static float _global_displayed_minute = 0;
    public static float _global_game_speed = 1.0F;
    public static string _global_game_time = "";
    public static string _global_map_name = "Map name placeholder";
    public static DayNightShift _global_day_night_shift = DayNightShift.Day;
    public static SelectedMenuTab _current_selected_menu_tab = SelectedMenuTab.Selection;
    private SelectedOptionsTab _current_selected_options_tab = SelectedOptionsTab.Graphics;
    public Font _digital_font;
    private bool _is_minimap_shown = true;
    private bool _is_minimap_animation_ended = false;
    public static bool _is_construction_shown = true;
    private bool _is_construction_animation_ended = true;
    public static bool _is_right_menu_shown = true;
    public static bool _is_menu_shown = false;
    public static bool _is_options_shown = false;
    public static bool _is_multiplayer = false;
    public static bool _is_paused = false;
    private GUIStyle _global_clock_style = new GUIStyle();
    public GUISkin _global_engine_skin;

    public float _master_volume_value = 0.05F;
    public float _master_volume_initial_value = 0.05F;
    public float _music_volume_value = 0.05F;
    public float _music_volume_initial_value = 0.05F;
    public float _fx_volume_value = 0.5F;
    public float _fx_volume_initial_value = 0.5F;
    public float _computer_volume_value = 0.05F;
    public float _computer_volume_initial_value = 0.05F;
    public float _units_volume_value = 0.05F;
    public float _units_volume_initial_value = 0.05F;
    public float _buildings_volume_value = 0.05F;
    public float _buildings_volume_initial_value = 0.05F;
    public bool _play_production_message = false;

    public static float _minimap_x_coord = -178;
    public static float _construction_y_coord = 0; //196 full
    private float _construction_x_coord = 178; //196 full
    private float _construction_width = 178;
    public static float _right_menu_x_coord = Screen.width; //Screen.width - 178 if shown
    private bool _is_quit_dialog_shown = false;

    private Texture _horizontal_borders_texture;
    private Texture _vertical_borders_texture;
    private Texture _gui_elements_texture;
    public Texture _checkbox_normal_texture;

    public AudioClip _minimap_animation;
    public AudioClip _menu_tab_switch;
    public AudioClip _new_mineral_field_located;
    public AudioClip _ambiental_1;
    Color32[] pix;

    //Singleton
    public static GUIManager main;

    //Member Variables
    private Rect m_MiniMapRect;
    private float m_MainMenuWidth;
    private float m_MainMenuHeight;

    private IManager m_Manager;

    public enum DayNightShift
    {
        Day = 0,
        Night = 1
    };

    public enum SelectedMenuTab
    {
        Selection = 0,
        Build = 1,
        Structures = 2,
        Platoons = 3,
        Units = 4
    };

    private enum SelectedOptionsTab
    {
        Graphics = 0,
        Sound = 1,
        Game = 2,
        Speed = 3,
        Keyboard = 4
    };

    #region PROPERTIES
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

    public float MainMenuHeight
    {
        get
        {
            return m_MainMenuHeight;
        }
        private set
        {
            if (Equals(m_MainMenuHeight, value))
            {
                return;
            }

            m_MainMenuHeight = value;

            GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
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
    #endregion

    void Awake()
    {
        main = this; //Set singleton
    }

    // Use this for initialization
    IEnumerator Start()
    {
        IMiniMapController miniMap = ManagerResolver.Resolve<IMiniMapController>();
        float tempWidth;
        float tempHeight;
        miniMap.LoadMiniMap(out tempWidth, out tempHeight, out m_MiniMapRect);
        MainMenuWidth = tempWidth;
        MainMenuHeight = tempHeight;
        m_Manager = ManagerResolver.Resolve<IManager>();
        _horizontal_borders_texture = Resources.Load("Textures/GUI/HorizontalBorders") as Texture;
        _vertical_borders_texture = Resources.Load("Textures/GUI/VerticalBorders") as Texture;
        _gui_elements_texture = Resources.Load("Textures/GUI/horizontalUIelements") as Texture;
        _is_minimap_animation_ended = false;
        _right_menu_x_coord = _is_right_menu_shown ? Screen.width - 178 : Screen.width; //screen.width - 178 if right menu shown
        SoundManager._instance._play_music(_ambiental_1);
        StartCoroutine(_animate_minimap_out_first());
        yield return new WaitForSeconds(2.3f);
        SoundManager._instance._play_computer(_new_mineral_field_located);
        //SoundManager._instance._play_effect_once(Resources.Load("Sounds/Computer/LocatedNewMineralField") as AudioClip);
    }

    void Update()
    {
        #region GAME SPEED
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
        Time.timeScale = _global_game_speed;
        #endregion
        _global_game_time = string.Format("{0:00}:{1:00}", _global_displayed_hour, _global_displayed_minute);
        GUIEvents.TellItemsToUpdate(Time.deltaTime); //Tell all items that are being built to update themselves
        if (Input.GetKeyDown("r"))
            Resize();
        if (Screen.fullScreen)
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }
   
    void OnGUI()
    {
        GUI.skin = _global_engine_skin; //override default game skin
        _global_clock_style.normal.textColor = Color.green; //draw in-game clock
        _global_clock_style.fontStyle = FontStyle.Bold;
        _global_clock_style.fontSize = 18;
        if (_digital_font != null)
            _global_clock_style.font = _digital_font;
        GUI.Label(new Rect(0, 0, 200, 50), _global_game_time, _global_clock_style);
        _horizontal_borders_texture.wrapMode = TextureWrapMode.Repeat;
        #region SELECTION/CONSTRUCTION BAR DRAWING
        GUI.BeginGroup(new Rect(_construction_x_coord, Screen.height - _construction_y_coord, Screen.width - _construction_width, 178)); //selection/construction group
        GUI.DrawTextureWithTexCoords(new Rect(8, 8, Screen.width - (_is_minimap_shown ? 194 : 16), 160), Resources.Load("Textures/GUI/GenericTile") as Texture, new Rect(0, 0, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 192 / 64));//construction menu, top border
        GUI.DrawTextureWithTexCoords(new Rect(8, 59, Screen.width - (_is_minimap_shown ? 194 : 16), 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f)); //draw light border between construction buttons and selection menu
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width - (_is_minimap_shown ? 178 : 0), 8), _horizontal_borders_texture, new Rect(0, 0.88f, (Screen.width - (_is_minimap_shown ? 178 : 0)) / 64, 0.12f));//construction menu, top border
        GUI.DrawTextureWithTexCoords(new Rect(8, 167, Screen.width - (_is_minimap_shown ? 194 : 16), 3), _horizontal_borders_texture, new Rect(0, 0.53f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.12f)); //selection menu, bottom dark line
        GUI.DrawTextureWithTexCoords(new Rect(0, 170, Screen.width - (_is_minimap_shown ? 178 : 0), 8), _horizontal_borders_texture, new Rect(0, 0.75f, (Screen.width - (_is_minimap_shown ? 178 : 0)) / 64, 0.12f));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 178), _vertical_borders_texture, new Rect(0, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - (_is_minimap_shown ? 186 : 8), 0, 8, 178), _vertical_borders_texture, new Rect(0.13f, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - (_is_minimap_shown ? 186 : 8), 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(0, 170, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - (_is_minimap_shown ? 186 : 8), 170, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border

        if (new Rect(_construction_x_coord + 8, (Screen.height - _construction_y_coord) + 36, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_menu_tab == SelectedMenuTab.Selection)
        {
            GUI.DrawTextureWithTexCoords(new Rect(8, 36, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(9, 37, 85, 23), "Selection", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(8, 36, 85, 23), "Selection", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
            if (GUI.Button(new Rect(new Rect(8, 36, 85, 25)), "", new GUIStyle()))
            {
                _current_selected_menu_tab = SelectedMenuTab.Selection;
                SoundManager._instance._play_effect_once(_menu_tab_switch);
            }
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(8, 36, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(9, 37, 85, 23), "Selection", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(8, 36, 85, 23), "Selection", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
        }

        if (new Rect(_construction_x_coord + 95, (Screen.height - _construction_y_coord) + 36, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_menu_tab == SelectedMenuTab.Build)
        {
            GUI.DrawTextureWithTexCoords(new Rect(95, 36, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(96, 37, 85, 23), "Build", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(95, 36, 85, 23), "Build", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
            if (GUI.Button(new Rect(new Rect(95, 36, 85, 25)), "", new GUIStyle()))
            { 
                _current_selected_menu_tab = SelectedMenuTab.Build;
                SoundManager._instance._play_effect_once(_menu_tab_switch);
            }
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(95, 36, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(96, 37, 85, 23), "Build", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(95, 36, 85, 23), "Build", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
        }

        if (new Rect(_construction_x_coord + 182, (Screen.height - _construction_y_coord) + 36, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_menu_tab == SelectedMenuTab.Structures)
        {
            GUI.DrawTextureWithTexCoords(new Rect(182, 36, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(183, 37, 85, 23), "Structures", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(182, 36, 85, 23), "Structures", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
            if (GUI.Button(new Rect(new Rect(182, 36, 85, 25)), "", new GUIStyle()))
            { 
                _current_selected_menu_tab = SelectedMenuTab.Structures;
                SoundManager._instance._play_effect_once(_menu_tab_switch);
            }
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(182, 36, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(183, 37, 85, 23), "Structures", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(182, 36, 85, 23), "Structures", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
        }

        if (new Rect(_construction_x_coord + 269, (Screen.height - _construction_y_coord) + 36, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_menu_tab == SelectedMenuTab.Platoons)
        {
            GUI.DrawTextureWithTexCoords(new Rect(269, 36, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(270, 37, 85, 23), "Platoons", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(269, 36, 85, 23), "Platoons", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
            if (GUI.Button(new Rect(new Rect(269, 36, 85, 25)), "", new GUIStyle()))
            { 
                _current_selected_menu_tab = SelectedMenuTab.Platoons;
                SoundManager._instance._play_effect_once(_menu_tab_switch);
            }
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(269, 36, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(270, 37, 85, 23), "Platoons", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(269, 36, 85, 23), "Platoons", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
        }

        if (new Rect(_construction_x_coord + 356, (Screen.height - _construction_y_coord) + 36, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_menu_tab == SelectedMenuTab.Units)
        {
            GUI.DrawTextureWithTexCoords(new Rect(356, 36, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(357, 37, 85, 23), "Units", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(356, 36, 85, 23), "Units", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
            if (GUI.Button(new Rect(new Rect(356, 36, 85, 25)), "", new GUIStyle()))
            { 
                _current_selected_menu_tab = SelectedMenuTab.Units;
                SoundManager._instance._play_effect_once(_menu_tab_switch);
            }
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(356, 36, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.Label(new Rect(357, 37, 85, 23), "Units", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(new Rect(356, 36, 85, 23), "Units", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
        }
        switch (_current_selected_menu_tab)
        {
            case SelectedMenuTab.Selection:
                GUI.DrawTextureWithTexCoords(new Rect(93, 59, 400, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTexture(new Rect(93, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));
                break;
            case SelectedMenuTab.Build:
                GUI.DrawTextureWithTexCoords(new Rect(180, 59, 400, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTextureWithTexCoords(new Rect(8, 59, 85, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTexture(new Rect(93, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));
                GUI.DrawTexture(new Rect(180, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));

                break;
            case SelectedMenuTab.Structures:
                GUI.DrawTextureWithTexCoords(new Rect(267, 59, 400, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTextureWithTexCoords(new Rect(8, 59, 172, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTexture(new Rect(180, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));
                GUI.DrawTexture(new Rect(267, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));

                break;
            case SelectedMenuTab.Platoons:
                GUI.DrawTextureWithTexCoords(new Rect(354, 59, 400, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTextureWithTexCoords(new Rect(8, 59, 259, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTexture(new Rect(267, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));
                GUI.DrawTexture(new Rect(354, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));

                break;
            case SelectedMenuTab.Units:
                GUI.DrawTextureWithTexCoords(new Rect(8, 59, 346, 2), _horizontal_borders_texture, new Rect(0, 0.1f, (Screen.width - (_is_minimap_shown ? 194 : 16)) / 64, 0.01f));
                GUI.DrawTexture(new Rect(354, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));
                GUI.DrawTexture(new Rect(441, 36, 2, 24), TextureGenerator.MakeTexture(0.6f, 0.75f, 0.82f, 0.5f));

                break;
        }
        GUI.EndGroup();
        #endregion
        #region MINIMAP BAR DRAWING
        if (_minimap_x_coord > -178)
        {
            GUI.BeginGroup(new Rect(_minimap_x_coord, Screen.height - 196, 178, 178)); //minimap group
            GUI.DrawTextureWithTexCoords(new Rect(8, 8, 162, 162), Resources.Load("Textures/GUI/MinimapBg") as Texture, new Rect(0, 0, 1, 1));//construction menu, top border
            GUI.DrawTextureWithTexCoords(new Rect(8, 0, 178, 8), _horizontal_borders_texture, new Rect(0, 0.88f, 192 / _horizontal_borders_texture.width, 0.12f));//construction menu, top border
            GUI.DrawTextureWithTexCoords(new Rect(8, 170, 178, 8), _horizontal_borders_texture, new Rect(0, 0.75f, 192 / _horizontal_borders_texture.width, 0.12f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 178), _vertical_borders_texture, new Rect(0, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(170, 0, 8, 178), _vertical_borders_texture, new Rect(0.13f, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(170, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 170, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(170, 170, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.EndGroup();
        }
        #endregion
        #region MAIN MENU BAR DRAWING
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, Screen.width, 19), _gui_elements_texture, new Rect(0, 0.18f, Screen.width / _gui_elements_texture.width, 0.13f));//main menu background
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.88f, Screen.width / _horizontal_borders_texture.width, 0.12f));//main menu, top border
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 8, Screen.width, 8), _horizontal_borders_texture, new Rect(0, 0.75f, Screen.width / _horizontal_borders_texture.width, 0.12f));//main menu, bottom border
        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 19, 10, 19), _vertical_borders_texture, new Rect(0.16f, 0.3f, 0.16f, 0.3f));
        GUITextures._tile_texture(Resources.Load("Textures/GUI/HorizontalBridge") as Texture, new Rect(0, 0, 10, 19), new Rect(10, Screen.height - 19, 10, 19), ScaleMode.ScaleAndCrop); //draw menu tabs background
        if (_is_minimap_shown) //draw lit/unlit minimap button
            GUI.DrawTexture(new Rect(22, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/MapToggle1") as Texture, ScaleMode.StretchToFill);
        else
            GUI.DrawTexture(new Rect(22, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/MapToggle2") as Texture, ScaleMode.StretchToFill);
        if (GUI.Button(new Rect(22, Screen.height - 19, 30, 19), "", new GUIStyle()))
        {
            _is_minimap_shown = !_is_minimap_shown;
            if (!_is_minimap_shown)
            {
                if (_is_construction_shown)
                {
                    _is_minimap_animation_ended = false;
                    StartCoroutine(_animate_minimap_out_first());
                }
                else
                {
                    _is_minimap_animation_ended = false;
                    StartCoroutine(_animate_minimap_out_fourth());
                }
            }
            else
            {
                _is_construction_animation_ended = false;
                StartCoroutine(_animate_minimap_in_first());
            }
        }
        GUI.DrawTextureWithTexCoords(new Rect(65, Screen.height - 19, 250, 19), Resources.Load("Textures/GUI/GenericBg") as Texture, new Rect(0, 0.75f, Screen.width / 64, 0.20f)); //map title background
        GUI.Label(new Rect(65, Screen.height - 19, 250, 19), _global_map_name, new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//map title
        GUI.DrawTextureWithTexCoords(new Rect(65, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f))); //begin box borders, map name
        GUI.DrawTextureWithTexCoords(new Rect(311, Screen.height - 19, 4, 19), _vertical_borders_texture, new Rect(new Rect(0.25f, 0.3f, 0.07f, 0.3f)));
        GUI.DrawTextureWithTexCoords(new Rect(66, Screen.height - 20, 246, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(67, Screen.height - 3, 244, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, map name
        if (_is_construction_shown) //draw lit/unlit construction button
            GUI.DrawTexture(new Rect(325, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/SelectionToggle1") as Texture, ScaleMode.StretchToFill);
        else
            GUI.DrawTexture(new Rect(325, Screen.height - 19, 30, 19), Resources.Load("Textures/GUI/SelectionToggle2") as Texture, ScaleMode.StretchToFill);
        if (GUI.Button(new Rect(325, Screen.height - 19, 30, 19), "", new GUIStyle()))
        {
            _is_construction_shown = !_is_construction_shown;
            _is_construction_animation_ended = false;
            StartCoroutine(_animate_construction());
        }
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

        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
        if (!_is_menu_shown)
        {
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 18, 150, 19), "Menu", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "Menu", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
        }
        else
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "Menu", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.green } });//menu text, opened
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 20, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 3, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
        if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 19, 150, 19), "", new GUIStyle()))
        {
            _is_menu_shown = !_is_menu_shown;
            _is_quit_dialog_shown = false;
        }
        if (new Rect(Screen.width - 456, Screen.height - 19, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
        {
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
        }
        else
        {
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 19, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
        }
        if (_is_menu_shown) //draw menu items
        {
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 38, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 36, 150, 19), "Quit", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 38, 150, 19), "Quit", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 39, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 19, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
            //with quit code
            if (new Rect(Screen.width - 456, Screen.height - 38, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 38, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 57, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 56, 150, 19), "Options", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 57, 150, 19), "Options", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 58, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 38, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
            //if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 57, 150, 19), "", new GUIStyle()))
            //{
            //    _is_menu_shown = !_is_menu_shown; //replace with options code
            //    _is_options_shown = true;
            //}
            if (new Rect(Screen.width - 456, Screen.height - 57, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 57, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 76, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 75, 150, 19), "Load", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 76, 150, 19), "Load", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 77, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 57, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
            if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 76, 150, 19), "", new GUIStyle()))
                _is_menu_shown = !_is_menu_shown; //replace with load code
            if (new Rect(Screen.width - 456, Screen.height - 76, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 76, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 95, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 94, 150, 19), "Save", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 95, 150, 19), "Save", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 96, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 76, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
            if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 95, 150, 19), "", new GUIStyle()))
                _is_menu_shown = !_is_menu_shown; //replace with save code
            if (new Rect(Screen.width - 456, Screen.height - 95, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 95, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 114, 150, 19), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0, 0.13f, 1.0f, 0.20f)); //menu background
            GUI.Label(new Rect(Screen.width - 455, Screen.height - 114, 150, 19), "Restart", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(Screen.width - 456, Screen.height - 115, 150, 19), "Restart", new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 454, Screen.height - 115, 149, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f));
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 95, 150, 4), _horizontal_borders_texture, new Rect(0, 0.68f, 3f, 0.08f)); //end box borders, menu button
            if (GUI.Button(new Rect(Screen.width - 456, Screen.height - 114, 150, 19), "", new GUIStyle()))
                _is_menu_shown = !_is_menu_shown; //replace with restart code
            if (new Rect(Screen.width - 456, Screen.height - 114, 150, 19).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuRightBorderLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 456, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuLeftBorder") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 306, Screen.height - 114, 6, 19), Resources.Load("Textures/GUI/MenuRightBorder") as Texture, new Rect(new Rect(0, 1, 1, 1)));
            }
        }
        #endregion
        #region MENU QUIT
        if (_is_menu_shown && GUI.Button(new Rect(Screen.width - 456, Screen.height - 38, 150, 19), "", new GUIStyle()))
        {
            _is_quit_dialog_shown = true;
            _is_menu_shown = false;
        }
        if (_is_quit_dialog_shown)
        {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 136, Screen.height / 2 - 72, 272, 144)); //quit dialog group
            GUI.DrawTextureWithTexCoords(new Rect(8, 8, 256, 128), Resources.Load("Textures/GUI/GenericTile") as Texture, new Rect(0, 0, 256 / 64, 128 / 64));//construction menu, top border
            GUI.DrawTextureWithTexCoords(new Rect(8, 0, 272, 8), _horizontal_borders_texture, new Rect(0, 0.88f, 192 / _horizontal_borders_texture.width, 0.12f));//construction menu, top border
            GUI.DrawTextureWithTexCoords(new Rect(8, 136, 272, 8), _horizontal_borders_texture, new Rect(0, 0.75f, 192 / _horizontal_borders_texture.width, 0.12f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 144), _vertical_borders_texture, new Rect(0, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(264, 0, 8, 144), _vertical_borders_texture, new Rect(0.13f, 0.75f, 0.1f, 192 / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(264, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 136, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(264, 136, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.Label(new Rect(10, 22, 256, 64), "Do you want to exit the game?", new GUIStyle() { alignment = TextAnchor.UpperCenter, fontSize = 16, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
            GUI.Label(new Rect(8, 20, 256, 64), "Do you want to exit the game?", new GUIStyle() { alignment = TextAnchor.UpperCenter, fontSize = 16, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            if (new Rect(Screen.width / 2 - 76, Screen.height / 2 + 18, 58, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(60, 90, 58, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(61, 91, 58, 23), "Yes", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
                GUI.Label(new Rect(60, 90, 58, 23), "Yes", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });//menu text, closed
                if (GUI.Button(new Rect(new Rect(60, 90, 58, 25)), "", new GUIStyle()))
                {
                    Application.Quit();
#if UNITY_EDITOR
                    _is_quit_dialog_shown = false;
#endif
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(60, 90, 58, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.Label(new Rect(61, 91, 58, 23), "Yes", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
                GUI.Label(new Rect(60, 90, 58, 23), "Yes", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            }
            if (new Rect(Screen.width / 2 + 18, Screen.height / 2 + 18, 58, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) //lit/unlit menu left/right borders
            {
                GUI.DrawTextureWithTexCoords(new Rect(154, 90, 58, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(155, 91, 58, 23), "No", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
                GUI.Label(new Rect(154, 90, 58, 23), "No", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });//menu text, closed
                if (GUI.Button(new Rect(154, 90, 58, 25), "", new GUIStyle()))
                    _is_quit_dialog_shown = false;
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(154, 90, 58, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1))); //begin box borders, menu button
                GUI.Label(new Rect(155, 91, 58, 23), "No", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });//menu text, closed
                GUI.Label(new Rect(154, 90, 58, 23), "No", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });//menu text, closed
            }
            GUI.EndGroup();
        }
        #endregion
        #region MENU OPTIONS
        if (_is_menu_shown && GUI.Button(new Rect(Screen.width - 456, Screen.height - 57, 150, 19), "", new GUIStyle()))
        {
            _is_options_shown = true;
            _is_menu_shown = false;
        }
        if (_is_options_shown)
        {
            GUI.BeginGroup(new Rect(100, 50, Screen.width - 200, Screen.height - 100)); //options dialog group
            GUI.DrawTextureWithTexCoords(new Rect(8, 8, Screen.width - 216, Screen.height - 116), Resources.Load("Textures/GUI/GenericTile") as Texture, new Rect(0, 0, (Screen.width - 216) / 64, (Screen.height - 116) / 64));//construction menu, top border

            GUI.DrawTextureWithTexCoords(new Rect(8, 0, Screen.width - 216, 8), _horizontal_borders_texture, new Rect(0, 0.88f, (Screen.width - 216) / _horizontal_borders_texture.width, 0.12f));//construction menu, top border
            GUI.DrawTextureWithTexCoords(new Rect(8, Screen.height - 108, Screen.width - 216, 8), _horizontal_borders_texture, new Rect(0, 0.75f, (Screen.width - 216) / _horizontal_borders_texture.width, 0.12f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 8, 8, Screen.height - 116), _vertical_borders_texture, new Rect(0, 0.75f, 0.1f, (Screen.height - 116) / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 208, 8, 8, Screen.height - 116), _vertical_borders_texture, new Rect(0.13f, 0.75f, 0.1f, (Screen.height - 116) / _vertical_borders_texture.height));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 208, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - 108, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border
            GUI.DrawTextureWithTexCoords(new Rect(Screen.width - 208, Screen.height - 108, 8, 8), Resources.Load("Textures/GUI/horizontalUIelements") as Texture, new Rect(0f, 0.87f, 0.13f, 0.13f));//selection menu, bottom border


            if (new Rect(108, 58, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_options_tab == SelectedOptionsTab.Graphics)
            {
                GUI.DrawTextureWithTexCoords(new Rect(8, 8, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(9, 9, 85, 23), "Graphics", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(8, 8, 85, 23), "Graphics", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
                if (GUI.Button(new Rect(new Rect(8, 8, 85, 25)), "", new GUIStyle()))
                {
                    _current_selected_options_tab = SelectedOptionsTab.Graphics;
                    SoundManager._instance._play_effect_once(_menu_tab_switch);
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(8, 8, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(9, 9, 85, 23), "Graphics", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(8, 8, 85, 23), "Graphics", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
            }

            if (new Rect(185, 58, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_options_tab == SelectedOptionsTab.Sound)
            {
                GUI.DrawTextureWithTexCoords(new Rect(95, 8, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(96, 9, 85, 23), "Sound", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(95, 8, 85, 23), "Sound", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
                if (GUI.Button(new Rect(new Rect(95, 8, 85, 25)), "", new GUIStyle()))
                {
                    _current_selected_options_tab = SelectedOptionsTab.Sound;
                    SoundManager._instance._play_effect_once(_menu_tab_switch);
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(95, 8, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(96, 9, 85, 23), "Sound", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(95, 8, 85, 23), "Sound", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
            }

            if (new Rect(272, 58, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_options_tab == SelectedOptionsTab.Game)
            {
                GUI.DrawTextureWithTexCoords(new Rect(182, 8, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(183, 9, 85, 23), "Game", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(182, 8, 85, 23), "Game", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
                if (GUI.Button(new Rect(new Rect(182, 8, 85, 25)), "", new GUIStyle()))
                {
                    _current_selected_options_tab = SelectedOptionsTab.Game;
                    SoundManager._instance._play_effect_once(_menu_tab_switch);
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(182, 8, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(183, 9, 85, 23), "Game", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(182, 8, 85, 23), "Game", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
            }

            if (new Rect(359, 58, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_options_tab == SelectedOptionsTab.Speed)
            {
                GUI.DrawTextureWithTexCoords(new Rect(269, 8, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(270, 9, 85, 23), "Speed", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(269, 8, 85, 23), "Speed", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
                if (GUI.Button(new Rect(new Rect(269, 8, 85, 25)), "", new GUIStyle()))
                {
                    _current_selected_options_tab = SelectedOptionsTab.Speed;
                    SoundManager._instance._play_effect_once(_menu_tab_switch);
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(269, 8, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(270, 9, 85, 23), "Speed", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(269, 8, 85, 23), "Speed", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
            }

            if (new Rect(446, 58, 85, 25).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || _current_selected_options_tab == SelectedOptionsTab.Keyboard)
            {
                GUI.DrawTextureWithTexCoords(new Rect(356, 8, 85, 25), Resources.Load("Textures/GUI/ButtonLit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(357, 9, 85, 23), "Keyboard", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(356, 8, 85, 23), "Keyboard", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.green } });
                if (GUI.Button(new Rect(new Rect(356, 8, 85, 25)), "", new GUIStyle()))
                {
                    _current_selected_options_tab = SelectedOptionsTab.Keyboard;
                    SoundManager._instance._play_effect_once(_menu_tab_switch);
                }
            }
            else
            {
                GUI.DrawTextureWithTexCoords(new Rect(356, 8, 85, 25), Resources.Load("Textures/GUI/ButtonUnlit") as Texture, new Rect(new Rect(0, 1, 1, 1)));
                GUI.Label(new Rect(357, 9, 85, 23), "Keyboard", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = Color.black } });
                GUI.Label(new Rect(356, 8, 85, 23), "Keyboard", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
            }
            GUI.DrawTexture(new Rect(441, 32, Screen.width - 649, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));

            switch (_current_selected_options_tab)
            {
                case SelectedOptionsTab.Graphics:
                    GUI.DrawTexture(new Rect(93, 32, 400, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(93, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));

                    break;
                case SelectedOptionsTab.Sound:
                    GUI.Box(new Rect(30, 40, 310, 170), "");
                    GUI.DrawTexture(new Rect(180, 32, 400, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(8, 32, 87, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(93, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(180, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.Label(new Rect(60, 45, 150, 30), "Master Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _master_volume_value = GUI.HorizontalSlider(new Rect(220, 55, 100, 20), _master_volume_value, 0.0F, 1f);
                    GUI.Label(new Rect(60, 75, 150, 20), "Music Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _music_volume_value = GUI.HorizontalSlider(new Rect(220, 80, 100, 20), _music_volume_value, 0.0F, 1f);
                    GUI.Label(new Rect(60, 100, 150, 20), "Computer Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _computer_volume_value = GUI.HorizontalSlider(new Rect(220, 105, 100, 20), _computer_volume_value, 0.0F, 1f);
                    GUI.Label(new Rect(60, 125, 150, 20), "Sounds FX Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _fx_volume_value = GUI.HorizontalSlider(new Rect(220, 130, 100, 20), _fx_volume_value, 0.0F, 1f);
                    GUI.Label(new Rect(60, 150, 150, 20), "Units Speech Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _units_volume_value = GUI.HorizontalSlider(new Rect(220, 155, 100, 20), _units_volume_value, 0.0F, 1f);
                    GUI.Label(new Rect(60, 175, 150, 20), "Buildings Speech Volume", new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = new GUIStyleState() { textColor = new Color(0.42f, 0.49f, 0.38f) } });
                    _buildings_volume_value = GUI.HorizontalSlider(new Rect(220, 180, 100, 20), _buildings_volume_value, 0.0F, 1f);
                    if (_master_volume_initial_value != _master_volume_value)
                    {
                        _master_volume_initial_value = _master_volume_value;
                        _music_volume_value = _master_volume_value;
                        _computer_volume_value = _master_volume_value;
                        _fx_volume_value = _master_volume_value;
                        _units_volume_value = _master_volume_value;
                        _buildings_volume_value = _master_volume_value;
                    }
                    if (_music_volume_value != _music_volume_initial_value)
                    {
                        SoundManager._instance._music_audio.volume = _music_volume_value;
                        _music_volume_initial_value = _music_volume_value;
                    }
                    if (_computer_volume_value != _computer_volume_initial_value)
                    {
                        SoundManager._instance._computer_audio.volume = _computer_volume_value;
                        _computer_volume_initial_value = _computer_volume_value;
                    }
                    if (_fx_volume_value != _fx_volume_initial_value)
                    {
                        SoundManager._instance._effect_audio.volume = _fx_volume_value;
                        _fx_volume_initial_value = _fx_volume_value;
                    }
                    if (_units_volume_value != _units_volume_initial_value)
                    {
                        SoundManager._instance._units_audio.volume = _units_volume_value;
                        _units_volume_initial_value = _units_volume_value;
                    }
                    if (_buildings_volume_value != _buildings_volume_initial_value)
                    {
                        SoundManager._instance._buildings_audio.volume = _buildings_volume_value;
                        _buildings_volume_initial_value = _buildings_volume_value;
                    }
                    GUI.Box(new Rect(30, 220, 310, 30), "");
                    _play_production_message = GUI.Toggle(new Rect(50, 225, 310, 30), _play_production_message, " Don't play production messages", _global_engine_skin.toggle);
                    break;
                case SelectedOptionsTab.Game:
                    GUI.DrawTexture(new Rect(267, 32, 400, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(8, 32, 174, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(180, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(267, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));

                    break;
                case SelectedOptionsTab.Speed:
                    GUI.DrawTexture(new Rect(354, 32, 400, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(8, 32, 261, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(267, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(354, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));

                    break;
                case SelectedOptionsTab.Keyboard:
                    GUI.DrawTexture(new Rect(8, 32, 348, 2), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(354, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));
                    GUI.DrawTexture(new Rect(441, 8, 2, 24), TextureGenerator.MakeTexture(.39f, .49f, .51f, 1f));

                    break;
            }
            GUI.EndGroup();
        }


        #endregion
        //if (_is_paused)
        //    GUI.Label(new Rect(Screen.width / 2 - 100, 200, 200, 50), "GAME PAUSED", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 20, normal = new GUIStyleState() { textColor = Color.green } });
    }

    #region MINIMAP OUT ANIMATION
    IEnumerator _animate_minimap_out_first()
    {
        while (!_is_minimap_animation_ended)
        {
            if (!_is_minimap_animation_ended && _is_minimap_shown)
            {
                Resize();
                if (_minimap_x_coord < 0)
                    _minimap_x_coord += 10;
                if (_minimap_x_coord >= 0)
                {
                    _minimap_x_coord = 0;
                    _is_minimap_animation_ended = true;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    StartCoroutine(_animate_minimap_out_second());
                    yield break;
                }
            }
            if (!_is_minimap_animation_ended && !_is_minimap_shown)
            {
                Resize();
                if (_minimap_x_coord > -178)
                    _minimap_x_coord -= 10;
                if (_minimap_x_coord <= -178)
                {
                    _minimap_x_coord = -178;
                    _is_minimap_animation_ended = true;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = false;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    StartCoroutine(_animate_minimap_out_second());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator _animate_minimap_out_second()
    {
        while (!_is_construction_animation_ended)
        {
            if (!_is_construction_animation_ended && _is_construction_shown)
            {
                Resize();
                if (_construction_y_coord < 196)
                    _construction_y_coord += 10;
                if (_construction_y_coord >= 196)
                {
                    _construction_y_coord = 196;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = true;
                    _construction_x_coord = 178;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            if (!_is_construction_animation_ended && !_is_construction_shown)
            {
                Resize();
                if (_construction_y_coord > 0)
                    _construction_y_coord -= 10;
                if (_construction_y_coord <= 0)
                {
                    _construction_y_coord = 0;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = false;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    StartCoroutine(_animate_minimap_out_third());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator _animate_minimap_out_third()
    {
        while (!_is_construction_animation_ended)
        {
            Resize();
            if (!_is_construction_animation_ended && !_is_construction_shown)
            {
                _construction_x_coord = 0;
                _construction_width = 0;
                if (_construction_y_coord < 196)
                    _construction_y_coord += 10;
                if (_construction_y_coord >= 196)
                {
                    _construction_y_coord = 196;
                    _is_construction_animation_ended = true;
                    _is_construction_shown = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator _animate_minimap_out_fourth()
    {
        while (!_is_minimap_animation_ended)
        {
            if (!_is_minimap_animation_ended && _is_minimap_shown)
            {
                Resize();
                if (_minimap_x_coord < 0)
                    _minimap_x_coord += 10;
                if (_minimap_x_coord >= 0)
                {
                    _minimap_x_coord = 0;
                    _is_minimap_animation_ended = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            if (!_is_minimap_animation_ended && !_is_minimap_shown)
            {
                Resize();
                if (_minimap_x_coord > -178)
                    _minimap_x_coord -= 10;
                if (_minimap_x_coord <= -178)
                {
                    _minimap_x_coord = -178;
                    _is_minimap_animation_ended = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            yield return null;
        }
    }
    #endregion

    #region MINIMAP IN ANIMATION
    IEnumerator _animate_minimap_in_first()
    {
        while (!_is_construction_animation_ended)
        {
            Resize();
            if (!_is_construction_animation_ended && _is_construction_shown)
            {
                _is_minimap_shown = false;
                _construction_width = 0;
                if (_construction_y_coord > 0)
                    _construction_y_coord -= 10;
                if (_construction_y_coord <= 0)
                {
                    _construction_y_coord = 0;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = false;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    StartCoroutine(_animate_minimap_in_second());
                    yield break;
                }
            }
            else if (!_is_construction_animation_ended && !_is_construction_shown)
            {
                Resize();
                if (_minimap_x_coord < 0)
                    _minimap_x_coord += 10;
                if (_minimap_x_coord >= 0)
                {
                    _minimap_x_coord = 0;
                    _is_minimap_animation_ended = true;
                    _is_construction_animation_ended = true;
                    _is_construction_shown = false;
                    _is_minimap_shown = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                }
            }
            yield return null;
        }
    }

    IEnumerator _animate_minimap_in_second()
    {
        while (!_is_construction_animation_ended)
        {
            if (!_is_construction_animation_ended && !_is_construction_shown)
            {
                Resize();
                _construction_x_coord = 178;
                if (_construction_y_coord < 196)
                    _construction_y_coord += 10;
                if (_construction_y_coord >= 196)
                {
                    _construction_y_coord = 196;
                    _is_construction_animation_ended = true;
                    _is_construction_shown = true;
                    _is_minimap_animation_ended = false;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    StartCoroutine(_animate_minimap_in_third());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator _animate_minimap_in_third()
    {
        while (!_is_minimap_animation_ended)
        {
            if (!_is_minimap_animation_ended && !_is_minimap_shown)
            {
                Resize();
                if (_minimap_x_coord < 0)
                    _minimap_x_coord += 10;
                if (_minimap_x_coord >= 0)
                {
                    _minimap_x_coord = 0;
                    _is_minimap_animation_ended = true;
                    _is_construction_animation_ended = false;
                    _is_construction_shown = true;
                    _is_minimap_shown = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                }
            }
            yield return null;
        }
    }
    #endregion

    #region SELECTION IN/OUT ANIMATION
    IEnumerator _animate_construction()
    {
        while (!_is_construction_animation_ended)
        {
            if (!_is_construction_animation_ended && _is_construction_shown)
            {
                Resize();
                if (_is_minimap_shown)
                    _construction_x_coord = 178;
                else
                {
                    _construction_x_coord = 0;
                    _construction_width = 0;
                }
                if (_construction_y_coord < 196)
                    _construction_y_coord += 10;
                if (_construction_y_coord >= 196)
                {
                    _construction_y_coord = 196;
                    _is_construction_animation_ended = true;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            if (!_is_construction_animation_ended && !_is_construction_shown)
            {
                Resize();
                if (_construction_y_coord > 0)
                    _construction_y_coord -= 10;
                if (_construction_y_coord <= 0)
                {
                    _construction_y_coord = 0;
                    _is_construction_animation_ended = true;
                    _is_construction_shown = false;
                    SoundManager._instance._play_effect_once(_minimap_animation);
                    Resize();
                    GUIEvents.MenuHeightHasChanged(m_MainMenuHeight);
                    GUIEvents.MenuWidthHasChanged(m_MainMenuWidth);
                    yield break;
                }
            }
            yield return null;
        }
    }
    #endregion


    public bool IsWithin(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector3 realScreenPos = new Vector3(screenPos.x, Screen.height - screenPos.y, screenPos.z);
        if (DragArea.Contains(realScreenPos))
            return true;
        return false;
    }

    public void Resize()
    {
        ManagerResolver.Resolve<IMiniMapController>().LoadMiniMap(out m_MainMenuWidth, out m_MainMenuHeight, out m_MiniMapRect); //Resolution has changed, resize all GUI elements (minimap first)
    }

    void OnApplicationQuit()
    {
        if (!_is_quit_dialog_shown)
        {
            _is_quit_dialog_shown = true;
            Application.CancelQuit();
        }
    }

    void OnApplicationFocus(bool focusStatus)
    {
        _is_paused = focusStatus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        _is_paused = pauseStatus;
    }
}
