using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct Node<Data>
{
    public Data data;
    public int parentNodeIndex;
    public int index;
    public bool endNode;
    public NodeChildren nodeChildren;


    public int GetMasterNodeIndex(NativeArray<Node<Data>> nodes)
    {
        if(parentNodeIndex == -1)
            return index;
        else
            nodes[parentNodeIndex].GetMasterNodeIndex(nodes);
        return -1;
    }

    public static Node<Data> CreateNewNode(Data data, int parentNodeIndex, int index)
    {
        return new Node<Data>() {
            data = data,
            parentNodeIndex = parentNodeIndex,
            index = index,
            endNode = true,
            nodeChildren = new NodeChildren() {
                child0 = -1,
                child1 = -1,
                child2 = -1,
                child3 = -1,
                child4 = -1,
                child5 = -1,
                child6 = -1,
                child7 = -1,
            },
        };
    }

    public Node<Data> PopulateChild(int siblingIndex, Data data, int index)
    {
        endNode = false;
        if(nodeChildren.GetChildIndex(siblingIndex) != -1)
        {
            Debug.Log($"Child {siblingIndex} of Node Index {index} is already populated");
        }
        nodeChildren.SetChildIndex(siblingIndex, index);
        return CreateNewNode(data, this.index, index);
    }
}

public struct NodeChildren
{
    public int child0;
    public int child1;
    public int child2;
    public int child3;
    public int child4;
    public int child5;
    public int child6;
    public int child7;

    public int GetChildIndex(int child)
    {
        switch (child)
        {
            case 0: return child0;
            case 1: return child1;
            case 2: return child2;
            case 3: return child3;
            case 4: return child4;
            case 5: return child5;
            case 6: return child6;
            case 7: return child7;
            default: return -1;
        }
    }

    public int SetChildIndex(int child, int value)
    {
        switch (child)
        {
            case 0: return child0;
            case 1: return child1;
            case 2: return child2;
            case 3: return child3;
            case 4: return child4;
            case 5: return child5;
            case 6: return child6;
            case 7: return child7;
            default: return -1;
        }
    }
}