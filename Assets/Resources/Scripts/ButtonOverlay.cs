using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOverlay : MonoBehaviour
{
    private void Awake()
    {

        GameObject player = PlayerManager.Inst.player;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = player.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Camera>();
    }
}
