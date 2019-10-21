using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColor : MonoBehaviour
{
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public Color color5;
    public Color color6;

    public int Stage1;
    public int Stage2;
    public int Stage3;
    public int Stage4;
    public int Stage5;
    public int Stage6;

    public float time = 0;
    public float speedMult = 1;
    public bool gameStarted;

    void Update()
    {
        if (!gameStarted)
        {
            GetComponent<Camera>().backgroundColor = color1;
            return;
        }
            

        time += Time.deltaTime;

        if (time >= Stage1 && time <= Stage2)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color1, 0.05f * Time.deltaTime * speedMult);
        } else if (time > Stage2 && time <= Stage3)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color2, 0.05f * Time.deltaTime * speedMult);
        }
        else if (time > Stage3 && time <= Stage4)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color3, 0.05f * Time.deltaTime * speedMult);
        }
        else if (time > Stage4 && time <= Stage5)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color4, 0.05f * Time.deltaTime * speedMult);
        }
        else if (time > Stage5 && time <= Stage6)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color5, 0.05f * Time.deltaTime * speedMult);
        }
        else if (time > Stage6 && time <= Stage6 + 20)
        {
            GetComponent<Camera>().backgroundColor = Color.Lerp(GetComponent<Camera>().backgroundColor, color6, 0.05f * Time.deltaTime * speedMult);
        } else
        {
            time = 0;
        }
    }
}
