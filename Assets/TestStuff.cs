using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStuff : MonoBehaviour
{
    NativeOctree<int> octree;
    void Start()
    {
        octree = new NativeOctree<int>(new Unity.Mathematics.double3(1,1,1));
        octree.Dispose();
    }
}
