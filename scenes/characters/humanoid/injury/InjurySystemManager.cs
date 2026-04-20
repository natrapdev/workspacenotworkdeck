using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;
using MyFirst3DGame.scenes.characters.states;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Central coordinator for the injury and death system.
/// Manages all subsystems and provides the main interface for damage processing.
/// </summary>
public partial class InjurySystemManager : Node, IDamageReceiver
{
    // Subsystems
    private CirculationSystem _circulationSystem;
    private DamageProcessor _damageProcessor;
    private LimbManager _limbManager;
    private DeathSystem _deathSystem;
    private InjuryEffectSystem _injuryEffectSystem;
    
    // References
    private HumanoidModel _humanoid;
    private DamageModel _damageModel;
    private HumanoidSkeleton _skeleton;
    private HumanoidResource _resource;
    
    // State
    private bool _isInitialized = false;
    private bool _isProcessingDamage = false;
    private readonly List<CancellationTokenSource> _activeCancellations = new();
    
    // Events
    /// <summary>
    /// Event fired when damage is processed.
    /// </summary>
    public event Action<DamageProcessingResult> OnDamageProcessed;
    
    /// <summary>
    /// Event fired when a limb is dismembered.
    /// </summary>
    public event Action<DismembermentResult> OnLimbDismembered;
    
    /// <summary>
    /// Event fired when the character dies.
    /// </summary>
    public event Action<DeathSystem.DeathCondition?> OnCharacterDeath;
    
    /// <summary>
    /// Event fired when stat modifiers change.
    /// </summary>
    public event Action<CharacterStatModifiers> OnStatModifiersChanged;
    
    // Properties
    /// <summary>
    /// Gets the circulation system.
    /// </summary>
    public CirculationSystem Circulation => _circulationSystem;
    
    /// <summary>
    /// Gets the limb manager.
    /// </summary>
    public LimbManager LimbManager => _limbManager;
    
    /// <summary>
    /// Gets the death system.
    /// </summary>
    public DeathSystem Death => _deathSystem;
    
    /// <summary>
    /// Gets the injury effect system.
    /// </summary>
    public InjuryEffectSystem InjuryEffects => _injuryEffectSystem;
    
    /// <summary>
    /// Gets whether the system is initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// Gets whether the character is dead.
    /// </summary>
    public bool IsDead => _deathSystem?.IsDead ?? false;
    
    /// <summary>
    /// Gets the current health state.
    /// </summary>
    public HealthState GetHealthState()
    {
        if (!_isInitialized) return new HealthState();
        
        return new HealthState(
            _circulationSystem.TotalBloodVolume,
            _circulationSystem.CurrentBloodVolume,
            _circulationSystem.GetLimbBloodVolume("thorax"),
            _circulationSystem.GetLimbBloodVolume("head")
        );
    }
    
    /// <summary>
    /// Initializes the injury system for a humanoid.
    /// </summary>
    public void InitializeInjurySystem(HumanoidModel humanoid)
    {
        _humanoid = humanoid;
        
        // Get references
        _damageModel = humanoid.GetNode<DamageModel>("DamageModel");
        _skeleton = (HumanoidSkeleton)humanoid.Skeleton;
        _resource = humanoid.Resource;
        
        if (_damageModel == null || _skeleton == null || _resource == null)
        {
            GD.PrintErr("Failed to initialize injury system: Missing required nodes");
            return;
        }
        
        // Initialize damage model if needed
        if (_damageModel.DamageModules.Count == 0)
        {
            _damageModel.OnReady();
        }
        
        // Create and initialize subsystems
        CreateSubsystems();
        
        // Wire up events
        WireUpEvents();
        
        _isInitialized = true;
        
        GD.Print($"Injury system initialized for {humanoid.Name}");
    }
    
    /// <summary>
    /// Creates and initializes all subsystems.
    /// </summary>
    private void CreateSubsystems()
    {
        // Create circulation system
        _circulationSystem = new CirculationSystem
        {
            Name = "CirculationSystem"
        };
        AddChild(_circulationSystem);
        _circulationSystem.Initialize(
            _damageModel.DamageModules,
            _resource.BodyPartMassCoefficients,
            _resource.TotalBloodVolume
        );
        
        // Create limb manager
        _limbManager = new LimbManager
        {
            Name = "LimbManager"
        };
        AddChild(_limbManager);
        _limbManager.Initialize(_damageModel.DamageModules, _skeleton);
        
        // Create damage processor
        _damageProcessor = new DamageProcessor
        {
            Name = "DamageProcessor"
        };
        AddChild(_damageProcessor);
        _damageProcessor.Initialize(_circulationSystem, _limbManager);
        
        // Create death system
        _deathSystem = new DeathSystem
        {
            Name = "DeathSystem"
        };
        AddChild(_deathSystem);
        _deathSystem.Initialize(_circulationSystem, _skeleton, _humanoid);
        
        // Create injury effect system
        _injuryEffectSystem = new InjuryEffectSystem
        {
            Name = "InjuryEffectSystem"
        };
        AddChild(_injuryEffectSystem);
        _injuryEffectSystem.Initialize(_circulationSystem, _limbManager, _humanoid);
    }
    
    /// <summary>
    /// Wires up events between subsystems.
    /// </summary>
    private void WireUpEvents()
    {
        // Damage processor events
        _damageProcessor.OnDamageResult += OnDamageResult;
        _damageProcessor.OnDismembermentRequired += OnDismembermentRequired;
        
        // Limb manager events
        _limbManager.OnLimbDismembered += OnLimbDismemberedInternal;
        _limbManager.OnLimbAttachmentChanged += OnLimbAttachmentChangedInternal;
        
        // Death system events
        _deathSystem.OnDeath += OnDeathInternal;
        
        // Circulation system events
        _circulationSystem.OnCriticalBloodLoss += OnCriticalBloodLossInternal;
        
        // Injury effect system events
        _injuryEffectSystem.OnStatModifiersChanged += OnStatModifiersChangedInternal;
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized) return;
        
        // Update circulation (blood loss propagation)
        _circulationSystem.UpdateCirculation((float)delta);
    }
    
    /// <summary>
    /// Processes damage asynchronously.
    /// Called by HumanoidCombat when a hit is detected.
    /// </summary>
    public async Task<DamageProcessingResult> ProcessDamageAsync(HitInfo hitInfo)
    {
        if (!_isInitialized || _deathSystem.IsDead)
        {
            return new DamageProcessingResult(
                DamageResult.Failed("unknown"),
                AnimationControlResult.Ignore
            );
        }
        
        // Find the hit limb
        var hitParent = hitInfo.HitNode.GetParent();
        string hitName = hitParent.Name.ToString();
        string limbName = TranslateName(hitName);
        
        int limbIndex = _limbManager.GetLimbIndex(limbName);
        if (limbIndex < 0)
        {
            return new DamageProcessingResult(
                DamageResult.Failed(limbName),
                AnimationControlResult.Ignore
            );
        }
        
        // Check if limb is attached
        if (!_limbManager.IsLimbAttached(limbName))
        {
            return new DamageProcessingResult(
                DamageResult.Failed(limbName),
                AnimationControlResult.Ignore
            );
        }
        
        // Get damage module
        DamageModule targetLimb = _damageModel.DamageModules[limbIndex];
        
        // Create cancellation token
        CancellationTokenSource cts = new CancellationTokenSource();
        _activeCancellations.Add(cts);
        
        // Create damage request
        DamageRequest request = new DamageRequest(
            hitInfo,
            targetLimb,
            limbIndex,
            cts.Token
        );
        
        // Process damage
        DamageProcessingResult result = await _damageProcessor.ProcessDamageAsync(request);
        
        // Clean up cancellation token
        _activeCancellations.Remove(cts);
        cts.Dispose();
        
        return result;
    }
    
    /// <summary>
    /// Receives damage asynchronously (IDamageReceiver implementation).
    /// </summary>
    public async Task ReceiveDamageAsync(DamageRequest request)
    {
        if (!_isInitialized) return;
        
        await _damageProcessor.ProcessDamageRequest(request);
    }
    
    /// <summary>
    /// Checks if damage can be received (IDamageReceiver implementation).
    /// </summary>
    public bool CanReceiveDamage()
    {
        return _isInitialized && !_deathSystem.IsDead;
    }
    
    /// <summary>
    /// Registers a custom injury effect modifier.
    /// </summary>
    public void RegisterInjuryEffectModifier(IInjuryEffectModifier modifier)
    {
        if (_injuryEffectSystem != null)
        {
            _injuryEffectSystem.RegisterEffectModifier(modifier);
        }
    }
    
    /// <summary>
    /// Unregisters a custom injury effect modifier.
    /// </summary>
    public void UnregisterInjuryEffectModifier(IInjuryEffectModifier modifier)
    {
        if (_injuryEffectSystem != null)
        {
            _injuryEffectSystem.UnregisterEffectModifier(modifier);
        }
    }
    
    /// <summary>
    /// Forces dismemberment of a limb.
    /// </summary>
    public DismembermentResult ForceDismemberLimb(string limbName)
    {
        if (!_isInitialized) return new DismembermentResult(false, limbName, null, Vector3.Zero);
        
        DismembermentRequest request = new DismembermentRequest(
            limbName,
            Vector3.Zero,
            Vector3.Zero,
            Vector3.Zero
        );
        
        return _limbManager.DismemberLimb(request);
    }
    
    /// <summary>
    /// Forces death with a specific cause.
    /// </summary>
    public void ForceDeath(DeathSystem.DeathCondition cause)
    {
        if (_deathSystem != null)
        {
            _deathSystem.ForceDeath(cause);
        }
    }
    
    /// <summary>
    /// Resets the injury system (for respawning).
    /// </summary>
    public void Reset()
    {
        if (!_isInitialized) return;
        
        // Cancel all active damage processing
        foreach (var cts in _activeCancellations)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _activeCancellations.Clear();
        
        // Reset death system
        _deathSystem.Reset();
        
        // Reset injury effects
        _injuryEffectSystem.ResetModifiers();
        
        GD.Print("Injury system reset");
    }
    
    // Event handlers
    
    private void OnDamageResult(DamageProcessingResult result)
    {
        OnDamageProcessed?.Invoke(result);
        
        GD.Print($"Damage processed: {result.DamageResult.LimbName}, " +
                 $"Severity: {result.DamageResult.Severity:F2}, " +
                 $"Dismember: {result.DamageResult.ShouldDismember}, " +
                 $"Anim: {result.AnimationControl}");
    }
    
    private void OnDismembermentRequired(DismembermentRequest request)
    {
        GD.Print($"Dismemberment required for: {request.LimbName}");
        
        // Process dismemberment
        DismembermentResult result = _limbManager.DismemberLimb(request);
        
        if (result.Success)
        {
            // Clear injuries for dismembered limb
            _circulationSystem.ClearLimbInjuries(request.LimbName);
        }
    }
    
    private void OnLimbDismemberedInternal(DismembermentResult result)
    {
        OnLimbDismembered?.Invoke(result);
        
        GD.Print($"Limb dismembered: {result.LimbName}");
    }
    
    private void OnLimbAttachmentChangedInternal(string limbName, bool isAttached)
    {
        GD.Print($"Limb attachment changed: {limbName}, Attached: {isAttached}");
        
        // Trigger stat modifier update
        _injuryEffectSystem?.UpdateStatModifiers();
    }
    
    private void OnDeathInternal(DeathSystem.DeathCondition? cause)
    {
        OnCharacterDeath?.Invoke(cause);
        
        GD.Print($"Character died: {DeathSystem.GetDeathCauseDescription(cause ?? DeathSystem.DeathCondition.TotalBloodBelow40Percent)}");
    }
    
    private void OnCriticalBloodLossInternal()
    {
        GD.Print("Critical blood loss detected!");
    }
    
    private void OnStatModifiersChangedInternal(CharacterStatModifiers modifiers)
    {
        OnStatModifiersChanged?.Invoke(modifiers);
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
