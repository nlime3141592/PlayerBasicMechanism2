using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player class
public class Human : MovableObject
{
    #region Components
    private SpriteRenderer spRenderer;

    #endregion
    #region Player State Constants
    private const int stIdleOnGround = 0;
    private const int stIdleLongOnGround = 1;
    private const int stSit = 2;
    private const int stHeadUp = 3;
    private const int stWalk = 4;
    private const int stRun = 5;
    private const int stFreeFall = 6;
    private const int stGliding = 7;
    private const int stIdleOnWall = 8;
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
    protected Vector2 leftLedgeHangingPos;
    protected Vector2 rightLedgeHangingPos;

    public bool canCheckLedgeHanging = true;
    protected RaycastHit2D leftHangingGround;
    protected RaycastHit2D rightHangingGround;
    public bool isHangingOnGround;

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
    private InputData preInputData;
    private uint preInputPressing = 0;
    private uint preInputDown = 0;

    #region State Constants and Variables
    private StateMachine machine;
    public int currentState => machine.state;

    // stIdleOnGround options
    public int proceedIdleOnGroundFrame;
    public int preInputFrame_IdleOnGround;

    // stLongIdleOnGround options
    public int longIdleTransitionFrame = 900;

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
    public int preInputFrame_FreeFall;

    // stGliding options
    public float glidingSpeed = 3.5f;
    public int glidingAccelFrameX = 39;
    public int glidingDeaccelFrameX = 26;
    private DiscreteGraph glidingAccelGraphX;
    private DiscreteGraph glidingDeaccelGraphX;
    public int proceedGlidingAccelFrameX;
    public int leftGlidingDeaccelFrameX;

    // stIdleOnWall options
    public int preInputFrame_IdleOnWall;

    // stWallSliding options
    public float maxWallSlidingSpeed = 1.5f;
    public int wallSlidingFrame = 26;
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
    public int jumpOnGroundCount = 1;
    public float jumpOnGroundSpeed = 5.5f;
    public int jumpOnGroundFrame = 18;
    private DiscreteGraph jumpOnGroundGraph;
    public int leftJumpOnGroundCount;
    public int leftJumpOnGroundFrame;
    public bool isCancelOfJumpOnGround;

    // stJumpDown options
    public float jumpDownSpeed = 1.5f;
    public int jumpDownFrame = 13;
    private DiscreteGraph jumpDownGraph;
    public Collider2D currentJumpDownGround; // layer of "ThroughableGround" only.
    public int leftJumpDownFrame;

    // stRoll options
    public float rollSpeed = 9.5f;
    public int rollStartFrame = 6;
    public int rollInvincibilityFrame = 18;
    public int rollWakeUpFrame = 6;
    private DiscreteGraph rollGraph;
    public int leftRollStartFrame;
    public int leftRollInvincibilityFrame;
    public int leftRollWakeUpFrame;
    public int leftRollFrame;
    public int rollLookingDirection;

    // stJumpOnAir options
    public int jumpOnAirCount = 1;
    public float jumpOnAirSpeed = 7.5f;
    public int jumpOnAirIdleFrame = 3;
    public int jumpOnAirFrame = 20;
    private DiscreteGraph jumpOnAirGraph;
    public int leftJumpOnAirCount;
    public int leftJumpOnAirIdleFrame;
    public int leftJumpOnAirFrame;
    public bool isCancelOfJumpOnAir;

    // stDash options
    public int dashCount = 1;
    public float dashSpeed = 36;
    public int dashIdleFrame = 6;
    public int dashInvincibilityFrame = 9;
    private DiscreteGraph dashGraph;
    public int leftDashCount;
    public int leftDashIdleFrame;
    public int leftDashInvincibilityFrame;
    public int dashLookingDirection;

    // stTakeDown
    public float takeDownSpeed = 48;
    public int takeDownAirIdleFrame = 18;
    public int takeDownLandingIdleFrame = 12;
    public int leftTakeDownAirIdleFrame;
    public int leftTakeDownLandingIdleFrame;
    public bool isLandingAfterTakeDown;

    // stJumpOnWall
    public float jumpOnWallSpeedX = 7;
    public float jumpOnWallSpeedY = 10;
    public int jumpOnWallFrame = 13;
    public int jumpOnWallForceFrame = 6;
    private DiscreteGraph jumpOnWallGraphX;
    private DiscreteGraph jumpOnWallGraphY;
    public int leftJumpOnWallFrame;
    public int leftJumpOnWallForceFrame;
    public int jumpOnWallLookingDirection;
    public bool isCancelOfJumpOnWallX;
    public bool isCancelOfJumpOnWallXY;
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

    protected float CheckVelocityX(float vx)
    {
        if(inputData.xInput == lookingDirection && (isHitFeetSideWall == lookingDirection || isHitHeadSideWall == lookingDirection))
            return 0.0f;

        return vx;
    }

    protected void CheckLedgeHanging()
    {
        if(!canCheckLedgeHanging)
        {
            leftHangingGround = default(RaycastHit2D);
            rightHangingGround = default(RaycastHit2D);
            isHangingOnGround = false;
            return;
        }

        leftLedgeHangingPos.Set(feetBox.bounds.min.x, feetBox.bounds.center.y);
        rightLedgeHangingPos.Set(feetBox.bounds.max.x, feetBox.bounds.center.y);

        float detectLength = 0.04f;
        int layer = LayerInfo.groundMask | LayerInfo.throughableGroundMask;

        leftHangingGround = Physics2D.Raycast(leftLedgeHangingPos, Vector2.down, detectLength, layer);
        rightHangingGround = Physics2D.Raycast(rightLedgeHangingPos, Vector2.down, detectLength, layer);

        isHangingOnGround = leftHangingGround || rightHangingGround;
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

            float adder = 0.1f;
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

            canCheckGround = true;
            canCheckCeil = true;
            canCheckLedgeHanging = true;
            return;
        }

        float detectLength = base.height + 0.5f;
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

        if(current)
        {
            canCheckGround = false;
            canCheckCeil = false;
            canCheckLedgeHanging = false;
        }
        else
        {
            canCheckGround = true;
            canCheckCeil = true;
            canCheckLedgeHanging = true;
        }
    }

    protected void CheckThroughableToDown(out RaycastHit2D ground)
    {
        float detectLength = base.height + 0.5f;
        int layer = LayerInfo.throughableGroundMask;

        ground = Physics2D.Raycast(headPos, Vector2.down, detectLength, layer);
    }

    // Unity Event Functions
    protected override void Start()
    {
        base.Start();

        spRenderer = GetComponent<SpriteRenderer>();

        machine = new StateMachine(stIdleOnGround);

        machine.SetCallbacks(stIdleOnGround, Input_IdleOnGround, Logic_IdleOnGround, Enter_IdleOnGround, End_IdleOnGround);
        machine.SetCallbacks(stIdleLongOnGround, Input_IdleLongOnGround, Logic_IdleLongOnGround, Enter_IdleLongOnGround, null);
        machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, End_Sit);
        machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, End_HeadUp);
        machine.SetCallbacks(stWalk, Input_Walk, Logic_Walk, Enter_Walk, null);
        machine.SetCallbacks(stRun, Input_Run, Logic_Run, Enter_Run, null);
        machine.SetCallbacks(stFreeFall, Input_FreeFall, Logic_FreeFall, Enter_FreeFall, null);
        machine.SetCallbacks(stGliding, Input_Gliding, Logic_Gliding, Enter_Gliding, null);
        machine.SetCallbacks(stIdleOnWall, Input_IdleOnWall, Logic_IdleOnWall, Enter_IdleOnWall, null);
        machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, null);
        machine.SetCallbacks(stLedgeClimb, Input_LedgeClimb, Logic_LedgeClimb, Enter_LedgeClimb, End_LedgeClimb);
        machine.SetCallbacks(stJumpOnGround, Input_JumpOnGround, Logic_JumpOnGround, Enter_JumpOnGround, null);
        machine.SetCallbacks(stJumpDown, Input_JumpDown, Logic_JumpDown, Enter_JumpDown, End_JumpDown);
        machine.SetCallbacks(stRoll, Input_Roll, Logic_Roll, Enter_Roll, null);
        machine.SetCallbacks(stJumpOnAir, Input_JumpOnAir, Logic_JumpOnAir, Enter_JumpOnAir, null);
        machine.SetCallbacks(stDash, Input_Dash, Logic_Dash, Enter_Dash, null);
        machine.SetCallbacks(stTakeDown, Input_TakeDown, Logic_TakeDown, Enter_TakeDown, null);
        machine.SetCallbacks(stJumpOnWall, Input_JumpOnWall, Logic_JumpOnWall, Enter_JumpOnWall, null);

        InitGraphs();

        // 파일 생성하는 기능
        // FileCreator.Initialize();
    }

    private void InitGraphs()
    {
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

        spRenderer.flipX = (lookingDirection == 1);

        // Terrain Checking
        // CheckGroundAll(out detectedGround, out isHitGround, feetPos, 0.04f);

        if(canCheckGround)
        {
            CheckGroundAll(out detectedGround, out isDetectedGround, feetPos, 0.5f);
            isHitGround = isDetectedGround && detectedGround.distance <= 0.04f;
        }
        else
        {
            detectedGround = default(RaycastHit2D);
            isDetectedGround = false;
            isHitGround = false;
        }

        if(canCheckCeil)
        {
            CheckCeil(out detectedCeil, out isHitCeil, headPos, 0.04f);
        }
        else
        {
            detectedCeil = default(RaycastHit2D);
            isHitCeil = false;
        }

        CheckThroughableToUp(ref headThroughableGroundBefore, ref headThroughableGround);
        CheckWall(out detectedFeetSideWall, out isHitFeetSideWall, feetSidePos, 0.04f, lookingDirection);
        CheckWall(out detectedHeadSideWall, out isHitHeadSideWall, headSidePos, 0.04f, lookingDirection);
        CheckLedge();

        // Physics Check After
        UpdateMoveDirection();
        CheckLedgeHanging();

        // Machine Logic
        machine.UpdateLogic();
    }

    #region Implement State; stIdleOnGround
    private void Enter_IdleOnGround()
    {
        DisableGravity();

        proceedIdleOnGroundFrame = 0;

        leftJumpOnGroundCount = jumpOnGroundCount;
        leftJumpOnAirCount = jumpOnAirCount;
        leftDashCount = dashCount;

        // 선입력 확인
        uint mask_jumpOnGround      = 0b00000000000000000000000000000001;
        uint mask_roll              = 0b00000000000000000000000000000010; // NOTE: 미구현
        preInputPressing = 0b00000000000000000000000000000000;
        preInputDown = 0b00000000000000000000000000000000;

        for(int i = 0; i < preInputFrame_IdleOnGround; i++)
        {
            preInputData.Copy(InputHandler.data);

            if((preInputPressing & mask_jumpOnGround) == 0 && preInputData.jumpPressing)
                preInputPressing |= mask_jumpOnGround;
            if((preInputDown & mask_jumpOnGround) == 0 && preInputData.jumpDown)
                preInputDown |= mask_jumpOnGround;
        }

        if((preInputDown & mask_jumpOnGround) != 0 && (preInputPressing & mask_jumpOnGround) != 0 && leftJumpOnGroundCount > 0)
            machine.ChangeState(stJumpOnGround);
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
    private void Enter_IdleLongOnGround()
    {
        DisableGravity();
    }

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
        DisableGravity();

        proceedSitFrame = 0;

        CheckThroughableToDown(out sitThroughableGround);
    }

    private void Input_Sit()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown)
        {
            if(sitThroughableGround)
                machine.ChangeState(stJumpDown);
            else
                machine.ChangeState(stJumpOnGround);
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
        DisableGravity();

        proceedHeadUpFrame = 0;
    }

    private void Input_HeadUp()
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

    private void End_HeadUp()
    {
        proceedHeadUpFrame = 0;
    }
    #endregion

    #region Implement State; stWalk
    private void Enter_Walk()
    {
        EnableGravity();
    }

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
        float speed = CheckVelocityX(walkSpeed);

        if(speed == 0)
            DisableGravity();
        else
            EnableGravity();

        Logic_MoveOnGround(moveDirection, speed, lookingDirection);
    }
    #endregion

    #region Implement State; stRun
    private void Enter_Run()
    {
        EnableGravity();
    }

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
        float speed = CheckVelocityX(runSpeed);

        if(speed == 0)
            DisableGravity();
        else
            EnableGravity();

        Logic_MoveOnGround(moveDirection, speed, lookingDirection);
    }
    #endregion

    #region Implement State; stFreeFall
    private void Enter_FreeFall()
    {
        EnableGravity();

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

        // 선입력
        uint mask_jumpOnAir = 0b00000000000000000000000000000001;
        preInputPressing = 0b00000000000000000000000000000000;
        preInputDown = 0b00000000000000000000000000000000;

        for(int i = 0; i < preInputFrame_FreeFall; i++)
        {
            preInputData.Copy(InputHandler.data);

            if((preInputPressing & mask_jumpOnAir) == 0 && preInputData.jumpPressing)
                preInputPressing |= mask_jumpOnAir;
            if((preInputDown & mask_jumpOnAir) == 0 && preInputData.jumpDown)
                preInputDown |= mask_jumpOnAir;
        }

        if((preInputDown & mask_jumpOnAir) != 0 && (preInputPressing & mask_jumpOnAir) != 0 && leftJumpOnAirCount > 0)
            machine.ChangeState(stJumpOnAir);
    }

    private void Input_FreeFall()
    {
        if(isHitGround)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(inputData.jumpDown)
        {
            if(inputData.yNegative != 0)
                machine.ChangeState(stTakeDown);
            else if(leftJumpOnAirCount > 0)
                machine.ChangeState(stJumpOnAir);
        }
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stGliding);
        }
        else if(inputData.xInput == lookingDirection)
        {
            if(isHitLedge)
                machine.ChangeState(stLedgeClimb);
            else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                machine.ChangeState(stIdleOnWall);
        }
    }

    private void Logic_FreeFall()
    {
        // TODO: velocity.y가 0이 되는 순간 프레임을 초기화 하는 방법은 어떤가?

        if(isHangingOnGround)
        {
            proceedFreeFallFrame = 0;
        }
        else
        {
            if(proceedFreeFallFrame < freeFallFrame)
                proceedFreeFallFrame++;

            float vx = GetMoveSpeed() * inputData.xInput;
            vx = CheckVelocityX(vx);
            float vy = -maxFreeFallSpeed * freeFallGraph[proceedFreeFallFrame - 1];

            SetVelocityXY(vx, vy);
        }
    }
    #endregion

    #region Implement State; stGliding
    private void Enter_Gliding()
    {
        EnableGravity();

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
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(inputData.yPositive == 0)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == lookingDirection)
        {
            if(isHitLedge)
                machine.ChangeState(stLedgeClimb);
            else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                machine.ChangeState(stIdleOnWall);
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
            vx = CheckVelocityX(vx);
        }
        else if(inputData.xInput != 0)
        {
            if(proceedGlidingAccelFrameX < glidingAccelFrameX)
                proceedGlidingAccelFrameX++;

            leftGlidingDeaccelFrameX = glidingDeaccelFrameX;

            vx = GetMoveSpeed() * glidingAccelGraphX[proceedGlidingAccelFrameX - 1] * lookingDirection;
            vx = CheckVelocityX(vx);
        }

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stIdleOnWall
    private void Enter_IdleOnWall()
    {
        DisableGravity();
        leftJumpOnAirCount = jumpOnAirCount;
        leftDashCount = dashCount;

        // 선입력
        uint mask_jumpOnWall = 0b00000000000000000000000000000001;
        preInputPressing = 0b00000000000000000000000000000000;
        preInputDown = 0b00000000000000000000000000000000;

        for(int i = 0; i < preInputFrame_FreeFall; i++)
        {
            preInputData.Copy(InputHandler.data);

            if((preInputPressing & mask_jumpOnWall) == 0 && preInputData.jumpPressing)
                preInputPressing |= mask_jumpOnWall;
            if((preInputDown & mask_jumpOnWall) == 0 && preInputData.jumpDown)
                preInputDown |= mask_jumpOnWall;
        }

        if((preInputDown & mask_jumpOnWall) != 0 && (preInputPressing & mask_jumpOnWall) != 0)
            machine.ChangeState(stJumpOnWall);
    }

    private void Input_IdleOnWall()
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

    private void Logic_IdleOnWall()
    {
        SetVelocityXY(0.0f, 0.0f);
    }
    #endregion

    #region Implement State; stWallSliding
    private void Enter_WallSliding()
    {
        EnableGravity();

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
            machine.ChangeState(stIdleOnWall);
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
        DisableGravity();

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

        // NOTE: 자유 낙하를 구현해야 하는 이유를 모르겠다.
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
        EnableGravity();

        leftJumpOnGroundCount--;
        leftJumpOnGroundFrame = jumpOnGroundFrame;
        isCancelOfJumpOnGround = false;
    }

    private void Input_JumpOnGround()
    {
        if(inputData.jumpUp && !isCancelOfJumpOnGround)
        {
            leftJumpOnGroundFrame /= 2;
        }

        if(isHitCeil || leftJumpOnGroundFrame == 0)
        {
            if(inputData.yPositive == 0)
                machine.ChangeState(stFreeFall);
            else
                machine.ChangeState(stGliding);
        }
        else if(inputData.jumpDown)
        {
            if(inputData.yNegative != 0)
                machine.ChangeState(stTakeDown);
            else if(leftJumpOnAirCount > 0)
                machine.ChangeState(stJumpOnAir);
        }
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(inputData.xInput == lookingDirection)
        {
            if(isHitLedge)
                machine.ChangeState(stLedgeClimb);
            else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                machine.ChangeState(stIdleOnWall);
        }
    }

    private void Logic_JumpOnGround()
    {
        if(leftJumpOnGroundFrame > 0)
            leftJumpOnGroundFrame--;

        float vx = inputData.xInput * GetMoveSpeed();
        float vy = jumpOnGroundSpeed * jumpOnGroundGraph[leftJumpOnGroundFrame];

        vx = CheckVelocityX(vx);

        SetVelocityXY(vx, vy);
    }
    #endregion

    #region Implement State; stJumpDown
    private void Enter_JumpDown()
    {
        EnableGravity();

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
        rollLookingDirection = lookingDirection;
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

        float speed = CheckVelocityX(rollSpeed);

        if(speed == 0)
            DisableGravity();
        else
            EnableGravity();

        Logic_MoveOnGround(moveDirection, speed * rollGraph[leftRollFrame], rollLookingDirection);
    }
    #endregion

    #region Implement State; stJumpOnAir
    private void Enter_JumpOnAir()
    {
        DisableGravity();

        leftJumpOnAirCount--;

        leftJumpOnAirIdleFrame = jumpOnAirIdleFrame;
        leftJumpOnAirFrame = 0;
        isCancelOfJumpOnAir = false;
    }

    private void Input_JumpOnAir()
    {
        if(inputData.jumpUp && !isCancelOfJumpOnAir)
        {
            leftJumpOnAirFrame /= 2;
        }

        if((leftJumpOnAirIdleFrame == 0 && leftJumpOnAirFrame == 0) || isHitCeil)
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
        else if(inputData.xInput == lookingDirection)
        {
            if(isHitLedge)
                machine.ChangeState(stLedgeClimb);
            else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                machine.ChangeState(stIdleOnWall);
        }
    }

    private void Logic_JumpOnAir()
    {
        if(leftJumpOnAirIdleFrame > 0)
        {
            leftJumpOnAirIdleFrame--;

            DisableGravity();
            SetVelocityXY(0.0f, 0.0f);

            if(leftJumpOnAirIdleFrame == 0)
                leftJumpOnAirFrame = jumpOnAirFrame;
        }
        else if(leftJumpOnAirFrame > 0)
        {
            EnableGravity();

            leftJumpOnAirFrame--;

            float vx = inputData.xInput * GetMoveSpeed();
            float vy = jumpOnAirSpeed * jumpOnAirGraph[leftJumpOnAirFrame];

            vx = CheckVelocityX(vx);

            SetVelocityXY(vx, vy);
        }
    }
    #endregion

    #region Implement State; stDash
    private void Enter_Dash()
    {
        DisableGravity();

        leftDashCount--;
        leftDashIdleFrame = dashIdleFrame;
        leftDashInvincibilityFrame = 0;
        dashLookingDirection = lookingDirection;
    }

    private void Input_Dash()
    {
        if(leftDashIdleFrame == 0 && leftDashInvincibilityFrame == 0)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == lookingDirection)
        {
            if(isHitLedge)
                machine.ChangeState(stLedgeClimb);
            else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                machine.ChangeState(stIdleOnWall);
        }
    }

    private void Logic_Dash()
    {
        if(leftDashIdleFrame > 0)
        {
            leftDashIdleFrame--;

            DisableGravity();

            SetVelocityXY(0.0f, 0.0f);

            if(leftDashIdleFrame == 0)
                leftDashInvincibilityFrame = dashInvincibilityFrame;
        }
        else if(leftDashInvincibilityFrame > 0)
        {
            leftDashInvincibilityFrame--;

            EnableGravity();

            float vx = dashSpeed * dashGraph[leftDashInvincibilityFrame] * dashLookingDirection;
            float vy = 0.0f;

            vx = CheckVelocityX(vx);

            SetVelocityXY(vx, vy);
        }
    }
    #endregion

    #region Implement State; stTakeDown
    private void Enter_TakeDown()
    {
        leftTakeDownAirIdleFrame = takeDownAirIdleFrame;
        leftTakeDownLandingIdleFrame = 0;
        isLandingAfterTakeDown = false;
    }

    private void Input_TakeDown()
    {
        if(leftTakeDownAirIdleFrame == 0 && leftTakeDownLandingIdleFrame == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(isLandingAfterTakeDown && !isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
    }

    private void Logic_TakeDown()
    {
        if(leftTakeDownAirIdleFrame > 0)
        {
            leftTakeDownAirIdleFrame--;

            DisableGravity();
            SetVelocityXY(0.0f, 0.0f);

            if(leftTakeDownAirIdleFrame == 0)
                leftTakeDownLandingIdleFrame = takeDownLandingIdleFrame;
        }
        else if(!isHitGround)
        {
            EnableGravity();
            SetVelocityXY(0.0f, -takeDownSpeed);
        }
        else if(leftTakeDownLandingIdleFrame > 0)
        {
            DisableGravity();

            // TODO: 부서지는 바닥 탐지를 이 곳에서 수행하고, 바닥을 부순다.

            leftTakeDownLandingIdleFrame--;
            isLandingAfterTakeDown = true;

            SetVelocityXY(0.0f, 0.0f);
        }
    }
    #endregion

    #region Implement State; stJumpOnWall
    private void Enter_JumpOnWall()
    {
        EnableGravity();

        leftJumpOnWallForceFrame = jumpOnWallForceFrame;
        leftJumpOnWallFrame = jumpOnWallFrame;
        jumpOnWallLookingDirection = -lookingDirection;
        isCancelOfJumpOnWallX = false;
        isCancelOfJumpOnWallXY = false;
    }

    private void Input_JumpOnWall()
    {
        if(leftJumpOnWallForceFrame == 0)
        {
            // 상태 기계의 상태변경을 위한 제어 함수
            if(inputData.jumpDown)
            {
                if(inputData.yNegative != 0 && !isDetectedGround)
                    machine.ChangeState(stTakeDown);
                else if(leftJumpOnAirCount > 0)
                    machine.ChangeState(stJumpOnAir);
            }
            else if(inputData.dashDown && leftDashCount > 0)
            {
                machine.ChangeState(stDash);
            }
            else if(isHitCeil || leftJumpOnWallFrame == 0)
            {
                if(inputData.yPositive == 0)
                    machine.ChangeState(stFreeFall);
                else
                    machine.ChangeState(stGliding);
            }
            else if(inputData.xInput == lookingDirection)
            {
                if(isHitLedge)
                    machine.ChangeState(stLedgeClimb);
                else if(!isDetectedGround && inputData.yNegative == 0 && isHitFeetSideWall == lookingDirection && isHitHeadSideWall == lookingDirection)
                    machine.ChangeState(stIdleOnWall);
            }

            // 내부 상태의 변경을 위한 제어 함수
            if(inputData.jumpUp && !isCancelOfJumpOnWallXY)
            {
                isCancelOfJumpOnWallXY = true;
                leftJumpOnWallFrame /= 2;
            }
            if(inputData.xInput != 0 && !isCancelOfJumpOnWallX)
            {
                isCancelOfJumpOnWallX = true;
            }
        }
    }

    private void Logic_JumpOnWall()
    {
        if(leftJumpOnWallForceFrame > 0)
        {
            leftJumpOnWallForceFrame--;
        }

        if(leftJumpOnWallFrame > 0)
        {
            leftJumpOnWallFrame--;

            float vx = isCancelOfJumpOnWallX ? GetMoveSpeed() * inputData.xInput : jumpOnWallSpeedX * jumpOnWallGraphX[leftJumpOnWallFrame] * jumpOnWallLookingDirection;
            float vy = jumpOnWallSpeedY * jumpOnWallGraphY[leftJumpOnWallFrame];

            SetVelocityXY(vx, vy);
        }
    }
    #endregion


    #region FILE_INPUT
    public void ApplyFile()
    {
        string path = Application.persistentDataPath + "/DataTable.txt";

        using(StreamReader stream = new StreamReader(path))
        {
            while(!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                if(line == "")
                    continue;

                // string message = line.Replace(" ", "").Split('#')[0];
                string message = line.Replace(" ", "");
                Debug.Log(message);
                string[] token = message.Split(':');
                foreach(string s in token)
                {
                    Debug.Log("\t" + s);
                }
                // SwitchFileData(token[0], token[1]);
            }
        }

        // 값 로드
        // 값 저장
        InitGraphs();

        transform.position = new Vector3(-15.0f, 4.0f, 0.0f);
    }

    private void SwitchFileData(string token_name, string token_value)
    {
        switch(token_name)
        {
            case "isRun":
                isRun = valueToBool(token_value);
                break;
            case "longIdleTransitionFrame":
                longIdleTransitionFrame = valueToInt(token_value);
                break;
            case "walkSpeed":
                walkSpeed = valueToFloat(token_value);
                break;
            case "runSpeed":
                runSpeed = valueToFloat(token_value);
                break;
            case "maxFreeFallSpeed":
                maxFreeFallSpeed = valueToFloat(token_value);
                break;
            case "freeFallFrame":
                freeFallFrame = valueToInt(token_value);
                break;
            case "glidingSpeed":
                glidingSpeed = valueToFloat(token_value);
                break;
            case "glidingAccelFrameX":
                glidingAccelFrameX = valueToInt(token_value);
                break;
            case "glidingDeaccelFrameX":
                glidingDeaccelFrameX = valueToInt(token_value);
                break;
            case "maxWallSlidingSpeed":
                maxWallSlidingSpeed = valueToFloat(token_value);
                break;
            case "wallSlidingFrame":
                wallSlidingFrame = valueToInt(token_value);
                break;
            case "jumpOnGroundCount":
                jumpOnGroundCount = valueToInt(token_value);
                break;
            case "jumpOnGroundSpeed":
                jumpOnGroundSpeed = valueToFloat(token_value);
                break;
            case "jumpOnGroundFrame":
                jumpOnGroundFrame = valueToInt(token_value);
                break;
            case "jumpDownSpeed":
                jumpDownSpeed = valueToFloat(token_value);
                break;
            case "jumpDownFrame":
                jumpDownFrame = valueToInt(token_value);
                break;
            case "rollSpeed":
                rollSpeed = valueToFloat(token_value);
                break;
            case "rollStartFrame":
                rollStartFrame = valueToInt(token_value);
                break;
            case "rollInvincibilityFrame":
                rollInvincibilityFrame = valueToInt(token_value);
                break;
            case "rollWakeUpFrame":
                rollWakeUpFrame = valueToInt(token_value);
                break;
            case "jumpOnAirCount":
                jumpOnAirCount = valueToInt(token_value);
                break;
            case "jumpOnAirSpeed":
                jumpOnAirSpeed = valueToFloat(token_value);
                break;
            case "jumpOnAirIdleFrame":
                jumpOnAirIdleFrame = valueToInt(token_value);
                break;
            case "jumpOnAirFrame":
                jumpOnAirFrame = valueToInt(token_value);
                break;
            case "dashCount":
                dashCount = valueToInt(token_value);
                break;
            case "dashSpeed":
                dashSpeed = valueToFloat(token_value);
                break;
            case "dashIdleFrame":
                dashIdleFrame = valueToInt(token_value);
                break;
            case "dashInvincibilityFrame":
                dashInvincibilityFrame = valueToInt(token_value);
                break;
            case "takeDownSpeed":
                takeDownSpeed = valueToFloat(token_value);
                break;
            case "takeDownAirIdleFrame":
                takeDownAirIdleFrame = valueToInt(token_value);
                break;
            case "takeDownLandingIdleFrame":
                takeDownLandingIdleFrame = valueToInt(token_value);
                break;
            case "jumpOnWallSpeedX":
                jumpOnWallSpeedX = valueToFloat(token_value);
                break;
            case "jumpOnWallSpeedY":
                jumpOnWallSpeedY = valueToFloat(token_value);
                break;
            case "jumpOnWallFrame":
                jumpOnWallFrame = valueToInt(token_value);
                break;
            case "jumpOnWallForceFrame":
                jumpOnWallForceFrame = valueToInt(token_value);
                break;
        }
    }

    private float valueToFloat(string str)
    {
        return float.Parse(str);
    }

    private bool valueToBool(string str)
    {
        return bool.Parse(str);
    }

    private int valueToInt(string str)
    {
        return int.Parse(str);
    }
    #endregion
}