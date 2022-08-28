using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    #region Player Constants

    // All Available Player States
    private const int stIdleBasic           = 0; // 정지(일반)
    private const int stIdleLong            = 1; // 정지(장시간)
    private const int stIdleWall            = 2; // 벽 붙기
    private const int stAir                 = 3; // 체공
    private const int stGliding             = 17; // 글라이딩
    private const int stMove                = 4; // 바닥에서의 움직임
    private const int stMoveWalk            = 18; // 바닥에서의 움직임(걷기)
    private const int stMoveRun             = 19; // 바닥에서의 움직임(달리기)
    private const int stJump                = 5; // 점프(일반)
    private const int stJumpAir             = 6; // 점프(공중)
    private const int stJumpWall            = 7; // 점프(벽)
    private const int stJumpDown            = 8; // 점프(하향)
    private const int stWallSliding         = 9; // 벽 슬라이딩
    private const int stLedgeHold           = 10; // 난간 잡기
    private const int stLedgeClimb          = 11; // 난간 오르기
    private const int stSit                 = 12; // 앉기
    private const int stHeadUp              = 13; // 고개 들기
    private const int stRoll                = 14; // 구르기
    private const int stDash                = 15; // 대쉬
    private const int stTakeDown            = 16; // 내려 찍기

    [Header("Player Constants")]
    // Idle related options
    public int longIdleFrame = 900;

    // Air related options
    public float maxGlidingSpeed = 4.5f;

    public int freeFallAccelFrame = 39;
    public int glidingAccelFrameX = 26;
    public int glidingAccelFrameY = 39;

    private DiscreteLinearGraph freeFallGraph;

    // Move related options
    public float runningWeight = 1.2f;

    public int runningAccelFrame = 13;

    // Jump related options
    public int continuousJumpCount = 1;
    public float jumpSpeed = 7.0f;
    public float airJumpSpeed = 7.0f;
    public float downJumpPreSpeed = 1.5f;
    public float wallJumpSpeed = 7.0f;

    public int jumpFrame = 13;
    public int airJumpIdleFrame = 3;
    public int airJumpFrame = 13;
    public int downJumpPreFrame = 30;
    public int wallJumpFrame = 13;
    public int wallJumpForceFrame = 6;

    private DiscreteLinearGraph jumpGraph;
    private DiscreteLinearGraph airJumpGraph;
    private DiscreteLinearGraph wallJumpGraph;

    // WallSliding related options
    public float maxWallSlidingSpeed = 5.0f;

    public int wallSlidingAccelFrame = 13;

    private DiscreteLinearGraph wallSlidingGraph;

    // Ledge related options
    [Range(0.1f, 2.0f)]
    public float checkerTopOffset = 0.2f;
    public float ledgeDetectingLength = 0.5f;
    public float ledgeEntityDetectingLength = 0.04f;

    // Sit & HeadUp related options
    public int sitCameraMoveFrame = 120;
    public int headUpCameraMoveFrame = 120;

    // Roll related options
    public float rollSpeed = 8.0f;

    public int rollPreFrame = 6;
    public int rollInvincibilityFrame = 18;
    public int rollWakeUpFrame = 6;
    public int rollCoolFrame = 180;

    private DiscreteLinearGraph rollGraph;

    // Dash related options
    public float dashSpeed = 8.0f;

    public int continuousDashCount = 1;
    public int dashIdleFrame = 6;
    public int dashInvincibilityFrame = 9;

    private DiscreteLinearGraph dashGraph;

    // TakeDown related options
    public float takeDownSpeed = 18.0f;

    public int takeDownPreIdleFrame = 18;
    public int takeDownPostIdleFrame = 12;

    // State Machine
    private StateMachine m_machine;

    #endregion

    #region Player Variables

    [Header("Input Options")]
    // Input options
    private bool xNegDown;
    private bool xNegUp;
    private bool xPosDown;
    private bool xPosUp;
    private int xNegative;
    private int xPositive;
    private int xInput;

    private bool yNegDown;
    private bool yNegUp;
    private bool yPosDown;
    private bool yPosUp;
    private int yNegative;
    private int yPositive;
    private int yInput;

    private bool jumpDown;
    private bool jumpUp;
    private bool jumpPressing;

    private bool dashDown;
    private bool dashUp;
    private bool dashPressing;

    [Header("Player Variables")]
    // Idle related options
    public int currentIdleBasicFrame = 0;

    // Air related options
    public int proceedFreeFallFrame;

    // Move related options

    // Jump related options
    public bool isReleasedJumpKey;
    public int leftContinuousJumpCount;
    public int leftJumpFrame;
    public int leftAirJumpIdleFrame;
    public int leftAirJumpFrame;
    public int leftWallJumpFrame;
    public int currentJumpedWallDirection;

    // WallSliding related options
    public int proceedWallSlidingFrame;

    // Ledge related options
    public Vector2 ledgeCheckerBottom;
    public Vector2 ledgeCheckerTop;
    private RaycastHit2D hitBottomChecker;
    private RaycastHit2D hitTopChecker;
    public bool isOnLedge;
    public Vector2 ledgeCornerPosition;
    public bool isOnLedgeClimbEnd;
    public bool isTeleportedPlayer;

    // Sit & HeadUp related options
    public int proceedSitFrame;
    public int proceedHeadUpFrame;

    // Roll related options
    public int leftRollPreFrame;
    public int leftRollInvincibilityFrame;
    public int leftRollWakeUpFrame;
    public int leftRollFrame;
    public int currentRollDirection;

    // Dash related options
    public int leftContinuousDashCount;
    public int leftDashIdleFrame;
    public int leftDashInvincibilityFrame;
    public int currentDashDirection;

    // TakeDown related options
    public int leftTakeDownPreIdleFrame;
    public int leftTakeDownPostIdleFrame;
    public bool isTakeDownLanding;

    #endregion
    
    #region Initializer

    protected override void Start()
    {
        base.Start();

        m_SetStateMachine();
        m_SetGraphs();
    }

    private void m_SetStateMachine()
    {
        m_machine = new StateMachine(stIdleBasic);

        // (상태 이름, 상태 전이 조건, 상태에서 수행할 로직, 상태가 시작될 때 수행하는 로직, 상태가 끝날 때 수행하는 로직)
        m_machine.SetCallbacks(stIdleBasic, Input_IdleBasic, Logic_IdleBasic, Enter_IdleBasic, End_IdleBasic);
        m_machine.SetCallbacks(stIdleLong, Input_IdleLong, Logic_IdleLong, Enter_IdleLong, End_IdleLong);
        m_machine.SetCallbacks(stIdleWall, Input_IdleWall, Logic_IdleWall, Enter_IdleWall, End_IdleWall);
        m_machine.SetCallbacks(stAir, Input_Air, Logic_Air, Enter_Air, End_Air);
        m_machine.SetCallbacks(stMove, Input_Move, Logic_Move, null, null);
        m_machine.SetCallbacks(stJump, Input_Jump, Logic_Jump, Enter_Jump, End_Jump);
        m_machine.SetCallbacks(stJumpAir, Input_JumpAir, Logic_JumpAir, Enter_JumpAir, End_JumpAir);
        m_machine.SetCallbacks(stJumpWall, Input_JumpWall, Logic_JumpWall, Enter_JumpWall, End_JumpWall);
        m_machine.SetCallbacks(stJumpDown, null, null, null, null);
        m_machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, End_WallSliding);
        m_machine.SetCallbacks(stLedgeHold, null, null, null, null);
        m_machine.SetCallbacks(stLedgeClimb, Input_LedgeClimb, Logic_LedgeClimb, Enter_LedgeClimb, End_LedgeClimb);
        m_machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, End_Sit);
        m_machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, End_HeadUp);
        m_machine.SetCallbacks(stRoll, Input_Roll, Logic_Roll, Enter_Roll, End_Roll);
        m_machine.SetCallbacks(stDash, Input_Dash, Logic_Dash, Enter_Dash, End_Dash);
        m_machine.SetCallbacks(stTakeDown, Input_TakeDown, Logic_TakeDown, Enter_TakeDown, End_TakeDown);
    }

    private void m_SetGraphs()
    {
        freeFallGraph = new DiscreteLinearGraph(freeFallAccelFrame);
        jumpGraph = new DiscreteLinearGraph(jumpFrame);
        airJumpGraph = new DiscreteLinearGraph(airJumpFrame);
        wallJumpGraph = new DiscreteLinearGraph(wallJumpFrame);
        wallSlidingGraph = new DiscreteLinearGraph(wallSlidingAccelFrame);
        rollGraph = new DiscreteLinearGraph(rollPreFrame + rollInvincibilityFrame + rollWakeUpFrame);
        dashGraph = new DiscreteLinearGraph(dashInvincibilityFrame);
    }

    #endregion

    #region Physics Checker

    protected void CheckLedge()
    {
        float bPosX, bPosY;
        float tPosX, tPosY;

        bPosX = ceilBox.bounds.max.x * lookingDirection;
        bPosY = ceilBox.bounds.center.y;
        tPosX = bPosX;
        tPosY = bPosY + checkerTopOffset;

        SetVector(ref ledgeCheckerBottom, bPosX, bPosY);
        SetVector(ref ledgeCheckerTop, tPosX, tPosY);

        hitBottomChecker = Physics2D.Raycast(ledgeCheckerBottom, Vector2.right * lookingDirection, ledgeDetectingLength, LayerInfo.groundMask);
        RaycastHit2D hitFromTop = Physics2D.Raycast(ledgeCheckerTop, Vector2.right * lookingDirection, ledgeDetectingLength, LayerInfo.groundMask);

        if(hitBottomChecker && !hitFromTop)
        {
            isOnLedge = true;

            float xDistance = hitBottomChecker.distance + 0.02f;

            tPosX = ledgeCheckerBottom.x + xDistance;
            tPosY = ledgeCheckerBottom.y + checkerTopOffset;
            SetVector(ref ledgeCheckerTop, tPosX, tPosY);

            hitTopChecker = Physics2D.Raycast(ledgeCheckerTop, Vector2.down, checkerTopOffset + 0.1f, LayerInfo.groundMask);

            if(hitTopChecker)
            {
                SetVector(ref ledgeCornerPosition, hitTopChecker.point.x, hitTopChecker.point.y);
            }
        }
        else
        {
            isOnLedge = false;
        }
    }

    #endregion

    #region Execute Object Actions

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        CheckLedge();

        m_machine.UpdateLogic();
    }

    private void Logic_IdleBasic()
    {
        currentIdleBasicFrame++;
        SetVelocity(0.0f, 0.0f);
    }

    private void Logic_IdleLong()
    {
        SetVelocity(0.0f, 0.0f);
    }

    private void Logic_IdleWall()
    {
        SetVelocity(0.0f, 0.0f);
    }

    private void Logic_Air()
    {
        if(xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
        {
            SetVelocityX(0.0f);
        }
        else
        {
            SetVelocityX(moveSpeed * lookingDirection);
        }

        if(currentVelocity.y < 0)
        {
            if(yPositive != 0)
            {
                
            }
            else
            {
                SetVelocityY(-maxFreeFallSpeed * freeFallGraph[proceedFreeFallFrame]);

                if(proceedFreeFallFrame < freeFallAccelFrame - 1)
                    proceedFreeFallFrame++;
            }
        }
    }

    protected void Logic_Move()
    {
        if(xInput != 0)
        {
            MoveOnGround(moveSpeed, lookingDirection);
        }
    }

    private void Logic_Jump()
    {
        if(xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
        {
            SetVelocityX(0.0f);
        }
        else
        {
            SetVelocityX(moveSpeed * lookingDirection);
        }

        if(leftJumpFrame > 0)
        {
            SetVelocityY(jumpSpeed * jumpGraph[leftJumpFrame-- - 1]);
        }
    }

    private void Logic_JumpAir()
    {
        if(leftAirJumpIdleFrame > 0)
        {
            if(rigid.gravityScale != 0.0f)
                rigid.gravityScale = 0.0f;

            SetVelocity(0.0f, 0.0f);
            leftAirJumpIdleFrame--;

            if(leftAirJumpIdleFrame == 0)
            {
                leftAirJumpFrame = airJumpFrame;
            }
        }
        else if(leftAirJumpFrame > 0)
        {
            if(rigid.gravityScale != 1.0f)
                rigid.gravityScale = 1.0f;

            if(xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
            {
                SetVelocityX(0.0f);
            }
            else
            {
                SetVelocityX(moveSpeed * lookingDirection);
            }

            SetVelocityY(airJumpSpeed * airJumpGraph[leftAirJumpFrame-- - 1]);
        }
    }

    private void Logic_JumpWall()
    {
        if(leftWallJumpFrame > 0)
        {
            if(leftWallJumpFrame <= wallJumpFrame - wallJumpForceFrame)
            {
                SetVelocityX(moveSpeed * currentJumpedWallDirection * -1);
            }
            else
            {
                SetVelocityX(wallJumpSpeed * wallJumpGraph[leftWallJumpFrame - 1] * currentJumpedWallDirection * -1);
            }

            SetVelocityY(wallJumpSpeed * wallJumpGraph[leftWallJumpFrame - 1]);

            leftWallJumpFrame--;
        }
    }

    private void Logic_WallSliding()
    {
        SetVelocity(0.0f, -maxWallSlidingSpeed * wallSlidingGraph[proceedWallSlidingFrame]);

        if(proceedWallSlidingFrame < wallSlidingAccelFrame - 1)
            proceedWallSlidingFrame++;
    }

    private void Logic_LedgeHold()
    {
        SetVelocity(0.0f, 0.0f);
    }

    private void Logic_LedgeClimb()
    {
        if(!isOnLedgeClimbEnd)
        {
            SetVelocity(0.0f, 0.0f);
        }
    }

    private void Logic_Sit()
    {
        if(proceedSitFrame < sitCameraMoveFrame)
            proceedSitFrame++;
    }

    private void Logic_HeadUp()
    {
        if(proceedHeadUpFrame < headUpCameraMoveFrame)
            proceedHeadUpFrame++;
    }

    private void Logic_Roll()
    {
        if(leftRollPreFrame > 0)
        {
            leftRollPreFrame--;

            if(leftRollPreFrame == 0)
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
        {
            MoveOnGround(moveSpeed * rollGraph[leftRollFrame-- - 1], currentRollDirection);
        }
    }

    private void Logic_Dash()
    {
        if(leftDashIdleFrame > 0)
        {
            SetVelocity(0.0f, 0.0f);

            leftDashIdleFrame--;

            if(leftDashIdleFrame == 0)
                leftDashInvincibilityFrame = dashInvincibilityFrame;
        }
        else if(leftDashInvincibilityFrame > 0)
        {
            float x = dashSpeed * dashGraph[leftDashInvincibilityFrame - 1] * currentDashDirection;
            float y = 0.0f;

            SetVelocity(x, y);

            leftDashInvincibilityFrame--;
        }
    }

    private void Logic_TakeDown()
    {
        if(leftTakeDownPreIdleFrame > 0)
        {
            SetVelocity(0.0f, 0.0f);

            leftTakeDownPreIdleFrame--;

            if(leftTakeDownPreIdleFrame == 0)
                leftTakeDownPostIdleFrame = takeDownPostIdleFrame;
        }
        else if(leftTakeDownPostIdleFrame > 0)
        {
            if(isOnGround)
            {
                if(!isTakeDownLanding)
                    isTakeDownLanding = true;

                SetVelocity(0.0f, 0.0f);

                leftTakeDownPostIdleFrame--;
            }
            else
            {
                SetVelocity(0.0f, -takeDownSpeed);
            }
        }
    }

    #endregion

    #region State Transition Input Checker

    private void CheckInput()
    {
        xNegDown = InputHandler.data.xNegDown;
        xNegUp = InputHandler.data.xNegUp;
        xPosDown = InputHandler.data.xPosDown;
        xPosUp = InputHandler.data.xPosUp;
        xNegative = InputHandler.data.xNegative;
        xPositive = InputHandler.data.xPositive;
        xInput = xNegative + xPositive;

        yNegDown = InputHandler.data.yNegDown;
        yNegUp = InputHandler.data.yNegUp;
        yPosDown = InputHandler.data.yPosDown;
        yPosUp = InputHandler.data.yPosUp;
        yNegative = InputHandler.data.yNegative;
        yPositive = InputHandler.data.yPositive;
        yInput = yNegative + yPositive;

        jumpDown = InputHandler.data.jumpDown;
        jumpUp = InputHandler.data.jumpUp;
        jumpPressing = InputHandler.data.jumpPressing;

        dashDown = InputHandler.data.dashDown;
        dashUp = InputHandler.data.dashUp;
        dashPressing = InputHandler.data.dashPressing;
    }

    #endregion

    #region State Transition Logics

    protected override void Update()
    {
        base.Update();

        CheckInput();
        CheckLookingDirection(xInput);

        m_machine.UpdateInput();
    }

    private void Input_IdleBasic()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(dashDown)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(jumpDown)
        {
            m_machine.ChangeState(stJump);
            return;
        }
        if(xInput != 0)
        {
            m_machine.ChangeState(stMove);
            return;
        }
        if(currentIdleBasicFrame >= longIdleFrame)
        {
            m_machine.ChangeState(stIdleLong);
            return;
        }
    }

    private void Input_IdleLong()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(dashDown)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(jumpDown)
        {
            m_machine.ChangeState(stJump);
            return;
        }
        if(xInput != 0)
        {
            m_machine.ChangeState(stMove);
            return;
        }
    }

    private void Input_IdleWall()
    {
        if(isOnWallFeet == 0 || isOnWallCeil == 0 || yNegDown)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(xInput == 0 && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection)
        {
            m_machine.ChangeState(stWallSliding);
            return;
        }
        if(jumpDown)
        {
            m_machine.ChangeState(stJumpWall);
            return;
        }
    }

    private void Input_Air()
    {
        if(isOnLedge)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
        if(isOnGround)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(jumpDown && yNegative != 0)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(dashDown && leftContinuousDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && xInput != 0 && lookingDirection == xInput && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(jumpDown && leftContinuousJumpCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
    }

    private void Input_Move()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(dashDown)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(jumpDown)
        {
            m_machine.ChangeState(stJump);
            return;
        }
        if(xInput == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
    }

    private void Input_Jump()
    {
        if(isOnLedge)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
        if(jumpDown && yNegative != 0)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(jumpDown && leftContinuousJumpCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
        if(dashDown && leftContinuousDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && xInput != 0 && lookingDirection == xInput && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(jumpUp && !isReleasedJumpKey)
        {
            isReleasedJumpKey = true;
            leftJumpFrame /= 2;
            return;
        }
        if(leftJumpFrame == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
    }

    private void Input_JumpAir()
    {
        if(isOnLedge)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
        if(jumpDown && yNegative != 0)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(jumpDown && leftContinuousJumpCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
        if(dashDown && leftContinuousDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && xInput != 0 && lookingDirection == xInput && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(jumpUp && !isReleasedJumpKey)
        {
            if(leftAirJumpIdleFrame > 0)
            {
                leftAirJumpIdleFrame = 0;
                leftAirJumpFrame = airJumpFrame / 2;
            }
            else
            {
                leftAirJumpFrame /= 2;
            }

            isReleasedJumpKey = true;
            return;
        }
        if(leftAirJumpFrame == 0 && leftAirJumpIdleFrame == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
    }

    private void Input_JumpWall()
    {
        if(leftWallJumpFrame == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if (leftWallJumpFrame <= wallJumpFrame - wallJumpForceFrame)
        {
            if(isOnLedge)
            {
                m_machine.ChangeState(stLedgeClimb);
                return;
            }
            if(jumpPressing && yNegative != 0)
            {
                m_machine.ChangeState(stTakeDown);
                return;
            }
            if(jumpPressing && !isReleasedJumpKey)
            {
                isReleasedJumpKey = true;
                leftWallJumpFrame /= 2;
                return;
            }
            if(dashDown)
            {
                m_machine.ChangeState(stDash);
                return;
            }
            if(yPosDown)
            {
                // TODO: 글라이딩 기믹 추가
                m_machine.ChangeState(stAir);
                return;
            }
            if(!isDetectedGround && xInput != 0 && lookingDirection == xInput && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection)
            {
                m_machine.ChangeState(stIdleWall);
                return;
            }
        }
    }

    private void Input_WallSliding()
    {
        if(isOnWallFeet == 0 || isOnWallCeil == 0 || yNegDown)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(isOnGround)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(!isDetectedGround && xInput != 0 && lookingDirection == xInput && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(jumpDown)
        {
            m_machine.ChangeState(stJumpWall);
            return;
        }
    }

    private void Input_LedgeHold()
    {
        // NOTE: 미구현, 혹시 필요할 경우에 이 곳에 작성해주면 된다.
    }

    private void Input_LedgeClimb()
    {
        if(isOnLedgeClimbEnd)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
    }

    private void Input_Sit()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(yNegative == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(jumpDown)
        {
            // TODO: 하향 점프 하는 코드
            // TODO: 일반 점프 하는 코드
            return;
        }
        if(dashDown)
        {
            m_machine.ChangeState(stRoll);
        }
    }

    private void Input_HeadUp()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(yNegative == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(jumpDown)
        {
            // TODO: 일반 점프 하는 코드
            return;
        }
        if(dashDown)
        {
            m_machine.ChangeState(stRoll);
        }
    }

    private void Input_Roll()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(leftRollFrame == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
    }

    private void Input_Dash()
    {
        if(leftDashIdleFrame == 0 && leftDashInvincibilityFrame == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(isOnLedge)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
        if(isOnWallFeet == lookingDirection ^ isOnWallCeil == lookingDirection)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(!isDetectedGround && xInput != 0 && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }

        // TODO: 난간 오르기로 상태 전이하는 코드 작성하기
    }

    private void Input_TakeDown()
    {
        if(isTakeDownLanding)
        {
            if(!isOnGround)
            {
                m_machine.ChangeState(stAir);
                return;
            }
            if(leftTakeDownPreIdleFrame == 0 && leftTakeDownPostIdleFrame == 0)
            {
                m_machine.ChangeState(stIdleBasic);
                return;
            }
        }
    }

    #endregion

    #region On State Enter

    private void Enter_IdleBasic()
    {
        rigid.gravityScale = 0.0f;
        currentIdleBasicFrame = 0;
        leftContinuousJumpCount = continuousJumpCount;
        leftContinuousDashCount = continuousDashCount;

        // 선입력 체크
    }

    private void Enter_IdleLong()
    {
        rigid.gravityScale = 0.0f;
    }

    private void Enter_IdleWall()
    {
        rigid.gravityScale = 0.0f;
        leftContinuousDashCount = continuousDashCount;
    }

    private void Enter_Air()
    {
        int i;

        if(currentVelocity.y >= 0)
        {
            proceedFreeFallFrame = 0;
        }
        else if(currentVelocity.y < -maxFreeFallSpeed * freeFallGraph[freeFallAccelFrame - 1])
        {
            proceedFreeFallFrame = freeFallAccelFrame - 1;
        }
        else
        {
            for(i = 0; i < freeFallAccelFrame; i++)
            {
                if(-maxFreeFallSpeed * freeFallGraph[i] <= currentVelocity.y)
                {
                    proceedFreeFallFrame = i;
                    break;
                }
            }
        }
    }

    private void Enter_Jump()
    {
        leftContinuousJumpCount--;
        leftJumpFrame = jumpFrame;
        isReleasedJumpKey = false;
    }

    private void Enter_JumpAir()
    {
        leftContinuousJumpCount--;
        leftAirJumpIdleFrame = airJumpIdleFrame;
        leftAirJumpFrame = airJumpFrame;
        isReleasedJumpKey = false;
        rigid.gravityScale = 1.0f;
    }

    private void Enter_JumpWall()
    {
        currentJumpedWallDirection = isOnWallFeet;
        leftWallJumpFrame = wallJumpFrame;
        isReleasedJumpKey = false;
    }

    private void Enter_WallSliding()
    {
        proceedWallSlidingFrame = 0;
    }

    private void Enter_LedgeHold()
    {

    }

    private void Enter_LedgeClimb()
    {
        rigid.gravityScale = 0.0f;
        isOnLedgeClimbEnd = false;
        isTeleportedPlayer = false;
    }

    private void Enter_Sit()
    {
        proceedSitFrame = 0;
    }

    private void Enter_HeadUp()
    {
        proceedHeadUpFrame = 0;
    }

    private void Enter_Roll()
    {
        currentRollDirection = lookingDirection;
        leftRollPreFrame = rollPreFrame;
        leftRollInvincibilityFrame = 0;
        leftRollWakeUpFrame = 0;
        leftRollFrame = rollPreFrame + rollInvincibilityFrame + rollWakeUpFrame;
    }

    private void Enter_Dash()
    {
        currentDashDirection = lookingDirection;
        leftContinuousDashCount--;
        leftDashIdleFrame = dashIdleFrame;
        leftDashInvincibilityFrame = 0;
    }

    private void Enter_TakeDown()
    {
        leftTakeDownPreIdleFrame = takeDownPreIdleFrame;
        leftTakeDownPostIdleFrame = 0;
        isTakeDownLanding = false;
    }

    #endregion

    #region On State End

    private void End_IdleBasic()
    {
        rigid.gravityScale = 1.0f;
        currentIdleBasicFrame = 0;
    }

    private void End_IdleLong()
    {
        rigid.gravityScale = 1.0f;
    }

    private void End_IdleWall()
    {
        rigid.gravityScale = 1.0f;
    }

    private void End_Air()
    {
        proceedFreeFallFrame = 0;
    }

    private void End_Jump()
    {
        leftJumpFrame = 0;
        isReleasedJumpKey = false;
    }

    private void End_JumpAir()
    {
        leftAirJumpIdleFrame = 0;
        leftAirJumpFrame = 0;
        isReleasedJumpKey = false;
        rigid.gravityScale = 1.0f;
    }

    private void End_JumpWall()
    {
        currentJumpedWallDirection = 0;
        leftWallJumpFrame = 0;
        isReleasedJumpKey = false;
    }

    private void End_WallSliding()
    {
        proceedWallSlidingFrame = 0;
    }

    private void End_LedgeHold()
    {

    }

    private void End_LedgeClimb()
    {
        float fPosX = feetBox.bounds.center.x;
        float fPosY = feetBox.bounds.min.y;
        float cPosX = transform.position.x;
        float cPosY = transform.position.y;

        transform.position = new Vector2(ledgeCornerPosition.x + cPosX - fPosX, ledgeCornerPosition.y + cPosY - fPosY);
        isOnLedge = false;

        rigid.gravityScale = 1.0f;
        isOnLedgeClimbEnd = false;
        isTeleportedPlayer = false;
    }

    private void End_Sit()
    {
        proceedSitFrame = 0;
    }

    private void End_HeadUp()
    {
        proceedHeadUpFrame = 0;
    }

    private void End_Roll()
    {
        leftRollPreFrame = 0;
        leftRollInvincibilityFrame = 0;
        leftRollWakeUpFrame = 0;
        leftRollFrame = 0;
    }

    private void End_Dash()
    {
        leftDashIdleFrame = 0;
        leftDashInvincibilityFrame = 0;
    }

    private void End_TakeDown()
    {
        leftTakeDownPreIdleFrame = 0;
        leftTakeDownPostIdleFrame = 0;
        isTakeDownLanding = false;
    }

    #endregion

    #region Debug Logics
    #if UNITY_EDITOR

    private void LateUpdate()
    {
        Debug.Log(string.Format("current state: {0}", m_machine.state));
    }

    #endif
    #endregion
}