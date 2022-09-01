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
    protected Vector2 bodyPos;
    protected Vector2 feetSidePos;
    protected Vector2 headSidePos;

    public bool canCheckGround = true;
    public RaycastHit2D detectedGround;
    public bool isDetectedGround;
    public bool isHitGround;

    public bool canCheckCeil = true;
    public RaycastHit2D detectedCeil;
    public bool isDetectedCeil;
    public bool isHitCeil;

    public RaycastHit2D detectedFeetSideWall;
    public int isHitFeetSideWall;

    public RaycastHit2D detectedHeadSideWall;
    public int isHitHeadSideWall;

    public Vector2 moveDirection;
    public bool canUpdateLookingDirection = true;
    public int lookingDirection = 1;
    public bool isRun = false;
    
    public bool canCheckThroughableGroundToUp = true;
    public RaycastHit2D headThroughableGroundBefore;
    public RaycastHit2D headThroughableGround;

    // Input Handling
    private InputData inputData;

    #region State Constants and Variables
    private StateMachine machine;

    // stIdleOnGround options
    public int proceedIdleOnGroundFrame;

    // stLongIdleOnGround options
    public int longIdleTransitionFrame = 120;

    // stSit options
    public int proceedSitFrame;
    public RaycastHit2D sitThroughableGround;

    // stHeadUp options
    public int proceedHeadUpFrame;

    // stWalk options
    public float walkSpeed = 3.5f;

    // stRun options
    public float runSpeed = 7.5f;

    // stFreeFall options
    public float maxFreeFallSpeed = 12.0f;
    public int freeFallFrame = 39;
    private DiscreteGraph freeFallGraph;
    public int proceedFreeFallFrame;

    // stGliding options
    public float glidingSpeed = 3.5f;
    public int glidingAccelFrameX = 39;
    public int glidingDeaccelFrameX = 26;
    private DiscreteGraph glidingAccelGraphX;
    private DiscreteGraph glidingDeaccelGraphX;
    public int proceedGlidingAccelFrameX;
    public int leftGlidingDeaccelFrameX;

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
    public Vector2 ledgeTeleportPos;
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
    public int leftRollFrame;

    // stJumpOnAir options
    public int jumpOnAirCount;
    public float jumpOnAirSpeed;
    public int jumpOnAirIdleFrame;
    public int jumpOnAirFrame;
    private DiscreteGraph jumpOnAirGraph;
    public int leftJumpOnAirCount;
    public int leftJumpOnAirIdleFrame;
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

    protected void UpdateBodyPosition()
    {
        bodyPos.Set(bodyBox.transform.position.x, bodyBox.transform.position.y);
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
        if(lookingDirection == 0)
            lookingDirection = 1;

        if(!canUpdateLookingDirection)
            return;

        int xInput = xNegative + xPositive;

        if(xInput != 0)
            lookingDirection = xInput;
    }

    protected void UpdateMoveDirection()
    {
        if(isHitGround)
        {
            float x = detectedGround.normal.y;
            float y = -detectedGround.normal.x;

            moveDirection.Set(x, y);
        }
        else
        {
            moveDirection.Set(1.0f, 0.0f);
        }
    }

    protected float GetMoveSpeed()
    {
        return isRun ? runSpeed : walkSpeed;
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

    protected void CheckThroughableToUp(ref RaycastHit2D before, ref RaycastHit2D current)
    {
        before = current;

        if(!canCheckThroughableGroundToUp)
        {
            current = default(RaycastHit2D);
            return;
        }

        float detectLength = base.height + 2.0f;
        int layer = LayerInfo.throughableGroundMask;

        before = current;
        current = Physics2D.Raycast(feetPos, Vector2.up, detectLength, layer);

        if(before)
        {
            if(!current)
            {
                AcceptCollision(before.collider);
            }
            else if(before.collider != current.collider)
            {
                AcceptCollision(before.collider);
                IgnoreCollision(current.collider);
            }
        }
        else if(current)
        {
            IgnoreCollision(current.collider);
        }
    }

    protected void CheckThroughableToDown(out RaycastHit2D ground)
    {
        float detectLength = base.height + 2.0f;
        int layer = LayerInfo.throughableGroundMask;

        ground = Physics2D.Raycast(headPos, Vector2.down, detectLength, layer);
    }

    // Unity Event Functions
    protected override void Start()
    {
        base.Start();

        machine = new StateMachine(stIdleOnGround);

        machine.SetCallbacks(stIdleOnGround, Input_IdleOnGround, Logic_IdleOnGround, Enter_IdleOnGround, null);
        machine.SetCallbacks(stIdleLongOnGround, Input_IdleLongOnGround, Logic_IdleLongOnGround, null, null);
        machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, End_Sit);
        machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, null);
        machine.SetCallbacks(stWalk, Input_Walk, Logic_Walk, null, null);
        machine.SetCallbacks(stRun, Input_Run, Logic_Run, null, null);
        machine.SetCallbacks(stFreeFall, Input_FreeFall, Logic_FreeFall, Enter_FreeFall, null);
        machine.SetCallbacks(stGliding, Input_Gliding, Logic_Gliding, Enter_Gliding, null);
        machine.SetCallbacks(stIdleWall, Input_IdleWall, Logic_IdleWall, Enter_IdleWall, End_IdleWall);
        machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, null);
        machine.SetCallbacks(stLedgeClimb, Input_LedgeClimb, Logic_LedgeClimb, Enter_LedgeClimb, End_LedgeClimb);
        machine.SetCallbacks(stJumpOnGround, Input_JumpOnGround, Logic_JumpOnGround, Enter_JumpOnGround, End_JumpOnGround);
        machine.SetCallbacks(stJumpDown, Input_JumpDown, Logic_JumpDown, Enter_JumpDown, End_JumpDown);
        machine.SetCallbacks(stRoll, Input_Roll, Logic_Roll, Enter_Roll, null);
        machine.SetCallbacks(stJumpOnAir, Input_JumpOnAir, Logic_JumpOnAir, Enter_JumpOnAir, null);
        machine.SetCallbacks(stDash, null, null, null, null);
        machine.SetCallbacks(stTakeDown, null, null, null, null);
        machine.SetCallbacks(stJumpOnWall, null, null, null, null);

        freeFallGraph = new DiscreteLinearGraph(freeFallFrame);
        glidingAccelGraphX = new DiscreteLinearGraph(glidingAccelFrameX);
        glidingDeaccelGraphX = new DiscreteLinearGraph(glidingDeaccelFrameX);
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

        inputData.Copy(InputHandler.data);

        machine.UpdateInput();

        Debug.Log(string.Format("current state: {0}", machine.state));
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Physics Check Before
        UpdateFeetPosition();
        UpdateHeadPosition();
        UpdateBodyPosition();
        UpdateSidePosition();
        UpdateLookingDirection(inputData.xNegative, inputData.xPositive);

        // Terrain Checking
        // CheckGroundAll(out detectedGround, out isHitGround, feetPos, 0.04f);
        CheckGroundAll(out detectedGround, out isDetectedGround, feetPos, 0.5f);
        isHitGround = isDetectedGround && detectedGround.distance <= 0.04f;
        CheckCeil(out detectedCeil, out isHitCeil, headPos, 0.04f);
        CheckThroughableToUp(ref headThroughableGroundBefore, ref headThroughableGround);
        CheckWall(out detectedFeetSideWall, out isHitFeetSideWall, feetSidePos, 0.04f, lookingDirection);
        CheckWall(out detectedHeadSideWall, out isHitHeadSideWall, headSidePos, 0.04f, lookingDirection);
        CheckLedge();

        // Physics Check After
        UpdateMoveDirection();

        // Machine Logic
        machine.UpdateLogic();
    }

    #region Implement State; stIdleOnGround
    private void Enter_IdleOnGround()
    {
        proceedIdleOnGroundFrame = 0;

        leftJumpOnGroundCount = jumpOnGroundCount;
        leftJumpOnAirCount = jumpOnAirCount;
    }

    private void Input_IdleOnGround()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.yNegative != 0)
        {
            machine.ChangeState(stSit);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stHeadUp);
        }
        else if(inputData.xInput != 0)
        {
            if(isRun)
                machine.ChangeState(stRun);
            else
                machine.ChangeState(stWalk);
        }
        else if(proceedIdleOnGroundFrame >= longIdleTransitionFrame)
        {
            machine.ChangeState(stIdleLongOnGround);
        }
    }

    private void Logic_IdleOnGround()
    {
        proceedIdleOnGroundFrame++;

        SetVelocityXY(0.0f, 0.0f);
    }

    private void End_IdleOnGround()
    {
        proceedIdleOnGroundFrame = 0;
    }
    #endregion

    #region Implement State; stIdleLongOnGround
    private void Input_IdleLongOnGround()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.yNegative != 0)
        {
            machine.ChangeState(stSit);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stHeadUp);
        }
        else if(inputData.xInput != 0)
        {
            if(isRun)
                machine.ChangeState(stRun);
            else
                machine.ChangeState(stWalk);
        }
    }

    private void Logic_IdleLongOnGround()
    {
        SetVelocityXY(0.0f, 0.0f);
    }
    #endregion

    #region Implement State; stSit
    private void Enter_Sit()
    {
        proceedSitFrame = 0;

        CheckThroughableToDown(out sitThroughableGround);
    }

    private void Input_Sit()
    {
        if(inputData.jumpDown)
        {
            if(sitThroughableGround)
            {
                machine.ChangeState(stJumpDown);
            }
            else
            {
                machine.ChangeState(stJumpOnGround);
            }
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.yNegative == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_Sit()
    {
        proceedSitFrame++;

        SetVelocityXY(0.0f, 0.0f);
    }

    private void End_Sit()
    {
        proceedSitFrame = 0;
    }
    #endregion

    #region Implement State; stHeadUp
    private void Enter_HeadUp()
    {
        proceedHeadUpFrame = 0;
    }

    private void Input_HeadUp()
    {
        if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.yPositive == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_HeadUp()
    {
        proceedHeadUpFrame++;

        SetVelocityXY(0.0f, 0.0f);
    }
    #endregion

    #region Implement State; stWalk
    private void Input_Walk()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.yNegative != 0)
        {
            machine.ChangeState(stSit);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stHeadUp);
        }
        else if(isRun)
        {
            machine.ChangeState(stRun);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.xInput == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_Walk()
    {
        Logic_MoveOnGround(moveDirection, walkSpeed, lookingDirection);
    }
    #endregion

    #region Implement State; stRun
    private void Input_Run()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.yNegative != 0)
        {
            machine.ChangeState(stSit);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stHeadUp);
        }
        else if(!isRun)
        {
            machine.ChangeState(stWalk);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.dashDown)
        {
            machine.ChangeState(stRoll);
        }
        else if(inputData.xInput == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_Run()
    {
        Logic_MoveOnGround(moveDirection, runSpeed, lookingDirection);
    }
    #endregion

    #region Implement State; stFreeFall
    private void Enter_FreeFall()
    {
        // proceedFreeFallFrame = 0;

        if(currentVelocity.y > 0.0f)
        {
            proceedFreeFallFrame = 0;
        }
        else if(currentVelocity.y < -maxFreeFallSpeed)
        {
            proceedFreeFallFrame = freeFallFrame;
        }
        else
        {
            for(int i = 0; i < freeFallFrame; i++)
            {
                if(currentVelocity.y >= -maxFreeFallSpeed * freeFallGraph[i])
                {
                    proceedFreeFallFrame = i;
                    break;
                }
            }
        }
    }

    private void Input_FreeFall()
    {
        if(isHitGround)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(inputData.jumpDown && leftJumpOnAirCount > 0)
        {
            machine.ChangeState(stJumpOnAir);
        }
        else if(inputData.xInput == lookingDirection && isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stGliding);
        }
        else if(inputData.xInput == lookingDirection && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection && inputData.yNegative == 0)
        {
            machine.ChangeState(stIdleWall);
        }
    }

    private void Logic_FreeFall()
    {
        // TODO: velocity.y가 0이 되는 순간 프레임을 초기화 하는 방법은 어떤가?

        if(proceedFreeFallFrame < freeFallFrame)
            proceedFreeFallFrame++;

        float vx = GetMoveSpeed() * inputData.xInput;
        float vy = -maxFreeFallSpeed * freeFallGraph[proceedFreeFallFrame - 1];

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stGliding
    private void Enter_Gliding()
    {
        // TODO: 자유낙하 프레임이 유지되듯이, x축 이동 프레임도 유지되어야 한다. 그 로직을 이 곳에 추가한다.
        if(Mathf.Abs(currentVelocity.x) == 0.0f)
        {
            leftGlidingDeaccelFrameX = 0;
            proceedGlidingAccelFrameX = 0;
        }
        else if(Mathf.Abs(currentVelocity.x) > GetMoveSpeed())
        {
            if(inputData.xInput == 0)
            {
                leftGlidingDeaccelFrameX = glidingDeaccelFrameX;
                proceedGlidingAccelFrameX = 0;
            }
            else
            {
                leftGlidingDeaccelFrameX = 0;
                proceedGlidingAccelFrameX = glidingAccelFrameX;
            }
        }
        else
        {
            if(inputData.xInput == 0)
            {
                for(int i = 0; i < glidingDeaccelFrameX; i++)
                {
                    if(Mathf.Abs(currentVelocity.x) >= glidingDeaccelGraphX[i])
                    {
                        leftGlidingDeaccelFrameX = i;
                        break;
                    }
                }
            }
            else
            {
                for(int i = 0; i < glidingAccelFrameX; i++)
                {
                    if(Mathf.Abs(currentVelocity.x) >= glidingAccelGraphX[i])
                    {
                        proceedGlidingAccelFrameX = i;
                        break;
                    }
                }
            }
        }
    }

    private void Input_Gliding()
    {
        if(isHitGround)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(inputData.jumpDown && leftJumpOnAirCount > 0)
        {
            machine.ChangeState(stJumpOnAir);
        }
        else if(inputData.xInput == lookingDirection && isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
        else if(inputData.yPositive == 0)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == lookingDirection && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection && inputData.yNegative == 0)
        {
            machine.ChangeState(stIdleWall);
        }
    }

    private void Logic_Gliding()
    {
        float vx = 0.0f;
        float vy = -glidingSpeed;

        if(currentVelocity.x * inputData.xInput < 0.0f)
        {
            vx = 0.0f;
            proceedGlidingAccelFrameX = 0;
            leftGlidingDeaccelFrameX = 0;
        }
        else if(inputData.xInput == 0)
        {
            if(leftGlidingDeaccelFrameX > 0)
                leftGlidingDeaccelFrameX--;

            proceedGlidingAccelFrameX = 0;

            vx = GetMoveSpeed() * glidingDeaccelGraphX[leftGlidingDeaccelFrameX] * lookingDirection;
        }
        else if(inputData.xInput != 0)
        {
            if(proceedGlidingAccelFrameX < glidingAccelFrameX)
                proceedGlidingAccelFrameX++;

            leftGlidingDeaccelFrameX = glidingDeaccelFrameX;

            vx = GetMoveSpeed() * glidingAccelGraphX[proceedGlidingAccelFrameX - 1] * lookingDirection;
        }

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stIdleWall
    private void Enter_IdleWall()
    {
        DisableGravity();
    }

    private void Input_IdleWall()
    {
        if(isDetectedGround || isHitFeetSideWall == 0 || isHitHeadSideWall == 0 || inputData.xNegDown)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == 0)
        {
            machine.ChangeState(stWallSliding);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnWall);
        }
    }

    private void Logic_IdleWall()
    {
        SetVelocityXY(0.0f, 0.0f);
    }

    private void End_IdleWall()
    {
        EnableGravity();
    }
    #endregion

    #region Implement State; stWallSliding
    private void Enter_WallSliding()
    {
        proceedWallSlidingFrame = 0;
    }

    private void Input_WallSliding()
    {
        if(isHitGround)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(isDetectedGround || isHitFeetSideWall == 0 || isHitHeadSideWall == 0 || inputData.yNegDown)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == lookingDirection && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection && inputData.yNegative == 0)
        {
            machine.ChangeState(stIdleWall);
        }
        else if(inputData.jumpDown)
        {
            machine.ChangeState(stJumpOnWall);
        }
    }

    private void Logic_WallSliding()
    {
        if(proceedWallSlidingFrame < wallSlidingFrame)
            proceedWallSlidingFrame++;

        float vx = 0.0f;
        float vy = -maxWallSlidingSpeed * wallSlidingGraph[proceedWallSlidingFrame - 1];

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stLedgeClimb
    private void Enter_LedgeClimb()
    {
        canUpdateLookingDirection = false;
        canCheckLedge = false;
        isEndOfLedgeAnimation = false;

        DisableGravity();

        Vector2 holdDir = transform.position - (Vector3)headSidePos;
        Vector2 teleportDir = transform.position - (Vector3)feetPos;

        transform.position = ledgeCornerSidePos + holdDir;
        ledgeTeleportPos = ledgeCornerTopPos + teleportDir;
    }

    private void Input_LedgeClimb()
    {
        // NOTE:
        // stIdleOnGround로 전이 시, stIdleOnGround -> stFreeFall -> stLedgeClimb를 한 프레임 안에 돌아서 오기 때문에
        // 플레이어가 난간 끝으로 텔레포트 하지 않는 현상이 발생한다.
        // => Enter Time에 transform.position = ledgeCornerSidePos + holdDir 호출 전, holdDir과 teleportDir을 설정해준다.
        // transform.position은 값 대입 즉시 갱신되지만, bodyPos, feetPos가 물리 프레임이 호출되기 전에 갱신이 되지 않으므로, 이에 따른 차이에 의해
        // 공중에 뜨는 현상이 발생한 것이다.
        if(isEndOfLedgeAnimation)
        {
            machine.ChangeState(stIdleOnGround);
        }

        // NOTE: 애니메이션 상태 머신 구현 전 임시 테스트 코드
        // TODO: 애니메이션 상태 머신 구현 후 이 코드를 지워야 한다.
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_LedgeClimb()
    {
        SetVelocityXY(0.0f, 0.0f);
    }

    private void End_LedgeClimb()
    {
        transform.position = ledgeTeleportPos;

        canUpdateLookingDirection = true;
        canCheckLedge = true;
    }
    #endregion

    #region Implement State; stJumpOnGround
    private void Enter_JumpOnGround()
    {
        leftJumpOnGroundCount--;
        leftJumpOnGroundFrame = jumpOnGroundFrame;
    }

    private void Input_JumpOnGround()
    {
        if(isHitCeil || leftJumpOnGroundFrame == 0)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == lookingDirection && isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
        else if(inputData.xInput == lookingDirection && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection && inputData.yNegative == 0)
        {
            machine.ChangeState(stIdleWall);
        }
        else if(inputData.jumpDown && leftJumpOnAirCount > 0)
        {
            machine.ChangeState(stJumpOnAir);
        }
    }

    private void Logic_JumpOnGround()
    {
        if(leftJumpOnGroundFrame > 0)
            leftJumpOnGroundFrame--;

        float vx = inputData.xInput * GetMoveSpeed();
        float vy = jumpOnGroundSpeed * jumpOnGroundGraph[leftJumpOnGroundFrame];

        SetVelocityXY(vx, vy);
    }

    private void End_JumpOnGround()
    {
        leftJumpOnGroundFrame = 0;
    }
    #endregion

    #region Implement State; stJumpDown
    private void Enter_JumpDown()
    {
        canCheckGround = false;
        canCheckLedge = false;
        canCheckThroughableGroundToUp = false;

        currentJumpDownGround = sitThroughableGround.collider;
        leftJumpDownFrame = jumpDownFrame;

        IgnoreCollision(currentJumpDownGround);
    }

    private void Input_JumpDown()
    {
        if(currentJumpDownGround == null || sitThroughableGround.collider != currentJumpDownGround)
        {
            machine.ChangeState(stFreeFall);
        }
    }

    private void Logic_JumpDown()
    {
        RaycastHit2D hit;
        CheckThroughableToDown(out hit);
        currentJumpDownGround = hit.collider;

        if(leftJumpDownFrame > 0)
        {
            proceedFreeFallFrame = 0;
            leftJumpDownFrame--;

            float vx = 0.0f;
            float vy = jumpDownSpeed * jumpDownGraph[leftJumpDownFrame];

            SetVelocityXY(vx, vy);
        }
        else
        {
            if(proceedFreeFallFrame < freeFallFrame)
                proceedFreeFallFrame++;

            float vx = 0.0f;
            float vy = -maxFreeFallSpeed * freeFallGraph[proceedFreeFallFrame - 1];

            SetVelocityXY(vx, vy);
        }
    }

    private void End_JumpDown()
    {
        AcceptCollision(sitThroughableGround.collider);

        canCheckGround = true;
        canCheckLedge = true;
        canCheckThroughableGroundToUp = true;
        currentJumpDownGround = null;
    }
    #endregion

    #region Implement State; stRoll
    private void Enter_Roll()
    {
        EnableGravity();

        leftRollStartFrame = rollStartFrame;
        leftRollInvincibilityFrame = 0;
        leftRollWakeUpFrame = 0;
        leftRollFrame = rollStartFrame + rollInvincibilityFrame + rollWakeUpFrame;
    }

    private void Input_Roll()
    {
        if(!isDetectedGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0 && leftRollFrame < rollInvincibilityFrame + rollWakeUpFrame)
        {
            machine.ChangeState(stJumpOnGround);
        }
        else if(inputData.xInput != 0 && leftRollFrame < rollWakeUpFrame)
        {
            if(isRun)
                machine.ChangeState(stRun);
            else
                machine.ChangeState(stWalk);
        }
        else if(leftRollFrame == 0)
        {
            if(inputData.yNegative != 0)
                machine.ChangeState(stSit);
            else
                machine.ChangeState(stIdleOnGround);
        }
    }

    private void Logic_Roll()
    {
        if(leftRollStartFrame > 0)
        {
            leftRollStartFrame--;

            if(leftRollStartFrame == 0)
                leftRollInvincibilityFrame = rollInvincibilityFrame;
        }
        else if(leftRollInvincibilityFrame > 0)
        {
            leftRollInvincibilityFrame--;

            if(leftRollInvincibilityFrame == 0)
                leftRollWakeUpFrame = rollWakeUpFrame;
        }
        else if(leftRollWakeUpFrame > 0)
        {
            leftRollWakeUpFrame--;
        }

        if(leftRollFrame > 0)
            leftRollFrame--;

        Logic_MoveOnGround(moveDirection, rollSpeed * rollGraph[leftRollFrame], lookingDirection);
    }
    #endregion

    #region Implement State; stJumpOnAir
    private void Enter_JumpOnAir()
    {
        leftJumpOnAirCount--;

        leftJumpOnAirIdleFrame = jumpOnAirIdleFrame;
        leftJumpOnAirFrame = 0;
    }

    private void Input_JumpOnAir()
    {
        if(isHitCeil)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(leftJumpOnAirIdleFrame == 0 && leftJumpOnAirFrame == 0)
        {
            if(inputData.yPositive == 0)
                machine.ChangeState(stFreeFall);
            else
                machine.ChangeState(stGliding);
        }
        else if(inputData.jumpDown)
        {
            if(leftJumpOnAirCount > 0)
                machine.RestartState();
            else if(inputData.yNegative != 0)
                machine.ChangeState(stTakeDown);
        }
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(inputData.xInput == lookingDirection && isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
    }

    private void Logic_JumpOnAir()
    {
        if(leftJumpOnAirIdleFrame > 0)
        {
            leftJumpOnAirIdleFrame--;

            SetVelocityXY(0.0f, 0.0f);

            if(leftJumpOnAirIdleFrame == 0)
                leftJumpOnAirFrame = jumpOnAirFrame;

            return;
        }
        else if(leftJumpOnAirFrame > 0)
        {
            leftJumpOnAirFrame--;
        }

        float vx = inputData.xInput * GetMoveSpeed();
        float vy = jumpOnAirSpeed * jumpOnAirGraph[leftJumpOnAirFrame];

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stDash
    private void Logic_Dash()
    {

    }
    #endregion

    #region Implement State; stTakeDown
    private void Logic_TakeDown()
    {

    }
    #endregion

    #region Implement State; stJumpOnWall
    private void Logic_JumpOnWall()
    {

    }
    #endregion
}