using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.Items;
using System;
using System.Collections.Generic;
using Godot.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Handles asynchronous damage processing.
/// Calculates damage, blood loss, and dismemberment in background tasks.
/// </summary>
public partial class DamageProcessor : Node
{
    private readonly Queue<DamageRequest> _damageRequestQueue = new();
    private readonly List<Task<DamageResult>> _activeTasks = [];
    private readonly object _queueLock = new();
    
    private CirculationSystem _circulationSystem;
    private LimbManager _limbManager;
    private Node _materials;
    private Dictionary _materialData;
    
    private bool _isProcessing = false;
    private int _maxConcurrentTasks = 4;
    
    /// <summary>
    /// Event fired when a damage result is ready.
    /// </summary>
    public event Action<DamageProcessingResult> OnDamageResult;
    
    /// <summary>
    /// Event fired when dismemberment is required.
    /// </summary>
    public event Action<DismembermentRequest> OnDismembermentRequired;
    
    /// <summary>
    /// Gets the number of pending damage requests.
    /// </summary>
    public int PendingRequests => _damageRequestQueue.Count;
    
    /// <summary>
    /// Gets the number of active processing tasks.
    /// </summary>
    public int ActiveTasks => _activeTasks.Count;
    
    /// <summary>
    /// Initializes the damage processor.
    /// </summary>
    public void Initialize(CirculationSystem circulationSystem, LimbManager limbManager)
    {
        _circulationSystem = circulationSystem;
        _limbManager = limbManager;
        
        _materials = GetNode<Node>("/root/Materials");
        if (_materials != null)
        {
            _materialData = (Dictionary)_materials.Get("material_data");
        }
    }
    
    public override void _Ready()
    {
        SetProcess(true);
    }
    
    public override void _Process(double delta)
    {
        ProcessDamageQueue();
        CleanupCompletedTasks();
    }
    
    /// <summary>
    /// Submits a damage request for asynchronous processing.
    /// </summary>
    public async Task<DamageProcessingResult> ProcessDamageAsync(DamageRequest request)
    {
        lock (_queueLock)
        {
            _damageRequestQueue.Enqueue(request);
        }
        
        // Wait for the result
        TaskCompletionSource<DamageProcessingResult> tcs = new();
        
        // Subscribe to result event (this is a simplified approach)
        // In production, you'd want a more robust callback system
        await Task.Run(() => ProcessDamageRequest(request));
        
        DamageResult result = await ProcessDamageRequest(request);
        
        AnimationControlResult animControl = DetermineAnimationControl(result);
        
        return new DamageProcessingResult(result, animControl);
    }
    
    /// <summary>
    /// Processes the damage request queue.
    /// </summary>
    private void ProcessDamageQueue()
    {
        if (_isProcessing) return;
        if (_damageRequestQueue.Count == 0) return;
        if (_activeTasks.Count >= _maxConcurrentTasks) return;
        
        _isProcessing = true;
        
        try
        {
            DamageRequest request;
            lock (_queueLock)
            {
                if (_damageRequestQueue.Count == 0) return;
                request = _damageRequestQueue.Dequeue();
            }
            
            Task<DamageResult> task = Task.Run(() => ProcessDamageRequest(request));
            _activeTasks.Add(task);
            
            // Continue processing
            ProcessDamageQueue();
        }
        finally
        {
            _isProcessing = false;
        }
    }
    
    /// <summary>
    /// Processes a single damage request.
    /// </summary>
    public async Task<DamageResult> ProcessDamageRequest(DamageRequest request)
    {
        float startTime = (float)Time.GetUnixTimeFromSystem();
        
        if (request.Token.IsCancellationRequested)
        {
            return DamageResult.Failed(request.TargetLimb.Name);
        }
        
        try
        {
            // Calculate damage effects
            (float cutDepth, float kineticEnergy) = CalculateHitEffects(request);
            
            // Get limb thickness
            float limbThickness = _limbManager.GetLimbThickness(request.LimbIndex);
            
            // Calculate blood loss
            (float immediateBloodLoss, float bleedRateIncrease, float severity) = CalculateBloodLoss(
                request,
                cutDepth,
                limbThickness
            );
            
            // Determine injury severity level
            InjurySeverity severityLevel = InjurySeverityHelper.GetSeverityFromRatio(severity);
            
            // Check if dismemberment should occur
            bool shouldDismember = DismembermentConfigs.CanDismember(
                request.TargetLimb.Name,
                cutDepth,
                limbThickness
            );
            
            // Create injury data
            InjuryData injury = new InjuryData(
                request.TargetLimb.Name,
                severity,
                immediateBloodLoss,
                bleedRateIncrease,
                request.HitInfo.HitPosition,
                request.HitInfo.HitNormal,
                cutDepth
            );
            
            // Apply injury to circulation system
            _circulationSystem.AddInjury(request.TargetLimb.Name, injury);
            
            float processingTime = (float)Time.GetUnixTimeFromSystem() - startTime;
            
            DamageResult result = new DamageResult(
                success: true,
                limbName: request.TargetLimb.Name,
                immediateBloodLoss: immediateBloodLoss,
                bleedRateIncrease: bleedRateIncrease,
                shouldDismember: shouldDismember,
                cutDepth: cutDepth,
                limbThickness: limbThickness,
                severity: severity,
                severityLevel: severityLevel,
                hitPosition: request.HitInfo.HitPosition,
                hitNormal: request.HitInfo.HitNormal,
                processingTime: processingTime
            );
            
            // Fire result event
            OnDamageResult?.Invoke(new DamageProcessingResult(result, DetermineAnimationControl(result)));
            
            // If dismemberment is required, fire event
            if (shouldDismember)
            {
                DismembermentRequest dismemberRequest = CreateDismembermentRequest(
                    request,
                    cutDepth,
                    limbThickness
                );
                OnDismembermentRequired?.Invoke(dismemberRequest);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error processing damage request: {ex.Message}");
            return DamageResult.Failed(request.TargetLimb.Name);
        }
    }
    
    /// <summary>
    /// Calculates hit effects (cut depth and kinetic energy).
    /// </summary>
    private (float cutDepth, float kineticEnergy) CalculateHitEffects(DamageRequest request)
    {
        Vector3 hitDirection = (request.HitInfo.HitPosition - request.HitInfo.WeaponHitSource).Normalized();
        Weapon weapon = request.HitInfo.WeaponNode;
        
        float impactAngle = GetImpactAngleRadians(hitDirection, request.HitInfo.HitNormal);
        float weaponSharpnessDivisor = weapon.Sharpness * 1.2f + 1f;
        
        float effectiveEnergyAbsorption = GetEffectiveEnergyAbsorption(
            GetEnergyAbsorption(request.TargetLimb.Material),
            GetImpactDepth(request),
            GetThicknessInLineOfSight(impactAngle, request.TargetLimb.Thickness)
        );
        
        float inflictedKineticEnergy = GetInflictedKineticEnergy(
            weapon.Mass,
            GetVelocityAtImpactAngle(request.HitInfo.WeaponVelocity.Length(), impactAngle),
            weaponSharpnessDivisor,
            effectiveEnergyAbsorption
        );
        
        return (GetImpactDepth(request), inflictedKineticEnergy);
    }
    
    /// <summary>
    /// Calculates blood loss from damage.
    /// </summary>
    private (float immediateBloodLoss, float bleedRateIncrease, float severity) CalculateBloodLoss(
        DamageRequest request,
        float cutDepth,
        float limbThickness)
    {
        // Calculate cut dimensions
        float cutTime = cutDepth / request.HitInfo.WeaponVelocity.Length();
        float cutArea = cutTime * request.HitInfo.WeaponVelocity.Length() * cutDepth;
        float cutVolume = cutArea * cutTime * cutDepth;
        float bloodLossFactor = cutVolume / request.TargetLimb.Volume;
        
        // Calculate immediate blood loss
        float immediateBloodLoss = request.TargetLimb.MaxBloodVolume * (bloodLossFactor + GetImpactDepthRatio(cutDepth, limbThickness));
        
        // Calculate bleed rate increase
        float bleedRateIncrease = request.TargetLimb.MaxBloodVolume * bloodLossFactor;
        
        return (immediateBloodLoss, bleedRateIncrease, bloodLossFactor);
    }
    
    /// <summary>
    /// Creates a dismemberment request from damage data.
    /// </summary>
    private DismembermentRequest CreateDismembermentRequest(
        DamageRequest request,
        float cutDepth,
        float limbThickness)
    {
        DismembermentConfig config = DismembermentConfigs.GetConfigForLimb(request.TargetLimb.Name);
        
        Vector3 detachForce = DismembermentConfigs.CalculateDetachForce(
            request.HitInfo.WeaponVelocity,
            request.HitInfo.HitNormal,
            config
        );
        
        Vector3 detachTorque = DismembermentConfigs.CalculateDetachTorque(
            request.HitInfo.WeaponVelocity,
            request.HitInfo.HitNormal,
            config
        );
        
        return new DismembermentRequest(
            request.TargetLimb.Name,
            request.HitInfo.HitPosition,
            detachForce,
            detachTorque
        );
    }
    
    /// <summary>
    /// Determines whether the attack animation should continue or stop.
    /// </summary>
    private AnimationControlResult DetermineAnimationControl(DamageResult result)
    {
        if (!result.Success)
        {
            return AnimationControlResult.Ignore;
        }
        
        // If dismemberment occurred, continue animation
        if (result.ShouldDismember)
        {
            return AnimationControlResult.Continue;
        }
        
        // If cut depth is significant enough, continue
        if (result.CutDepth >= result.LimbThickness * 0.5f)
        {
            return AnimationControlResult.Continue;
        }
        
        // Otherwise, stop animation
        return AnimationControlResult.Stop;
    }
    
    /// <summary>
    /// Cleans up completed tasks.
    /// </summary>
    private void CleanupCompletedTasks()
    {
        for (int i = _activeTasks.Count - 1; i >= 0; i--)
        {
            if (_activeTasks[i].IsCompleted)
            {
                _activeTasks.RemoveAt(i);
            }
        }
    }
    
    // Helper methods for damage calculations
    
    private float GetImpactDepth(DamageRequest request)
    {
        return GetPerforation(
            request.HitInfo.EffectiveWeaponLength,
            GetDensity(request.HitInfo.WeaponNode.Material),
            GetDensity(request.TargetLimb.Material, request.TargetLimb.Thickness)
        );
    }
    
    private float GetImpactDepthRatio(float cutDepth, float limbThickness)
    {
        return cutDepth / limbThickness;
    }
    
    private static float GetEffectiveEnergyAbsorption(float absorption, float impactDepth, float hitThicknessLos)
    {
        return absorption - (impactDepth / hitThicknessLos);
    }
    
    private static float GetInflictedKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor, float energyAbsorption)
    {
        return GetImpactKineticEnergy(workingMass, impactVelocity, sharpnessFactor) * (1 - energyAbsorption);
    }
    
    private static float GetImpactKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor)
    {
        return workingMass / 2 * Mathf.Pow(impactVelocity / sharpnessFactor, 2);
    }
    
    private static float GetVelocityAtImpactAngle(float impactVelocity, float impactAngle)
    {
        return Mathf.Abs(impactVelocity * Mathf.Cos(impactAngle));
    }
    
    private static float GetPerforation(float workingLength, float workingDensity, float targetDensity)
    {
        return workingLength * (workingDensity / targetDensity);
    }
    
    private float GetEnergyAbsorption(string material)
    {
        if (_materialData == null || !_materialData.ContainsKey(material))
        {
            return 0.5f; // Default absorption
        }
        
        return (float)((Dictionary)_materialData[material])["absorption"];
    }
    
    private static float GetImpactAngleRadians(Vector3 hitDirection, Vector3 hitNormal)
    {
        return Mathf.Acos(hitDirection.Dot(hitNormal));
    }
    
    private float GetDensity(string material, float thickness)
    {
        float colliderDensity = GetDensity(material);
        if (material.Equals("gambeson")) colliderDensity *= thickness;
        return colliderDensity;
    }
    
    private float GetDensity(string material)
    {
        if (_materialData == null || !_materialData.ContainsKey(material))
        {
            return 1000f; // Default density (water)
        }
        
        return (float)((Dictionary)_materialData[material])["density"];
    }
    
    private static float GetThicknessInLineOfSight(float impactAngle, float thickness)
    {
        return Mathf.Abs(thickness / Mathf.Cos(impactAngle));
    }
}
