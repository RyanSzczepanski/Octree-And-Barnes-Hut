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
    public NativeList<Node<NBodyOctreeData>> nodes;

    public void Execute()
    {
        SortRework();
    }

    public void SortRework()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            RecursiveRework(positions[i], 0, true);
        }
    }


    private void RecursiveRework(Vector3 position, int searchNodeIndex, bool generateAllChildren)
    {
        Node<NBodyOctreeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            if (!generateAllChildren)
            {
                //TODO: Duplicate code break out into function
                int childOctants = currentNode.spacialData.GetChildOctantsIndex(position);
                int nextNodeToSearch = currentNode.nodeChildren.GetChildIndex(childOctants);
                if (nextNodeToSearch == -1)
                {
                    nodes.Add(currentNode.PopulateChild(
                        childOctants,
                        new NBodyOctreeData(),
                        nodes.Length));
                    nextNodeToSearch = nodes.Length - 1;
                }
                nodes[searchNodeIndex] = currentNode;
                RecursiveRework(position, nextNodeToSearch, generateAllChildren);
                return;
            }
            else
            {
                nodes[searchNodeIndex] = currentNode;
                RecursiveRework(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position))/*Index Of Child Quadrent That Planet Is In*/, generateAllChildren);
                return;
            }
        }

        //If node has a planet bump both down a layer
        if (currentNode.data.hasPlanet)
        {
            Vector3 preExistingPlanet = currentNode.data.centerOfMass;
            currentNode.data.hasPlanet = false;
            if (!generateAllChildren)
            {
                //TODO: Duplicate code break out into function
                int childOctants = currentNode.spacialData.GetChildOctantsIndex(position);
                int nextNodeToSearch = currentNode.nodeChildren.GetChildIndex(childOctants);
                if (nextNodeToSearch == -1)
                {
                    nodes.Add(currentNode.PopulateChild(
                        childOctants,
                        new NBodyOctreeData(),
                        nodes.Length));
                    nextNodeToSearch = nodes.Length - 1;
                }

                int childOctants2 = currentNode.spacialData.GetChildOctantsIndex(preExistingPlanet);
                int nextNodeToSearch2 = currentNode.nodeChildren.GetChildIndex(childOctants);
                if (nextNodeToSearch2 == -1)
                {
                    nodes.Add(currentNode.PopulateChild(
                        childOctants2,
                        new NBodyOctreeData(),
                        nodes.Length));
                    nextNodeToSearch2 = nodes.Length - 1;
                    nodes[searchNodeIndex] = currentNode;
                }

                nodes[searchNodeIndex] = currentNode;
                RecursiveRework(preExistingPlanet, nextNodeToSearch2, generateAllChildren);
                RecursiveRework(position, nextNodeToSearch, generateAllChildren);
                return;
                //
            }
            else
            {
                for (int l = 0; l < 8; l++)
                {
                    nodes.Add(currentNode.PopulateChild(
                        l,
                        new NBodyOctreeData(),
                        nodes.Length));

                    nodes[searchNodeIndex] = currentNode;
                }
                nodes[searchNodeIndex] = currentNode;
                RecursiveRework(preExistingPlanet, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(preExistingPlanet)), generateAllChildren);
                RecursiveRework(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position)), generateAllChildren);
                return;
            }
        }
        //Add planet to node
        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = position;
        nodes[searchNodeIndex] = currentNode;
        return;
    }
}
