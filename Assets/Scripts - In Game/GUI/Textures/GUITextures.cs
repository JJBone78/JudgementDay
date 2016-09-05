using UnityEngine;
using System.Collections;

public static class GUITextures 
{
	private static Texture2D m_TypeButtonNormal;
	private static Texture2D m_TypeButtonHover;
	private static Texture2D m_TypeButtonSelected;
	
	private static Texture2D m_QueueButtonNormal;
	private static Texture2D m_QueueButtonHover;
	private static Texture2D m_QueueButtonSelected;
	
	private static Texture2D m_QueueContentNormal;
	private static Texture2D m_QueueContentHover;
	private static Texture2D m_QueueContentSelected;
	
	public static Texture2D TypeButtonNormal
	{
		get
		{
			return m_TypeButtonNormal ?? (m_TypeButtonNormal = Resources.Load ("GUI/Buttons/TypeButtons/Normal") as Texture2D);
		}
	}
	
	public static Texture2D TypeButtonHover
	{
		get
		{
			return m_TypeButtonHover ?? (m_TypeButtonHover = Resources.Load ("GUI/Buttons/TypeButtons/Hover") as Texture2D);
		}
	}
	
	public static Texture2D TypeButtonSelected
	{
		get
		{
			return m_TypeButtonSelected ?? (m_TypeButtonSelected = Resources.Load ("GUI/Buttons/TypeButtons/Selected") as Texture2D);
		}
	}

    /// <summary>
    /// Tiles an <_area_to_fill> with a <_texture> which is scaled to the size of a <_tile> using a given <_scale_mode>
    /// </summary>
    /// <param name="_texture">Texture to draw</param>
    /// <param name="_tile">Size of a unit tile</param>
    /// <param name="_area_to_fill">Size of the area to fill with tiles</param>
    /// <param name="_scale_mode">Scale mode to draw the texture</param>
    public static void _tile_texture(Texture _texture, Rect _tile, Rect _area_to_fill, ScaleMode _scale_mode)
    { 
        for (float y = _area_to_fill.y; y < _area_to_fill.y + _area_to_fill.height; y = y + _tile.height)
        {
            for (float x = _area_to_fill.x; x < _area_to_fill.x + _area_to_fill.width; x = x + _tile.width)
            {
                _tile.x = x; _tile.y = y;
                GUI.DrawTexture(_tile, _texture, _scale_mode);
            }
        }
    }

    /// <summary>
    /// Tiles a <_region> with an <_area_to_fill> region of a <_texture>
    /// </summary>
    /// <param name="_region">Size of the area to fill with tiles</param>
    /// <param name="_texture">Texture to draw</param>
    /// <param name="_area_to_fill">The region of the texture to be drown, in percentage (0 to 1)</param>
    public static void _tile_texture_with_coordinates(Rect _region, Texture _texture, Rect _area_to_fill)
    {
        for (float y = _region.y; y < _region.y + _region.height; y = y + _area_to_fill.height)
        {
            for (float x = _region.x; x < _region.x + _region.width; x = x + _area_to_fill.width)
            {
                _region.x = x; _region.y = y;
                GUI.DrawTextureWithTexCoords(_region, _texture, _area_to_fill);
            }
        }
    }
}
