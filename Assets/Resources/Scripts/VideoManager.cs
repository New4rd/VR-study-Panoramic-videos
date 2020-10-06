using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    static private VideoManager inst;

    static public VideoManager Inst
    {
        get
        {
            return inst;
        }
    }

    public bool videoDone;

    private void Awake()
    {
        inst = this;
        videoDone = false;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
        videoDone = true;
    }
}
