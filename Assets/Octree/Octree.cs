using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct Octree
{
    public NativeList<Node<NBodyNodeData>> nodes;
    public NativeList<Node<NBodyNodeData>> newNodes;
    public NativeArray<int> occupiedNodes;

    public Node<NBodyNodeData>[] debugArray;

    public void Init(int size)
    {
        nodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.Persistent);
        occupiedNodes = new NativeArray<int>(size, Allocator.Persistent);
    }

    private void PreWork()
    {
        newNodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.TempJob);

        newNodes.Add(Node<NBodyNodeData>.CreateNewNode(
            new NBodyNodeData(),
            new SpacialOctreeData()
            {
                center = float3.zero,
                radius = 64,
            },
            -1,
            0));
    }

    public void GenerateTree(NativeArray<NBodyNodeData> data)
    {
        PreWork();

        NativeArray<int> occupiedNodes = new NativeArray<int>(data.Length, Allocator.TempJob);

        BarnesHut barnesHut = new BarnesHut()
        {
            occupiedNodes = occupiedNodes,
            bodies = data,
            nodes = newNodes,
        };

        JobHandle jobHandle = barnesHut.Schedule();
        jobHandle.Complete();
        this.occupiedNodes.CopyFrom(barnesHut.occupiedNodes);
        this.nodes.CopyFrom(barnesHut.nodes);

        newNodes.Dispose();
        occupiedNodes.Dispose();
        data.Dispose();



        for (int i = 0; i < this.occupiedNodes.Length; i++)
        {
            //Debug.Log(this.nodesWithPlanets[i]);
        }

        debugArray = nodes.ToArray();
    }

    public Node<NBodyNodeData>[] GetAllNodesAtDepth(int parentIndex, int depth)
    {
        if (nodes[parentIndex].GetDepth(nodes) == depth)
        {
            return new Node<NBodyNodeData>[1] { nodes[parentIndex] };
        }
        else if (nodes[parentIndex].GetDepth(nodes) < depth)
        {
            if (nodes[parentIndex].endNode) { return new Node<NBodyNodeData>[0]; }

            if (nodes[parentIndex].GetDepth(nodes) == depth - 1)
            {
                Node<NBodyNodeData>[] childNodes = new Node<NBodyNodeData>[8];
                for (int i = 0; i < 8; i++)
                {
                    childNodes[i] = nodes[nodes[parentIndex].nodeChildren.GetChildIndex(i)];
                }
                return childNodes;
            }
            else
            {
                List<Node<NBodyNodeData>> nodeses = new List<Node<NBodyNodeData>>();
                for (int i = 0; i < 8; i++)
                {
                    nodeses.AddRange(GetAllNodesAtDepth(nodes[parentIndex].nodeChildren.GetChildIndex(i), depth));
                }
                return nodeses.ToArray();
            }
        }
        return new Node<NBodyNodeData>[0];
    }
}