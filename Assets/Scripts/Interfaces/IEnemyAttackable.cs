using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttackable
{
    public GameObject GetAttackingAgent();
    public Transform GetTransform();
    public bool GetIsBeingAttacked();
    public float GetHealth();
    public void SetHealth(float _health);
    public void SetIsInDanger(bool _isInDanger);
    public void SetIsBeingAttacked(bool _isBeingAttacked);
    public void SetAttackingAgent(GameObject _agent, bool _add);
}
