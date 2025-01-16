using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IAgent
{
    TextMeshProUGUI getDisplayText();

    //string getCurrentActiveLeaf();
    void setCurrentActiveLeaf(string leafName);
    void setIsInDanger(bool _isInDanger);
    Transform getTransform();
}
