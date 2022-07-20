using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CreateTree : MonoBehaviour
{
    public int n;

    [Range(-1f, 10f)]
    public int drawDepth;
    public NativeList<Node<NBodyNodeData>> nodes;
    public NativeList<Node<NBodyNodeData>> newNodes;
    //public Node<NBodyNodeData>[] nodesDebug;
    [Header("")]
    public bool runEveryFrame;

    public GameObject prefab;
    public Material material;

    NativeList<float3> positionsList;



    bool isScheduled = false;
    Timer timer;
    BarnesHut barnesHut;
    JobHandle jobHandle;

    private void Start()
    {
        DebugRenderer.CreateMesh();
        nodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.Persistent);
        newNodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.Persistent);
        positionsList = new NativeList<float3>(0, Allocator.Persistent);
        if (!runEveryFrame)
        {
            //timer = new Timer();
            PreWork();
            barnesHut = new BarnesHut()
            {
                positions = positionsList,
                nodes = newNodes,
            };
        }

    }

    private void PreWork()
    {
        newNodes.Clear();
        positionsList.Clear();

        if (n > 0)
            ObjectSpawner(n, 63, prefab, false);

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

    private void Update()
    {
        if (!jobHandle.IsCompleted) { return; }
        if (runEveryFrame)
        {
            GenerateTree();
        }
        else
        {
            if (jobHandle.IsCompleted && isScheduled)
            {
                jobHandle.Complete();
                nodes = barnesHut.nodes;
                isScheduled = false;
                DebugRenderer.DoDraw(true);
                DebugRenderer.StartNewBatch();
                DrawInBuild();
                DebugRenderer.DoDraw(false);
            }
        }
        //Data Drawer
        //if (nodes.Length <= 0) { return; }
        //if (drawDepth != -1)
        //{
        //    Node<NBodyNodeData>[] nodesToDraw = GetAllNodesAtDepth(0, drawDepth);
        //    foreach (Node<NBodyNodeData> node in nodesToDraw)
        //    {
        //        DebugRenderer.DebugDrawCube(node.spacialData.center, node.spacialData.radius, new Color(1, 1 - node.GetDepth(nodes) / 8f, 0), 0f);
        //    }
        //}
        //else
        //{
        //    for (int i = 0; i < nodes.Length; i++)
        //    {
        //        if (nodes[i].endNode)
        //        {
        //            DebugRenderer.DebugDrawCube(nodes[i].spacialData.center, nodes[i].spacialData.radius, new Color(1, 1 - nodes[i].GetDepth(nodes) / 6f, 0), 0f);
        //        }
        //    }
        //}
    }

    public void DrawInBuild()
    {
        if (nodes.Length <= 0) { return; }
        if (drawDepth != -1)
        {
            Node<NBodyNodeData>[] nodesToDraw = GetAllNodesAtDepth(0, drawDepth);
            foreach (Node<NBodyNodeData> node in nodesToDraw)
            {
                DebugRenderer.DrawCube(node.spacialData.center, node.spacialData.radius, nodesToDraw.Length, material);
            }
        }
        else
        {
            for (int i = 0, j = 1; i < nodes.Length; i++)
            {
                if (nodes[i].endNode)
                {
                    DebugRenderer.DrawCube(nodes[i].spacialData.center, nodes[i].spacialData.radius, j, material);
                    j++;
                }
            }
        }
    }

    public void BuildTreeStart()
    {
        PreWork();

        BarnesHut barnesHut = new BarnesHut()
        {
            positions = positionsList,
            nodes = newNodes,
        };
        //timer.Start();
        jobHandle = barnesHut.Schedule();
        isScheduled = true;
    }

    private void GenerateTree()
    {
        PreWork();

        BarnesHut barnesHut = new BarnesHut()
        {
            positions = positionsList,
            nodes = newNodes,
        };

        jobHandle = barnesHut.Schedule();
        jobHandle.Complete();
        nodes = barnesHut.nodes;
    }

    private Node<NBodyNodeData>[] GetAllNodesAtDepth(int parentIndex, int depth)
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

    private Transform[] ObjectSpawner(int num, float radius, GameObject prefab, bool visableObject)
    {
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < num; i++)
        {
            positionsList.Add(new Vector3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius)));
            if (visableObject)
            {
                transforms.Add(Instantiate(prefab).transform);
                transforms[i].position = positionsList[i];
            }
        }
        return transforms.ToArray();
    }
}