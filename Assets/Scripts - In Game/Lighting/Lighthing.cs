using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Lighting : MonoBehaviour, ILightable
{
    public abstract void CommandLighting(int _order_type);
    public abstract LightsController.LightsMode GetLighting();
}
