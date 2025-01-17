using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Condition : IStrategy
{
    readonly Func<bool> predicate;

    public Condition(Func<bool> predicate)
    {
        this.predicate = predicate;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if (predicate())
        {
            return Node.Status.Success;
        }
        else
        {
            return Node.Status.Failure;
        }
    }
}

