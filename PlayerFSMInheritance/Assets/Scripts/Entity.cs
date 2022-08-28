using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    #region Entity Constants

    [Header("Entity Constants")]
    public float moveSpeed = 5.5f;
    public float maxFreeFallSpeed = 7.0f;

    #endregion

    #region Entity Variables

    protected Vector2 currentVelocity;
    protected Vector2 velocityWorkspace;

    protected float groundDetectingLength = 0.5f;
    protected float groundEntityDetectingLength = 0.04f;
    protected RaycastHit2D detectedGround;
    protected bool isDetectedGround;
    protected bool isOnGround;
    protected bool isDetectedCeil;
    protected bool isOnCeil;
    protected Vector2 moveDirection;
    protected int lookingDirection;

    protected float wallDetectingLength = 0.3f;
    protected float wallEntityDetectingLength = 0.1f;
    protected RaycastHit2D detectedWallFeet;
    protected RaycastHit2D detectedWallCeil;
    public bool isDetectedWallFeet;
    public bool isDetectedWallCeil;
    public int isOnWallFeet;
    public int isOnWallCeil;

    protected Rigidbody2D rigid { get; private set; }
    protected BoxCollider2D feetBox { get; private set; }
    protected BoxCollider2D ceilBox { get; private set; }
    protected Vector2 feetPosition;
    protected Vector2 ceilPosition;
    protected Vector2 wallPositionFeet;
    protected Vector2 wallPositionCeil;

    #endregion

    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody2D>();

        m_SetFeetBox();
        m_SetCeilBox();
    }

    #region Initializer

    private void m_SetFeetBox()
    {
        GameObject boxObj = GameObject.Find("FeetBox");

        if(boxObj != null && boxObj.transform.parent == transform)
            feetBox = boxObj.GetComponent<BoxCollider2D>();
    }

    private void m_SetCeilBox()
    {
        GameObject ceilObj = GameObject.Find("CeilBox");

        if(ceilObj != null && ceilObj.transform.parent == transform)
            ceilBox = ceilObj.GetComponent<BoxCollider2D>();
    }

    #endregion

    #region Physics Utilities

    protected void SetVectorX(ref Vector2 vector, float x)
    {
        vector.Set(x, vector.y);
    }

    protected void SetVectorY(ref Vector2 vector, float y)
    {
        vector.Set(vector.x, y);
    }

    protected void SetVector(ref Vector2 vector, float x, float y)
    {
        vector.Set(x, y);
    }

    protected void SetVelocityX(float x)
    {
        velocityWorkspace.Set(x, currentVelocity.y);
        SetFinalVelocity();
    }

    protected void SetVelocityY(float y)
    {
        velocityWorkspace.Set(currentVelocity.x, y);
        SetFinalVelocity();
    }

    protected void SetVelocity(float x, float y)
    {
        velocityWorkspace.Set(x, y);
        SetFinalVelocity();
    }

    protected void SetFinalVelocity()
    {
        currentVelocity = velocityWorkspace;
        rigid.velocity = velocityWorkspace;
    }

    #endregion

    #region Physics Checker

    protected void CheckGround()
    {
        float posX = feetBox.bounds.center.x;
        float posY = feetBox.bounds.min.y;

        SetVector(ref feetPosition, posX, posY);

        // 1. Can player detect ground?
        detectedGround = Physics2D.Raycast(feetPosition, Vector2.down, groundDetectingLength, LayerInfo.groundMask);
        isDetectedGround = detectedGround;

        // 2. Does player hit ground?
        RaycastHit2D hitEntity;
        Vector2 dir = Vector2.one;

        if(isDetectedGround)
        {
            hitEntity = Physics2D.Raycast(detectedGround.point, Vector2.up, groundEntityDetectingLength, LayerInfo.entityMask);
            isOnGround = hitEntity;

            dir = Vector2.Perpendicular(-detectedGround.normal).normalized;

            if(!isOnGround)
            {
                rigid.AddForce(Vector2.down * 80.0f, ForceMode2D.Force);
            }
        }
        else
        {
            isOnGround = false;
            dir = Vector2.right;
        }

        SetVector(ref moveDirection, dir.x, dir.y);
    }

    protected void CheckCeil()
    {

    }

    protected void CheckDetectingWall()
    {
        float fPosX = 0.0f;
        float fPosY = 0.0f;
        float cPosX = 0.0f;
        float cPosY = 0.0f;

        fPosX = feetBox.bounds.center.x + feetBox.bounds.extents.x * lookingDirection;
        fPosY = feetBox.bounds.center.y;
        cPosX = ceilBox.bounds.center.x + ceilBox.bounds.extents.x * lookingDirection;
        cPosY = ceilBox.bounds.center.y;

        SetVector(ref wallPositionFeet, fPosX, fPosY);
        SetVector(ref wallPositionCeil, cPosX, cPosY);

        detectedWallFeet = Physics2D.Raycast(wallPositionFeet, Vector2.right * lookingDirection, wallDetectingLength, LayerInfo.groundMask);
        detectedWallCeil = Physics2D.Raycast(wallPositionCeil, Vector2.right * lookingDirection, wallDetectingLength, LayerInfo.groundMask);

        isDetectedWallFeet = detectedWallFeet;
        isDetectedWallCeil = detectedWallCeil;
    }

    protected void CheckHittingWall(RaycastHit2D hitWall, bool isDetectedWall, ref int isOnWall)
    {
        if(isDetectedWall)
        {
            RaycastHit2D hitEntity = Physics2D.Raycast(hitWall.point, Vector2.left * lookingDirection, wallEntityDetectingLength, LayerInfo.entityMask);

            if(hitEntity)
            {
                isOnWall = lookingDirection;
            }
            else
            {
                isOnWall = 0;
            }
        }
        else
        {
            isOnWall = 0;
        }
    }

    #endregion

    #region State Transition Input Checker

    // WARNING:
    // 강제로 바라보는 방향을 설정하기 때문에 플레이어 상태 머신이 엉킬 여지가 있을 수 있음. (현재까지는 문제 상황 발견 안 됨.)
    // 맵 이동 후 플레이어 위치 텔레포트 등 극히 제한적인 경우에만 사용할 것을 권장.
    protected void SetLookingDirectionImmediate(int dir)
    {
        lookingDirection = dir;
    }

    protected void CheckLookingDirection(int xInput)
    {
        if(xInput != 0)
        {
            lookingDirection = xInput;
        }
    }

    #endregion

    #region Execute Object Actions

    protected virtual void FixedUpdate()
    {
        currentVelocity = rigid.velocity;

        if(lookingDirection == 0)
            lookingDirection = 1;

        CheckGround();
        CheckCeil();
        CheckDetectingWall();
        CheckHittingWall(detectedWallFeet, isDetectedWallFeet, ref isOnWallFeet);
        CheckHittingWall(detectedWallCeil, isDetectedWallCeil, ref isOnWallCeil);
    }

    protected void MoveOnGround(float speed, int lookDir)
    {
        float angle = Vector2.Angle(Vector2.right, moveDirection) * Mathf.Deg2Rad;
        float tan = Mathf.Tan(angle);
        if(moveDirection.y < 0) tan = -tan;
        float x = speed * lookDir;
        float y = speed * lookDir * tan;

        SetVelocity(x, y);
    }

    #endregion

    #region State Transition Logics

    protected virtual void Update()
    {

    }

    #endregion
}