using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttackable
{
    GameObject GetAttackingAgent();
    Transform GetTransform();
    bool GetIsBeingAttacked();
    float GetHealth();
    void SetHealth(float _health);
    void SetIsInDanger(bool _isInDanger);
    void SetIsBeingAttacked(bool _isBeingAttacked);
    void SetAttackingAgent(GameObject _agent, bool _add);
}
