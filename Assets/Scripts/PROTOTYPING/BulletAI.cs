using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAI : MonoBehaviour
{
    Vector3 _prevPos;
    private void Start()
    {
        // Time limit on bullet
        //Destroy(this.gameObject, 1f);

        // TESTING //  SHOOT RAY TO KNOW WHERE BULLET WILL IMPACT
        Ray ray = new Ray(transform.position, transform.forward);
        //RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward * 100, Color.red, 2f);
    }

    private void Update()
    {
        // Check if bullet hit target by comparing previous position of bullet to next frame, using rays
        _prevPos = transform.position;

        transform.position += transform.forward * 45 * Time.deltaTime;

        RaycastHit[] hits = Physics.RaycastAll(new Ray(_prevPos, 
                                                      (transform.position - _prevPos).normalized), 
                                                      (transform.position - _prevPos).magnitude);
        if(hits.Length > 0)
        {
            Destroy(this.gameObject);
            Debug.Log(hits[0].collider.gameObject.name + " Has Been Hit!");
            //if(hits[0].collider.CompareTag("Chara"))
            //{

            //}
        }
    }
}
