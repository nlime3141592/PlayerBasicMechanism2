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
            case 0: return "IdleGround";
            case 1: return "IdleGroundLong";
            case 2: return "Sit";
            case 3: return "HeadUp";
            case 4: return "Walk";
            case 5: return "Run";
            case 6: return "FreeFall";
            case 7: return "Gliding";
            case 8: return "IdleWall";
            case 9: return "WallSliding";
            case 10: return "LedgeClimbHead";
            case 11: return "LedgeClimbBody";
            case 12: return "JumpGround";
            case 13: return "JumpDown";
            case 14: return "Roll";
            case 15: return "JumpAir";
            case 16: return "Dash";
            case 17: return "TakeDown";
            case 18: return "JumpWall";
            default: return "ERROR";
        }
    }
}
