using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct BarnesHut : IJob
{
    public NativeArray<NBodyNodeData> bodies;
    public NativeList<Node<NBodyNodeData>> nodes;
    public NativeArray<int> occupiedNodes;

    public void Execute()
    {
        Sort();
    }

    public void Sort()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            Recursive(i, bodies[i], 0);
        }
    }


    private void Recursive(int i, NBodyNodeData body, int searchNodeIndex)
    {
        Node<NBodyNodeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            //Needs to jump in and update node before decending
            currentNode.data.centerOfMass = (currentNode.data.mass * currentNode.data.centerOfMass + body.mass * body.centerOfMass) / (currentNode.data.mass * body.mass);
            currentNode.data.mass += body.mass;
            nodes[searchNodeIndex] = currentNode;
            Recursive(i, body, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(body.centerOfMass))/*Index Of Child Quadrent That Planet Is In*/);
            return;
        }

        //If node has a planet bump both down a layer
        if (currentNode.data.hasPlanet)
        {
            NBodyNodeData existingPlanet = currentNode.data;
            int existingPlanetIndex = occupiedNodes.IndexOf(currentNode.index);
            currentNode.data.hasPlanet = false;

            //Generates all children
            for (int l = 0; l < 8; l++)
            {
                nodes.Add(currentNode.PopulateChild(
                    l,
                    new NBodyNodeData(),
                    nodes.Length));
            }
            //Needs to jump in and update node before decending
            currentNode.data.centerOfMass = (currentNode.data.mass * currentNode.data.centerOfMass + body.mass * body.centerOfMass) / (currentNode.data.mass * body.mass);
            currentNode.data.mass += body.mass;
            //Bumps existing planet down a node
            nodes[searchNodeIndex] = currentNode;
            Recursive(existingPlanetIndex, existingPlanet, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(existingPlanet.centerOfMass)));
            //Continues the search for a node
            Recursive(i, body, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(body.centerOfMass)));
            return;
        }
        //Found node and adds planet
        occupiedNodes[i] = currentNode.index;
        currentNode.data = body;
        currentNode.data.hasPlanet = true;        
        nodes[searchNodeIndex] = currentNode;
        return;
    }
}