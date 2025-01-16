using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Ninja : MonoBehaviour, IAgent
{
    [SerializeField] Transform raycastOrigin;
    [SerializeField] GameObject agentToHelp;
    IAgent playerAgent;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] float hoverRange;
    NavMeshAgent agent;
    BehaviourTree tree;
    Dictionary<string, Func<bool>> strategyBreaks;
    public string currentActiveLeaf;
    bool isInDanger;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);
        playerAgent = agentToHelp.GetComponent<IAgent>();   

        //Guard Enemy Behaviour
        NinjaBehaviour();
        AddStrategyBreaks();
    }

    Transform IAgent.getTransform()
    {
        return transform;
    }

    TextMeshProUGUI IAgent.getDisplayText()
    {
        return displayText;
    }

    void IAgent.setCurrentActiveLeaf(string leafName)
    {
        currentActiveLeaf = leafName;
    }

    void IAgent.setIsInDanger(bool _isInDanger)
    {
        isInDanger = _isInDanger;
    }

    private void NinjaBehaviour()
    {
        PrioritySequence actions = new PrioritySequence("Guard Logic");

        PrioritySelector followPlayer = new PrioritySelector("FollowPlayer", 0);
            followPlayer.AddChild(new Leaf("CheckInHoverRange", new Condition(() => CalculateDistanceToPlayer() <= hoverRange), 10));
            followPlayer.AddChild(new Leaf("FollowPlayer", new MoveToTarget(this, gameObject.transform, agent, playerAgent.getTransform(), 5f), 5));

        actions.AddChild(followPlayer);
        tree.AddChild(actions);
    }

    private void Update()
    {
        bool interrupt = CheckStrategyBreaks();
        Debug.Log($"Interrupt? {interrupt}");
        tree.Process(interrupt);
    }

    Transform FindCover()
    {
        return null;
    }

    float CalculateAgentSpeed()
    {
        float newSpeed = 0;
        return newSpeed;
    }

    float CalculateDistanceToPlayer()
    {
        return (transform.position - playerAgent.getTransform().position).magnitude;
    }

    bool CheckStrategyBreaks()
    {
        Debug.Log("Current Active Leaf: " + currentActiveLeaf);
        if(!strategyBreaks.ContainsKey(currentActiveLeaf)) return false;
        if(strategyBreaks[currentActiveLeaf]()) return true;
        return false;
    }

    void AddStrategyBreaks()
    {
        strategyBreaks = new()
        {
            ["FollowPlayer"] = () => CalculateDistanceToPlayer() <= hoverRange
        };
    }
}
