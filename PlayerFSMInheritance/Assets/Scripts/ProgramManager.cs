using UnityEngine;

public class ProgramManager : MonoBehaviour
{
    void Update()
    {
        if(UnityWinAPI.hasFatalError)
        {
            Application.Quit();
        }
    }
}