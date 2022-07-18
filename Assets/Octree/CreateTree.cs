using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int depth;

    public int n;

    [Range(-1f, 10f)]
    public int drawDepth;
    public NativeList<Node<NBodyOctreeData>> nodes;
    public Node<NBodyOctreeData>[] nodesDebug;

    //public List<NodeRef<NBodyOctreeData>> nodesRef;

    public bool useOriginal;

    public GameObject prefab;

    public Transform[] objects;

    NativeArray<Vector3> positions;
    NativeList<Vector3> positionsList;

    private void Start()
    {

        objects = ObjectSpawner(n, 63f, prefab);

        //PreWork();
        //Debug.Log("Original Method");
        //Timer.Start();
        //BarnesHut(positions);
        //Timer.Stop();

        //positions.Dispose();
        //positionsList.Dispose();


        //PreWork();
        //Debug.Log("Reworked Method");
        //Timer.Start();
        //BarnesHutRework(positions);
        //Timer.Stop();

        //positions.Dispose();
        //positionsList.Dispose();


        PreWork();
        Debug.Log("Second Reworked Method");
        Timer.Start();
        BarnesHutSecondRework(positionsList);
        Timer.Stop();

        positions.Dispose();
        positionsList.Dispose();


        nodesDebug = nodes.ToArray();
    }

    private void PreWork()
    {
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
        positionsList = new NativeList<Vector3>(0, Allocator.Temp);
        positions = new NativeArray<Vector3>(objects.Length, Allocator.Temp);
        for (int i = 0; i < objects.Length; i++)
        {
            positionsList.Add(objects[i].position);
            positions[i] = objects[i].position;
        }
    }

    private void Update()
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

    void BarnesHutSecondRework(NativeList<Vector3> positions)
    {
        for (int i = positions.Length - 1; i >= 0; i--)
        {
            if (RecursiveSecondRework(positions, i, 0))
            {
                positions.RemoveAt(i);
            }
        }
    }


    private bool RecursiveSecondRework(NativeArray<Vector3> positions, int i, int searchNodeIndex)
    {
        Node<NBodyOctreeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            //TODO: Duplicate code break out into function
            int childOctants = currentNode.spacialData.GetChildOctantsIndex(positions[i]);
            int nextNodeToSearch = currentNode.nodeChildren.GetChildIndex(childOctants);
            if (nextNodeToSearch == -1)
            {
                nodes.Add(currentNode.PopulateChild(
                    childOctants,
                    new NBodyOctreeData(),
                    nodes.Length));
                nextNodeToSearch = nodes.Length - 1;
                nodes[searchNodeIndex] = currentNode;
            }
            //
            return RecursiveSecondRework(positions, i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(positions[i]))/*Index Of Child Quadrent That Planet Is In*/);
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
                //TODO: Duplicate code break out into function
                int childOctants = currentNode.spacialData.GetChildOctantsIndex(positions[i]);
                int nextNodeToSearch = currentNode.nodeChildren.GetChildIndex(childOctants);
                if (nextNodeToSearch == -1)
                {
                    nodes.Add(currentNode.PopulateChild(
                        childOctants,
                        new NBodyOctreeData(),
                        nodes.Length));
                    nextNodeToSearch = nodes.Length - 1;
                    nodes[searchNodeIndex] = currentNode;
                }
                return RecursiveSecondRework(positions, i, nextNodeToSearch);
                //
                //for (int l = 0; l < 8; l++)
                //{
                //    nodes.Add(currentNode.PopulateChild(
                //        l,
                //        new NBodyOctreeData(),
                //        nodes.Length));

                //    nodes[searchNodeIndex] = currentNode;
                //}
                //return RecursiveSecondRework(positions, i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(positions[i])));

            }
        }
        //Add planet to node
        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = positions[i];
        nodes[searchNodeIndex] = currentNode;
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