using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDetection : MonoBehaviour
{
    RaycastHit hit;

    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            GameObject button = hit.collider.gameObject;
            Button buttonComp = button.GetComponent<Button>();

            if (buttonComp != null)
            {
                EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(button);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                buttonComp.onClick.Invoke();
            }
        }
    }
}