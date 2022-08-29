using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player class
public class Human : MovableObject
{
    #region Player State Constants
    private const int stIdleOnGround = 0;
    private const int stIdleLongOnGround = 1;
    private const int stSit = 2;
    private const int stHeadUp = 3;
    private const int stWalk = 4;
    private const int stRun = 5;
    private const int stFreeFall = 6;
    private const int stGliding = 7;
    private const int stIdleWall = 8;
    private const int stWallSliding = 9;
    private const int stLedgeClimb = 10;
    private const int stJumpOnGround = 11;
    private const int stJumpDown = 12;
    private const int stRoll = 13;
    private const int stJumpOnAir = 14;
    private const int stDash = 15;
    private const int stTakeDown = 16;
    private const int stJumpOnWall = 17;
    #endregion

    // Entity Physics
    protected Vector2 feetPos;
    protected Vector2 headPos;
    protected Vector2 feetSidePos;
    protected Vector2 headSidePos;

    public RaycastHit2D detectedGround;
    public bool isDetectedGround;
    public bool isHitGround;

    public RaycastHit2D detectedCeil;
    public bool isDetectedCeil;
    public bool isHitCeil;

    public RaycastHit2D detectedFeetSideWall;
    public int isHitFeetSideWall;

    public RaycastHit2D detectedHeadSideWall;
    public int isHitHeadSideWall;

    public int lookingDirection;
    public bool isRun;

    #region State Constants and Variables
    private StateMachine machine;

    // stIdleOnGround options
    public int proceedIdleOnGroundFrame;

    // stLongIdleOnGround options
    public int longIdleTransitionFrame;

    // stSit options
    public int proceedSitFrame;
    public RaycastHit2D sitThroughableGround;

    // stHeadUp options
    public int proceedHeadUpFrame;

    // stWalk options
    public float walkSpeed;

    // stRun options
    public float runSpeed;

    // stFreeFall options
    public float maxFreeFallSpeed;
    public int freeFallFrame;
    private DiscreteGraph freeFallGraph;
    public int proceedFreeFallFrame;

    // stGliding options
    public float glidingSpeed;
    public int glidingFrameX;
    private DiscreteGraph glidingGraphX;
    public int proceedGlidingFrameX;

    // stIdleWall options

    // stWallSliding options
    public float maxWallSlidingSpeed;
    public int wallSlidingFrame;
    private DiscreteGraph wallSlidingGraph;
    public int proceedWallSlidingFrame;

    // stLedgeClimb options
    public bool canCheckLedge;
    public RaycastHit2D detectedLedge;
    public bool isHitLedge;
    public Vector2 ledgeCornerTopPos;
    public Vector2 ledgeCornerSidePos;
    public bool isEndOfLedgeAnimation;

    // stJumpOnGround options
    public int jumpOnGroundCount;
    public float jumpOnGroundSpeed;
    public int jumpOnGroundFrame;
    private DiscreteGraph jumpOnGroundGraph;
    public int leftJumpOnGroundCount;
    public int leftJumpOnGroundFrame;

    // stJumpDown options
    public float jumpDownSpeed;
    public int jumpDownFrame;
    private DiscreteGraph jumpDownGraph;
    public Collider2D currentJumpDownGround; // layer of "ThroughableGround" only.
    public int leftJumpDownFrame;

    // stRoll options
    public float rollSpeed;
    public int rollStartFrame;
    public int rollInvincibilityFrame;
    public int rollWakeUpFrame;
    private DiscreteGraph rollGraph;
    public int leftRollStartFrame;
    public int leftRollInvincibilityFrame;
    public int leftRollWakeUpFrame;

    // stJumpOnAir options
    public int jumpOnAirCount;
    public float jumpOnAirSpeed;
    public int jumpOnAirFrame;
    private DiscreteGraph jumpOnAirGraph;
    public int leftJumpOnAirCount;
    public int leftJumpOnAirFrame;

    // stDash options
    public int dashCount;
    public float dashSpeed;
    public int dashIdleFrame;
    public int dashInvincibilityFrame;
    private DiscreteGraph dashGraph;
    public int leftDashCount;
    public int leftDashIdleFrame;
    public int leftDashInvincibilityFrame;

    // stTakeDown
    public float takeDownSpeed;
    public int takeDownAirIdleFrame;
    public int takeDownLandingIdleFrame;
    public int leftTakeDownAirIdleFrame;
    public int leftTakeDownLandingIdleFrame;

    // stJumpOnWall
    public float jumpOnWallSpeedX;
    public float jumpOnWallSpeedY;
    public int jumpOnWallFrame;
    public int jumpOnWallForceFrame;
    private DiscreteGraph jumpOnWallGraphX;
    private DiscreteGraph jumpOnWallGraphY;
    public int leftJumpOnWallFrame;
    public int leftJumpOnWallForceFrame;
    #endregion

    // Update Physics
    protected void UpdateFeetPosition()
    {
        feetPos.Set(feetBox.bounds.center.x, feetBox.bounds.min.y);
    }

    protected void UpdateHeadPosition()
    {
        headPos.Set(headBox.bounds.center.x, headBox.bounds.max.y);
    }

    protected void UpdateSidePosition()
    {
        float fx = feetBox.bounds.center.x + feetBox.bounds.extents.x * lookingDirection;
        float fy = feetBox.bounds.min.y;

        float hx = headBox.bounds.center.x + headBox.bounds.extents.x * lookingDirection;
        float hy = headBox.bounds.max.y;

        feetSidePos.Set(fx, fy);
        headSidePos.Set(hx, hy);
    }

    // Unity Event Functions
    protected override void Start()
    {
        base.Start();

        machine.SetCallbacks(stIdleOnGround, null, null, null, null);
        machine.SetCallbacks(stIdleLongOnGround, null, null, null, null);
        machine.SetCallbacks(stSit, null, null, null, null);
        machine.SetCallbacks(stHeadUp, null, null, null, null);
        machine.SetCallbacks(stWalk, null, null, null, null);
        machine.SetCallbacks(stRun, null, null, null, null);
        machine.SetCallbacks(stFreeFall, null, null, null, null);
        machine.SetCallbacks(stGliding, null, null, null, null);
        machine.SetCallbacks(stIdleWall, null, null, null, null);
        machine.SetCallbacks(stWallSliding, null, null, null, null);
        machine.SetCallbacks(stLedgeClimb, null, null, null, null);
        machine.SetCallbacks(stJumpOnGround, null, null, null, null);
        machine.SetCallbacks(stJumpDown, null, null, null, null);
        machine.SetCallbacks(stRoll, null, null, null, null);
        machine.SetCallbacks(stJumpOnAir, null, null, null, null);
        machine.SetCallbacks(stDash, null, null, null, null);
        machine.SetCallbacks(stTakeDown, null, null, null, null);
        machine.SetCallbacks(stJumpOnWall, null, null, null, null);

        freeFallGraph = new DiscreteLinearGraph(freeFallFrame);
        glidingAccelGraphX = new DiscreteLinearGraph(glidingAccelFrameX);
        glidingDeaccelGraphX = new DiscreteLinearGraph(glidingDeaccelFrameX);
        glidingGraphY = new DiscreteLinearGraph(glidingFrameY);
        wallSlidingGraph = new DiscreteLinearGraph(wallSlidingFrame);
        jumpOnGroundGraph = new DiscreteLinearGraph(jumpOnGroundFrame);
        jumpDownGraph = new DiscreteLinearGraph(jumpDownFrame);
        rollGraph = new DiscreteLinearGraph(rollStartFrame + rollInvincibilityFrame + rollWakeUpFrame);
        jumpOnAirGraph = new DiscreteLinearGraph(jumpOnAirFrame);
        dashGraph = new DiscreteLinearGraph(dashInvincibilityFrame);
        jumpOnWallGraphX = new DiscreteLinearGraph(jumpOnWallFrame);
        jumpOnWallGraphY = new DiscreteLinearGraph(jumpOnWallFrame);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #region Implement State; stIdleOnGround
    #endregion
}