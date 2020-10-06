using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveStrip : MonoBehaviour
{
    GameObject player;
    private GameObject playerHand;

    private void Awake()
    {
        player = PlayerManager.Inst.player;

        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(player.transform);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == "OVRControllerPrefab")
                playerHand = c.gameObject;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }

        Debug.Log("PLAYER HAND::: " + playerHand.name);
    }

    private void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerHand.transform.position, playerHand.transform.forward, out hit))
        {
            if (hit.collider.name == "Wall")
            {
                Vector3 pos = new Vector3(
                    transform.position.x,
                    //hit.point.normalized.y * hit.distance,
                    hit.point.y,
                    transform.position.z);

                transform.position = pos;
            }

            if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P))
            {
                ButtonManager.Inst.userChoice = transform.position.y.ToString();
                ButtonManager.Inst.clicked = true;
            }
        }
    }
}
