using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    /*
    Node Suggestions:
    -UntilSuccess
    -Repeat
    */
    public enum Status { Success, Failure, Running }

    public readonly string name;
    public int priority;

    public readonly List<Node> children = new();
    //public for debugging
    public int currentChild;

    public Node(string name = "Node", int priority = 0) 
    {
        this.name = name;
        this.priority = priority;
    }

    public void AddChild(Node child)
    {
        children.Add(child);
    }

    public virtual Status Process(bool isInterrupted = false) 
    {
        return children[currentChild].Process(isInterrupted);
    }

    public virtual void Reset()
    {
        currentChild = 0;
        foreach(Node child in children)
        {
            child.Reset();
        }
    }
}

