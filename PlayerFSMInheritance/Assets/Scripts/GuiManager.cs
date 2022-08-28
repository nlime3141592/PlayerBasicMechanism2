using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuiManager : MonoBehaviour
{
    public __Player player;
    public Text stateText;
    public Text rollCoolFrame;
    public Text leftJumpCount;
    public Text leftJumpAirCount;
    public Text leftDashCount;

    private string[] states = new string[]
    {
        "Idle Basic",
        "Idle Long",
        "Idle Wall",
        "Air",
        "Gliding",
        "Move Walk",
        "Move Run",
        "Jump Basic",
        "Jump Air",
        "Jump Wall",
        "Jump Down",
        "Wall Sliding",
        "Ledge Climb",
        "Sit",
        "Head Up",
        "Roll",
        "Dash",
        "Take Down"
    };

    void Start()
    {
        
    }

    void Update()
    {
        if(player == null) return;

        stateText.text = string.Format("Current State: {0}", states[player.CURRENT_STATE]);
        rollCoolFrame.text = string.Format("Roll Cool Frame: {0}", player.leftRollCoolFrame);
        leftJumpCount.text = string.Format("Left Jump Basic Count: {0}", player.leftJumpBasicCount);
        leftJumpAirCount.text = string.Format("Left Jump Air Count: {0}", player.leftJumpAirCount);
        leftDashCount.text = string.Format("Left Dash Count: {0}", player.leftDashCount);
    }
}
