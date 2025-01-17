using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Leaf : Node
{
    readonly IStrategy strategy;
    new readonly string name;

    public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
    {
        this.strategy = strategy;
        this.name = name;
    }

    public override Status Process(bool isInterrupted)
    {
        return strategy.Process(isInterrupted, name);
    }

    public override void Reset()
    {
        strategy.Reset();
    }
}
