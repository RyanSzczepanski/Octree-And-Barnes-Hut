using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct BarnesHut : IJob
{
    public NativeList<Vector3> positions;
    public NativeList<Node<NBodyNodeData>> nodes;

    public void Execute()
    {
        Sort();
    }

    public void Sort()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            Recursive(positions[i], 0);
        }
    }


    private void Recursive(Vector3 position, int searchNodeIndex)
    {
        Node<NBodyNodeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            Recursive(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position))/*Index Of Child Quadrent That Planet Is In*/);
            return;
        }

        //If node has a planet bump both down a layer
        if (currentNode.data.hasPlanet)
        {
            Vector3 existingPlanetPos = currentNode.data.centerOfMass;
            currentNode.data.hasPlanet = false;
            //Generates all children
            for (int l = 0; l < 8; l++)
            {
                nodes.Add(currentNode.PopulateChild(
                    l,
                    new NBodyNodeData(),
                    nodes.Length));

                nodes[searchNodeIndex] = currentNode;
            }
            nodes[searchNodeIndex] = currentNode;
            //Bumps existing planet down a node
            Recursive(existingPlanetPos, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(existingPlanetPos)));
            //Continues the search for a node
            Recursive(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position)));
            return;
        }
        //Found node and adds planet
        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = position;
        nodes[searchNodeIndex] = currentNode;
        return;
    }
}