using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Lives : MonoBehaviour
{
    #region Components
    public BoxCollider2D feetBox;
    public BoxCollider2D headBox;
    public BoxCollider2D bodyBox;
    protected Rigidbody2D rigid { get; private set; }

    public float entityHeight
    {
        get
        {
            return headBox.bounds.max.y - feetBox.bounds.min.y;
        }
    }

    public float entityWidth
    {
        get
        {
            float feet = feetBox.bounds.extents.x;
            float head = feetBox.bounds.extents.x;
            
            return feet > head ? 2.0f * feet : 2.0f * head;
        }
    }
    #endregion

    #region Entity Constants
    #endregion

    #region Entity Variables
    // Velocity options
    protected Vector2 currentVelocity;
    private Vector2 tempVelocity;
    #endregion

    #region Initializer
    protected virtual void InitComponents()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    protected virtual void InitGraphs()
    {

    }

    protected virtual void InitPhysics()
    {

    }
    #endregion

    #region Unity Event Functions
    protected virtual void Start()
    {
        InitComponents();
        InitGraphs();
        InitPhysics();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }
    #endregion

    #region Physics Utilities
    // Update positions
    protected virtual void GetFeetPosition(ref Vector2 vector, BoxCollider2D feetCollider)
    {
        vector.Set(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
    }

    protected virtual void GetHeadPosition(ref Vector2 vector, BoxCollider2D headCollider)
    {
        vector.Set(headCollider.bounds.center.x, headCollider.bounds.max.y);
    }

    protected virtual void GetFeetSidePosition(ref Vector2 vector, BoxCollider2D feetCollider, int lookDir)
    {
        float x = feetCollider.bounds.center.x + feetCollider.bounds.extents.x * lookDir;
        vector.Set(x, feetCollider.bounds.center.y);
    }

    protected virtual void GetHeadSidePosition(ref Vector2 vector, BoxCollider2D headCollider, int lookDir)
    {
        float x = headCollider.bounds.center.x + headCollider.bounds.extents.x * lookDir;
        vector.Set(x, headCollider.bounds.center.y);
    }

    // Change gravity
    protected void EnableGravity() => rigid.gravityScale = 1.0f;
    protected void DisableGravity() => rigid.gravityScale = 0.0f;

    // Change velocity
    protected void SetVelocityX(float x)
    {
        tempVelocity.Set(x, currentVelocity.y);
        SetVelocityFinal();
    }

    protected void SetVelocityY(float y)
    {
        tempVelocity.Set(currentVelocity.x, y);
        SetVelocityFinal();
    }

    protected void SetVelocity(float x, float y)
    {
        tempVelocity.Set(x, y);
        SetVelocityFinal();
    }

    private void SetVelocityFinal()
    {
        currentVelocity = tempVelocity;
        rigid.velocity = currentVelocity;
    }
    #endregion

    // Change Collision State
    protected void AcceptCollision(Collider2D collider)
    {
        Physics2D.IgnoreCollision(feetBox, collider, false);
        Physics2D.IgnoreCollision(headBox, collider, false);
        Physics2D.IgnoreCollision(bodyBox, collider, false);
    }

    protected void IgnoreCollision(Collider2D collider)
    {
        Physics2D.IgnoreCollision(feetBox, collider, true);
        Physics2D.IgnoreCollision(headBox, collider, true);
        Physics2D.IgnoreCollision(bodyBox, collider, true);
    }

    #region Physics Checker
    // Checker
    protected void CheckGround(out RaycastHit2D detectedGround, out bool hit, bool canCheck, Vector2 feetPosition, float detectLength, float hitLength)
    {
        if(!canCheck)
        {
            detectedGround = default(RaycastHit2D);
            hit = false;
            return;
        }

        int layer = LayerInfo.groundMask | LayerInfo.throughableGroundMask;

        detectedGround = Physics2D.Raycast(feetPosition, Vector2.down, detectLength, layer);
        hit = detectedGround && detectedGround.distance <= hitLength;
    }

    protected void CheckCeil(out RaycastHit2D detectedCeil, out bool hit, bool canCheck, Vector2 headPosition, float detectLength, float hitLength)
    {
        if(!canCheck)
        {
            detectedCeil = default(RaycastHit2D);
            hit = false;
            return;
        }

        int layer = LayerInfo.groundMask;

        detectedCeil = Physics2D.Raycast(headPosition, Vector2.up, detectLength, layer);
        hit = detectedCeil && detectedCeil.distance <= hitLength;
    }

    protected void CheckWall(out RaycastHit2D detectedWall, out int hit, bool canCheck, Vector2 sidePosition, float detectLength, float hitLength, int lookDir)
    {
        if(!canCheck)
        {
            detectedWall = default(RaycastHit2D);
            hit = 0;
            return;
        }

        int layer = LayerInfo.groundMask;

        detectedWall = Physics2D.Raycast(sidePosition, Vector2.right * lookDir, detectLength, layer);
        hit = detectedWall && detectedWall.distance <= hitLength ? lookDir : 0;
    }

    protected int CheckLookingDirection(int xNegative, int xPositive, int currentLookDir)
    {
        int xInput = xNegative + xPositive;

        if(xInput == 0)
            return currentLookDir;
        else
            return xInput;
    }
    #endregion

    #region Physics Logics
    protected void Logic_IdleX()
    {
        SetVelocityX(0.0f);
    }

    protected void Logic_IdleY()
    {
        SetVelocityY(0.0f);
    }

    protected void Logic_IdleXY()
    {
        SetVelocity(0.0f, 0.0f);
    }

    protected void Logic_MoveOnGround(Vector2 moveDirection, float speed, int lookDir)
    {
        float dv = speed * lookDir;
        float wx = 1.0f;
        float wy = moveDirection.y / moveDirection.x;

        SetVelocity(dv * wx, dv * wy);
    }

    protected void Logic_MoveOnGround(Vector2 moveDirection, float speed, int lookDir, DiscreteGraph graph, int currentFrame)
    {
        Logic_MoveOnGround(moveDirection, speed * graph[currentFrame], lookDir);
    }

    protected void Logic_MoveOnAirX(float speed_x, int lookDir)
    {
        SetVelocityX(speed_x * lookDir);
    }

    protected void Logic_MoveOnAirX(float speed_x, int lookDir, DiscreteGraph graph_x, int currentFrame_x)
    {
        Logic_MoveOnAirX(speed_x * graph_x[currentFrame_x], lookDir);
    }

    protected void Logic_MoveOnAirY(float speed_y)
    {
        SetVelocityY(speed_y);
    }

    protected void Logic_MoveOnAirY(float speed_y, DiscreteGraph graph_y, int currentFrame_y)
    {
        Logic_MoveOnAirY(speed_y * graph_y[currentFrame_y]);
    }

    protected void Logic_MoveOnAirXY(float speed_x, float speed_y, int lookDir)
    {
        SetVelocity(speed_x * lookDir, speed_y);
    }

    protected void Logic_MoveOnAirXY(float speed_x, float speed_y, int lookDir, DiscreteGraph graph_x, int currentFrame_x, DiscreteGraph graph_y, int currentFrame_y)
    {
        Logic_MoveOnAirXY(speed_x * graph_x[currentFrame_x], speed_y * graph_y[currentFrame_y], lookDir);
    }
    #endregion

    #region State Transition Checkers
    #endregion
}