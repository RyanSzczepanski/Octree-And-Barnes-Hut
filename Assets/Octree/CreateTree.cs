using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;
    [Range(-1f, 10f)]
    public int drawDepth;
    public NativeList<Node<NBodyOctreeData>> nodes;
    public Node<NBodyOctreeData>[] nodesDebug;

    //public List<NodeRef<NBodyOctreeData>> nodesRef;

    public bool useRef;

    public GameObject prefab;

    public Transform[] objects;

    private void Start()
    {
        
        objects = ObjectSpawner(5000, 63f, prefab);

        //Data
        
        nodes = new NativeList<Node<NBodyOctreeData>>(0, Allocator.Persistent);

        nodes.Add(Node<NBodyOctreeData>.CreateNewNode(
            new NBodyOctreeData(),
            new SpacialOctreeData()
            {
                center = Vector3.zero,
                radius = 64,
            },
            -1,
            0));

        NativeArray<Vector3> positions = new NativeArray<Vector3>(objects.Length, Allocator.TempJob);
        for (int i = 0; i < objects.Length; i++)
        {
            positions[i] = objects[i].position;
        }
        Timer.Start();
        BarnesHut(positions);
        Timer.Stop();

        //Timer.Start();
        //BarnesHutRework(positions);
        //Timer.Stop();

        //foreach (NBodyOctreeData data in nodes[0].GetAllData(nodes))
        //{
        //    //Debug.Log(data.centerOfMass);
        //    //Debug.Log(data.mass);
        //}

    }

    private void Update()
    {
        if (!useRef)
        {
            //Data Drawer
            for (int i = 0; i < nodes.Length; i++)
            {
                if (drawDepth == -1)
                    DebugRenderer.DrawCube(nodes[i].spacialData.center, nodes[i].spacialData.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
                else if (nodes[i].GetDepth(nodes) == drawDepth)
                    DebugRenderer.DrawCube(nodes[i].spacialData.center, nodes[i].spacialData.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
            }
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
        nodes.Add(Node<NBodyOctreeData>.CreateNewNode(
            new NBodyOctreeData(),
            new SpacialOctreeData()
            {
                center = Vector3.zero,
                radius = 64,
            },
            -1,
            0));

        for (int i = 0; i < depth; i++)
        {
            int nodeLength = nodes.Length;
            for (int j = 0; j < nodeLength; j++)
            {
                if (nodes[j].endNode)
                {
                    Node<NBodyOctreeData> currentNode = nodes[j];
                    for (int k = 0; k < 8; k++)
                    {
                        Vector3 offset = SpacialOctreeData.GetOffsetVector(k);
                        nodes.Add(currentNode.PopulateChild(
                            k,
                            new NBodyOctreeData(),
                            nodes.Length));
                    }
                    nodes[j] = currentNode;
                }
            }
        }
    }

    void BarnesHut(NativeArray<Vector3> positions)
    {
        NativeArray<int> nativeArrray = new NativeArray<int>(1, Allocator.Temp);
        nativeArrray[0] = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            Recursive(positions, i, nativeArrray);
        }
        nativeArrray.Dispose();
    }

    void BarnesHutRework(NativeArray<Vector3> positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            RecursiveRework(positions, i, 0);
        }
    }




    //TODO: Bruh clean this shit up, like for real you wrote this?
    private bool Recursive(NativeArray<Vector3> positions, int i, NativeArray<int> searchNodeIndexes)
    {
        //This needs to be recursive and recalled after creating more nodes if none are valid
        //Loop all nodes looking for open node
        for (int j = 0; j < searchNodeIndexes.Length; j++)
        {
            Node<NBodyOctreeData> currentNode = nodes[searchNodeIndexes[j]];
            //If node contains transforms position
            if (NodeContainsTransform(positions[i], currentNode))
            {
                //If node isnt and end node skip to children
                if (!currentNode.endNode) { continue; }
                //Loop all transforms to check for other transforms in same area
                for (int k = 0; k < positions.Length; k++)
                {
                    //Check if its looking at its self
                    if (k == i) { continue; }
                    //If node also contains another planet split node
                    if (NodeContainsTransform(positions[k], currentNode))
                    {
                        NativeArray<int> childIndexes = new NativeArray<int>(8, Allocator.Temp);
                        //If it is end node create children
                        if(currentNode.endNode)
                        {
                            for (int l = 0; l < 8; l++)
                            {
                                Vector3 offset = SpacialOctreeData.GetOffsetVector(l);

                                nodes.Add(currentNode.PopulateChild(
                                    l,
                                    new NBodyOctreeData(),
                                    nodes.Length));

                                nodes[searchNodeIndexes[j]] = currentNode;
                                childIndexes[l] = currentNode.nodeChildren.GetChildIndex(l);
                            }
                        }
                        else
                        {
                            for (int l = 0; l < 8; l++)
                            {
                                childIndexes[l] = currentNode.nodeChildren.GetChildIndex(l);
                            }
                        }
                        //Debug.Log("Oopsie");
                       
                        Recursive(positions, i, childIndexes);
                        childIndexes.Dispose();
                        return false;
                    }
                }
                //Add planet to node
                currentNode.data.hasPlanet = true;
                currentNode.data.centerOfMass = positions[i];
                nodes[searchNodeIndexes[j]] = currentNode;
                //Debug.Log("Found Node");
                return true;
            }

        }
        return false;
    }

    private bool RecursiveRework(NativeArray<Vector3> positions, int i, int searchNodeIndex)
    {
        Node<NBodyOctreeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            RecursiveRework(positions, i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildQuadIndex(positions[i]))/*Index Of Child Quadrent That Planet Is In*/);
            return false;
        }
        //Loop all transforms to check for other transforms in same area
        //TODO: Possible optimaztions to widdle down positions array as more planets get sorted
        for (int k = 0; k < positions.Length; k++)
        {
            //Check if its looking at its self
            if (k == i) { continue; }
            //If node also contains another planet split node
            if (NodeContainsTransform(positions[k], currentNode))
            {
                for (int l = 0; l < 8; l++)
                {
                    nodes.Add(currentNode.PopulateChild(
                        l,
                        new NBodyOctreeData(),
                        nodes.Length));

                    nodes[searchNodeIndex] = currentNode;
                }
                RecursiveRework(positions, i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildQuadIndex(positions[i])));
                return false;
            }
        }
        //Add planet to node

        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = positions[i];
        nodes[searchNodeIndex] = currentNode;
        //Debug.Log("Found Node");
        return true;
    }

    public static bool NodeContainsTransform(Vector3 pos, Node<NBodyOctreeData> node)
    {
        return (pos.x < node.spacialData.center.x + node.spacialData.radius && pos.x > node.spacialData.center.x - node.spacialData.radius &&
                pos.y < node.spacialData.center.y + node.spacialData.radius && pos.y > node.spacialData.center.y - node.spacialData.radius &&
                pos.z < node.spacialData.center.z + node.spacialData.radius && pos.z > node.spacialData.center.z - node.spacialData.radius);
    }

    public Transform[] ObjectSpawner(int num, float radius, GameObject prefab)
    {
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < num; i++)
        {
            transforms.Add(Instantiate(prefab).transform);
            transforms[i].position = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius));
        }
        return transforms.ToArray();
    }
}