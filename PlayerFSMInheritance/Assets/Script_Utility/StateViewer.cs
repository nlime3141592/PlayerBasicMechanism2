using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateViewer : MonoBehaviour
{
    public Player human;
    private Text comp;
    
    void Start()
    {
        comp = GetComponent<Text>();
    }

    void Update()
    {
        comp.text = string.Format("State: {0}", SwitchState());
    }

    string SwitchState()
    {
        int state = human.currentState;

        switch(state)
        {
            case 0: return "IdleOnGround";
            case 1: return "IdleLongOnGround";
            case 2: return "Sit";
            case 3: return "HeadUp";
            case 4: return "Walk";
            case 5: return "Run";
            case 6: return "FreeFall";
            case 7: return "Gliding";
            case 8: return "IdleOnWall";
            case 9: return "WallSliding";
            case 10: return "LedgeClimb";
            case 11: return "JumpOnGround";
            case 12: return "JumpDown";
            case 13: return "Roll";
            case 14: return "JumpOnAir";
            case 15: return "Dash";
            case 16: return "TakeDown";
            case 17: return "JumpOnWall";
            default: return "ERROR";
        }
    }
}
