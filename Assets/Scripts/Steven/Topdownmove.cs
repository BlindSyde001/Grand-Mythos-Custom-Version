using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Topdownmove : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody rb;

    private Vector3 moveInput;

    private Vector3 moveVel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //might have to adjust to hor / vert 
        moveInput = new Vector3(Input.GetAxisRaw("Vertical"), 0f, Input.GetAxisRaw("Horizontal"));

        moveVel = moveInput * moveSpeed;
    }

    void FixedUpdate()
    {
        rb.velocity = moveVel;
    }
}
