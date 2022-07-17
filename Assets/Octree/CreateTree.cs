using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;
    public NativeList<Node<Vector3>> nodes;

    private void Start()
    {
        nodes = new NativeList<Node<Vector3>>(0, Allocator.Persistent);
        


        int nodeCount = 1;

        for (int currentDepth = 0; currentDepth < depth; currentDepth++)
        {
            for (int child = 0; child < Mathf.Pow(8, currentDepth + 1); child++)
            {
                nodeCount++;
            }
        }

        Debug.Log(nodeCount);

        NativeList<Node<Vector3>> newNodes = new NativeList<Node<Vector3>>(nodeCount, Allocator.Persistent);
        nodes.Add(Node<Vector3>.CreateNewNode(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), -1, 0));
        
        for (int i = 0; i < depth; i++)
        {
            int nodeLength = nodes.Length;
            for (int j = 0; j < nodeLength; j++)
            {
                if (nodes[j].endNode)
                {
                    Node<Vector3> currentNode = nodes[j];
                    for (int k = 0; k < 8; k++)
                    {
                        nodes.Add(currentNode.PopulateChild(k, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), nodes.Length));
                    }
                    nodes[j] = currentNode;
                }
            }
        }

        Debug.Log(nodes.Length);
    }
}
