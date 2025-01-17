using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    Node.Status Process(bool isInterrupted, string leafName);
    void Reset()
    {
        //Noop
    }
}
