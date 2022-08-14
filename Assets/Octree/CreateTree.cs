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
    public bool runEveryFrame;

    public GameObject prefab;
    public Material material;

    NativeArray<NBodyNodeData> data;

    public Octree octree;

    private void Start()
    {
        octree = new Octree();
        octree.Init(n);
        //CreateNewTree();
        //octree.Init(n);
        //ObjectGenerator(n, 63);
        //octree.GenerateTree(data);
    }

    private void Update()
    {
        if (runEveryFrame)
        {
            CreateNewTree();
        }
        //DrawInEditor(drawDepth);
    }

    private void OnDrawGizmos()
    {
        DrawInEditor(drawDepth);
    }

    private void LateUpdate()
    {
        if (runEveryFrame)
        {
            //octree.Dispose();
        }
    }

    public void CreateNewTree()
    {            
        //octree.Init(n);
        ObjectGenerator(n, 63);
        octree.GenerateTree(data);
    }

    public void DrawInBuild()
    {
        if (octree.nodes.Length <= 0) { return; }
        if (drawDepth != -1)
        {
            Node<NBodyNodeData>[] nodesToDraw = octree.GetAllNodesAtDepth(0, drawDepth);
            foreach (Node<NBodyNodeData> node in nodesToDraw)
            {
                DebugRenderer.DrawCube(node.spacialData.center, node.spacialData.radius, nodesToDraw.Length, material);
            }
        }
        else
        {
            for (int i = 0, j = 1; i < octree.nodes.Length; i++)
            {
                if (octree.nodes[i].endNode)
                {
                    DebugRenderer.DrawCube(octree.nodes[i].spacialData.center, octree.nodes[i].spacialData.radius, j, material);
                    j++;
                }
            }
        }
    }

    public void DrawInEditor(int drawDepth)
    {
        if (octree.nodes.Length <= 0) { return; }
        if (drawDepth != -1)
        {
            Node<NBodyNodeData>[] nodesToDraw = octree.GetAllNodesAtDepth(0, drawDepth);
            foreach (Node<NBodyNodeData> node in nodesToDraw)
            {
                DebugRenderer.GizmoDrawCube(node.spacialData.center, node.spacialData.radius, new Color(1, 1 - node.GetDepth(octree.nodes) / 6f, 0));
            }
        }
        else
        {
            for (int i = 0; i < octree.nodes.Length; i++)
            {
                if (octree.nodes[i].endNode)
                {
                    DebugRenderer.GizmoDrawCube(octree.nodes[i].spacialData.center, octree.nodes[i].spacialData.radius, new Color(1, 1 - octree.nodes[i].GetDepth(octree.nodes) / 6f, 0));
                }
            }
        }
    }

    private void ObjectGenerator(int num, float radius)
    {
        data = new NativeArray<NBodyNodeData>(num, Allocator.Persistent);
        for (int i = 0; i < num; i++)
        {
            data[i] = new NBodyNodeData()
            {
                centerOfMass = new float3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius)),
                mass = UnityEngine.Random.Range(5, 50),
                hasPlanet = true,
            };
            //GameObject temp = Instantiate(prefab);
            //temp.transform.position = data[i].centerOfMass;
        }
    }
}