using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Leaf : Node
{
    private readonly IStrategy strategy;
    new private readonly string name;

    public Leaf(string _name, IStrategy _strategy, int _priority = 0) : base(_name, _priority)
    {
        strategy = _strategy;
        name = _name;
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
