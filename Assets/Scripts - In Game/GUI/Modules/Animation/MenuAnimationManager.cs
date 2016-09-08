using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MenuAnimationManager : MonoBehaviour
{
    public float animationTime = 0.1f;
    private float animationCounter = 0;
    private int index = 0;

    public virtual void Animate(float deltaTime)
    {
        animationCounter += deltaTime;

        if (animationCounter >= animationTime)
        {
            animationCounter = 0;
            index++;
            if (index >= 100) // 100 = CursorPicture.Count
            {
                index = 0;
            }
        }
    }
}