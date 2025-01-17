using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public enum Status { Success, Failure, Running }

    public readonly string name;
    public int priority, currentChild;
    public readonly List<Node> children = new();

    public Node(string _name = "Node", int _priority = 0) 
    {
        name = _name;
        priority = _priority;
    }

    public void AddChild(Node _child)
    {
        children.Add(_child);
    }

    public virtual Status Process(bool _isInterrupted = false) 
    {
        return children[currentChild].Process(_isInterrupted);
    }

    public virtual void Reset()
    {
        currentChild = 0;
        foreach(Node _child in children)
        {
            _child.Reset();
        }
    }
}

