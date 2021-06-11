using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerInputNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;

    internal bool isHoriPressed;
    internal bool isVertPressed;

    // UPDATES
    private void Update()
    {
        switch(_PlayerCircuit._GameState)
        {
            case (GameState.OVERWORLD):

                if (Input.GetButton("Horizontal"))
                    isHoriPressed = true;
                else
                    isHoriPressed = false;

                if (Input.GetButton("Vertical"))
                    isVertPressed = true;
                else
                    isVertPressed = false;

                if (isHoriPressed || isVertPressed)
                    _PlayerCircuit.isMoving = true;
                else
                    _PlayerCircuit.isMoving = false;
                break;
        }
    }
}
