using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class __Player : Entity
{
    public int CURRENT_STATE;
    #region Player State Constants

    // All Available Player States
    private const int stIdleBasic           = 0; // 바닥에서의 정지(일반)
    private const int stIdleLong            = 1; // 바닥에서의 정지(장시간)
    private const int stIdleWall            = 2; // 벽 붙기
    private const int stAir                 = 3; // 체공
    private const int stGliding             = 4; // 글라이딩
    private const int stMoveWalk            = 5; // 바닥에서의 움직임(걷기)
    private const int stMoveRun             = 6; // 바닥에서의 움직임(달리기)
    private const int stJumpBasic           = 7; // 점프(일반)
    private const int stJumpAir             = 8; // 점프(공중)
    private const int stJumpWall            = 9; // 점프(벽)
    private const int stJumpDown            = 10; // 점프(하향)
    private const int stWallSliding         = 11; // 벽 슬라이딩
    private const int stLedgeClimb          = 12; // 난간 오르기
    private const int stSit                 = 13; // 앉기
    private const int stHeadUp              = 14; // 고개 들기
    private const int stRoll                = 15; // 구르기
    private const int stDash                = 16; // 대쉬
    private const int stTakeDown            = 17; // 내려 찍기

    #endregion

    #region Player Constants
    [Header("Player Constants")]
    // IdleBasic options
    public int longIdleTransitionFrame = 900;

    // IdleLong options

    // IdleWall options

    // Air options
    // float base.maxFreeFallSpeedY;
    public int freeFallAccelFrame = 39;
    private DiscreteGraph freeFallAccelGraph;

    // Gliding options
    public float glidingSpeedX = 6.0f;
    public float maxGlidingSpeedY = 2.5f;
    public int glidingAccelFrameX = 26;
    public int glidingDeaccelFrameX = 39;
    public int glidingAccelFrameY = 39;
    private DiscreteGraph glidingAccelGraphX;
    private DiscreteGraph glidingDeaccelGraphX;
    private DiscreteGraph glidingAccelGraphY;

    // MoveWalk options
    // float base.moveSpeed

    // MoveRun options
    public float runSpeed = 7.5f;
    public int runAccelFrame = 13;
    private DiscreteGraph runAccelGraph;

    // JumpBasic options
    public int maxJumpBasicCount = 1;
    public float jumpBasicSpeed = 7.0f;
    public int jumpBasicDeaccelFrame = 13;
    private DiscreteGraph jumpBasicDeaccelGraph;

    // JumpAir options
    public int maxJumpAirCount = 1;
    public float jumpAirSpeed = 8.0f;
    public int jumpAirIdleFrame = 3;
    public int jumpAirDeaccelFrame = 13;
    private DiscreteGraph jumpAirDeaccelGraph;
        
    // JumpWall options
    public float jumpWallSpeedX = 7.0f;
    public float jumpWallSpeedY = 7.0f;
    public int jumpWallDeaccelFrame = 13;
    public int jumpWallForceFrame = 6;
    private DiscreteGraph jumpWallDeaccelGraphX;
    private DiscreteGraph jumpWallDeaccelGraphY;

    // JumpDown options
    public float jumpDownSpeed = 1.5f;
    public int jumpDownDeaccelFrame = 30;
    private DiscreteGraph jumpDownDeaccelGraph;

    // WallSliding options
    public float maxWallSlidingSpeedY = 2.0f;
    public int wallSlidingAccelFrame = 13;
    private DiscreteGraph wallSlidingAccelGraph;

    // LedgeClimb options
    public float ledgeCheckRangeY = 0.2f;
    public float ledgeDetectingLength = 0.04f;

    // Sit options
    public int sitCameraMoveFrame = 120;

    // HeadUp options
    public int headUpCameraMoveFrame = 120;

    // Roll options
    public float rollSpeed = 8.0f;
    public int rollPreparingFrame = 6;
    public int rollInvincibilityFrame = 18;
    public int rollWakeUpFrame = 6;
    public int rollCoolFrame = 120;
    private DiscreteGraph rollDeaccelGraph;

    // Dash options
    public float dashSpeed = 8.0f;
    public int maxDashCount = 1;
    public int dashIdleFrame = 6;
    public int dashInvincibilityFrame = 9;
    private DiscreteLinearGraph dashDeaccelGraph;

    // TakeDown options
    public float takeDownSpeed = 32.0f;
    public int takeDownStartingIdleFrame = 18;
    public int takeDownLandingIdleFrame = 12;

    // State Machine
    private StateMachine m_machine;

    #endregion

    #region Player Variables
    [Header("Player Variables")]
    // IdleBasic options
    public int proceedLongIdleTransitionFrame;

    // IdleLong options

    // IdleWall options

    // Air options
    // float base.maxFreeFallSpeedY;
    public int proceedFreeFallAccelFrame;

    // Gliding options
    public int proceedGlidingAccelFrameX;
    public int leftGlidingDeaccelFrameX;
    public int proceedGlidingAccelFrameY;

    // MoveWalk options
    public bool isRunState;

    // MoveRun options
    public int proceedRunAccelFrame;

    // JumpBasic options
    public int leftJumpBasicCount;
    public int leftJumpBasicDeaccelFrame;

    // JumpAir options
    public int leftJumpAirCount;
    public int leftJumpAirIdleFrame;
    public int leftJumpAirDeaccelFrame;
        
    // JumpWall options
    public int currentJumpedWallDirection;
    public int leftJumpWallDeaccelFrame;
    public int leftJumpWallForceFrame;
    public bool canceledWallJump;

    // JumpDown options
    public int leftJumpDownDeaccelFrame;
    protected float throughableGroundThickness;
    protected Vector2 throughableGroundRaycasterPosition;
    protected RaycastHit2D detectedThroughableGround;

    // WallSliding options
    public int proceedWallSlidingAccelFrame;

    // LedgeClimb options
    public bool canDetectLedge;
    public bool isOnLedge;
    public bool isEndOfLedgeAnimation;
    protected RaycastHit2D detectedLedgeBottom;
    protected RaycastHit2D detectedLedgeTop;
    protected Vector2 ledgeCornerPosition;

    // Sit options
    public int proceedSitCameraMoveFrame;

    // HeadUp options
    public int proceedHeadUpCameraMoveFrame;

    // Roll options
    public int currentRollDirection;
    public int leftRollFrame;
    public int leftRollPreparingFrame;
    public int leftRollInvincibilityFrame;
    public int leftRollWakeUpFrame;
    public int leftRollCoolFrame;

    // Dash options
    public int currentDashDirection;
    public int leftDashCount;
    public int leftDashIdleFrame;
    public int leftDashInvincibilityFrame;

    // TakeDown options
    public int leftTakeDownStartingIdleFrame;
    public int leftTakeDownLandingIdleFrame;
    public bool isOnLandingAfterTakeDown;

    #endregion

    #region Input Options

    [Header("Input Options")]
    // Input options
    public InputData inputData;

    #endregion

    #region Internal Initializer (m_SetXXX)

    private void m_SetStateMachine()
    {
        m_machine = new StateMachine(stIdleBasic);

        m_machine.SetCallbacks(stIdleBasic, Input_IdleBasic, Logic_IdleBasic, Enter_IdleBasic, End_IdleBasic); // 완성
        m_machine.SetCallbacks(stIdleLong, Input_IdleLong, Logic_IdleLong, Enter_IdleLong, End_IdleLong); // 완성
        m_machine.SetCallbacks(stIdleWall, Input_IdleWall, Logic_IdleWall, Enter_IdleWall, null);
        m_machine.SetCallbacks(stAir, Input_Air, Logic_Air, Enter_Air, End_Air); // 완성
        m_machine.SetCallbacks(stGliding, Input_Gliding, Logic_Gliding, Enter_Gliding, null);
        m_machine.SetCallbacks(stMoveWalk, Input_MoveWalk, Logic_MoveWalk, Enter_MoveWalk, null); // 완성
        m_machine.SetCallbacks(stMoveRun, Input_MoveRun, Logic_MoveRun, Enter_MoveRun, End_MoveRun); // 완성
        m_machine.SetCallbacks(stJumpBasic, Input_JumpBasic, Logic_JumpBasic, Enter_JumpBasic, null);
        m_machine.SetCallbacks(stJumpAir, Input_JumpAir, Logic_JumpAir, Enter_JumpAir, null);
        m_machine.SetCallbacks(stJumpWall, Input_JumpWall, Logic_JumpWall, Enter_JumpWall, null);
        m_machine.SetCallbacks(stJumpDown, Input_JumpDown, null, null, null);
        m_machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, null);
        m_machine.SetCallbacks(stLedgeClimb, Input_LedgeClimb, Logic_LedgeClimb, Enter_LedgeClimb, End_LedgeClimb);
        m_machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, End_Sit); // 완성
        m_machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, End_HeadUp); // 완성
        m_machine.SetCallbacks(stRoll, Input_Roll, Logic_Roll, Enter_Roll, null);
        m_machine.SetCallbacks(stDash, Input_Dash, Logic_Dash, Enter_Dash, null);
        m_machine.SetCallbacks(stTakeDown, Input_TakeDown, Logic_TakeDown, Enter_TakeDown, null);
    }

    private void m_SetGraphs()
    {
        freeFallAccelGraph                  = new DiscreteLinearGraph(freeFallAccelFrame);
        glidingAccelGraphX                  = new DiscreteLinearGraph(glidingAccelFrameX);
        glidingDeaccelGraphX                = new DiscreteLinearGraph(glidingDeaccelFrameX);
        glidingAccelGraphY                  = new DiscreteLinearGraph(glidingAccelFrameY);
        runAccelGraph                       = new DiscreteLinearGraph(runAccelFrame);
        jumpBasicDeaccelGraph               = new DiscreteLinearGraph(jumpBasicDeaccelFrame);
        jumpAirDeaccelGraph                 = new DiscreteLinearGraph(jumpAirDeaccelFrame);
        jumpWallDeaccelGraphX               = new DiscreteLinearGraph(jumpWallDeaccelFrame);
        jumpWallDeaccelGraphY               = new DiscreteParabolaGraph(jumpWallDeaccelFrame);
        jumpDownDeaccelGraph                = new DiscreteLinearGraph(jumpDownDeaccelFrame);
        wallSlidingAccelGraph               = new DiscreteLinearGraph(wallSlidingAccelFrame);
        rollDeaccelGraph                    = new DiscreteLinearGraph(rollPreparingFrame + rollInvincibilityFrame + rollWakeUpFrame);
        dashDeaccelGraph                    = new DiscreteLinearGraph(dashInvincibilityFrame);
    }
    
    #endregion

    #region Unity Event Functions

    protected override void Start()
    {
        base.Start();

        m_SetStateMachine();
        m_SetGraphs();
    }

    protected override void Update()
    {
        inputData.Copy(InputHandler.data);
        CheckLookingDirection(inputData.xInput);

        base.Update();

        m_machine.UpdateInput();
        CURRENT_STATE = m_machine.state;

        // Debug.Log(string.Format("Current State: {0}", CURRENT_STATE));

        // NOTE: Test Input for existing LedgeClimb State.
        if(CURRENT_STATE == stLedgeClimb && Input.GetKeyDown(KeyCode.Return))
        {
            isEndOfLedgeAnimation = true;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            isRunState = !isRunState;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        CheckLedge();

        CheckCoolFrame();

        m_machine.UpdateLogic();
    }

    #endregion

    #region Physics Checker

    protected void CheckLedge()
    {
        if(!canDetectLedge)
        {
            isOnLedge = false;
            detectedLedgeBottom = default(RaycastHit2D);
            detectedLedgeTop = default(RaycastHit2D);

            return;
        }

        float bPosX, bPosY;
        float tPosX, tPosY;

        bPosX = ceilBox.bounds.center.x + ceilBox.bounds.extents.x * lookingDirection;
        bPosY = ceilBox.bounds.center.y;
        tPosX = bPosX;
        tPosY = bPosY + ledgeCheckRangeY;

        Vector2 ledgeCheckerPositionBottom = new Vector2(bPosX, bPosY);
        Vector2 ledgeCheckerPositionTop = new Vector2(tPosX, tPosY);

        detectedLedgeBottom = Physics2D.Raycast(ledgeCheckerPositionBottom, Vector2.right * lookingDirection, ledgeDetectingLength, LayerInfo.groundMask);
        detectedLedgeTop = default(RaycastHit2D);
        RaycastHit2D temp_detectedLedgeTop = Physics2D.Raycast(ledgeCheckerPositionTop, Vector2.right * lookingDirection, ledgeDetectingLength, LayerInfo.groundMask);

        // TODO:
        // 이 곳에 Ledge를 판정하는 Hinge를 찾는 구문을 추가, Hinge가 없는 Ledge라면 아래 코드를 실행하지 않는다.
        // 변수 detectedLedgeBottom을 이용한 구문을 작성한다.

        if(detectedLedgeBottom && !temp_detectedLedgeTop)
        {
            isOnLedge = true;

            float adder = ceilBox.bounds.extents.x;
            float xDistance = detectedLedgeBottom.distance + adder;

            tPosX = bPosX + xDistance * lookingDirection;
            tPosY = bPosY + ceilBox.bounds.extents.y + ledgeCheckRangeY;
            ledgeCheckerPositionTop.Set(tPosX, tPosY);

            detectedLedgeTop = Physics2D.Raycast(ledgeCheckerPositionTop, Vector2.down, ledgeCheckRangeY + 0.02f, LayerInfo.groundMask);

            if(detectedLedgeTop)
            {
                SetVector(ref ledgeCornerPosition, detectedLedgeTop.point.x, detectedLedgeTop.point.y);
            }
            else
            {
                // TODO: 치명적인 오류 발생으로 프로그램 종료함. 따로 처리를 할 필요가 있음. 웬만해서는 일어나지 않을 듯?
            }
        }
        else
        {
            isOnLedge = false;
        }
    }

    protected void CheckCoolFrame()
    {
        if(leftRollCoolFrame > 0)
            leftRollCoolFrame--;
    }

    protected float GetMoveSpeed()
    {
        return isRunState ? runSpeed : moveSpeed;
    }

    #endregion

    #region Implement State; stIdleBasic

    private void Enter_IdleBasic()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        leftJumpBasicCount = maxJumpBasicCount;
        leftJumpAirCount = maxJumpAirCount;
        leftDashCount = maxDashCount;

        // 선입력 프레임 수
        int bufferedFrame = 6;
        int i;
        InputData idat;

        for(i = 0; i < bufferedFrame; i++)
        {
            idat = InputBuffer.GetBufferedData(i);

            if(idat.jumpDown && leftJumpBasicCount > 0)
            {
                m_machine.ChangeState(stJumpBasic);
                break;
            }
            if(idat.dashDown && leftRollCoolFrame == 0)
            {
                m_machine.ChangeState(stRoll);
                break;
            }
            /*
            if(idat.yNegative != 0)
            {
                m_machine.ChangeState(stSit);
                break;
            }*/
        }
    }

    private void Input_IdleBasic()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(proceedLongIdleTransitionFrame == longIdleTransitionFrame)
        {
            m_machine.ChangeState(stIdleLong);
            return;
        }
        if(inputData.yNegative != 0)
        {
            m_machine.ChangeState(stSit);
            return;
        }
        if(inputData.yPositive != 0)
        {
            m_machine.ChangeState(stHeadUp);
            return;
        }
        if(inputData.jumpDown && leftJumpBasicCount > 0)
        {
            m_machine.ChangeState(stJumpBasic);
            return;
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(inputData.xInput != 0)
        {
            if(isRunState)
                m_machine.ChangeState(stMoveRun);
            else
                m_machine.ChangeState(stMoveWalk);

            return;
        }
    }

    private void Logic_IdleBasic()
    {
        SetVelocity(0.0f, 0.0f);

        if(proceedLongIdleTransitionFrame < longIdleTransitionFrame)
        {
            proceedLongIdleTransitionFrame++;
        }
    }

    private void End_IdleBasic()
    {
        proceedLongIdleTransitionFrame = 0;
    }

    #endregion

    #region Implement State; stIdleLong

    private void Enter_IdleLong()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        proceedLongIdleTransitionFrame = 0;
    }

    private void Input_IdleLong()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.yNegative != 0)
        {
            m_machine.ChangeState(stSit);
            return;
        }
        if(inputData.yPositive != 0)
        {
            m_machine.ChangeState(stHeadUp);
            return;
        }
        if(inputData.jumpDown && leftJumpBasicCount > 0)
        {
            m_machine.ChangeState(stJumpBasic);
            return;
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(inputData.xInput != 0)
        {
            if(isRunState)
                m_machine.ChangeState(stMoveRun);
            else
                m_machine.ChangeState(stMoveWalk);

            return;
        }
    }

    private void Logic_IdleLong()
    {
        SetVelocity(0.0f, 0.0f);
    }

    private void End_IdleLong()
    {
        proceedLongIdleTransitionFrame = 0;
    }

    #endregion

    #region Implement State; stIdleWall

    private void Enter_IdleWall()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = true;

        if(leftDashCount != maxDashCount)
            leftDashCount = maxDashCount;

        leftJumpAirCount = maxJumpAirCount;

        // 선입력 프레임 수
        int bufferedFrame = 6;
        int i;
        InputData idat;

        for(i = 0; i < bufferedFrame; i++)
        {
            idat = InputBuffer.GetBufferedData(i);

            if(idat.jumpDown)
            {
                m_machine.ChangeState(stJumpWall);
                break;
            }
        }
    }

    private void Input_IdleWall()
    {
        if(isOnWallFeet == 0 || isOnWallCeil == 0 || inputData.yNegDown)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.xInput == 0)
        {
            m_machine.ChangeState(stWallSliding);
            return;
        }
        if(inputData.jumpDown)
        {
            m_machine.ChangeState(stJumpWall);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_IdleWall()
    {
        SetVelocity(0.0f, 0.0f);
    }

    #endregion

    #region Implement State; stAir

    private void Enter_Air()
    {
        int i;

        rigid.gravityScale = 1.0f;
        canDetectLedge = true;

        if(leftJumpBasicCount == maxJumpBasicCount)
            leftJumpBasicCount--;

        if(currentVelocity.y >= 0)
        {
            proceedFreeFallAccelFrame = 0;
        }
        else if(currentVelocity.y < -maxFreeFallSpeed * freeFallAccelGraph[freeFallAccelFrame - 1])
        {
            proceedFreeFallAccelFrame = freeFallAccelFrame - 1;
        }
        else
        {
            for(i = 0; i < freeFallAccelFrame; i++)
            {
                if(-maxFreeFallSpeed * freeFallAccelGraph[i] <= currentVelocity.y)
                {
                    proceedFreeFallAccelFrame = i;
                    break;
                }
            }
        }

        // 선입력 프레임 수
        int bufferedFrame = 6;
        InputData idat;

        for(i = 0; i < bufferedFrame; i++)
        {
            idat = InputBuffer.GetBufferedData(i);

            if(idat.jumpDown && leftJumpAirCount > 0)
            {
                m_machine.ChangeState(stJumpAir);
                break;
            }
        }
    }

    private void Input_Air()
    {
        if(isOnGround)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.jumpDown && leftJumpAirCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
        if(inputData.dashDown && leftDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(inputData.yPositive != 0)
        {
            m_machine.ChangeState(stGliding);
            return;
        }
        if(inputData.yNegative != 0 && inputData.jumpPressing)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_Air()
    {
        if(inputData.xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
        {
            SetVelocityX(0.0f);
        }
        else
        {
            SetVelocityX(GetMoveSpeed() * lookingDirection);
        }

        if(currentVelocity.y < 0)
        {
            SetVelocityY(-maxFreeFallSpeed * freeFallAccelGraph[proceedFreeFallAccelFrame]);

            if(proceedFreeFallAccelFrame < freeFallAccelFrame - 1)
                proceedFreeFallAccelFrame++;
        }
    }

    private void End_Air()
    {
        proceedFreeFallAccelFrame = 0;
    }

    #endregion

    #region Implement State; stGliding

    private void Enter_Gliding()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = true;
    }

    private void Input_Gliding()
    {
        if(inputData.yPositive == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(isOnGround)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.jumpDown && leftJumpAirCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
        if(inputData.dashDown && leftDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_Gliding()
    {
        int i;

        if(isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
        {
            SetVelocityX(0.0f);
        }
        else if(inputData.xInput == 0) // 감속
        {
            if(proceedGlidingAccelFrameX > 0)
            {
                proceedGlidingAccelFrameX = 0;

                for(i = glidingDeaccelFrameX - 1; i >= 0; i--)
                {
                    if(glidingSpeedX * glidingDeaccelGraphX[i] <= Mathf.Abs(currentVelocity.x))
                    {
                        leftGlidingDeaccelFrameX = i;
                        break;
                    }
                }
            }

            SetVelocityX(glidingSpeedX * glidingDeaccelGraphX[leftGlidingDeaccelFrameX] * lookingDirection);

            if(leftGlidingDeaccelFrameX > 0)
                leftGlidingDeaccelFrameX--;
        }
        else if(inputData.xInput != 0) // 가속
        {
            if(leftGlidingDeaccelFrameX < glidingDeaccelFrameX - 1)
            {
                leftGlidingDeaccelFrameX = glidingDeaccelFrameX - 1;

                for(i = 0; i < glidingAccelFrameX; i++)
                {
                    if(glidingSpeedX * glidingAccelGraphX[i] >= Mathf.Abs(currentVelocity.x))
                    {
                        proceedGlidingAccelFrameX = i;
                        break;
                    }
                }
            }

            SetVelocityX(glidingSpeedX * glidingAccelGraphX[proceedGlidingAccelFrameX] * lookingDirection);

            if(proceedGlidingAccelFrameX < glidingAccelFrameX - 1)
                proceedGlidingAccelFrameX++;
        }

        if(currentVelocity.y < 0)
        {
            SetVelocityY(-maxGlidingSpeedY);
            /*
            SetVelocityY(-maxGlidingSpeedY * freeFallAccelGraph[proceedFreeFallAccelFrame]);

            if(proceedFreeFallAccelFrame < freeFallAccelFrame - 1)
                proceedFreeFallAccelFrame++;
                */
        }
    }

    #endregion

    #region Implement State; stMoveWalk

    private void Enter_MoveWalk()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = false;
    }

    private void Input_MoveWalk()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.xInput == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.yNegative != 0)
        {
            m_machine.ChangeState(stSit);
            return;
        }
        if(inputData.yPositive != 0)
        {
            m_machine.ChangeState(stHeadUp);
            return;
        }
        if(inputData.jumpDown && leftJumpBasicCount > 0)
        {
            m_machine.ChangeState(stJumpBasic);
            return;
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(isRunState)
        {
            m_machine.ChangeState(stMoveRun);
            return;
        }
    }

    private void Logic_MoveWalk()
    {
        MoveOnGround(GetMoveSpeed(), lookingDirection);
    }

    #endregion

    #region Implement State; stMoveRun

    private void Enter_MoveRun()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = false;

        proceedRunAccelFrame = 0;
    }

    private void Input_MoveRun()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.xInput == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.yNegative != 0)
        {
            m_machine.ChangeState(stSit);
            return;
        }
        if(inputData.yPositive != 0)
        {
            m_machine.ChangeState(stHeadUp);
            return;
        }
        if(inputData.jumpDown && leftJumpBasicCount > 0)
        {
            m_machine.ChangeState(stJumpBasic);
            return;
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
        if(!isRunState)
        {
            m_machine.ChangeState(stMoveWalk);
            return;
        }
    }

    private void Logic_MoveRun()
    {
        MoveOnGround(runSpeed * runAccelGraph[proceedRunAccelFrame], lookingDirection);

        if(proceedRunAccelFrame < runAccelFrame - 1)
            proceedRunAccelFrame++;
    }

    private void End_MoveRun()
    {
        proceedRunAccelFrame = 0;
    }

    #endregion

    #region Implement State; stJumpBasic

    private void Enter_JumpBasic()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = true;

        leftJumpBasicCount--;
        leftJumpBasicDeaccelFrame = jumpBasicDeaccelFrame;
    }

    private void Input_JumpBasic()
    {
        if((currentVelocity.y <= 0.0f && leftJumpBasicDeaccelFrame == 0) || isOnCeil)
        {
            if(inputData.yPositive == 0)
                m_machine.ChangeState(stAir);
            else
                m_machine.ChangeState(stGliding);

            return;
        }
        if(inputData.jumpDown && leftJumpAirCount > 0)
        {
            m_machine.ChangeState(stJumpAir);
            return;
        }
        if(inputData.dashDown && leftDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(inputData.yNegative != 0 && inputData.jumpPressing && !isDetectedGround)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_JumpBasic()
    {
        if(inputData.xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
        {
            SetVelocityX(0.0f);
        }
        else
        {
            SetVelocityX(GetMoveSpeed() * lookingDirection);
        }

        if(leftJumpBasicDeaccelFrame > 0)
        {
            leftJumpBasicDeaccelFrame--;

            SetVelocityY(jumpBasicSpeed * jumpBasicDeaccelGraph[leftJumpBasicDeaccelFrame]);
        }
    }

    #endregion

    #region Implement State; stJumpAir

    private void Enter_JumpAir()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = true;

        leftJumpAirCount--;
        leftJumpAirIdleFrame = jumpAirIdleFrame;
        leftJumpAirDeaccelFrame = 0;
    }

    private void Input_JumpAir()
    {
        if(leftJumpAirIdleFrame > 0)
            return;

        if((currentVelocity.y <= 0.0f && leftJumpAirDeaccelFrame == 0) || isOnCeil)
        {
            if(inputData.yPositive == 0)
                m_machine.ChangeState(stAir);
            else
                m_machine.ChangeState(stGliding);

            return;
        }
        if(inputData.jumpDown && leftJumpAirCount > 0)
        {
            m_machine.RestartState();
            return;
        }
        if(inputData.dashDown && leftDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(inputData.yNegative != 0 && inputData.jumpPressing)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_JumpAir()
    {
        if(leftJumpAirIdleFrame > 0)
        {
            leftJumpAirIdleFrame--;

            SetVelocity(0.0f, 0.0f);

            if(leftJumpAirIdleFrame == 0)
                leftJumpAirDeaccelFrame = jumpAirDeaccelFrame;
        }
        else if(leftJumpAirDeaccelFrame > 0)
        {
            if(inputData.xInput == 0 || isOnWallFeet == lookingDirection || isOnWallCeil == lookingDirection)
            {
                SetVelocityX(0.0f);
            }
            else
            {
                SetVelocityX(GetMoveSpeed() * lookingDirection);
            }

            leftJumpAirDeaccelFrame--;

            SetVelocityY(jumpAirSpeed * jumpAirDeaccelGraph[leftJumpAirDeaccelFrame]);
        }
    }

    #endregion

    #region Implement State; stJumpWall

    private void Enter_JumpWall()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = true;

        currentJumpedWallDirection = isOnWallFeet;
        leftJumpWallDeaccelFrame = jumpWallDeaccelFrame;
        leftJumpWallForceFrame = jumpWallForceFrame;
        canceledWallJump = false;

        leftJumpAirCount = maxJumpAirCount;
    }

    private void Input_JumpWall()
    {
        if(leftJumpWallForceFrame != 0)
            return;

        if((currentVelocity.y <= 0.0f && leftJumpWallDeaccelFrame == 0) || isOnCeil)
        {
            if(inputData.yPositive == 0)
                m_machine.ChangeState(stAir);
            else
                m_machine.ChangeState(stGliding);

            return;
        }
        if(inputData.dashDown && leftDashCount > 0)
        {
            m_machine.ChangeState(stDash);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
        if(inputData.yNegative != 0 && inputData.jumpPressing)
        {
            m_machine.ChangeState(stTakeDown);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
    }

    private void Logic_JumpWall()
    {
        if(leftJumpWallForceFrame > 0)
        {
            leftJumpWallForceFrame--;
        }
        else if(inputData.xInput != 0)
        {
            canceledWallJump = true;
        }

        if(leftJumpWallDeaccelFrame > 0)
        {
            leftJumpWallDeaccelFrame--;

            if(canceledWallJump)
            {
                SetVelocityX(GetMoveSpeed() * lookingDirection);
            }
            else
            {
                SetVelocityX(jumpWallSpeedX * jumpWallDeaccelGraphX[leftJumpWallDeaccelFrame] * currentJumpedWallDirection * -1);
            }

            SetVelocityY(jumpWallSpeedY * jumpWallDeaccelGraphY[leftJumpWallDeaccelFrame]);
        }
    }

    #endregion

    #region Implement State; stJumpDown

    private void Input_JumpDown()
    {
        // TODO: 보류
    }

    #endregion

    #region Implement State; stWallSliding

    private void Enter_WallSliding()
    {
        rigid.gravityScale = 1.0f;
        canDetectLedge = false;

        proceedWallSlidingAccelFrame = 0;

        if(leftDashCount != maxDashCount)
            leftDashCount = maxDashCount;
    }

    private void Input_WallSliding()
    {
        if(isOnWallFeet == 0 || isOnWallCeil == 0 || inputData.yNegDown)
        {
            m_machine.ChangeState(stAir);
        }
        if(isDetectedGround)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.jumpDown)
        {
            m_machine.ChangeState(stJumpWall);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
    }

    private void Logic_WallSliding()
    {
        SetVelocity(0.0f, -maxWallSlidingSpeedY * wallSlidingAccelGraph[proceedWallSlidingAccelFrame]);

        if(proceedWallSlidingAccelFrame < wallSlidingAccelFrame - 1)
            proceedWallSlidingAccelFrame++;
    }

    #endregion

    #region Implement State; stLedgeClimb

    private void Enter_LedgeClimb()
    {
        // rigid.gravityScale = 1.0f;
        // rigid.AddForce(Vector2.right * lookingDirection * 80.0f, ForceMode2D.Force);

        float extentX = ceilBox.bounds.extents.x;
        Vector2 wallCornerPosition = new Vector2(ledgeCornerPosition.x + extentX * lookingDirection * -1, ledgeCornerPosition.y - extentX);
        Vector2 dir = new Vector2(transform.position.x - ceilBox.bounds.max.x, transform.position.y - ceilBox.bounds.center.y);
        transform.position = wallCornerPosition + dir;

        GameObject obj = new GameObject();
        obj.transform.position = ledgeCornerPosition;
        obj = new GameObject();
        obj.transform.position = wallCornerPosition;

        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        isEndOfLedgeAnimation = false;
    }

    private void Input_LedgeClimb()
    {
        if(isEndOfLedgeAnimation)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
    }

    private void Logic_LedgeClimb()
    {
        SetVelocity(0.0f, 0.0f);
    }

    private void End_LedgeClimb()
    {
        Vector2 dir = transform.position - (Vector3)feetPosition;
        transform.position = ledgeCornerPosition + dir;

        isEndOfLedgeAnimation = false;
    }

    #endregion

    #region Implement State; stSit

    private void Enter_Sit()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        proceedSitCameraMoveFrame = 0;
    }

    private void Input_Sit()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.jumpDown)
        {
            if(detectedThroughableGround)
            {
                m_machine.ChangeState(stJumpDown);
            }
            else
            {
                m_machine.ChangeState(stJumpBasic);
            }
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
    }

    private void Logic_Sit()
    {
        SetVelocity(0.0f, 0.0f);

        if(proceedSitCameraMoveFrame < sitCameraMoveFrame)
            proceedSitCameraMoveFrame++;
    }

    private void End_Sit()
    {
        proceedSitCameraMoveFrame = 0;
    }

    #endregion

    #region Implement State; stHeadUp

    private void Enter_HeadUp()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        proceedHeadUpCameraMoveFrame = 0;
    }

    private void Input_HeadUp()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(inputData.yPositive == 0)
        {
            m_machine.ChangeState(stIdleBasic);
            return;
        }
        if(inputData.jumpDown && leftJumpBasicCount > 0)
        {
            m_machine.ChangeState(stJumpBasic);
            return;
        }
        if(inputData.dashDown && leftRollCoolFrame == 0)
        {
            m_machine.ChangeState(stRoll);
            return;
        }
    }

    private void Logic_HeadUp()
    {
        SetVelocity(0.0f, 0.0f);

        if(proceedHeadUpCameraMoveFrame < headUpCameraMoveFrame)
            proceedHeadUpCameraMoveFrame++;
    }

    private void End_HeadUp()
    {
        proceedHeadUpCameraMoveFrame = 0;
    }

    #endregion

    #region Implement State; stRoll

    private void Enter_Roll()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        currentRollDirection = lookingDirection;
        leftRollFrame = rollPreparingFrame + rollInvincibilityFrame + rollWakeUpFrame;
        leftRollPreparingFrame = rollPreparingFrame;
        leftRollInvincibilityFrame = 0;
        leftRollWakeUpFrame = 0;
    }

    private void Input_Roll()
    {
        if(!isDetectedGround)
        {
            m_machine.ChangeState(stAir);
            return;
        }

        // NOTE: 점프, 앉기로 이동하는 입력을 받아들일 수 있는 타이밍을 제어하는 변수
        int frame = rollInvincibilityFrame;

        if(leftRollPreparingFrame == 0)
        {
            if(leftRollInvincibilityFrame < frame)
            {
                if(inputData.jumpDown && leftJumpBasicCount > 0)
                {
                    m_machine.ChangeState(stJumpBasic);
                    return;
                }
                if(inputData.yNegative != 0)
                {
                    m_machine.ChangeState(stSit);
                    return;
                }
            }

            if(leftRollInvincibilityFrame == 0)
            {
                if(leftRollWakeUpFrame == 0)
                {
                    m_machine.ChangeState(stIdleBasic);
                    return;
                }
                else if(inputData.xInput != 0)
                {
                    if(isRunState)
                        m_machine.ChangeState(stMoveRun);
                    else
                        m_machine.ChangeState(stMoveWalk);
                    return;
                }
            }
        }
    }

    private void Logic_Roll()
    {
        if(leftRollFrame <= 0)
            return;

        if(leftRollPreparingFrame > 0)
        {
            leftRollPreparingFrame--;

            if(leftRollPreparingFrame == 0)
            {
                leftRollInvincibilityFrame = rollInvincibilityFrame;
                leftRollCoolFrame = rollCoolFrame;
            }
        }
        else if(leftRollInvincibilityFrame > 0)
        {
            leftRollInvincibilityFrame--;

            if(leftRollInvincibilityFrame == 0)
            {
                leftRollWakeUpFrame = rollWakeUpFrame;
            }
        }
        else if(leftRollWakeUpFrame > 0)
        {
            leftRollWakeUpFrame--;
        }

        leftRollFrame--;

        MoveOnGround(rollSpeed * rollDeaccelGraph[leftRollFrame], currentRollDirection);
    }

    #endregion

    #region Implement State; stDash

    private void Enter_Dash()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = true;

        currentDashDirection = lookingDirection;
        leftDashCount--;
        leftDashIdleFrame = dashIdleFrame;
        leftDashInvincibilityFrame = 0;
    }

    private void Input_Dash()
    {
        if(leftDashIdleFrame == 0 && leftDashInvincibilityFrame == 0)
        {
            m_machine.ChangeState(stAir);
            return;
        }
        if(isOnLedge && inputData.xInput == lookingDirection)
        {
            m_machine.ChangeState(stLedgeClimb);
            return;
        }
        if(!isDetectedGround && inputData.xInput == lookingDirection && isOnWallFeet == lookingDirection && isOnWallCeil == lookingDirection && inputData.yNegative == 0)
        {
            m_machine.ChangeState(stIdleWall);
            return;
        }
    }

    private void Logic_Dash()
    {
        if(leftDashIdleFrame > 0)
        {
            leftDashIdleFrame--;

            SetVelocity(0.0f, 0.0f);

            if(leftDashIdleFrame == 0)
            {
                leftDashInvincibilityFrame = dashInvincibilityFrame;
            }
        }
        else if(leftDashInvincibilityFrame > 0)
        {
            leftDashInvincibilityFrame--;

            SetVelocity(dashSpeed * dashDeaccelGraph[leftDashInvincibilityFrame] * currentDashDirection, 0.0f);
        }
    }

    #endregion

    #region Implement State; stTakeDown

    private void Enter_TakeDown()
    {
        rigid.gravityScale = 0.0f;
        canDetectLedge = false;

        leftTakeDownStartingIdleFrame = takeDownStartingIdleFrame;
        leftTakeDownLandingIdleFrame = 0;
        isOnLandingAfterTakeDown = false;
    }

    private void Input_TakeDown()
    {
        if(isOnLandingAfterTakeDown)
        {
            if(!isDetectedGround)
            {
                m_machine.ChangeState(stAir);
                return;
            }
            if(leftTakeDownLandingIdleFrame > 0)
            {
                m_machine.ChangeState(stIdleBasic);
                return;
            }
        }
    }

    private void Logic_TakeDown()
    {
        if(leftTakeDownStartingIdleFrame > 0)
        {
            SetVelocity(0.0f, 0.0f);

            leftTakeDownStartingIdleFrame--;

            if(leftTakeDownStartingIdleFrame == 0)
                leftTakeDownLandingIdleFrame = takeDownLandingIdleFrame;
        }
        else if(leftTakeDownLandingIdleFrame > 0)
        {
            if(isOnGround)
            {
                if(!isOnLandingAfterTakeDown)
                    isOnLandingAfterTakeDown = true;

                SetVelocity(0.0f, 0.0f);

                leftTakeDownLandingIdleFrame--;
            }
            else
            {
                SetVelocity(0.0f, -takeDownSpeed);
            }
        }
    }

    #endregion
}
