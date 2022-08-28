using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Lives
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

    #region Player Constants
    private StateMachine machine;

    // stIdleOnGround options
    public int longIdleTransitionFrame;
    // stIdleLongOnGround options
    // stSit options
    public int sitFrame;
    // stHeadUp options
    public int headUpFrame;
    // stWalk options
    public float walkSpeed;
    // stRun options
    public bool isRunState;
    public float runSpeed;
    public int runAccelFrameX;
    // stFreeFall options
    public float maxFreeFallSpeedY;
    public int freeFallAccelFrameY;
    private DiscreteGraph freeFallAccelGraphY;
    // stGliding options
    public float glidingSpeed;
    public int glidingAccelFrameX;
    public int glidingDeaccelFrameX;
    private DiscreteGraph glidingAccelGraphX;
    private DiscreteGraph glidingDeaccelGraphX;
    // stIdleWall options
    // stWallSliding options
    public float maxWallSlidingSpeedY;
    public int wallSlidingAccelFrameY;
    private DiscreteGraph wallSlidingAccelGrpahY;
    // stLedgeClimb options
    // stJumpOnGround options
    public int jumpOnGroundCount;
    public float jumpOnGroundSpeed;
    public int jumpOnGroundDeaccelFrameY;
    private DiscreteGraph jumpOnGroundDeaccelGraphY;
    // stJumpDown options
    public float jumpDownSpeed;
    public int jumpDownDeaccelFrameY;
    private DiscreteGraph jumpDownDeaccelGraphY;
    // stRoll options
    public float rollSpeed;
    public int rollStartFrame;
    public int rollInvincibilityFrame;
    public int rollWakeUpFrame;
    // stJumpOnAir options
    public int jumpOnAirCount;
    public float jumpOnAirSpeed;
    public int jumpOnAirDeaccelFrameY;
    private DiscreteGraph jumpOnAirDeaccelGraphY;
    // stDash options
    public int dashCount;
    public float dashSpeed;
    public int dashStartFrame;
    public int dashInvincibilityFrame;
    // stTakeDown options
    public float takeDownSpeed;
    public int takeDownAirIdleFrame;
    public int takeDownLandingIdleFrame;
    // stJumpOnWall options
    public float jumpOnWallSpeedX;
    public float jumpOnWallSpeedY;
    public int jumpOnWallDeaccelFrame;
    private DiscreteGraph jumpOnWallDeaccelGraphX;
    private DiscreteGraph jumpOnWallDeaccelGraphY;
    
    #endregion

    #region Player Variables
    // physics options
    public Vector2 feetPosition;
    public Vector2 headPosition;
    public Vector2 feetSidePosition;
    public Vector2 headSidePosition;

    public Vector2 moveDirection;
    public int lookingDirection = 1;

    public bool canCheckGround = true;
    public RaycastHit2D detectedGround;
    public bool isDetectedGround = false;
    public bool isHitGround = false;

    public bool canCheckCeil = true;
    public RaycastHit2D detectedCeil;
    public bool isDetectedCeil = false;
    public bool isHitCeil = false;

    public bool canCheckFeetSideWall = true;
    public RaycastHit2D detectedFeetSideWall;
    public bool isDetectedFeetSideWall = false;
    public int isHitFeetSideWall = 0;

    public bool canCheckHeadSideWall = true;
    public RaycastHit2D detectedHeadSideWall;
    public bool isDetectedHeadSideWall = false;
    public int isHitHeadSideWall = 0;

    public bool canCheckJumpDownLower;
    public RaycastHit2D detectedJumpDownLowerGroundBefore;
    public RaycastHit2D detectedJumpDownLowerGround;

    public bool canCheckJumpDownUpper;
    public RaycastHit2D detectedJumpDownUpperGroundBefore;
    public RaycastHit2D detectedJumpDownUpperGround;

    public bool canCheckLedge;
    public bool isHitLedge;

    // stAbility options
    public bool isEndOfAbility = false;
    public bool isCancelOfAbility = false;

    // stIdleOnGround options
    public int proceedLongIdleTransitionFrame;
    // stIdleLongOnGround options
    // stSit options
    public int proceedSitFrame;
    // stHeadUp options
    public int proceedHeadUpFrame;
    // stWalk options
    // stRun options
    public int proceedRunAccelFrameX;
    // stFreeFall options
    public int proceedFreeFallAccelFrameY;
    // stGliding options
    public int proceedGlidingAccelFrameX;
    public int leftGlidingDeaccelFrameX;
    // stIdleWall options
    // stWallSliding options
    public int proceedWallSlidingAccelFrameY;
    // stLedgeClimb options
    // stJumpOnGround options
    public int leftJumpOnGroundCount;
    public int leftJumpOnGroundDeaccelFrameY;
    // stJumpDown options
    public int leftJumpDownDeaccelFrameY;
    // stRoll options
    public int leftRollStartFrame;
    public int leftRollInvincibilityFrame;
    public int leftRollWakeUpFrame;
    // stJumpOnAir options
    public int leftJumpOnAirCount;
    public int leftJumpOnAirDeaccelFrameY;
    // stDash options
    public int leftDashCount;
    public int leftDashStartFrame;
    public int leftDashInvincibilityFrame;
    // stTakeDown options
    public int leftTakeDownAirIdleFrame;
    public int leftTakeDownLandingIdleFrame;
    // stJumpOnWall options
    public int leftJumpOnWallDeaccelFrame;
    #endregion

    #region Input Datas
    public InputData inputData;
    #endregion

    #region Initializer
    protected override void InitComponents()
    {
        base.InitComponents();
    }

    protected override void InitGraphs()
    {
        base.InitGraphs();

        freeFallAccelGraphY = new DiscreteLinearGraph(freeFallAccelFrameY);
        glidingAccelGraphX = new DiscreteLinearGraph(glidingAccelFrameX);
        glidingDeaccelGraphX = new DiscreteLinearGraph(glidingDeaccelFrameX);
        wallSlidingAccelGrpahY = new DiscreteLinearGraph(wallSlidingAccelFrameY);
        jumpOnGroundDeaccelGraphY = new DiscreteLinearGraph(jumpOnGroundDeaccelFrameY);
        jumpDownDeaccelGraphY = new DiscreteLinearGraph(jumpDownDeaccelFrameY);
    }

    protected override void InitPhysics()
    {
        base.InitPhysics();
    }

    private void InitStateMachine()
    {
        machine = new StateMachine(stIdleOnGround);

        machine.SetCallbacks(stIdleOnGround, Input_IdleOnGround, Logic_IdleOnGround, Enter_IdleOnGround, null);
        machine.SetCallbacks(stIdleLongOnGround, Input_IdleLongOnGround, Logic_IdleLongOnGround, Enter_IdleLongOnGround, End_IdleLongOnGround);
        machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, null);
        machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, null);
        machine.SetCallbacks(stWalk, Input_Walk, Logic_Walk, Enter_Walk, null);
        machine.SetCallbacks(stRun, Input_Run, Logic_Run, Enter_Run, null);
        machine.SetCallbacks(stFreeFall, Input_FreeFall, Logic_FreeFall, Enter_FreeFall, null);
        machine.SetCallbacks(stGliding, Input_Gliding, Logic_Gliding, Enter_Gliding, null);
        machine.SetCallbacks(stIdleWall, Input_IdleWall, Logic_IdleWall, Enter_IdleWall, End_IdleWall);
        machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, null);
        machine.SetCallbacks(stLedgeClimb, null, null, null, null);
        machine.SetCallbacks(stJumpOnGround, Input_JumpOnGround, Logic_JumpOnGround, Enter_JumpOnGround, null);
        machine.SetCallbacks(stJumpDown, Input_JumpDown, Logic_JumpDown, Enter_JumpDown, End_JumpDown);
        machine.SetCallbacks(stRoll, null, null, null, null);
        machine.SetCallbacks(stJumpOnAir, null, null, null, null);
        machine.SetCallbacks(stDash, null, null, null, null);
        machine.SetCallbacks(stTakeDown, null, null, null, null);
        machine.SetCallbacks(stJumpOnWall, null, null, null, null);
    }
    #endregion

    #region Unity Event Functions
    protected override void Start()
    {
        base.Start();

        InitStateMachine();
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

        CheckPhysics();
        machine.UpdateLogic();
    }
    #endregion

    #region Physics Utility
    private float GetMoveSpeed()
    {
        return isRunState ? runSpeed : walkSpeed;
    }
    #endregion

    #region Physics Checker
    protected void CheckThroughableGroundUp(out RaycastHit2D detectedGround, bool canCheck, Vector2 feetPosition, float detectLength)
    {
        int layer = LayerInfo.throughableGroundMask;

        if(canCheck)
        {
            detectedGround = Physics2D.Raycast(feetPosition, Vector2.up, detectLength, layer);
        }
        else
        {
            detectedGround = default(RaycastHit2D);
        }
    }

    protected void CheckThroughableGroundDown(out RaycastHit2D detectedGround, bool canCheck, Vector2 headPosition, float detectLength)
    {
        int layer = LayerInfo.throughableGroundMask;

        if(canCheck)
        {
            detectedGround = Physics2D.Raycast(headPosition, Vector2.down, detectLength, layer);
        }
        else
        {
            detectedGround = default(RaycastHit2D);
        }
    }

    private void CheckLedge()
    {

    }
    
    private void CheckPhysics()
    {
        float detectLength = 0.5f;
        float hitLength = 0.04f;

        GetFeetPosition(ref feetPosition, feetBox);
        GetHeadPosition(ref headPosition, headBox);
        GetFeetSidePosition(ref feetSidePosition, feetBox, lookingDirection);
        GetHeadSidePosition(ref headSidePosition, headBox, lookingDirection);

        CheckGround(out detectedGround, out isHitGround, canCheckGround, feetPosition, detectLength, hitLength);
        CheckCeil(out detectedCeil, out isHitCeil, canCheckCeil, headPosition, detectLength, hitLength);
        CheckWall(out detectedFeetSideWall, out isHitFeetSideWall, canCheckFeetSideWall, feetSidePosition, detectLength, hitLength, lookingDirection);
        CheckWall(out detectedHeadSideWall, out isHitHeadSideWall, canCheckHeadSideWall, headSidePosition, detectLength, hitLength, lookingDirection);

        isDetectedGround = detectedGround;
        isDetectedCeil = detectedCeil;
        isDetectedFeetSideWall = detectedFeetSideWall;
        isDetectedHeadSideWall = detectedHeadSideWall;

        CheckMoveDirection();
        lookingDirection = CheckLookingDirection(inputData.xNegative, inputData.xPositive, lookingDirection);

        CheckThroughableGroundUpper();
    }

    private void CheckMoveDirection()
    {
        if(isDetectedGround)
        {
            moveDirection.Set(detectedGround.normal.y, -detectedGround.normal.x);
        }
        else
        {
            moveDirection.Set(1.0f, 0.0f);
        }
    }

    private void CheckThroughableGroundUpper()
    {
        detectedJumpDownUpperGroundBefore = detectedJumpDownUpperGround;
        CheckThroughableGroundUp(out detectedJumpDownUpperGround, canCheckJumpDownUpper, feetPosition, entityHeight + 1.0f);

        if(detectedJumpDownUpperGroundBefore)
        {
            if(!detectedJumpDownUpperGround)
            {
                AcceptCollision(detectedJumpDownUpperGroundBefore.collider);
            }
            else if(detectedJumpDownUpperGroundBefore != detectedJumpDownUpperGround)
            {
                AcceptCollision(detectedJumpDownUpperGroundBefore.collider);
                IgnoreCollision(detectedJumpDownUpperGround.collider);
            }
        }
        else if(detectedJumpDownUpperGround)
        {
            IgnoreCollision(detectedJumpDownUpperGround.collider);
        }
    }

    private void CheckThroughableGroundLower()
    {
        detectedJumpDownLowerGroundBefore = detectedJumpDownLowerGround;
        CheckThroughableGroundDown(out detectedJumpDownLowerGround, canCheckJumpDownLower, headPosition + Vector2.up * 0.5f, entityHeight + 2.5f);

        if(detectedJumpDownLowerGroundBefore)
        {
            if(!detectedJumpDownLowerGround)
            {
                AcceptCollision(detectedJumpDownLowerGroundBefore.collider);
            }
            else if(detectedJumpDownLowerGroundBefore != detectedJumpDownLowerGround)
            {
                AcceptCollision(detectedJumpDownLowerGroundBefore.collider);
                IgnoreCollision(detectedJumpDownLowerGround.collider);
            }
            else
            {
                IgnoreCollision(detectedJumpDownLowerGround.collider);
            }
        }
        else if(detectedJumpDownLowerGround)
        {
            IgnoreCollision(detectedJumpDownLowerGround.collider);
        }
    }
    #endregion

    #region Implement Super State; Ground
    private void Enter_Ground()
    {
        leftJumpOnGroundCount = jumpOnGroundCount;
        leftJumpOnAirCount = jumpOnAirCount;
        leftDashCount = dashCount;
    }
    #endregion

    #region Implement Super State; Air
    private void Enter_Air()
    {
        if(leftJumpOnGroundCount == jumpOnGroundCount)
            leftJumpOnGroundCount--;
    }
    #endregion

    #region Implement Super State; Wall
    private void Enter_Wall()
    {
        leftJumpOnAirCount = jumpOnAirCount;
    }
    #endregion

    #region Implement Super State; Ability
    private void Enter_Ability()
    {
        isEndOfAbility = false;
        isCancelOfAbility = false;
    }
    #endregion

    #region Implement State; stIdleOnGround
    private void Enter_IdleOnGround()
    {
        Enter_Ground();

        proceedLongIdleTransitionFrame = 0;
    }

    private void Input_IdleOnGround()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput != 0)
        {
            if(isRunState)
            {
                machine.ChangeState(stRun);
            }
            else
            {
                machine.ChangeState(stWalk);
            }
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
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
        else if(proceedLongIdleTransitionFrame >= longIdleTransitionFrame)
        {
            machine.ChangeState(stIdleLongOnGround);
        }
    }

    private void Logic_IdleOnGround()
    {
        Logic_IdleXY();

        proceedLongIdleTransitionFrame++;
    }
    #endregion

    #region Implement State; stIdleLongOnGround
    private void Enter_IdleLongOnGround()
    {
        Enter_Ground();
    }

    private void Input_IdleLongOnGround()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput != 0)
        {
            if(isRunState)
            {
                machine.ChangeState(stRun);
            }
            else
            {
                machine.ChangeState(stWalk);
            }
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
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
    }

    private void Logic_IdleLongOnGround()
    {
        Logic_IdleXY();
    }

    private void End_IdleLongOnGround()
    {
        proceedLongIdleTransitionFrame = 0;
    }
    #endregion

    #region Implement State; stSit
    private void Enter_Sit()
    {
        Enter_Ground();

        proceedSitFrame = 0;
        canCheckJumpDownUpper = false;
        canCheckJumpDownLower = true;
    }

    private void Input_Sit()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
        {
            if(detectedJumpDownLowerGround)
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
        Logic_IdleXY();

        if(inputData.yNegative != 0)
        {
            float detectLength = 0.04f;
            CheckThroughableGroundDown(out detectedJumpDownLowerGround, canCheckJumpDownLower, feetPosition, detectLength);
        }

        proceedSitFrame++;
    }

    private void End_Sit()
    {
        canCheckJumpDownUpper = true;
        canCheckJumpDownLower = false;
    }
    #endregion

    #region Implement State; stHeadUp
    private void Enter_HeadUp()
    {
        Enter_Ground();

        proceedHeadUpFrame = 0;
    }

    private void Input_HeadUp()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
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
        Logic_IdleXY();

        proceedHeadUpFrame++;
    }
    #endregion

    #region Implement State; stWalk
    private void Enter_Walk()
    {
        Enter_Ground();
    }

    private void Input_Walk()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
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
        else if(isRunState)
        {
            machine.ChangeState(stRun);
        }
    }

    private void Logic_Walk()
    {
        Logic_MoveOnGround(moveDirection, walkSpeed, lookingDirection);
    }
    #endregion

    #region Implement State; stRun
    private void Enter_Run()
    {
        Enter_Ground();
    }

    private void Input_Run()
    {
        if(!isHitGround)
        {
            machine.ChangeState(stFreeFall);
        }
        else if(inputData.xInput == 0)
        {
            machine.ChangeState(stIdleOnGround);
        }
        else if(inputData.jumpDown && leftJumpOnGroundCount > 0)
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
        else if(!isRunState)
        {
            machine.ChangeState(stWalk);
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
        Enter_Air();

        if(currentVelocity.y >= 0.0f)
        {
            proceedFreeFallAccelFrameY = 0;
        }
        else if(currentVelocity.y <= -maxFreeFallSpeedY)
        {
            proceedFreeFallAccelFrameY = freeFallAccelFrameY;
        }
        else
        {
            for(int i = 0; i < freeFallAccelFrameY; i++)
            {
                if(currentVelocity.y <= -maxFreeFallSpeedY * freeFallAccelGraphY[i])
                {
                    proceedFreeFallAccelFrameY = i;
                    break;
                }
            }
        }
    }

    private void Input_FreeFall()
    {
        if(isHitGround)
        {
            if(inputData.xInput == 0)
            {
                machine.ChangeState(stIdleOnGround);
            }
            else if(isRunState)
            {
                machine.ChangeState(stRun);
            }
            else
            {
                machine.ChangeState(stWalk);
            }
        }
        else if(inputData.jumpDown)
        {
            if(inputData.yNegative == 0 && leftJumpOnAirCount > 0)
            {
                machine.ChangeState(stJumpOnAir);
            }
            else if(inputData.yNegative != 0)
            {
                machine.ChangeState(stTakeDown);
            }
        }
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(inputData.yPositive != 0)
        {
            machine.ChangeState(stGliding);
        }
        else if(!isDetectedGround && inputData.xInput == lookingDirection && isHitFeetSideWall == inputData.xInput && isHitHeadSideWall == inputData.xInput)
        {
            machine.ChangeState(stIdleWall);
        }
        else if(isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
    }

    private void Logic_FreeFall()
    {
        if(inputData.xInput == 0)
        {
            Logic_IdleX();
        }
        else
        {
            Logic_MoveOnAirX(GetMoveSpeed(), lookingDirection);
        }

        if(proceedFreeFallAccelFrameY < freeFallAccelFrameY)
            proceedFreeFallAccelFrameY++;

        Logic_MoveOnAirY(-maxFreeFallSpeedY, freeFallAccelGraphY, proceedFreeFallAccelFrameY - 1);
    }
    #endregion

    #region Implement State; stGliding
    private void Enter_Gliding()
    {
        Enter_Air();

        if(currentVelocity.x == 0)
        {
            proceedGlidingAccelFrameX = 0;
            leftGlidingDeaccelFrameX = 0;
        }
        else if(Mathf.Abs(currentVelocity.x) >= GetMoveSpeed())
        {
            proceedGlidingAccelFrameX = glidingAccelFrameX;
        }
        else
        {
            for(int i = 0; i < glidingAccelFrameX; i++)
            {
                if(Mathf.Abs(currentVelocity.x) >= GetMoveSpeed() * glidingAccelGraphX[i])
                {
                    proceedGlidingAccelFrameX = i;
                    break;
                }
            }

            leftGlidingDeaccelFrameX = glidingDeaccelFrameX;
        }
    }

    private void Input_Gliding()
    {
        if(isHitGround)
        {
            if(inputData.xInput == 0)
            {
                machine.ChangeState(stIdleOnGround);
            }
            else if(isRunState)
            {
                machine.ChangeState(stRun);
            }
            else
            {
                machine.ChangeState(stWalk);
            }
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
        else if(!isDetectedGround && inputData.xInput == lookingDirection && isHitFeetSideWall == inputData.xInput && isHitHeadSideWall == inputData.xInput)
        {
            machine.ChangeState(stIdleWall);
        }
        else if(isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
    }

    private void Logic_Gliding()
    {
        // 왼쪽에서 오른쪽으로 방향 전환
        if(inputData.xInput * currentVelocity.x < 0)
        {
            proceedGlidingAccelFrameX = 0;

            if(leftGlidingDeaccelFrameX > 0)
                leftGlidingDeaccelFrameX--;
            if(leftGlidingDeaccelFrameX > 0)
                leftGlidingDeaccelFrameX--;

            Logic_MoveOnAirX(GetMoveSpeed(), -lookingDirection, glidingDeaccelGraphX, leftGlidingDeaccelFrameX);
        }
        // x축 움직임에서 x축 정지로
        else if(inputData.xInput == 0)
        {
            proceedGlidingAccelFrameX = 0;

            if(leftGlidingDeaccelFrameX > 0)
                leftGlidingDeaccelFrameX--;

            Logic_MoveOnAirX(GetMoveSpeed(), lookingDirection, glidingDeaccelGraphX, leftGlidingDeaccelFrameX);
        }
        // x축 정지에서 x축 움직임으로
        else if(inputData.xInput != 0)
        {
            if(proceedGlidingAccelFrameX < glidingAccelFrameX)
                proceedGlidingAccelFrameX++;

            if(leftGlidingDeaccelFrameX < glidingDeaccelFrameX && Mathf.Abs(currentVelocity.x) >= glidingDeaccelGraphX[leftGlidingDeaccelFrameX])
                leftGlidingDeaccelFrameX++;

            Logic_MoveOnAirX(GetMoveSpeed(), lookingDirection, glidingAccelGraphX, proceedGlidingAccelFrameX - 1);
        }

        Logic_MoveOnAirY(-glidingSpeed);
    }
    #endregion

    #region Implement State; stIdleWall
    private void Enter_IdleWall()
    {
        Enter_Wall();

        DisableGravity();
    }

    private void Input_IdleWall()
    {
        if(isDetectedGround || isHitFeetSideWall == 0 || isHitHeadSideWall == 0 || inputData.yNegDown)
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
        Logic_IdleXY();
    }

    private void End_IdleWall()
    {
        EnableGravity();
    }
    #endregion

    #region Implement State; stWallSliding
    private void Enter_WallSliding()
    {
        Enter_Wall();

        proceedWallSlidingAccelFrameY = 0;
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
        else if(!isDetectedGround && inputData.xInput == lookingDirection && isHitFeetSideWall == inputData.xInput && isHitHeadSideWall == inputData.xInput)
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
        if(proceedWallSlidingAccelFrameY < wallSlidingAccelFrameY)
            proceedWallSlidingAccelFrameY++;

        Logic_IdleX();
        Logic_MoveOnAirY(-maxWallSlidingSpeedY, wallSlidingAccelGrpahY, proceedWallSlidingAccelFrameY - 1);
    }
    #endregion

    #region Implement State; stLedgeClimb
    #endregion

    #region Implement State; stJumpOnGround
    private void Enter_JumpOnGround()
    {
        Enter_Ability();

        leftJumpOnGroundCount--;
        leftJumpOnGroundDeaccelFrameY = jumpOnGroundDeaccelFrameY;
    }

    private void Input_JumpOnGround()
    {
        if(inputData.jumpUp && !isCancelOfAbility)
        {
            isCancelOfAbility = true;
            leftJumpOnGroundDeaccelFrameY /= 2;
        }

        if(isHitCeil || leftJumpOnGroundDeaccelFrameY == 0)
        {
            if(inputData.yPositive == 0)
            {
                isEndOfAbility = true;
                machine.ChangeState(stFreeFall);
            }
            else
            {
                isEndOfAbility = true;
                machine.ChangeState(stGliding);
            }
        }
        else if(inputData.jumpDown)
        {
            if(inputData.yNegative == 0 && leftJumpOnAirCount > 0)
            {
                machine.ChangeState(stJumpOnAir);
            }
            else if(inputData.yNegative != 0)
            {
                machine.ChangeState(stTakeDown);
            }
        }
        else if(inputData.dashDown && leftDashCount > 0)
        {
            machine.ChangeState(stDash);
        }
        else if(!isDetectedGround && inputData.xInput == lookingDirection && isHitFeetSideWall == inputData.xInput && isHitHeadSideWall == inputData.xInput)
        {
            machine.ChangeState(stIdleWall);
        }
        else if(isHitLedge)
        {
            machine.ChangeState(stLedgeClimb);
        }
    }

    private void Logic_JumpOnGround()
    {
        if(leftJumpOnGroundDeaccelFrameY > 0)
            leftJumpOnGroundDeaccelFrameY--;

        Logic_MoveOnAirX(inputData.xInput == 0 ? 0.0f : GetMoveSpeed(), lookingDirection);
        Logic_MoveOnAirY(jumpOnGroundSpeed, jumpOnGroundDeaccelGraphY, leftJumpOnGroundDeaccelFrameY);
    }
    #endregion

    #region Implement State; stJumpDown
    private void Enter_JumpDown()
    {
        Enter_Ability();

        canCheckJumpDownUpper = false;
        leftJumpDownDeaccelFrameY = jumpDownDeaccelFrameY;
        proceedFreeFallAccelFrameY = 0;
    }

    private void Input_JumpDown()
    {
        if(leftJumpDownDeaccelFrameY == 0 && !detectedJumpDownLowerGround)
        {
            machine.ChangeState(stFreeFall);
        }
    }

    private void Logic_JumpDown()
    {
        CheckThroughableGroundLower();

        if(leftJumpDownDeaccelFrameY > 0)
        {
            leftJumpDownDeaccelFrameY--;
            Logic_MoveOnAirY(jumpDownSpeed, jumpDownDeaccelGraphY, leftJumpDownDeaccelFrameY);
        }
        else
        {
            if(proceedFreeFallAccelFrameY < freeFallAccelFrameY)
                proceedFreeFallAccelFrameY++;

            Logic_MoveOnAirY(-maxFreeFallSpeedY, freeFallAccelGraphY, proceedFreeFallAccelFrameY - 1);
        }
    }

    private void End_JumpDown()
    {
        canCheckJumpDownUpper = true;
        CheckThroughableGroundLower();
    }
    #endregion

    #region Implement State; stRoll
    #endregion

    #region Implement State; stJumpOnAir
    #endregion

    #region Implement State; stDash
    #endregion

    #region Implement State; stTakeDown
    #endregion

    #region Implement State; stJumpOnWall
    #endregion
}