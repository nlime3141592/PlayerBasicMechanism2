using System;
using System.IO;
using System.Text;
using System.Threading;

public static class PlayerExtensions
{
    public static void CheckDataTable(this Player player, string path)
    {
        if(File.Exists(path))
            return;

        Action createFile = () =>
        {
            using(FileStream stream = new FileStream(path, FileMode.Create))
            using(StreamWriter writer = new StreamWriter(stream))
            {
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

jumpGroundCount: 1
jumpGroundSpeed: 5.5
jumpGroundFrame: 18

jumpDownSpeed: 1.5
jumpDownFrame: 13

rollSpeed: 9.5
rollStartFrame: 6
rollInvincibilityFrame: 18
rollWakeUpFrame: 6

jumpAirCount: 1
jumpAirSpeed: 7.5
jumpAirIdleFrame: 3
jumpAirFrame: 20

dashCount: 1
dashSpeed: 36
dashIdleFrame: 6
dashInvincibilityFrame: 9

takeDownSpeed: 48
takeDownAirIdleFrame: 18
takeDownLandingIdleFrame: 12

jumpWallSpeedX: 7
jumpWallSpeedY: 10
jumpWallFrame: 13
jumpWallForceFrame: 6"
                );
            }
        };

        Thread createThread = new Thread(new ThreadStart(createFile));

        try
        {
            createThread.Start();
        }
        catch(Exception)
        {
            UnityWinAPI.Exit();
        }
    }

    public static void LoadDataTable(this Player player, string path)
    {
        using(FileStream stream = new FileStream(path, FileMode.Open))
        using(StreamReader reader = new StreamReader(stream))
        {
            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine().Split('#')[0].Replace(" ", "");

                if(String.IsNullOrEmpty(line))
                    continue;

                string[] token = line.Split(':');

                try
                {
                    player.SwitchFileData(token[0], token[1]);
                }
                catch(Exception)
                {
                    UnityWinAPI.Exit();
                }
            }
        }

        player.InitGraphs();
    }

    private static void SwitchFileData(this Player player, string tok_name, string tok_value)
    {
        switch(tok_name)
        {
            case "isRun":
                player.isRun = bool.Parse(tok_value);
                break;
            case "longIdleTransitionFrame":
                player.idleLongTransitionFrame = int.Parse(tok_value);
                break;
            case "walkSpeed":
                player.walkSpeed = float.Parse(tok_value);
                break;
            case "runSpeed":
                player.runSpeed = float.Parse(tok_value);
                break;
            case "maxFreeFallSpeed":
                player.maxFreeFallSpeed = float.Parse(tok_value);
                break;
            case "freeFallFrame":
                player.freeFallFrame = int.Parse(tok_value);
                break;
            case "glidingSpeed":
                player.glidingSpeed = float.Parse(tok_value);
                break;
            case "glidingAccelFrameX":
                player.glidingAccelFrameX = int.Parse(tok_value);
                break;
            case "glidingDeaccelFrameX":
                player.glidingDeaccelFrameX = int.Parse(tok_value);
                break;
            case "maxWallSlidingSpeed":
                player.maxWallSlidingSpeed = float.Parse(tok_value);
                break;
            case "wallSlidingFrame":
                player.wallSlidingFrame = int.Parse(tok_value);
                break;
            case "jumpGroundCount":
                player.jumpGroundCount = int.Parse(tok_value);
                break;
            case "jumpGroundSpeed":
                player.jumpGroundSpeed = float.Parse(tok_value);
                break;
            case "jumpGroundFrame":
                player.jumpGroundFrame = int.Parse(tok_value);
                break;
            case "jumpDownSpeed":
                player.jumpDownSpeed = float.Parse(tok_value);
                break;
            case "jumpDownFrame":
                player.jumpDownFrame = int.Parse(tok_value);
                break;
            case "rollSpeed":
                player.rollSpeed = float.Parse(tok_value);
                break;
            case "rollStartFrame":
                player.rollStartFrame = int.Parse(tok_value);
                break;
            case "rollInvincibilityFrame":
                player.rollInvincibilityFrame = int.Parse(tok_value);
                break;
            case "rollWakeUpFrame":
                player.rollWakeUpFrame = int.Parse(tok_value);
                break;
            case "jumpAirCount":
                player.jumpAirCount = int.Parse(tok_value);
                break;
            case "jumpAirSpeed":
                player.jumpAirSpeed = float.Parse(tok_value);
                break;
            case "jumpAirIdleFrame":
                player.jumpAirIdleFrame = int.Parse(tok_value);
                break;
            case "jumpAirFrame":
                player.jumpAirFrame = int.Parse(tok_value);
                break;
            case "dashCount":
                player.dashCount = int.Parse(tok_value);
                break;
            case "dashSpeed":
                player.dashSpeed = float.Parse(tok_value);
                break;
            case "dashIdleFrame":
                player.dashIdleFrame = int.Parse(tok_value);
                break;
            case "dashInvincibilityFrame":
                player.dashInvincibilityFrame = int.Parse(tok_value);
                break;
            case "takeDownSpeed":
                player.takeDownSpeed = float.Parse(tok_value);
                break;
            case "takeDownAirIdleFrame":
                player.takeDownAirIdleFrame = int.Parse(tok_value);
                break;
            case "takeDownLandingIdleFrame":
                player.takeDownLandingIdleFrame = int.Parse(tok_value);
                break;
            case "jumpWallSpeedX":
                player.jumpWallSpeedX = float.Parse(tok_value);
                break;
            case "jumpWallSpeedY":
                player.jumpWallSpeedY = float.Parse(tok_value);
                break;
            case "jumpWallFrame":
                player.jumpWallFrame = int.Parse(tok_value);
                break;
            case "jumpWallForceFrame":
                player.jumpWallForceFrame = int.Parse(tok_value);
                break;
        }
    }
}