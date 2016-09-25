using UnityEngine;
using System.Collections;

public class Selena : Air
{
    // Use this for initialization
    protected new void Start()
    {
        //Assign variables for health/movement and so on..
        AssignDetails(ItemDB.Selena);
        GetComponent<Movement>().AssignDetails(ItemDB.Selena);

        //Call base class start
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }
}
