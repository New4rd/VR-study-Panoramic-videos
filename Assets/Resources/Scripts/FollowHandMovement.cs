using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowHandMovement : MonoBehaviour
{

    private GameObject hand;

    private void Start()
    {
        hand = PlayerManager.Inst.player.transform.
            Find("OVRCameraRig/TrackingSpace/RightHandAnchor").gameObject;
    }


    private void Update()
    {
        transform.position = hand.transform.position + new Vector3(0, .1f, 0);
    }
}
