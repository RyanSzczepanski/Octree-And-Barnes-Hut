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

    public void Sort()
    {
        for (int i = positions.Length - 1; i >= 0; i--)
        {
            if (Recursive(i, 0, true))
            {
                positions.RemoveAt(i);
            }
        }
    }


    private bool Recursive(int i, int searchNodeIndex, bool generateAllChildren)
    {
        Node<NBodyOctreeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            if (!generateAllChildren)
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
                return Recursive(i, nextNodeToSearch, generateAllChildren);
            }
            else
            {
                return Recursive(i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(positions[i]))/*Index Of Child Quadrent That Planet Is In*/, generateAllChildren);
            }

            
        }
        //Loop all remaining transforms to check for other transforms in same area
        for (int k = 0; k < positions.Length; k++)
        {
            //Check if its looking at its self
            if (k == i) { continue; }
            //If node also contains another planet split node
            if (currentNode.spacialData.ContainsPosition(positions[k]))
            {
                if (!generateAllChildren)
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
                    return Recursive(i, nextNodeToSearch, generateAllChildren);
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
                    return Recursive(i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(positions[i])), generateAllChildren);
                }
            }
        }
        //Add planet to node
        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = positions[i];
        nodes[searchNodeIndex] = currentNode;
        return true;
    }


    public void SortRework()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            if (RecursiveRework(positions[i], 0, true))
            {
                positions.RemoveAt(i);
            }
        }
    }


    private bool RecursiveRework(Vector3 position, int searchNodeIndex, bool generateAllChildren)
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
                    nodes[searchNodeIndex] = currentNode;
                }
                //
                return RecursiveRework(position, nextNodeToSearch, generateAllChildren);
            }
            else
            {
                return RecursiveRework(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position))/*Index Of Child Quadrent That Planet Is In*/, generateAllChildren);
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
                    nodes[searchNodeIndex] = currentNode;
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
                RecursiveRework(preExistingPlanet, nextNodeToSearch2, generateAllChildren);
                RecursiveRework(position, nextNodeToSearch, generateAllChildren);
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
                RecursiveRework(preExistingPlanet, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(preExistingPlanet)), generateAllChildren);
                return RecursiveRework(position, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(position)), generateAllChildren);
            }
        }
        //Add planet to node
        currentNode.data.hasPlanet = true;
        currentNode.data.centerOfMass = position;
        nodes[searchNodeIndex] = currentNode;
        return true;
    }
}
