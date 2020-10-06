using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{

    static private ButtonManager inst;

    static public ButtonManager Inst
    {
        get
        {
            return inst;
        }
    }

   public string userChoice;

   public string UserChoice
    {
        get
        {
            return userChoice;
        }

        set
        {
            userChoice = value;
        }
    }

    public bool clicked;

    //public GameObject canvas;

    private void Awake()
    {
        inst = this;
        clicked = false;
    }
}
