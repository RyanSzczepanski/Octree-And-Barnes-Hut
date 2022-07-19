using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NBodyNodeData
{
    public bool hasPlanet;
    public Vector3 centerOfMass;
    public double mass;
}