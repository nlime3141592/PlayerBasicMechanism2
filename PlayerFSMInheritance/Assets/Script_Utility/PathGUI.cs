using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PathGUI : MonoBehaviour
{
    private Text comp;
    
    void Start()
    {
        comp = GetComponent<Text>();
        comp.text = string.Format("데이터테이블 파일 경로:\n{0}", Application.persistentDataPath);
    }
}
