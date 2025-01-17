using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IAgent
{
    TextMeshProUGUI getDisplayText();
    void setCurrentActiveLeaf(string leafName);
    Transform getTransform();
    void setIsStunned();
    void DropWeapon();
    Weapon GetWeapon();
}
