using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Treasure : MapObjectBase
{
    public enum Type
    {
        LifeUp,
        FoodUp,
        Weapon,
    }
    public Type CurrentType = Type.LifeUp;
    public int Value = 5;
}
