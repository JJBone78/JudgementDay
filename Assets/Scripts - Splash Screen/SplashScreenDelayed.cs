using UnityEngine;
using System.Collections;

public class SplashScreenDelayed : MonoBehaviour {
    public float _delay_time = 3;
	// Use this for initialization
	IEnumerator Start () 
    {
        yield return new WaitForSeconds(_delay_time);
        //Application.LoadedLevel(1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
