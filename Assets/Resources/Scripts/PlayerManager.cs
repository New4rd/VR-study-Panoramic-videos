using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;

    static private PlayerManager inst;

    static public PlayerManager Inst
    {
        get
        {
            return inst;
        }
    }

    private void Awake()
    {
        {
            inst = this;
        }
    }

    public bool PlayerLooksInRightDirection (int threshold)
    {
        return (    90 - threshold < player.transform.rotation.eulerAngles.y &&
                    player.transform.rotation.eulerAngles.y < 90 + threshold &&
                    -threshold < player.transform.rotation.eulerAngles.x &&
                    player.transform.rotation.eulerAngles.x < threshold);
    }
}