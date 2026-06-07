using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Array = Godot.Collections.Array;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class HumanoidLimbs : Node
{
    [Export] public DamageModel Model { get; set; }
    public Dictionary<string, Limb> Limbs { get; } = new(17);

    public List<List<string>> LimbLayout = [
        ["thorax", "neck", "head"],
        ["thorax", "abdomen", "pelvis", "thigh", "shin", "foot"],
        ["thorax", "upper arm", "forearm", "hand"]
    ];

    public void Initialize()
    {
        AcceptSkeleton();
    }

    private void AcceptSkeleton()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Limb limb)
            {
                SetProperties(limb);
            }
        }
        GD.Print("\n");
        GraphLimbs();
    }

    private void GraphLimbs()
    {
        foreach (Limb limb in Limbs.Values)
        {
            limb.Neighbours = GetNeighbouringLimbs(limb);
        }
    }

    private List<Limb> GetNeighbouringLimbs(Limb limb)
    {
        return TranslateNameKeepSidePrefix(limb.Name) switch
        {
            "head" => [Limbs["neck"]],
            "neck" => [Limbs["head"], Limbs["thorax"]],
            "thorax" => [Limbs["abdomen"], Limbs["neck"], Limbs["left upper arm"], Limbs["right upper arm"]],
            "abdomen" => [Limbs["thorax"], Limbs["pelvis"]],
            "pelvis" => [Limbs["abdomen"], Limbs["right thigh"], Limbs["left thigh"]],
            "right upper arm" => [Limbs["thorax"], Limbs["right forearm"]],
            "left upper arm" => [Limbs["thorax"], Limbs["left forearm"]],
            "right forearm" => [Limbs["right upper arm"], Limbs["right hand"]],
            "left forearm" => [Limbs["left upper arm"], Limbs["left hand"]],
            "right hand" => [Limbs["right forearm"]],
            "left hand" => [Limbs["left forearm"]],
            "right thigh" => [Limbs["pelvis"], Limbs["right shin"]],
            "left thigh" => [Limbs["pelvis"], Limbs["left shin"]],
            "right foot" => [Limbs["right shin"]],
            "left foot" => [Limbs["left shin"]],
            _ => []
        };
    }

    private void SetProperties(Limb limb)
    {  
        limb.Model = Model;
        limb.ExternalSkeleton = limb.GetPathTo(limb.Model.Skeleton);
        limb.LimbName = TranslateName(limb.Name);
        limb.DetectionArea = limb.GetChild<Area3D>(0);
        limb.PhysicalVolume = GetNodeVolume(limb.DetectionArea.GetChild(0) as Node3D);
        limb.Mass = Model.BodyMass * Model.BodyPartBleedMultiplier.GetValueOrDefault(limb.LimbName, 1f);
        limb.MaxBloodVolume = Model.Circulation.TotalBloodVolume * Model.BodyPartMassCoefficients.GetValueOrDefault(limb.LimbName, 0f);
        limb.BleedMultiplier = Model.BodyPartBleedMultiplier.GetValueOrDefault(limb.LimbName, 1f);
        limb.Thickness = GetCollisionShapeThickness(limb.DetectionArea.GetChild(0) as CollisionShape3D);
        limb.CirculationSystem = Model.Circulation;
        Model.Circulation.CurrentBloodVolume += limb.MaxBloodVolume;
        
        limb.Initialize();
        Limbs.Add(TranslateNameKeepSidePrefix(limb.Name), limb);
    }

    /// <summary>
    /// Calculates the number of limbs between two limbs in the limb graph.
    /// Uses BFS to find the shortest path between the two limbs.
    /// </summary>
    /// <param name="start">The starting limb.</param>
    /// <param name="end">The target limb.</param>
    /// <returns>The number of limbs between the two limbs. Returns -1 if no path exists.</returns>
    public static int GetLimbDifference(Limb start, Limb end)
    {
        if (start is null || end is null)
            return -1;

        if (start == end)
            return 0;

        // BFS end find shortest path
        Queue<(Limb limb, int distance)> queue = new(8);
        HashSet<Limb> visited = [];

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            (Limb currentLimb, int distance) = queue.Dequeue();

            foreach (Limb neighbour in currentLimb.Neighbours)
            {
                if (neighbour == null)
                    continue;

                if (neighbour == end)
                    return distance + 1;

                if (!visited.Add(neighbour)) continue;
                queue.Enqueue((neighbour, distance + 1));
            }
        }

        // No path found
        return -1;
    }

    public static float GetNodeVolume(Node3D node)
    {
        switch (node)
        {
            case CollisionShape3D collisionShape: return GetCollisionShapeVolume(collisionShape);
            case MeshInstance3D {Mesh: ArrayMesh arrayMesh}:
            {
                float volume = 0f;

                for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++)
                {
                    Array arrays = arrayMesh.SurfaceGetArrays(i);
                    var vertices = (Vector3[])arrays[(int)Mesh.ArrayType.Vertex];
                    int[] indices = (int[])arrays[(int)Mesh.ArrayType.Index];

                    for (int j = 0; j < indices.Length; j += 3)
                    {
                        Vector3 v0 = vertices[indices[j]];
                        Vector3 v1 = vertices[indices[j + 1]];
                        Vector3 v2 = vertices[indices[j + 2]];

                        volume += Math.Abs(v0.Dot(v1.Cross(v2))) / 6f;
                    }
                }

                return volume;
            }
            default: return 0f;
        }
    }

    private static float GetCollisionShapeVolume(CollisionShape3D collisionShape)
    {
        Shape3D shape = collisionShape.Shape;

        switch (shape)
        {
            case BoxShape3D box:
            {
                Vector3 extents = box.Size;
                return extents.X * extents.Y * extents.Z;
            }
            case SphereShape3D sphere:
            {
                float radius = sphere.Radius;
                return 1.333f * Mathf.Pi * Mathf.Pow(radius, 3);
            }
            case CapsuleShape3D capsule:
            {
                float radius = capsule.Radius;
                float height = capsule.Height;

                float cylinderHeight = Mathf.Max(0, height - 2f * radius);
                float cylinderVolume = Mathf.Pi * Mathf.Pow(radius, 2) * cylinderHeight;
                float sphereVolume = 1.333f * Mathf.Pi * Mathf.Pow(radius, 3);

                return cylinderVolume + sphereVolume;
            }
            default: return 0f;
        }
    }

    private static float GetCollisionShapeThickness(CollisionShape3D shape)
    {
        return shape.Shape switch
        {
            BoxShape3D box => box.Size.X, // TODO: consider size in all directions
            SphereShape3D sphere => sphere.Radius * 2,
            CapsuleShape3D capsule => capsule.Radius * 2,
            _ => 0f
        };
    }


    [GeneratedRegex("(?<!^)(?=[A-Z])", RegexOptions.Compiled)]
    private static partial Regex PascalCaseSplitRegex();

    private static string TranslateNameKeepSidePrefix(string name)
    {
        return PascalCaseSplitRegex().Replace(name, " ").ToLower().Trim();
    }

    private static string TranslateName(string nodeName)
    {
        RemoveLeftRightPrefix(ref nodeName);
        return PascalCaseSplitRegex().Replace(nodeName, " ").ToLower().Trim();
    }

    private static void RemoveLeftRightPrefix(ref string name)
    {
        string toRemove = "Left";
        int index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);

        if (index != -1) name = name.Remove(index, toRemove.Length);

        toRemove = "Right";
        index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);

        if (index != -1) name = name.Remove(index, toRemove.Length);
    }
}
