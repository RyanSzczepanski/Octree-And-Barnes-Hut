using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public struct NBodyNodeData
{
    public bool hasPlanet;
    public float3 centerOfMass;
    public float mass;
}