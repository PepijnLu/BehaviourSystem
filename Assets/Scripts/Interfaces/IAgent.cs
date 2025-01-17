using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IAgent
{
    Weapon GetWeapon();
    TextMeshProUGUI GetDisplayText();
    Transform GetTransform();
    void SetCurrentActiveLeaf(string leafName);
    void SetIsStunned();
    void DropWeapon();
}
