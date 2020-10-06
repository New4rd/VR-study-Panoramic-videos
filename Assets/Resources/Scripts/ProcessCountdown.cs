using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessCountdown : MonoBehaviour
{

    static private ProcessCountdown inst;

    static public ProcessCountdown Inst
    {
        get
        {
            return inst;
        }
    }

    public bool waitOver = false;


    private void Awake()
    {
        inst = this;
    }


    IEnumerator Start()
    {

        Text text = GetComponent<Text>();

        for (int i = 2; i > 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }
        waitOver = true;
    }
}
