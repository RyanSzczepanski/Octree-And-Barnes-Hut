using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct Node<Data>
{
    public Data data;
    public int parentNodeIndex;
    public int index;
    public NativeArray<int> childrenIndexes;


    public int GetMasterNodeIndex(NativeArray<Node<Data>> nodes)
    {
        if(parentNodeIndex == -1)
            return index;
        else
            nodes[parentNodeIndex].GetMasterNodeIndex(nodes);
        return -1;
    }

    public void DestroyNode()
    {
        childrenIndexes.Dispose();
    }
}
