using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wepon")]
class Weapon : ScriptableObject
{
    public string Name = "";
    public int Attack = 1;

    //•Ší‚Ì‘•”õ
    public void Attach(MapObjectBase obj)
    {
        obj.Attack += Attack;
    }

    //•Ší‚Ì‰ğœ
    public void Detach(MapObjectBase obj)
    {
        obj.Attack -= Attack;
    }

    public Weapon Merge(Weapon other)
    {
        var newWeapon = ScriptableObject.CreateInstance<Weapon>();
        newWeapon.Name = Name;
        newWeapon.Attack = Attack;
        if (other != null) newWeapon.Attack = other.Attack;

        return newWeapon;
    }

    public override string ToString()
    {
        return $"{Attack}";
    }
}
