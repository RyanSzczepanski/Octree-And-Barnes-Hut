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
        BarnesHut barnesHut = new BarnesHut()
        {
            nodes = nodes,
            positions = positionsList
        };
        JobHandle jobHanlde = barnesHut.Schedule();
        jobHanlde.Complete();
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
        positionsList = new NativeList<Vector3>(0, Allocator.TempJob);
        positions = new NativeArray<Vector3>(objects.Length, Allocator.TempJob);
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