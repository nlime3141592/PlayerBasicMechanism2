using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어의 상태 머신
// 본 플레이어는 3가지 상태를 가진다.
//   0: 정지 (Idle)
//   1: 점프 (Jump)
//   2: 체공 (Air)
// 각 상태별로 각기 다른 역할을 수행하는 4개의 함수를 구현할 수 있다.
//   - 상태 시작 시 (Enter_XXX 함수)
//   - 상태 전이 조건을 판단하는 함수 (Input_XXX 함수)
//   - 상태가 가지는 물리 동작을 수행하는 함수 (Logic_XXX 함수)
//   - 상태 종료 시 (End_XXX 함수)
public class _Player : _Entity
{
    public Rigidbody2D rigid;
    public BoxCollider2D box;

    private RaycastHit2D hitGround;
    public bool onGround;

    public int state = 0;

    private StateMachine machine;
    private const int stIdle = 0;
    private const int stJump = 1;
    private const int stAir = 2;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        // StateMachine을 생성하고, 초기 상태를 대입해준다.
        machine = new StateMachine(stIdle);

        // StateMachine이 상태를 인식하고 동작을 수행할 수 있도록
        // 상태 인덱스와 함수를 제공해준다.
        machine.SetCallbacks(stIdle, Input_Idle, null, Enter_Idle, null);
        machine.SetCallbacks(stJump, null, null, Enter_Jump, null);
        machine.SetCallbacks(stAir, Input_Air, null, null, null);
    }

    /////////////////////////////
    // 물리 동작 수행
    void FixedUpdate()
    {
        CheckGround();
        
        machine.UpdateLogic();
    }

    void CheckGround()
    {
        Vector2 start = box.bounds.min + box.bounds.extents.x * Vector3.right;

        hitGround = Physics2D.Raycast(start, Vector2.down, 0.05f, 1 << LayerMask.NameToLayer("Ground"));
        onGround = hitGround;
    }

    /////////////////////////////
    // 상태 전이 조건 탐색
    void Update()
    {
        machine.UpdateInput();
        state = machine.state;
    }

    private void Input_Idle()
    {
        if(InputHandler.data.jumpDown)
        {
            machine.ChangeState(stJump);
        }
        else if(!onGround)
        {
            machine.ChangeState(stAir);
        }
    }

    private void Input_Air()
    {
        // rigid.velocity.y <= 0.0f 조건을 추가한 이유
        // Idle 상태에서 선입력으로 인해 Jump 상태에 바로 진입하는 경우에 문제 소지가 발생했기 때문이다.
        // Enter 함수는 ChangeState 함수가 호출되면 즉시 실행되기 때문에,
        // Enter -> ChangeState -> Enter -> ChangeState -> ... 와 같은 형식으로 단 1프레임 안에 연쇄적으로 호출될 여지가 있다.
        // 본 예시에서는, Idle에서의 선입력 조건에 의해 Air(1) -> Idle(1) -> Jump -> Air(2) -> Idle(2) 순으로 연쇄적으로 호출된다.
        // Enter_Jump() 함수에서 rigid.velocity.y를 10.0f로 설정하기 때문에, 본 조건문이 이 곳에 존재한다면, Air(2)에서 Idle(2)로 바로 넘어가는 현상을 제거할 수 있다.
        if(onGround && rigid.velocity.y <= 0.0f)
        {
            machine.ChangeState(stIdle);
        }
    }

    // 선입력이 어떻게 구현되어 있는지 볼 수 있다.
    // InputBuffer 클래스는 최근 60(물리)프레임동안의 입력 정보를 가질 수 있으므로,
    // for 반복문의 인덱스 증가 기능을 이용해 순차적으로 이전 프레임의 입력 정보를 가져올 수 있도록 설계했다.
    // 선입력은 일반적으로 짧은 순간에 다른 상태로 전이해야 하기 때문에 Enter_XXX 함수에서 수행하는 것이 바람직하다고 판단했다.
    // 짧은 순간에 다른 상태로 전이하는 다른 테크닉에 대해서는 Enter_Jump() 함수의 주석을 참조.
    void Enter_Idle()
    {
        int i;
        int frame = 6; // 선입력을 받아들일 수 있는 최대 프레임 수를 for문의 종료 조건으로 제공한다.

        // for(초기값; 종료조건; 변화량)
        for(i = 0; i < frame; i++)
        {
            InputData data = InputBuffer.GetBufferedData(i);

            if(data.jumpPressing)
            {
                machine.ChangeState(stJump);
                return;
            }
        }
    }

    /////////////////////////////
    // 기타 상태 머신에 들어가는 기능
    void Enter_Jump()
    {
        // 어떤 기능을 단 1프레임만 수행하고 다른 상태로 넘어가거나 1프레임 내에 여러 단계의 상태 전이를 구현할 때 사용할 수 있는 테크닉이다.
        // 상태 진입 시(Enter_XXX 함수) 다른 상태로 넘어가는 명령어를 넣어주면, 상태 진입 시 함수를 연쇄적으로 호출할 수 있다.
        // 본 예시에서는, Idle -> Jump -> Air 상태 전이가 단 1프레임 안에 수행되는 예시를 보여준다.
        // Idle 상태에서의 상태 전이에 대한 자세한 내용은 Input_Idle() 함수를 참조.
        rigid.velocity = new Vector2(rigid.velocity.x, 10.0f);
        machine.ChangeState(stAir);
    }
}
