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
    public NativeList<Node<NBodyOctreeData>> newNodes;
    public Node<NBodyOctreeData>[] nodesDebug;

    public bool useOriginal;

    public bool jobCompleted = false;

    public GameObject prefab;

    public Transform[] objects = new Transform[0];

    NativeList<Vector3> positionsList;

    //BarnesHut barnesHut;
    JobHandle jobHandle;

    private void Start()
    {
        nodes = new NativeList<Node<NBodyOctreeData>>(0, Allocator.Persistent);
        newNodes = new NativeList<Node<NBodyOctreeData>>(0, Allocator.Persistent);
        positionsList = new NativeList<Vector3>(0, Allocator.Persistent);

        
    }

    private void PreWork()
    {
        newNodes.Clear();
        positionsList.Clear();

        if (n > 0)
            objects = ObjectSpawner(n, 63, prefab, false);

        newNodes.Add(Node<NBodyOctreeData>.CreateNewNode(
            new NBodyOctreeData(),
            new SpacialOctreeData()
            {
                center = Vector3.zero,
                radius = 64,
            },
            -1,
            0));
       
        if(n <= 0)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                positionsList.Add(objects[i].position);
            }
        }   
    }

    private void LateUpdate()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            Destroy(objects[i].gameObject);
        }
    }

    private void Update()
    {
        //if (jobHandle.IsCompleted && !jobCompleted)
        //{
        //    jobCompleted = true;

        //    jobHandle.Complete();
        //    barnesHutTimer.Stop();
        //    Debug.Log(barnesHut.nodes.Length);
        //    //nodes.Clear();
        //    //nodes.AddRange(barnesHut.nodes);

        //    nodesDebug = nodes.ToArray();
        //}
        //if (jobCompleted)
        //{
        //    if (n > 0)
        //        objects = ObjectSpawner(n, 63f, prefab, false);

        //    PreWork();
        //    barnesHutTimer.Start();

        //    barnesHut = new BarnesHut()
        //    {
        //        nodes = newNodes,
        //        positions = positionsList
        //    };
        //    jobHandle = barnesHut.Schedule();

        //    jobCompleted = false;
        //}
        

        PreWork();

        BarnesHut barnesHut = new BarnesHut()
        {
            positions = positionsList,
            nodes = newNodes,
        };

        jobHandle = barnesHut.Schedule();
        jobHandle.Complete();
        nodes = barnesHut.nodes;
        //nodesDebug = nodes.ToArray();

        



        //List<Node<NBodyOctreeData>> nodesToDraw = new List<Node<NBodyOctreeData>>();
        //Data Drawer
        if (drawDepth != -1)
        {
            foreach (Node<NBodyOctreeData> node in GetAllNodesAtDepth(0, drawDepth))
            {
                DebugRenderer.DrawCube(node.spacialData.center, node.spacialData.radius, new Color(1, 1 - node.GetDepth(nodes) / (float)depth, 0), 0f);
            }
        }
        else
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].endNode)
                {
                    DebugRenderer.DrawCube(nodes[i].spacialData.center, nodes[i].spacialData.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / (float)depth, 0), 0f);
                }
            }
        }
    }
    private Node<NBodyOctreeData>[] GetAllNodesAtDepth(int parentIndex, int depth)
    {
        if (nodes[parentIndex].GetDepth(nodes) == depth)
        {
            return new Node<NBodyOctreeData>[1] { nodes[parentIndex] };
        }
        else if (nodes[parentIndex].GetDepth(nodes) < depth)
        {
            if (nodes[parentIndex].endNode) { return new Node<NBodyOctreeData>[0]; }

            if (nodes[parentIndex].GetDepth(nodes) == depth - 1)
            {
                Node<NBodyOctreeData>[] childNodes = new Node<NBodyOctreeData>[8];
                for (int i = 0; i < 8; i++)
                {
                    childNodes[i] = nodes[nodes[parentIndex].nodeChildren.GetChildIndex(i)];
                }
                return childNodes;
            }
            else
            {
                List<Node<NBodyOctreeData>> nodeses = new List<Node<NBodyOctreeData>>();
                for (int i = 0; i < 8; i++)
                {
                    nodeses.AddRange(GetAllNodesAtDepth(nodes[parentIndex].nodeChildren.GetChildIndex(i), depth));
                }
                return nodeses.ToArray();
            }
        }
        return new Node<NBodyOctreeData>[0];
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

    

    public Transform[] ObjectSpawner(int num, float radius, GameObject prefab, bool visableObject)
    {
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < num; i++)
        {
            positionsList.Add(new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius)));
            if (visableObject)
            {
                transforms.Add(Instantiate(prefab).transform);
                transforms[i].position = positionsList[i];
            }
        }
        return transforms.ToArray();
    }
}