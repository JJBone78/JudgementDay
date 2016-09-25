using UnityEngine;
using System.Collections;
using System;

public class LightsController : Lighting
{
    public float Intensity = 4f;
    public Light[] _lights = new Light[1];
    public VLight[] _v_lights = new VLight[1];
    private LightsMode _light_mode = LightsMode.Auto;

    public enum LightsMode
    {
        Auto = 0,
        On = 1,
        Off = 2
    }

    void Update()
    {
        if (_light_mode == LightsMode.Auto)
        {
            if (GUIManager._global_displayed_hour >= 6f && GUIManager._global_displayed_hour < 18f)
            {
                foreach (Light _light in _lights)
                {
                    if (_light.intensity > 0f)
                        _light.intensity -= 0.04f;
                    else
                        _light.intensity = 0f;
                }
                foreach (VLight _v_light in _v_lights)
                {
                    if (_v_light.lightMultiplier > 0f)
                        _v_light.lightMultiplier -= 0.01f;
                    else
                        _v_light.lightMultiplier = 0f;
                }
            }
            else
            {
                foreach (Light _light in _lights)
                {
                    if (_light.intensity < 4f)
                        _light.intensity += 0.04f;
                    else
                        _light.intensity = 4f;
                }
                foreach (VLight _v_light in _v_lights)
                {
                    if (_v_light.lightMultiplier < 1f)
                        _v_light.lightMultiplier += 0.01f;
                    else
                        _v_light.lightMultiplier = 1f;
                }
            }
        }
        else if (_light_mode == LightsMode.On)
        {
            foreach (Light _light in _lights)
                _light.intensity = 4f;
            foreach (VLight _v_light in _v_lights)
                _v_light.lightMultiplier = 1f;
        }
        else if (_light_mode == LightsMode.Off)
        {
            foreach (Light _light in _lights)
                _light.intensity = 0f;
            foreach (VLight _v_light in _v_lights)
                _v_light.lightMultiplier = 0f;
        }
    }

    public override void CommandLighting(int _order_type)
    {
        if (_order_type == 4) //lights on
            _light_mode = LightsMode.On;
        else if (_order_type == 5) //lights off
            _light_mode = LightsMode.Off;
        else if (_order_type == 6) //lights auto
            _light_mode = LightsMode.Auto;
    }

    public override LightsMode GetLighting()
    {
        return _light_mode;
    }
}
