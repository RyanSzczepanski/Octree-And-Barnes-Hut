using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public struct Node<Data>
{
    public Data data;

    public int index;
    public int parentNodeIndex;
    public NodeChildren nodeChildren;

    public bool endNode;

    public SpacialOctreeData spacialData;

    public int GetDepth(NativeArray<Node<Data>> nodes)
    {
        int depth = 0;
        if (parentNodeIndex == -1)
            return depth;
        else
        {
            depth++;
            depth += nodes[parentNodeIndex].GetDepth(nodes);
        }
        return depth;
    }

    public List<Data> GetAllData(NativeArray<Node<Data>> nodes)
    {
        List<Data> list = new List<Data>();
        if (endNode)
        {
            list.Add(data);
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                if(nodeChildren.GetChildIndex(i) == -1)
                {
                    Debug.Log("BADDDDD");
                }
                list.AddRange(nodes[nodeChildren.GetChildIndex(i)].GetAllData(nodes));
            }
        }
        return list;
    }

    public int GetMasterNodeIndex(NativeArray<Node<Data>> nodes)
    {
        if(parentNodeIndex == -1)
            return index;
        else
            nodes[parentNodeIndex].GetMasterNodeIndex(nodes);
        return -1;
    }

    public static Node<Data> CreateNewNode(Data data, SpacialOctreeData spacialData, int parentNodeIndex, int index)
    {
        return new Node<Data>() {
            data = data,

            index = index,
            parentNodeIndex = parentNodeIndex,
            endNode = true,
            
            spacialData = spacialData,
            
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
            Debug.Log($"Child {siblingIndex} of Node Index {index} is already populated with Node Index {nodeChildren.GetChildIndex(siblingIndex)}");
        }
        nodeChildren.SetChildIndex(siblingIndex, index);
        return CreateNewNode(
            data,
            new SpacialOctreeData() {
                center = this.spacialData.center + SpacialOctreeData.GetOffsetVector(siblingIndex) * spacialData.radius / 2,
                radius = spacialData.radius / 2},
            this.index,
            index);
    }
}

[System.Serializable]
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

    public void SetChildIndex(int child, int value)
    {
        switch (child)
        {
            case 0: child0 = value; break;
            case 1: child1 = value; break;
            case 2: child2 = value; break;
            case 3: child3 = value; break;
            case 4: child4 = value; break;
            case 5: child5 = value; break;
            case 6: child6 = value; break;
            case 7: child7 = value; break;
        }
    }
}

[System.Serializable]
public struct SpacialOctreeData
{
    public Vector3 center;
    public float radius;

    public static Vector3 GetOffsetVector(int ChildIndex)
    {
        Vector3 offset = Vector3.zero;
        switch (ChildIndex)
        {
            case (int)OctreeChild.RightTopBack:
                offset = new Vector3(1, 1, 1);
                break;
            case (int)OctreeChild.RightTopFront:
                offset = new Vector3(1, 1, -1);
                break;
            case (int)OctreeChild.RightBottomBack:
                offset = new Vector3(1, -1, 1);
                break;
            case (int)OctreeChild.RightBottomFront:
                offset = new Vector3(1, -1, -1);
                break;
            case (int)OctreeChild.LeftTopBack:
                offset = new Vector3(-1, 1, 1);
                break;
            case (int)OctreeChild.LeftTopFront:
                offset = new Vector3(-1, 1, -1);
                break;
            case (int)OctreeChild.LeftBottomBack:
                offset = new Vector3(-1, -1, 1);
                break;
            case (int)OctreeChild.LeftBottomFront:
                offset = new Vector3(-1, -1, -1);
                break;
        }
        return offset;
    }

    public int GetChildQuadIndex(Vector3 position)
    {
        Vector3 localPosition = position - center;
        char[] binary = new char[3];
        if (localPosition.x > 0)
            binary[0] = '0';
        else
            binary[0] = '1';
        if (localPosition.y > 0)
            binary[1] = '0';
        else
            binary[1] = '1';
        if (localPosition.z > 0)
            binary[2] = '0';
        else
            binary[2] = '1';

        return Convert.ToInt32(new string(binary), 2);
    }
}

public struct NBodyOctreeData
{
    public bool hasPlanet;
    public Vector3 centerOfMass;
    public double mass;
}

public enum OctreeChild
{
    RightTopBack = 0,     //000
    RightTopFront = 1,    //001
    RightBottomBack = 2,  //010
    RightBottomFront = 3, //011
    LeftTopBack = 4,      //100
    LeftTopFront = 5,     //101
    LeftBottomBack = 6,   //110
    LeftBottomFront = 7,  //111
}