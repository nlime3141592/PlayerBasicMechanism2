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
    
    public RaycastHit2D headThroughableGroundBefore;
    public RaycastHit2D headThroughableGround;

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
        float fy = feetBox.bounds.max.y;

        float hx = headBox.bounds.center.x + headBox.bounds.extents.x * lookingDirection;
        float hy = headBox.bounds.min.y;

        feetSidePos.Set(fx, fy);
        headSidePos.Set(hx, hy);
    }

    protected void UpdateLookingDirection(int xNegative, int xPositive)
    {
        int xInput = xNegative + xPositive;

        if(lookingDirection == 0)
            lookingDirection = 1;

        if(xInput != 0)
            lookingDirection = xInput;
    }

    // Terrain Checker
    protected void CheckLedge()
    {
        if(!canCheckLedge)
        {
            isHitLedge = false;
            detectedLedge = default(RaycastHit2D);
            ledgeCornerTopPos = Vector2.zero;
            ledgeCornerSidePos = Vector2.zero;
            return;
        }

        int layer = LayerInfo.groundMask;

        float detectLength = 0.04f;
        float offsetLength = 0.2f;
        Vector2 headSideTopPos = headSidePos + Vector2.up * offsetLength;
        Vector2 detectDir = Vector2.right * lookingDirection;

        RaycastHit2D headSide = Physics2D.Raycast(headSidePos, detectDir, detectLength, layer);
        RaycastHit2D headTopSide = Physics2D.Raycast(headSideTopPos, detectDir, detectLength, layer);

        if(headSide && !headTopSide)
        {
            isHitLedge = true;

            float adder = 0.02f;
            float distance = (headSide.distance + adder) * lookingDirection;
            detectedLedge = Physics2D.Raycast(headSideTopPos + Vector2.right * distance, Vector2.down, offsetLength, layer);
            ledgeCornerTopPos.Set(detectedLedge.point.x, detectedLedge.point.y);
            ledgeCornerSidePos.Set(detectedLedge.point.x - adder * lookingDirection, detectedLedge.point.y - adder);
        }
        else
        {
            isHitLedge = false;
            detectedLedge = default(RaycastHit2D);
            ledgeCornerTopPos = Vector2.zero;
            ledgeCornerSidePos = Vector2.zero;
        }
    }

    protected void CheckThroughableToUp()
    {
        float detectLength = base.height + 2.0f;
        int layer = LayerInfo.throughableGroundMask;

        headThroughableGroundBefore = headThroughableGround;
        headThroughableGround = Physics2D.Raycast(feetPos, Vector2.up, detectLength, layer);

        if(headThroughableGroundBefore)
        {
            if(!headThroughableGround)
            {
                AcceptCollision(headThroughableGroundBefore.collider);
            }
            else if(headThroughableGroundBefore.collider != headThroughableGround.collider)
            {
                AcceptCollision(headThroughableGroundBefore.collider);
                IgnoreCollision(headThroughableGround.collider);
            }
        }
        else if(headThroughableGround)
        {
            IgnoreCollision(headThroughableGround.collider);
        }
    }

    protected void CheckThroughableToDown()
    {
        float detectLength = base.height + 2.0f;
        int layer = LayerInfo.throughableGroundMask;

        sitThroughableGround = Physics2D.Raycast(headPos, Vector2.down, detectLength, layer);
    }

    // Unity Event Functions
    protected override void Start()
    {
        base.Start();

        machine = new StateMachine(stIdleOnGround);

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
        glidingGraphX = new DiscreteLinearGraph(glidingFrameX);
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

        int sp = 3;
        int xInput = InputHandler.data.xInput * sp;
        int yInput = InputHandler.data.yInput * sp;

        DisableGravity();

        SetVelocityXY(xInput, yInput);

        UpdateFeetPosition();
        UpdateHeadPosition();
        UpdateSidePosition();
        UpdateLookingDirection(InputHandler.data.xNegative, InputHandler.data.xPositive);

        // CheckGroundBasic(out detectedGround, out isHitGround, feetPos, 0.04f);
        CheckGroundThroughable(out detectedGround, out isHitGround, feetPos, 0.04f);
        CheckCeil(out detectedCeil, out isHitCeil, headPos, 0.04f);
        CheckWall(out detectedFeetSideWall, out isHitFeetSideWall, feetSidePos, 0.04f, lookingDirection);
        CheckWall(out detectedHeadSideWall, out isHitHeadSideWall, headSidePos, 0.04f, lookingDirection);
        CheckLedge();
        // CheckThroughableToUp();
        CheckThroughableToDown();
    }

    #region Implement State; stIdleOnGround
    #endregion
}