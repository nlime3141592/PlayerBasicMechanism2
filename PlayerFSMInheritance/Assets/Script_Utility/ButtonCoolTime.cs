using UnityEngine;
using UnityEngine.UI;

public class ButtonCoolTime : MonoBehaviour
{
    public int coolFrame = 120;
    private int leftCoolFrame = 0;

    public Text frameViewer;
    private Button bt;

    void Start()
    {
        bt = GetComponent<Button>();
    }

    void Update()
    {
        bt.interactable = leftCoolFrame <= 0;

        if(leftCoolFrame > 0)
        {
            frameViewer.text = string.Format("CoolTime: {0}", leftCoolFrame.ToString());
        }
        else
        {
            frameViewer.text = string.Empty;
        }
    }

    void FixedUpdate()
    {
        if(leftCoolFrame > 0)
            leftCoolFrame--;
    }

    public void OnClickButton()
    {
        leftCoolFrame = coolFrame;
    }
}