using System;
using UnityEngine;

// 최적화 코드
public class _Player : Entity
{
    #region Components
    private SpriteRenderer spRenderer;
    #endregion

    #region Player State Constants
    private const int stIdleGround = 0;
    private const int stIdleGroundLong = 1;
    private const int stSit = 2;
    private const int stHeadUp = 3;
    private const int stWalk = 4;
    private const int stRun = 5;
    private const int stFreeFall = 6;
    private const int stGliding = 7;
    private const int stIdleWall = 8;
    private const int stWallSliding = 9;
    private const int stLedgeClimb = 10;
    private const int stJumpGround = 11;
    private const int stJumpDown = 12;
    private const int stRoll = 13;
    private const int stJumpAir = 14;
    private const int stDash = 15;
    private const int stTakeDown = 16;
    private const int stJumpWall = 17;
    #endregion

    // NOTE: 플레이어 초기 위치, 테스트를 위한 벡터 변수, 릴리즈 시 제거해야 함.
    private Vector2 initialPos;

    // 충돌 판정의 특이점들 (singular points)
    private Bounds hBounds;
    private Bounds fBounds;
    private Bounds cBounds;
    protected Vector2 hPos; // Head Position
    protected Vector2 hlPos; // Head Left Position
    protected Vector2 hrPos; // Head Right Position
    protected Vector2 fPos; // Feet Position
    protected Vector2 flPos; // Feet Left Position
    protected Vector2 frPos; // Feet Right Position
    protected Vector2 cPos; // Center Position (Body Position)
    protected Vector2 clPos;
    protected Vector2 crPos;
    protected Vector2 ltPos; // Left Top Position
    protected Vector2 lbPos; // Left Bottom Position
    protected Vector2 rtPos; // Right Top Position
    protected Vector2 rbPos; // Right Bottom Position

    // Check Ground
    protected bool canCheckBasicGround = true;
    protected bool canCheckSemiGround = true;
    protected RaycastHit2D detectedGround;
    public bool isDetectedGround;
    public bool isHitGround;

    // Check Ceil
    protected bool canCheckCeil = true;
    protected RaycastHit2D detectedCeil;
    public bool isDetectedCeil;
    public bool isHitCeil;

    // Check Wall
    protected bool canCheckWallT = true;
    protected bool canCheckWallB = true;
    protected RaycastHit2D detectedWallT;
    protected RaycastHit2D detectedWallB;
    public int isHitWallT;
    public int isHitWallB;

    // Check Head-Over Semi Ground
    protected bool canCheckHeadOverSemiGround = true;
    protected RaycastHit2D headOverSemiGroundBefore;
    protected RaycastHit2D headOverSemiGroundCurrent;

    // Check Ledge
    protected bool canCheckLedge = true;
    protected RaycastHit2D detectedLedge;
    public float ledgeCheckingWidth = 0.04f;
    public float ledgeCheckingHeight = 0.2f;
    protected Vector2 ledgeCornerTopPos;
    protected Vector2 ledgeCornerSidePos;
    // protected Vector2 ledgeTeleportPos; // 쓸까 말까?
    public bool isHitLedgeHead;
    public bool isHitLedgeBody;

    // Check Ledge Hanging
    protected bool canCheckHangingBasic = true;
    protected bool canCheckHangingSemi = true;
    protected RaycastHit2D leftHangingGround;
    protected RaycastHit2D rightHangingGround;
    public bool isHangingGround;

    // Input Handling
    private InputData inputData;
    private InputData preInputData;
    private uint preInputPressing = 0;
    private uint preInputDown = 0;

    // 플레이어 이동 관련 속성
    private bool canUpdateLookDir = true;
    private int lookDir;
    public bool isRun;
    protected float vx; // NOTE: 값 임시 저장을 위한 변수
    protected float vy; // NOTE: 값 임시 저장을 위한 변수
    protected Vector2 moveDir;

    #region State Constants and Variables
    // common state options
    public int currentState => machine.state;
    private StateMachine machine;

    // stIdleGround options
    private int proceedIdleGroundFrame;
    public int preInputFrameIdleGround;

    // stIdleGroundLong options
    // TODO: 외부 클래스에서 접근해야 할 필요가 있음.
    private int idleLongTransitionFrame = 900;

    // stSit options
    // TODO: 외부 클래스에서 접근해야 할 필요가 있음.
    private int proceedSitFrame;

    // stHeadUp options
    // TODO: 외부 클래스에서 접근해야 할 필요가 있음.
    private int proceedHeadUpFrame;

    // stWalk options
    public float walkSpeed = 3.5f;

    // stRun options
    public float runSpeed = 7.0f;

    // stFreeFall options
    public float maxFreeFallSpeed = 12.0f;
    public int freeFallFrame = 39;
    private DiscreteGraph freeFallGraph;
    private int proceedFreeFallFrame;
    public int preInputFrameFreeFall;

    // stGliding options
    public float glidingSpeed = 0.05f;
    public int glidingAccelFrameX = 39;
    public int glidingDeaccelFrameX = 26;
    private DiscreteGraph glidingAccelGraphX;
    private DiscreteGraph glidingDeaccelGraphX;
    private int proceedGlidingAccelFrameX;
    private int leftGlidingDeaccelFrameX;

    // stIdleOnWall options
    public int preInputFrameIdleWall;

    // stWallSliding options
    public float maxWallSlidingSpeed = 1.5f;
    public int wallSlidingFrame = 26;
    private DiscreteGraph wallSlidingGraph;
    private int proceedWallSlidingFrame;

    // stLedgeClimb options
    public bool isEndOfLedgeAnimation;

    // stJumpGround options
    public int jumpGroundCount = 1;
    public float jumpGroundSpeed = 5.5f;
    public int jumpGroundFrame = 18;
    private DiscreteGraph jumpGroundGraph;
    private int leftJumpGroundCount;
    private int leftJumpGroundFrame;
    private bool isJumpGroundCanceled;

    // stJumpDown options
    public float jumpDownSpeed = 1.5f;
    public int jumpDownFrame = 13;
    private DiscreteGraph jumpDownGraph;
    private RaycastHit2D currentJumpDownGround;
    private int leftJumpDownFrame;

    // stRoll options
    public float rollSpeed = 9.5f;
    public int rollStartFrame = 6;
    public int rollInvincibilityFrame = 18;
    public int rollWakeUpFrame = 6;
    private DiscreteGraph rollGraph;
    private int leftRollStartFrame;
    private int leftRollInvincibilityFrame;
    private int leftRollWakeUpFrame;
    private int leftRollFrame;
    private int rollLookDir;

    // stJumpAir options
    public int jumpAirCount = 1;
    public float jumpAirSpeed = 7.5f;
    public int jumpAirIdleFrame = 3;
    public int jumpAirFrame = 20;
    private DiscreteGraph jumpAirGraph;
    private int leftJumpAirCount;
    private int leftJumpAirIdleFrame;
    private int leftJumpAirFrame;
    private bool isJumpAirCanceled;

    // stDash options
    public int dashCount = 1;
    public float dashSpeed = 36.0f;
    public int dashIdleFrame = 6;
    public int dashInvincibilityFrame = 9;
    private DiscreteGraph dashGraph;
    private int leftDashCount;
    private int leftDashIdleFrame;
    private int leftDashInvincibilityFrame;
    private int dashLookDir;

    // stTakeDown options
    public float takeDownSpeed = 48.0f;
    public int takeDownAirIdleFrame = 18;
    public int takeDownLandingIdleFrame = 12;
    private int leftTakeDownAirIdleFrame;
    private int leftTakeDownLandingIdleFrame;
    private bool isLandingAfterTakeDown;

    // stJumpWall options
    public float jumpWallSpeedX = 7.0f;
    public float jumpWallSpeedY = 10.0f;
    public int jumpWallFrame = 13;
    public int jumpWallForceFrame = 6;
    private DiscreteGraph jumpWallGraphX;
    private DiscreteGraph jumpWallGraphY;
    private int leftJumpWallFrame;
    private int leftJumpWallForceFrame;
    private int jumpWallLookDir;
    private bool isJumpWallCanceledX;
    private bool isJumpWallCanceledXY;
    #endregion

    private void m_UpdatePositions()
    {
        hBounds = headBox.bounds;
        fBounds = feetBox.bounds;
        cBounds = bodyBox.bounds;
        hPos.Set(hBounds.center.x, hBounds.max.y);
        hlPos.Set(hBounds.min.x, hBounds.center.y);
        hrPos.Set(hBounds.max.x, hBounds.center.y);
        fPos.Set(fBounds.center.x, fBounds.min.y);
        flPos.Set(fBounds.min.x, fBounds.center.y);
        frPos.Set(fBounds.max.x, fBounds.center.y);
        cPos.Set(cBounds.center.x, cBounds.center.y);
        clPos.Set(cBounds.min.x, cBounds.center.y);
        crPos.Set(cBounds.max.x, cBounds.center.y);
        ltPos.Set(hBounds.min.x, hBounds.min.y);
        lbPos.Set(fBounds.min.x, fBounds.max.y);
        rtPos.Set(hBounds.max.x, hBounds.min.y);
        rbPos.Set(fBounds.max.x, fBounds.max.y);
    }

    protected void UpdateLookDir()
    {
        if(lookDir == 0)
            lookDir = 1;

        if(!canUpdateLookDir)
            return;

        int xInput = inputData.xNegative + inputData.xPositive;

        if(xInput != 0)
            lookDir = xInput;
    }

    protected void UpdateMoveDirection()
    {
        if(isHitGround)
        {
            vx = detectedGround.normal.y;
            vy = -detectedGround.normal.x;

            moveDir.Set(vx, vy);
        }
        else
        {
            moveDir.Set(1.0f, 0.0f);
        }
    }

    protected float GetMoveSpeed()
    {
        return isRun ? runSpeed : walkSpeed;
    }

    protected float CheckVelocityX(float currentVx)
    {
        if(inputData.xInput == lookDir && (isHitWallT == lookDir || isHitWallB == lookDir))
            return 0.0f;

        return currentVx;
    }
// Check Ground/Ceil/(Wall)/(Head-Over Semi Ground)/(Ledge)/(Ledge Hanging)
    protected void CheckGrounds()
    {
        if(canCheckBasicGround && canCheckSemiGround)
        {
            CheckGroundAll(out detectedGround, out isDetectedGround, fPos, 0.5f);
            isHitGround = isDetectedGround && detectedGround.distance <= 0.04f;
        }
        else if(canCheckBasicGround && !canCheckSemiGround)
        {
            CheckGroundBasic(out detectedGround, out isDetectedGround, fPos, 0.5f);
            isHitGround = isDetectedGround && detectedGround.distance <= 0.04f;
        }
        else if(!canCheckBasicGround && canCheckSemiGround)
        {
            CheckGroundThroughable(out detectedGround, out isDetectedGround, fPos, 0.5f);
            isHitGround = isDetectedGround && detectedGround.distance <= 0.04f;
        }
        else
        {
            detectedGround = default(RaycastHit2D);
            isDetectedGround = false;
            isHitGround = false;
        }
    }

    protected void CheckCeil()
    {
        if(canCheckCeil)
        {
            CheckCeil(out detectedCeil, out isHitCeil, hPos, 0.04f);
        }
        else
        {
            detectedCeil = default(RaycastHit2D);
            isHitCeil = false;
        }
    }

    protected void CheckWall()
    {
        Vector2 fsPos = Vector2.zero;
        Vector2 hsPos = Vector2.zero;

        if(lookDir == 1)
        {
            fsPos = rbPos;
            hsPos = rtPos;
        }
        else if(lookDir == -1)
        {
            fsPos = lbPos;
            hsPos = ltPos;
        }

        float detectLength = 0.04f;

        base.CheckWall(out detectedWallB, out isHitWallB, fsPos, detectLength, lookDir);
        base.CheckWall(out detectedWallT, out isHitWallT, hsPos, detectLength, lookDir);
    }

    protected void CheckHeadOverSemiGround()
    {
        headOverSemiGroundBefore = headOverSemiGroundCurrent;

        if(!canCheckHeadOverSemiGround)
        {
            headOverSemiGroundCurrent = default(RaycastHit2D);

            canCheckBasicGround = true;
            canCheckSemiGround = true;
            canCheckHangingBasic = true;
            canCheckHangingSemi = true;
        }
        else
        {
            float detectLength = base.height + 0.5f;
            int layer = LayerInfo.throughableGroundMask;

            headOverSemiGroundCurrent = Physics2D.Raycast(fPos, Vector2.up, detectLength, layer);

            if(headOverSemiGroundBefore)
            {
                if(!headOverSemiGroundCurrent)
                {
                    AcceptCollision(headOverSemiGroundBefore.collider);
                }
                else if(headOverSemiGroundBefore.collider != headOverSemiGroundCurrent.collider)
                {
                    AcceptCollision(headOverSemiGroundBefore.collider);
                    IgnoreCollision(headOverSemiGroundCurrent.collider);
                }
            }
            else if(headOverSemiGroundCurrent)
            {
                IgnoreCollision(headOverSemiGroundCurrent.collider);
            }

            if(headOverSemiGroundCurrent)
            {
                canCheckBasicGround = false;
                canCheckSemiGround = false;
                canCheckCeil = false;
                canCheckHangingBasic = true;
                canCheckHangingSemi = false;
            }
            else
            {
                canCheckBasicGround = true;
                canCheckSemiGround = true;
                canCheckCeil = true;
                canCheckHangingBasic = true;
                // canCheckHangingSemi = !currentJumpDownGround;
                canCheckHangingSemi = true;
            }
        }
    }

    protected void CheckLedge()
    {
        if(!canCheckLedge)
        {
            isHitLedgeHead = false;
            isHitLedgeBody = false;
            detectedLedge = default(RaycastHit2D);
            ledgeCornerTopPos.Set(0.0f, 0.0f);
            ledgeCornerSidePos.Set(0.0f, 0.0f);
            return;
        }

        int layer = LayerInfo.groundMask;
        Vector2 sidePos = Vector2.zero;
        Vector2 sideOverPos = Vector2.zero;
        Vector2 bodyPos = Vector2.zero;
        Vector2 detectDir = Vector2.zero;

        if(lookDir == 1)
        {
            sidePos = hrPos;
            sideOverPos.Set(hrPos.x, hrPos.y + ledgeCheckingHeight);
            bodyPos = crPos;
            detectDir.Set(1.0f, 0.0f);
        }
        else if(lookDir == -1)
        {
            sidePos = hlPos;
            sideOverPos.Set(hlPos.x, hlPos.y + ledgeCheckingHeight);
            bodyPos = clPos;
            detectDir.Set(-1.0f, 0.0f);
        }

        RaycastHit2D sideHit = Physics2D.Raycast(sidePos, detectDir, ledgeCheckingWidth, layer);
        RaycastHit2D sideOverHit = Physics2D.Raycast(sideOverPos, detectDir, ledgeCheckingWidth, layer);
        RaycastHit2D bodyHit = Physics2D.Raycast(bodyPos, detectDir, ledgeCheckingWidth, layer);

        if(!sideOverHit)
        {
            if(sideHit)
            {
                isHitLedgeHead = true;
                isHitLedgeBody = false;
            }
            else if(bodyHit)
            {
                isHitLedgeHead = false;
                isHitLedgeBody = true;
            }
            else
            {
                isHitLedgeHead = false;
                isHitLedgeBody = false;
            }
        }
        else
        {
            isHitLedgeHead = false;
            isHitLedgeBody = false;
        }
    }

    protected void CheckLedgeHanging()
    {
        float detectLength = 0.04f;
        int layer = 0;

        if(canCheckHangingBasic)
            layer |= LayerInfo.groundMask;
        if(canCheckHangingSemi)
            layer |= LayerInfo.throughableGroundMask;

        leftHangingGround = Physics2D.Raycast(flPos, Vector2.down, detectLength, layer);
        rightHangingGround = Physics2D.Raycast(frPos, Vector2.down, detectLength, layer);

        isHangingGround = leftHangingGround || rightHangingGround;
    }

    #region Unity Event Functions
    protected override void Start()
    {
        base.Start();
        // this.CheckDataTable(Application.persistentDataPath + "/DataTable.txt");

        // initialPosition = transform.position;

        spRenderer = GetComponent<SpriteRenderer>();

        machine = new StateMachine(stIdleGround);

        machine.SetCallbacks(stIdleGround, Input_IdleGround, Logic_IdleGround, Enter_IdleGround, End_IdleGround);
        machine.SetCallbacks(stIdleGroundLong, Input_IdleGroundLong, Logic_IdleGroundLong, Enter_IdleGroundLong, null);
        machine.SetCallbacks(stSit, Input_Sit, Logic_Sit, Enter_Sit, End_Sit);
        //machine.SetCallbacks(stHeadUp, Input_HeadUp, Logic_HeadUp, Enter_HeadUp, End_HeadUp);
        //machine.SetCallbacks(stWalk, Input_Walk, Logic_Walk, Enter_Walk, null);
        //machine.SetCallbacks(stRun, Input_Run, Logic_Run, Enter_Run, null);
        machine.SetCallbacks(stFreeFall, Input_FreeFall, Logic_FreeFall, Enter_FreeFall, null);
        //machine.SetCallbacks(stGliding, Input_Gliding, Logic_Gliding, Enter_Gliding, null);
        //machine.SetCallbacks(stIdleWall, Input_IdleWall, Logic_IdleWall, Enter_IdleWall, null);
        //machine.SetCallbacks(stWallSliding, Input_WallSliding, Logic_WallSliding, Enter_WallSliding, null);
        //machine.SetCallbacks(stLedgeClimb, Input_LedgeClimb, Logic_LedgeClimb, Enter_LedgeClimb, End_LedgeClimb);
        //machine.SetCallbacks(stJumpGround, Input_JumpGround, Logic_JumpGround, Enter_JumpGround, null);
        machine.SetCallbacks(stJumpDown, Input_JumpDown, Logic_JumpDown, Enter_JumpDown, End_JumpDown);
        //machine.SetCallbacks(stRoll, Input_Roll, Logic_Roll, Enter_Roll, null);
        //machine.SetCallbacks(stJumpAir, Input_JumpOnAir, Logic_JumpOnAir, Enter_JumpOnAir, null);
        //machine.SetCallbacks(stDash, Input_Dash, Logic_Dash, Enter_Dash, null);
        //machine.SetCallbacks(stTakeDown, Input_TakeDown, Logic_TakeDown, Enter_TakeDown, null);
        //machine.SetCallbacks(stJumpWall, Input_JumpWall, Logic_JumpWall, Enter_JumpWall, null);

        InitGraphs();
    }

    public void InitGraphs()
    {
        freeFallGraph = new DiscreteLinearGraph(freeFallFrame);
        glidingAccelGraphX = new DiscreteLinearGraph(glidingAccelFrameX);
        glidingDeaccelGraphX = new DiscreteLinearGraph(glidingDeaccelFrameX);
        wallSlidingGraph = new DiscreteLinearGraph(wallSlidingFrame);
        jumpGroundGraph = new DiscreteLinearGraph(jumpGroundFrame);
        jumpDownGraph = new DiscreteLinearGraph(jumpDownFrame);
        rollGraph = new DiscreteLinearGraph(rollStartFrame + rollInvincibilityFrame + rollWakeUpFrame);
        jumpAirGraph = new DiscreteLinearGraph(jumpAirFrame);
        dashGraph = new DiscreteLinearGraph(dashInvincibilityFrame);
        jumpWallGraphX = new DiscreteLinearGraph(jumpWallFrame);
        jumpWallGraphY = new DiscreteLinearGraph(jumpWallFrame);
    }

    protected override void Update()
    {
        base.Update();

        inputData.Copy(InputHandler.data);

        machine.UpdateInput();

        UnityEngine.Debug.Log(string.Format("current state: {0}", machine.state));
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        m_UpdatePositions();

        CheckGrounds();
        CheckCeil();
        CheckWall();
        CheckHeadOverSemiGround();
        curCol = headOverSemiGroundCurrent.collider;
        befCol = headOverSemiGroundBefore.collider;
        CheckLedge();
        CheckLedgeHanging();

        UpdateLookDir();

        machine.UpdateLogic();
    }
    #endregion

    #region Implement State; stIdleGround
    private void Enter_IdleGround()
    {
        DisableGravity();

        proceedIdleGroundFrame = 0;

        leftJumpGroundCount = jumpGroundCount;
        leftJumpAirCount = jumpAirCount;
        leftDashCount = dashCount;

        // 선입력 확인
        uint mask_jumpGround = 0b01;
        uint mask_roll = 0b10;
        preInputPressing = 0;
        preInputDown = 0;

        for(int i = 0; i < preInputFrameIdleGround; i++)
        {
            preInputData.Copy(InputHandler.data);

            if((preInputPressing & mask_jumpGround) == 0 && preInputData.jumpPressing)
                preInputPressing |= mask_jumpGround;
            if((preInputDown & mask_jumpGround) == 0 && preInputData.jumpDown)
                preInputDown |= mask_jumpGround;
            if((preInputPressing & mask_roll) == 0 && preInputData.dashPressing)
                preInputPressing |= mask_roll;
            if((preInputDown & mask_roll) == 0 && preInputData.dashDown)
                preInputDown |= mask_roll;
        }

        if((preInputDown & mask_roll) != 0 && (preInputPressing & mask_roll) != 0)
            machine.ChangeState(stRoll);
        else if((preInputDown & mask_jumpGround) != 0 && (preInputPressing & mask_jumpGround) != 0 && leftJumpGroundCount > 0)
            machine.ChangeState(stJumpGround);
    }

    private void Input_IdleGround()
    {
        if(!isHitGround)
            machine.ChangeState(stFreeFall);
        else if(inputData.jumpDown)
            machine.ChangeState(stJumpGround);
        else if(inputData.dashDown)
            machine.ChangeState(stRoll);
        else if(inputData.yNegative != 0)
            machine.ChangeState(stSit);
        else if(inputData.yPositive != 0)
            machine.ChangeState(stHeadUp);
        else if(inputData.xInput != 0)
            machine.ChangeState(isRun ? stRun : stWalk);
        else if(proceedIdleGroundFrame >= idleLongTransitionFrame)
            machine.ChangeState(stIdleGroundLong);
    }

    private void Logic_IdleGround()
    {
        proceedIdleGroundFrame++;
        SetVelocityXY(0.0f, 0.0f);
    }

    private void End_IdleGround()
    {
        proceedIdleGroundFrame = 0;
    }
    #endregion

    #region Implement State; stIdleGroundLong
    private void Enter_IdleGroundLong()
    {
        DisableGravity();
    }

    private void Input_IdleGroundLong()
    {
        if(!isHitGround)
            machine.ChangeState(stFreeFall);
        else if(inputData.jumpDown)
            machine.ChangeState(stJumpGround);
        else if(inputData.dashDown)
            machine.ChangeState(stRoll);
        else if(inputData.yNegative != 0)
            machine.ChangeState(stSit);
        else if(inputData.yPositive != 0)
            machine.ChangeState(stHeadUp);
        else if(inputData.xInput != 0)
            machine.ChangeState(isRun ? stRun : stWalk);
    }

    private void Logic_IdleGroundLong()
    {
        SetVelocityXY(0.0f, 0.0f);
    }
    #endregion

    #region Implement State; stSit
    private void Enter_Sit()
    {
        DisableGravity();
        proceedSitFrame = 0;
    }

    private void Input_Sit()
    {
        if(!isHitGround)
            machine.ChangeState(stFreeFall);
        else if(inputData.jumpDown)
        {
            if(detectedGround.collider.gameObject.layer == LayerInfo.throughableGround)
                machine.ChangeState(stJumpDown);
            else
                machine.ChangeState(stJumpGround);
        }
        else if(inputData.dashDown)
            machine.ChangeState(stRoll);
        else if(inputData.yNegative == 0)
            machine.ChangeState(stIdleGround);
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
        uint mask_jumpAir = 0b01;
        preInputPressing = 0b00;
        preInputDown = 0b00;

        for(int i = 0; i < preInputFrameFreeFall; i++)
        {
            preInputData.Copy(InputHandler.data);

            if((preInputPressing & mask_jumpAir) == 0 && preInputData.jumpPressing)
                preInputPressing |= mask_jumpAir;
            if((preInputDown & mask_jumpAir) == 0 && preInputData.jumpDown)
                preInputDown |= mask_jumpAir;
        }

        if((preInputDown & mask_jumpAir) != 0 && (preInputPressing & mask_jumpAir) != 0 && leftJumpAirCount > 0)
            machine.ChangeState(stJumpAir);
    }

    private void Input_FreeFall()
    {
        if(isHitGround)
            machine.ChangeState(stIdleGround);
        else if(inputData.jumpDown)
        {
            if(inputData.yNegative != 0)
                machine.ChangeState(stTakeDown);
            else if(leftJumpAirCount > 0)
                machine.ChangeState((stJumpAir));
        }
        else if(inputData.dashDown && leftDashCount > 0)
            machine.ChangeState(stDash);
        else if(inputData.yPositive != 0)
            machine.ChangeState(stGliding);

        // TODO: 렛지 코드 추가 요망
    }

    private void Logic_FreeFall()
    {
        if(isHangingGround)
            proceedFreeFallFrame = 0;
        else
        {
            if(proceedFreeFallFrame < freeFallFrame)
                proceedFreeFallFrame++;

            vx = CheckVelocityX(GetMoveSpeed() * inputData.xInput);
            vy = -maxFreeFallSpeed * freeFallGraph[proceedFreeFallFrame - 1];
            SetVelocityXY(vx, vy);
        }
    }
    #endregion

    #region Implement State; stJumpDown
    private void Enter_JumpDown()
    {
        EnableGravity();

        currentJumpDownGround = detectedGround;
        leftJumpDownFrame = jumpDownFrame;

        IgnoreCollision(currentJumpDownGround.collider);
        curJumpDownCol = currentJumpDownGround.collider;
    }
public Collider2D curCol;
public Collider2D befCol;
public Collider2D curJumpDownCol;
    private void Input_JumpDown()
    {
        if(headOverSemiGroundCurrent && headOverSemiGroundCurrent.collider == currentJumpDownGround.collider && headOverSemiGroundCurrent.distance >= 0.1f)
            machine.ChangeState(stFreeFall);
    }

    private void Logic_JumpDown()
    {
        if(leftJumpDownFrame > 0)
        {
            leftJumpDownFrame--;
            vx = GetMoveSpeed() * inputData.xInput;
            vy = jumpDownSpeed * jumpDownGraph[leftJumpDownFrame];
            SetVelocityXY(vx, vy);
        }
    }

    private void End_JumpDown()
    {
        currentJumpDownGround = default(RaycastHit2D);
    }
    #endregion
}