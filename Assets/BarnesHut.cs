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
        Sort();
    }

    public void Sort()
    {
        for (int i = positions.Length - 1; i >= 0; i--)
        {
            if (Recursive(i, 0))
            {
                positions.RemoveAt(i);
            }
        }
    }


    private bool Recursive(int i, int searchNodeIndex)
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
            return Recursive(i, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(positions[i]))/*Index Of Child Quadrent That Planet Is In*/);
        }
        //Loop all transforms to check for other transforms in same area
        //TODO: Possible optimaztions to widdle down positions array as more planets get sorted
        for (int k = 0; k < positions.Length; k++)
        {
            //Check if its looking at its self
            if (k == i) { continue; }
            //If node also contains another planet split node
            if (currentNode.spacialData.ContainsPosition(positions[k]))
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
                return Recursive(i, nextNodeToSearch);
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
}
