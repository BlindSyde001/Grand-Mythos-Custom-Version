using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightToggle : MonoBehaviour
{

    public GameObject Day;
    public GameObject Night;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (Day.activeInHierarchy)
            {
                Night.SetActive(true);
                Day.SetActive(false);
            }
            else
            {
                Night.SetActive(false);
                Day.SetActive(true);
            }
        }
    }
}
