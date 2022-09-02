using System;
using System.IO;
using UnityEngine;

public static class FileCreator
{
    public static void Initialize()
    {
        string path = Application.persistentDataPath + "/DataTable.txt";

        if(!File.Exists(path))
        {
            using(FileStream stream = new FileStream(path, FileMode.Create))
            using(StreamWriter writer = new StreamWriter(stream))
            {

                writer.Write(
@"isRun: false
longIdleTransitionFrame: 900

walkSpeed: 3.5
runSpeed: 7

maxFreeFallSpeed: 12.0
freeFallFrame: 39

glidingSpeed: 0.05
glidingAccelFrameX: 39
glidingDeaccelFrameX: 26

maxWallSlidingSpeed: 1.5
wallSlidingFrame: 26

jumpOnGroundCount: 1
jumpOnGroundSpeed: 5.5
jumpOnGroundFrame: 18

jumpDownSpeed: 1.5
jumpDownFrame: 13

rollSpeed: 9.5
rollStartFrame: 6
rollInvincibilityFrame: 18
rollWakeUpFrame: 6

jumpOnAirCount: 1
jumpOnAirSpeed: 7.5
jumpOnAirIdleFrame: 3
jumpOnAirFrame: 20

dashCount: 1
dashSpeed: 36
dashIdleFrame: 6
dashInvincibilityFrame: 9

takeDownSpeed: 48
takeDownAirIdleFrame: 18
takeDownLandingIdleFrame: 12

jumpOnWallSpeedX: 7
jumpOnWallSpeedY: 10
jumpOnWallFrame: 13
jumpOnWallForceFrame: 6"
                );
/*
                writer.Write(
@"# 플레이어의 이동 테스트를 위해 다양한 옵션을 이 파일에서 조정할 수 있습니다.
# 이 파일을 저장하고, 프로그램 화면의 데이터테이블 파일 경로에 본 파일을 DataTable.txt (대소문자 구분함)로 저장하세요.
# # 뒤의 모든 글자는 주석 처리됩니다.

# 프로그램을 실행하더라도, Apply DataTable 버튼을 누르지 않으면 프로그램이 가진 자체 초기값을 플레이어의 속성으로 사용합니다.
# DataTable 값을 적용하려면 반드시 Apply DataTable 버튼을 눌러주세요.

# 변수 설명
# isXXX 형태의 변수: boolean, true 또는 false
# XXXFrame, XXXCount 형태의 변수: int, 정수
# XXXSpeed 형태의 변수: float, 실수

# 이상한 값을 넣으면 프로그램이 비정상 동작할 수 있습니다.
# 프로그램을 끄고 값을 다시 확인한 후 프로그램을 재실행하세요.

isRun: false
longIdleTransitionFrame: 900

walkSpeed: 3.5
runSpeed: 7

maxFreeFallSpeed: 12.0
freeFallFrame: 39

glidingSpeed: 0.05
glidingAccelFrameX: 39
glidingDeaccelFrameX: 26

maxWallSlidingSpeed: 1.5
wallSlidingFrame: 26

jumpOnGroundCount: 1
jumpOnGroundSpeed: 5.5
jumpOnGroundFrame: 18

jumpDownSpeed: 1.5
jumpDownFrame: 13

rollSpeed: 9.5
rollStartFrame: 6
rollInvincibilityFrame: 18
rollWakeUpFrame: 6

jumpOnAirCount: 1
jumpOnAirSpeed: 7.5
jumpOnAirIdleFrame: 3
jumpOnAirFrame: 20

dashCount: 1
dashSpeed: 36
dashIdleFrame: 6
dashInvincibilityFrame: 9

takeDownSpeed: 48
takeDownAirIdleFrame: 18
takeDownLandingIdleFrame: 12

jumpOnWallSpeedX: 7
jumpOnWallSpeedY: 10
jumpOnWallFrame: 13
jumpOnWallForceFrame: 6"
                );
*/
            }
        }
    }
}