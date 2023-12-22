using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Trap : MapObjectBase
{
    public enum Type
    {
        LifeDown,
        FoodDown,
    }

    public Type CurrentType = Type.LifeDown;
    public int Value = 5;
    public bool Hide = true;


    void Start()
    {
        SetHide(Hide);
    }
    public void SetHide(bool doHide)
    {
        Hide = doHide;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(doHide);
        }
    }
}