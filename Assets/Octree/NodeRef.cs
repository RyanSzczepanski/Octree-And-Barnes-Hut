using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeRef<Data>
{
    public Data data;
    [HideInInspector]
    public NodeRef<Data> parentNode;
    public bool endNode;
    [HideInInspector]
    public NodeRef<Data>[] nodeChildren;

    public int GetDepth()
    {
        int depth = 0;
        if (parentNode is null)
            return depth;
        else
        {
            depth++;
            depth += parentNode.GetDepth();
        }
        return depth;
    }

    public NodeRef<Data> GetMasterNodeIndex()
    {
        if (parentNode is null)
            return this;
        else
            return parentNode.GetMasterNodeIndex();
    }

    public static NodeRef<Data> CreateNewNode(Data data, NodeRef<Data> parentNode)
    {
        return new NodeRef<Data>()
        {
            data = data,
            parentNode = parentNode,
            endNode = true,
            nodeChildren = new NodeRef<Data>[8],
        };
    }

    public void PopulateChild(int siblingIndex, Data data)
    {
        endNode = false;
        if (nodeChildren[siblingIndex] != null)
        {
            Debug.Log($"Child {siblingIndex} of Node is already populated");
        }
        nodeChildren[siblingIndex] = CreateNewNode(data, this);
    }
}