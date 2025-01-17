using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttackable
{
    public void SetAttackingAgent(GameObject agent, bool add);
    public GameObject GetAttackingAgent();
    Transform getTransform();
    public void setIsInDanger(bool _isInDanger);
    public void setIsBeingAttacked(bool _isBeingAttacked);
    public bool getIsBeingAttacked();
}
