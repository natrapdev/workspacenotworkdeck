using System.Collections.Generic;
using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Represents a node in the blood circulation graph.
/// Each limb is a node, connected to parent and child limbs.
/// </summary>
public struct CirculationNode(
    string limbName,
    int parentIndex,
    float bloodVolume,
    float maxBloodVolume,
    int distanceFromHeart)
{
    public string LimbName = limbName;
    public int ParentIndex = parentIndex; // -1 for root (thorax)
    public int ChildCount = 0;
    public float BloodVolume = bloodVolume;
    public float MaxBloodVolume = maxBloodVolume;
    public float BleedRate = 0f;
    public int DistanceFromHeart = distanceFromHeart; // In circulation path steps
    public float BloodFlowRate = 0f; // Blood flow through this node
}

/// <summary>
/// Blood circulation graph structure.
/// Maintains the hierarchy of limbs for blood propagation.
/// </summary>
public struct CirculationGraph
{
    public const int MaxNodes = 16;
    public int NodeCount;
    public CirculationNode[] Nodes;
    public int[,] ChildIndices; // [nodeIndex, childIndex] -> childNodeIndex
    
    public CirculationGraph()
    {
        NodeCount = 0;
        Nodes = new CirculationNode[MaxNodes];
        ChildIndices = new int[MaxNodes, 4]; // Max 4 children per node
        for (int i = 0; i < MaxNodes; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                ChildIndices[i, j] = -1;
            }
        }
    }
    
    public int AddNode(CirculationNode node)
    {
        if (NodeCount >= MaxNodes) return -1;
        
        int index = NodeCount;
        Nodes[index] = node;
        
        // Add to parent's children if not root
        if (node.ParentIndex >= 0 && node.ParentIndex < NodeCount)
        {
            int parentIndex = node.ParentIndex;
            int childSlot = Nodes[parentIndex].ChildCount;
            if (childSlot < 4)
            {
                ChildIndices[parentIndex, childSlot] = index;
                Nodes[parentIndex].ChildCount++;
            }
        }
        
        NodeCount++;
        return index;
    }
    
    public int GetNodeIndex(string limbName)
    {
        for (int i = 0; i < NodeCount; i++)
        {
            if (Nodes[i].LimbName == limbName)
            {
                return i;
            }
        }
        return -1;
    }
    
    public ref CirculationNode GetNode(string limbName)
    {
        int index = GetNodeIndex(limbName);
        return ref Nodes[index >= 0 ? index : 0];
    }
    
    public ref CirculationNode GetNode(int index)
    {
        return ref Nodes[index];
    }
    
    public int GetChildIndex(int parentIndex, int childSlot)
    {
        if (parentIndex < 0 || parentIndex >= NodeCount) return -1;
        if (childSlot < 0 || childSlot >= 4) return -1;
        return ChildIndices[parentIndex, childSlot];
    }
    
    public void Clear()
    {
        NodeCount = 0;
        Nodes = new CirculationNode[MaxNodes];
        for (int i = 0; i < MaxNodes; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                ChildIndices[i, j] = -1;
            }
        }
    }
}

/// <summary>
/// Predefined limb hierarchy for humanoids.
/// Maps the blood circulation paths from heart to extremities.
/// </summary>
public static class LimbHierarchy
{
    /// <summary>
    /// Creates the standard humanoid limb hierarchy.
    /// Returns a circulation graph with all limbs properly connected.
    /// </summary>
    public static CirculationGraph CreateHumanoidHierarchy()
    {
        CirculationGraph graph = new CirculationGraph();
        
        // Root: Thorax (heart location)
        int thorax = graph.AddNode(new CirculationNode("thorax", -1, 0f, 0f, 0));
        
        // Level 1: Direct connections to thorax
        int head = graph.AddNode(new CirculationNode("head", thorax, 0f, 0f, 1));
        int neck = graph.AddNode(new CirculationNode("neck", thorax, 0f, 0f, 1));
        int abdomen = graph.AddNode(new CirculationNode("abdomen", thorax, 0f, 0f, 1));
        int leftUpperArm = graph.AddNode(new CirculationNode("left upper arm", thorax, 0f, 0f, 1));
        int rightUpperArm = graph.AddNode(new CirculationNode("right upper arm", thorax, 0f, 0f, 1));
        
        // Level 2: Arms and pelvis
        int leftForearm = graph.AddNode(new CirculationNode("left forearm", leftUpperArm, 0f, 0f, 2));
        int rightForearm = graph.AddNode(new CirculationNode("right forearm", rightUpperArm, 0f, 0f, 2));
        int pelvis = graph.AddNode(new CirculationNode("pelvis", abdomen, 0f, 0f, 2));
        
        // Level 3: Hands and legs
        int leftHand = graph.AddNode(new CirculationNode("left hand", leftForearm, 0f, 0f, 3));
        int rightHand = graph.AddNode(new CirculationNode("right hand", rightForearm, 0f, 0f, 3));
        int leftThigh = graph.AddNode(new CirculationNode("left thigh", pelvis, 0f, 0f, 3));
        int rightThigh = graph.AddNode(new CirculationNode("right thigh", pelvis, 0f, 0f, 3));
        
        // Level 4: Shins
        int leftShin = graph.AddNode(new CirculationNode("left shin", leftThigh, 0f, 0f, 4));
        int rightShin = graph.AddNode(new CirculationNode("right shin", rightThigh, 0f, 0f, 4));
        
        // Level 5: Feet
        int leftFoot = graph.AddNode(new CirculationNode("left foot", leftShin, 0f, 0f, 5));
        int rightFoot = graph.AddNode(new CirculationNode("right foot", rightShin, 0f, 0f, 5));
        
        return graph;
    }
    
    /// <summary>
    /// Gets the distance from heart for a given limb.
    /// </summary>
    public static int GetDistanceFromHeart(string limbName, CirculationGraph graph)
    {
        int index = graph.GetNodeIndex(limbName);
        if (index < 0) return 0;
        return graph.Nodes[index].DistanceFromHeart;
    }
    
    /// <summary>
    /// Gets the path from heart to a limb.
    /// Returns list of limb names in order from thorax to target.
    /// </summary>
    public static List<string> GetPathToLimb(string limbName, CirculationGraph graph)
    {
        List<string> path = new List<string>();
        int currentIndex = graph.GetNodeIndex(limbName);
        
        while (currentIndex >= 0)
        {
            path.Insert(0, graph.Nodes[currentIndex].LimbName);
            currentIndex = graph.Nodes[currentIndex].ParentIndex;
        }
        
        return path;
    }
}

/// <summary>
/// Blood propagation constants and helper methods.
/// </summary>
public static class BloodPropagation
{
    /// <summary>
    /// Reduction factor per circulation step from injury source.
    /// </summary>
    public const float DistanceReductionFactor = 0.15f;
    
    /// <summary>
    /// Calculates the bleed rate at a node based on distance from injury source.
    /// </summary>
    public static float CalculatePropagatedBleedRate(float sourceBleedRate, int distanceFromSource)
    {
        float reduction = distanceFromSource * DistanceReductionFactor;
        float propagatedRate = sourceBleedRate * (1.0f - reduction);
        return Mathf.Max(0f, propagatedRate);
    }
    
    /// <summary>
    /// Gets the circulation distance between two limbs.
    /// </summary>
    public static int GetCirculationDistance(string fromLimb, string toLimb, CirculationGraph graph)
    {
        List<string> fromPath = LimbHierarchy.GetPathToLimb(fromLimb, graph);
        List<string> toPath = LimbHierarchy.GetPathToLimb(toLimb, graph);
        
        // Find common ancestor
        int commonIndex = 0;
        while (commonIndex < fromPath.Count && commonIndex < toPath.Count && 
               fromPath[commonIndex] == toPath[commonIndex])
        {
            commonIndex++;
        }
        
        // Calculate total distance
        int distance = (fromPath.Count - commonIndex) + (toPath.Count - commonIndex);
        return distance;
    }
}
