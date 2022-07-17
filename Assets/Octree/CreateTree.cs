using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;
    public NativeList<Node<Vector3>> nodes;
    public NativeArray<Node<Vector3>> nodes;

    private void Start()
    {
        nodes = new NativeArray<Node<Vector3>>(1 + (depth.);
    }
}
