using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttackable
{
    public void SetAttackingAgent(IAgent agent, bool add);
}
