using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Manages pseudo blood circulation between limbs.
/// Handles blood loss propagation and circulation graph management.
/// </summary>
public partial class CirculationSystem : Node
{
    private CirculationGraph _circulationGraph;
    private Dictionary<string, LimbInjuries> _limbInjuries;
    private Dictionary<string, float> _limbBleedRates;
    private Dictionary<string, float> _limbBloodVolumes;
    private Dictionary<string, float> _limbMaxBloodVolumes;
    
    private float _totalBloodVolume;
    private float _currentBloodVolume;
    private float _bloodLossRate;
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// Event fired when blood volume changes significantly.
    /// </summary>
    public event Action<float, float> OnBloodVolumeChanged; // (current, max)
    
    /// <summary>
    /// Event fired when total blood drops below critical threshold.
    /// </summary>
    public event Action OnCriticalBloodLoss;
    
    /// <summary>
    /// Gets the current total blood volume.
    /// </summary>
    public float CurrentBloodVolume => _currentBloodVolume;
    
    /// <summary>
    /// Gets the maximum total blood volume.
    /// </summary>
    public float TotalBloodVolume => _totalBloodVolume;
    
    /// <summary>
    /// Gets the current blood volume ratio (0.0 to 1.0).
    /// </summary>
    public float BloodVolumeRatio => _totalBloodVolume > 0 ? _currentBloodVolume / _totalBloodVolume : 0f;
    
    /// <summary>
    /// Gets the blood volume of a specific limb.
    /// </summary>
    public float GetLimbBloodVolume(string limbName)
    {
        return _limbBloodVolumes.GetValueOrDefault(limbName, 0f);
    }
    
    /// <summary>
    /// Gets the blood volume ratio of a specific limb.
    /// </summary>
    public float GetLimbBloodRatio(string limbName)
    {
        float current = GetLimbBloodVolume(limbName);
        float max = _limbMaxBloodVolumes.GetValueOrDefault(limbName, 1f);
        return max > 0 ? current / max : 0f;
    }
    
    /// <summary>
    /// Gets the bleed rate of a specific limb.
    /// </summary>
    public float GetLimbBleedRate(string limbName)
    {
        return _limbBleedRates.GetValueOrDefault(limbName, 0f);
    }
    
    /// <summary>
    /// Gets the circulation graph for external access.
    /// </summary>
    public CirculationGraph GetCirculationGraph()
    {
        return _circulationGraph;
    }
    
    /// <summary>
    /// Initializes the circulation system with limb data.
    /// </summary>
    public void Initialize(List<DamageModule> damageModules, Dictionary<string, float> bodyPartMassCoefficients, float totalBloodVolume)
    {
        _circulationGraph = LimbHierarchy.CreateHumanoidHierarchy();
        _limbInjuries = new Dictionary<string, LimbInjuries>();
        _limbBleedRates = new Dictionary<string, float>();
        _limbBloodVolumes = new Dictionary<string, float>();
        _limbMaxBloodVolumes = new Dictionary<string, float>();
        
        _totalBloodVolume = totalBloodVolume;
        _currentBloodVolume = totalBloodVolume;
        _bloodLossRate = 0f;
        
        // Initialize limb data from damage modules
        foreach (DamageModule module in damageModules)
        {
            string limbName = TranslateName(module.Name);
            
            _limbInjuries[limbName] = new LimbInjuries();
            _limbBleedRates[limbName] = 0f;
            _limbBloodVolumes[limbName] = module.BloodVolume;
            _limbMaxBloodVolumes[limbName] = module.MaxBloodVolume;
            
            // Update circulation graph with blood volumes
            int nodeIndex = _circulationGraph.GetNodeIndex(limbName);
            if (nodeIndex >= 0)
            {
                ref CirculationNode node = ref _circulationGraph.GetNode(nodeIndex);
                node.BloodVolume = module.BloodVolume;
                node.MaxBloodVolume = module.MaxBloodVolume;
            }
        }
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Updates the circulation system.
    /// Should be called each frame.
    /// </summary>
    public void UpdateCirculation(float delta)
    {
        if (!_isInitialized) return;
        
        float totalBloodLoss = 0f;
        
        // Calculate blood loss from all limbs
        foreach (var kvp in _limbBleedRates)
        {
            string limbName = kvp.Key;
            float bleedRate = kvp.Value;
            
            if (bleedRate > 0f)
            {
                float bloodLoss = bleedRate * delta;
                totalBloodLoss += bloodLoss;
                
                // Apply blood loss to the limb
                _limbBloodVolumes[limbName] = Mathf.Max(0f, _limbBloodVolumes[limbName] - bloodLoss);
                
                // Propagate blood loss to connected limbs
                PropagateBloodLoss(limbName, bloodLoss, delta);
            }
        }
        
        // Update total blood volume
        _currentBloodVolume = Mathf.Max(0f, _currentBloodVolume - totalBloodLoss);
        _bloodLossRate = totalBloodLoss / delta;
        
        // Fire events
        OnBloodVolumeChanged?.Invoke(_currentBloodVolume, _totalBloodVolume);
        
        if (BloodVolumeRatio < 0.4f)
        {
            OnCriticalBloodLoss?.Invoke();
        }
    }
    
    /// <summary>
    /// Propagates blood loss to connected limbs based on circulation graph.
    /// </summary>
    private void PropagateBloodLoss(string sourceLimb, float sourceBloodLoss, float delta)
    {
        int sourceIndex = _circulationGraph.GetNodeIndex(sourceLimb);
        if (sourceIndex < 0) return;
        
        ref CirculationNode sourceNode = ref _circulationGraph.GetNode(sourceIndex);
        
        // Propagate to parent limbs (toward heart)
        int parentIndex = sourceNode.ParentIndex;
        int distanceFromSource = 1;
        
        while (parentIndex >= 0)
        {
            ref CirculationNode parentNode = ref _circulationGraph.GetNode(parentIndex);
            
            // Calculate propagated bleed rate
            float propagatedRate = BloodPropagation.CalculatePropagatedBleedRate(
                sourceBloodLoss / delta,
                distanceFromSource
            );
            
            // Apply propagated blood loss
            float propagatedLoss = propagatedRate * delta * 0.5f; // 50% reduction for upstream propagation
            _limbBloodVolumes[parentNode.LimbName] = Mathf.Max(0f, _limbBloodVolumes[parentNode.LimbName] - propagatedLoss);
            
            parentIndex = parentNode.ParentIndex;
            distanceFromSource++;
        }
        
        // Propagate to child limbs (away from heart)
        PropagateToChildren(sourceIndex, sourceBloodLoss, delta, 1);
    }
    
    /// <summary>
    /// Recursively propagates blood loss to child limbs.
    /// </summary>
    private void PropagateToChildren(int parentIndex, float sourceBloodLoss, float delta, int distanceFromSource)
    {
        ref CirculationNode parentNode = ref _circulationGraph.GetNode(parentIndex);
        
        for (int i = 0; i < parentNode.ChildCount; i++)
        {
            int childIndex = _circulationGraph.GetChildIndex(parentIndex, i);
            if (childIndex < 0) continue;
            
            ref CirculationNode childNode = ref _circulationGraph.GetNode(childIndex);
            
            // Calculate propagated bleed rate
            float propagatedRate = BloodPropagation.CalculatePropagatedBleedRate(
                sourceBloodLoss / delta,
                distanceFromSource
            );
            
            // Apply propagated blood loss
            float propagatedLoss = propagatedRate * delta;
            _limbBloodVolumes[childNode.LimbName] = Mathf.Max(0f, _limbBloodVolumes[childNode.LimbName] - propagatedLoss);
            
            // Recursively propagate to grandchildren
            PropagateToChildren(childIndex, sourceBloodLoss, delta, distanceFromSource + 1);
        }
    }
    
    /// <summary>
    /// Adds an injury to a limb.
    /// </summary>
    public void AddInjury(string limbName, InjuryData injury)
    {
        if (!_limbInjuries.ContainsKey(limbName)) return;
        
        _limbInjuries[limbName].AddInjury(injury);
        _limbBleedRates[limbName] += injury.BleedRateIncrease;
        
        // Apply immediate blood loss
        _limbBloodVolumes[limbName] = Mathf.Max(0f, _limbBloodVolumes[limbName] - injury.ImmediateBloodLoss);
        _currentBloodVolume = Mathf.Max(0f, _currentBloodVolume - injury.ImmediateBloodLoss);
    }
    
    /// <summary>
    /// Removes all injuries from a limb (e.g., after dismemberment).
    /// </summary>
    public void ClearLimbInjuries(string limbName)
    {
        if (_limbInjuries.ContainsKey(limbName))
        {
            _limbInjuries[limbName].Clear();
            _limbBleedRates[limbName] = 0f;
        }
    }
    
    /// <summary>
    /// Gets the injuries for a specific limb.
    /// </summary>
    public LimbInjuries GetLimbInjuries(string limbName)
    {
        return _limbInjuries.GetValueOrDefault(limbName, new LimbInjuries());
    }
    
    /// <summary>
    /// Gets all limb injuries.
    /// </summary>
    public Dictionary<string, LimbInjuries> GetAllInjuries()
    {
        return _limbInjuries;
    }
    
    /// <summary>
    /// Gets the total bleed rate across all limbs.
    /// </summary>
    public float GetTotalBleedRate()
    {
        float total = 0f;
        foreach (var kvp in _limbBleedRates)
        {
            total += kvp.Value;
        }
        return total;
    }
    
    /// <summary>
    /// Checks if a specific limb is critically injured.
    /// </summary>
    public bool IsLimbCritical(string limbName, float threshold = 0.3f)
    {
        return GetLimbBloodRatio(limbName) < threshold;
    }
    
    /// <summary>
    /// Checks if the thorax is critically injured.
    /// </summary>
    public bool IsThoraxCritical(float threshold = 0.3f)
    {
        return IsLimbCritical("thorax", threshold);
    }
    
    /// <summary>
    /// Checks if the head is critically injured.
    /// </summary>
    public bool IsHeadCritical(float threshold = 0.0f)
    {
        return GetLimbBloodRatio("head") <= threshold;
    }
    
    /// <summary>
    /// Translates a node name to a limb name.
    /// </summary>
    private string TranslateName(string nodeName)
    {
        string name = nodeName;
        
        // Remove left/right prefixes
        string toRemove = "Left";
        int index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }
        
        toRemove = "Right";
        index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }
        
        // Convert PascalCase to lowercase with spaces
        return System.Text.RegularExpressions.Regex.Replace(name, "(?<!^)(?=[A-Z])", " ").ToLower().Trim();
    }
}
