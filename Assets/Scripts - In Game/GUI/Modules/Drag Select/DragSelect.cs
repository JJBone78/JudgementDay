using UnityEngine;
using System.Collections;

public class DragSelect : MonoBehaviour {
	
	private GUIStyle m_DragStyle = new GUIStyle();
	
	private Vector2 m_DragLocationStart;
	private Vector2 m_DragLocationEnd;
	
	private bool m_CheckDeselect = false;
	private bool m_Dragging = false;
	
	private IGUIManager m_GuiManager;
	private ISelectedManager m_SelectedManager;

    private float _gui_width = 0;
    private float _gui_height = 0;

    void Awake()
	{
        GUIEvents.MenuWidthChanged += MenuWidthChanged;
        GUIEvents.MenuHeightChanged += MenuHeightChanged;
    }

    // Use this for initialization
    void Start () 
	{
		m_DragStyle.normal.background = TextureGenerator.MakeTexture (0.8f, 0.8f, 0.8f, 0.3f);
		m_DragStyle.border.bottom = 1;
		m_DragStyle.border.top = 1;
		m_DragStyle.border.left = 1;
		m_DragStyle.border.right = 1;
		
		m_GuiManager = ManagerResolver.Resolve<IGUIManager>();
		m_SelectedManager = ManagerResolver.Resolve<ISelectedManager>();
		
		ManagerResolver.Resolve<IEventsManager>().MouseClick += LeftButtonPressed;
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_CheckDeselect)
		{
			if (Mathf.Abs (Input.mousePosition.x - m_DragLocationStart.x) > 2 && Mathf.Abs (Input.mousePosition.y-m_DragLocationStart.y) > 2)
			{
				m_CheckDeselect = false;
				m_Dragging = true;
				m_GuiManager.Dragging = true;
				m_SelectedManager.DeselectAll ();
			}
		}
	}
	
	void OnGUI()
	{
		if (m_Dragging)
		{
			m_DragLocationEnd = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			DragBox (m_DragLocationStart, m_DragLocationEnd, m_DragStyle);
		}
	}
	
	public void LeftButtonPressed(object sender, MouseEventArgs e)
	{
        if (e.button == 0)
		{
			if (!e.buttonUp && e.X < Screen.width - _gui_width && e.Y > Screen.height - (Screen.height - _gui_height))
			{
				m_DragLocationStart = new Vector2(e.X, e.Y);
				m_CheckDeselect = true;
			}
			else
			{
				m_CheckDeselect = false;
				m_Dragging = false;
				m_GuiManager.Dragging = false;
			}
		}
	}
	
	public void DragBox(Vector2 topLeft, Vector2 bottomRight, GUIStyle style)
	{
		float minX = Mathf.Max (topLeft.x, bottomRight.x);
		float maxX = Mathf.Min (topLeft.x, bottomRight.x);
		
		float minY = Mathf.Max (Screen.height-topLeft.y, Screen.height-bottomRight.y);
		float maxY = Mathf.Min (Screen.height-topLeft.y, Screen.height-bottomRight.y);
				
		Rect rect = new Rect(minX, minY, maxX-minX, maxY-minY);
		
		//Don't let the dragged area interfere with the gui
		if (rect.xMin > Screen.width-_gui_width)
			rect.xMin = Screen.width-_gui_width;
        if (rect.yMin > Screen.height - _gui_height)
            rect.yMin = Screen.height - _gui_height;
        if (rect.xMax > Screen.width - _gui_width)
            rect.xMax = Screen.width - _gui_width;
	    if (rect.yMax > Screen.height - _gui_height)
	        rect.yMax = Screen.height - _gui_height;

        m_GuiManager.DragArea = new Rect(maxX, maxY, minX-maxX, minY-maxY);
		
		GUI.Box (rect, "", style);
	}
	
	public void MenuWidthChanged(float _new_width)
	{
		_gui_width = _new_width;
	}

    public void MenuHeightChanged(float _new_height)
    {
        _gui_height = _new_height;
    }
}