using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;
    [Range(0f, 5f)]
    public int drawDepth;
    public NativeList<Node<OctreeData>> nodes;

    private void Start()
    {
        nodes = new NativeList<Node<OctreeData>>(0, Allocator.Persistent);

        BuildNodesToDepth(depth);
    }

    private void Update()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].GetDepth(nodes) == drawDepth)
                DebugDrawSquare(nodes[i].data.position, nodes[i].data.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
        }
    }

    private void BuildNodesToDepth(int depth)
    {
        int nodeCount = 1;

        for (int currentDepth = 0; currentDepth < depth; currentDepth++)
        {
            for (int child = 0; child < Mathf.Pow(8, currentDepth + 1); child++)
            {
                nodeCount++;
            }
        }

        nodes.Add(Node<OctreeData>.CreateNewNode(new OctreeData() { position = Vector3.zero, radius = 64}, -1, 0));

        for (int i = 0; i < depth; i++)
        {
            int nodeLength = nodes.Length;
            for (int j = 0; j < nodeLength; j++)
            {
                if (nodes[j].endNode)
                {
                    Node<OctreeData> currentNode = nodes[j];
                    for (int k = 0; k < 8; k++)
                    {
                        Vector3 offset = new Vector3(1, 1, 1);
                        switch (k)
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
                        nodes.Add(currentNode.PopulateChild(k, new OctreeData() {  position = currentNode.data.position + offset * currentNode.data.radius * .5f, radius = currentNode.data.radius * .5f}, nodes.Length));
                    }
                    nodes[j] = currentNode;
                }
            }
        }
    }

    public void DebugDrawSquare(Vector3 center, float radius, Color color, float time)
    {
        for (int i = 0; i < 8; i++)
        {
            //Face1
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(2) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(2) * radius, color, time);
            //Face2
            Debug.DrawLine(center + OctreeData.GetOffsetVector(4) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(4) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(7) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(7) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            //Connecting Arms
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(4) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(1) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(2) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(7) * radius, color, time);
        }
    }
}
