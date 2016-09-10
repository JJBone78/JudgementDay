using UnityEngine;
using System.Collections;

public interface IMiniMapController {

	void LoadMiniMap(out float guiWidth, out float guiHeight, out Rect miniMapRect);
	void ReCalculateViewRect();
}
