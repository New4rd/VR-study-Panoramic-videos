using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    public void ButtonNumberSelection()
    {
        ButtonManager.Inst.UserChoice = name;
        Debug.Log("YOU CHOOSED::: " + name);
        ButtonManager.Inst.clicked = true;
    }
}
