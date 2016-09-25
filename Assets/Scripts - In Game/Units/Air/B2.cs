using UnityEngine;
using System.Collections;

public class B2 : MonoBehaviour
{
    public float _descending_speed = 200;
    public bool _is_descending = true;

    public float journeyTime = 0.5f;
    private float startTime;
   
    void Start()
    {
        //AssignDetails(ItemDB.B2);
        //GetComponent<Movement>().AssignDetails(ItemDB.B2);
        //base.Start();
        iTween.MoveTo(gameObject, iTween.Hash("y", 15, "easeType", "easeOutQuart", "loopType", "PingPong", "time", 10));
    }

    void Update()
    {




        //base.Update();
        if (_is_descending)
        {
            if (transform.position.y > 15)
            {
                //transform.Translate(Vector3.down * _descending_speed * Time.deltaTime);
                //if (transform.position.y < 250 && transform.position.y > 150)
                //{
                //    if (_descending_speed > 90)
                //        _descending_speed -= 2.2f;
                //}
                //else if (transform.position.y < 150 && transform.position.y > 100)
                //{
                //    if (_descending_speed > 60)
                //        _descending_speed -= 1.8f;
                //}
                //else if (transform.position.y < 100 && transform.position.y > 50)
                //{
                //    if (_descending_speed > 30)
                //        _descending_speed -= 1f;
                //}
                //else if (transform.position.y < 50 && transform.position.y > 15)
                //{
                //    if (_descending_speed > 1)
                //        _descending_speed -= 0.5f;
                //}
                ////else if (transform.position.y > 150 && transform.position.y > 1000)
                ////    if (_descending_speed > 200)
                ////        _descending_speed -= 1f;
            }
            else
            {
                //yield return new WaitForSeconds(1f);
                _is_descending = false;
            }
        }
        else
        {
            if (transform.position.y < 1000)
            {
                //transform.Translate(Vector3.up * _descending_speed * Time.deltaTime); // 15-50 0.1   50-150 0.2 150-1000 0.4
                //if (transform.position.y < 50)
                //{
                //    if (_descending_speed < 30)
                //        _descending_speed += 0.5f;
                //}
                //else if (transform.position.y > 50 && transform.position.y < 100)
                //{
                //    if (_descending_speed < 100)
                //        _descending_speed += 1f;
                //}
                //else if (transform.position.y > 100 && transform.position.y < 250)
                //{
                //    if (_descending_speed < 200)
                //        _descending_speed += 1.8f;
                //}
                //else
                //{
                //    if (_descending_speed < 200)
                //        _descending_speed += 2.2f;
                //}
                ////else if (transform.position.y > 150 && transform.position.y < 1000)
                ////    if (_descending_speed < 200)
                ////        _descending_speed += 1f;
            }
            else
            {
                _descending_speed = 200;
                _is_descending = true;
            }
        }
        Debug.Log(_descending_speed);
    }
}
