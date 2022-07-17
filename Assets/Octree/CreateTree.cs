using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;
    [Range(-1f, 10f)]
    public int drawDepth;
    public NativeList<Node<OctreeData>> nodes;
    public Node<OctreeData>[] nodesDebug;

    public Transform[] objects;

    private void Start()
    {
        nodes = new NativeList<Node<OctreeData>>(0, Allocator.Persistent);

        nodes.Add(Node<OctreeData>.CreateNewNode(new OctreeData()
        {
            center = Vector3.zero,
            radius = 64,
            hasPlanet = false
        }, -1, 0));

        BarnesHut(objects);
        nodesDebug = nodes.ToArray();
        //BuildNodesToDepth(depth);
    }

    private void Update()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (drawDepth == -1)
                DebugRenderer.DrawCube(nodes[i].data.center, nodes[i].data.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
            else if (nodes[i].GetDepth(nodes) == drawDepth)
                DebugRenderer.DrawCube(nodes[i].data.center, nodes[i].data.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
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

        //Initial Node
        nodes.Add(Node<OctreeData>.CreateNewNode(new OctreeData() {
            center = Vector3.zero,
            radius = 64,
            hasPlanet = false
        }, -1, 0));

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
                        Vector3 offset = OctreeData.GetOffsetVector(k);
                        nodes.Add(currentNode.PopulateChild(
                            k,
                            new OctreeData() {
                                center = currentNode.data.center + offset * currentNode.data.radius * .5f,
                                radius = currentNode.data.radius * .5f,
                                hasPlanet = false
                            },
                            nodes.Length));
                    }
                    nodes[j] = currentNode;
                }
            }
        }
    }

    private void BarnesHut(Transform[] objects)
    {
        List<Transform> transforms = new List<Transform>(objects);
        //Loop all transforms needed to be sorted
        for (int i = 0; i < transforms.Count; i++)
        {
            if (Recursive(transforms, i)) { transforms.RemoveAt(i); }
        }
    }

    //TODO: Bruh clean this shit up, like for real you wrote this?
    private bool Recursive(List<Transform> transforms, int i)
    {
        //This needs to be recursive and recalled after creating more nodes if none are valid
        //Loop all nodes looking for open node
        for (int j = 0; j < nodes.Length; j++)
        {
            Node<OctreeData> currentNode = nodes[j];
            //If node contains transforms position
            if (NodeContainsTransform(transforms[i].position, nodes[j]))
            {
                //If node already has a planet continue
                if (nodes[j].data.hasPlanet == true || !nodes[j].endNode) { continue; }
                //Loop all transforms to check for other transforms in same area
                for (int k = 0; k < transforms.Count; k++)
                {
                    //Check if its looking at its self
                    if (k == i) { continue; }
                    //If node also contains another planet split node
                    if (NodeContainsTransform(transforms[k].position, currentNode))
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            Vector3 offset = OctreeData.GetOffsetVector(l);
                            nodes.Add(currentNode.PopulateChild(
                                l,
                                new OctreeData()
                                {
                                    center = currentNode.data.center + offset * currentNode.data.radius * .5f,
                                    radius = currentNode.data.radius * .5f,
                                    hasPlanet = false
                                },
                                nodes.Length));
                            nodes[j] = currentNode;
                        }
                        Debug.Log("Oopsie");
                        Recursive(transforms, i);
                        return false;
                    }
                }
                //Add planet to node
                currentNode.data.hasPlanet = true;
                currentNode.data.objectPos = transforms[i].position;
                nodes[j] = currentNode;
                Debug.Log("Found Node");
                return true;
            }
            
        }
        return false;
    }


    public bool NodeContainsTransform(Vector3 pos, Node<OctreeData> node)
    {
        return (pos.x < node.data.center.x + node.data.radius && pos.x > node.data.center.x - node.data.radius &&
                pos.y < node.data.center.y + node.data.radius && pos.y > node.data.center.y - node.data.radius &&
                pos.z < node.data.center.z + node.data.radius && pos.z > node.data.center.z - node.data.radius);
    }
}
