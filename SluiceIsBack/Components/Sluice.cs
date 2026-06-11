namespace SluiceIsBack.Components;

#nullable disable

/// <summary>
/// A sluice that can be opened or closed to control water flow, either manually or automatically based on water contamination or height. When open, the sluice allows water to flow freely.
/// When closed, it blocks water flow with a physical obstacle and/or an inflow limit.
/// </summary>
/// <remarks>
/// Timberborn's old flow-controller API could explicitly request increased or decreased flow at
/// the sluice tile. The current water API no longer exposes that controller, so this component
/// follows the game's FillValve approach instead: it stores the active flow amount in
/// <see cref="_currentFlow"/> and applies it through <c>IWaterService.SetInflowLimit</c>.
///
/// In automatic mode, contamination checks are evaluated first. If the source-side water should
/// close the sluice, the physical obstacle is enabled and any flow limit is cleared. Otherwise,
/// when outflow limiting is enabled, the sluice compares the downstream water height against
/// <c>MaxHeight + OutflowLimit</c>. Below the target height it slowly raises the inflow cap;
/// above the target height it lowers the cap using the same non-linear closing curve as
/// FillValve. A small hysteresis value keeps the flow from rapidly flipping states around the
/// target height.
///
/// Once the inflow cap reaches zero while closing, the sluice becomes a full obstacle. This is
/// not a perfect simulation-level match for the removed DecreaseFlow controller, but it preserves
/// the old player-facing behavior as closely as the current public water service allows.
/// </remarks>
public class Sluice : TickableComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity, IDuplicable<Sluice>, IDuplicable
{
    static readonly ComponentKey SluiceKey = new("Sluice");
    static readonly PropertyKey<bool> ObstacleAddedKey = new("ObstacleAdded");
    static readonly PropertyKey<FlowControllerState> FlowControllerStateKey = new("FlowControllerState");
    static readonly PropertyKey<float> CurrentFlowKey = new("CurrentFlow");

    static readonly float SluiceOverflowLimit = 0.02f;
    static readonly float OpeningStep = 0.005f;
    static readonly float MaxFlow = 10f;
    
    readonly WaterService _waterService;
    readonly IThreadSafeWaterMap _threadSafeWaterMap;

    // The removed FlowController API used to hold this state inside the water simulation.
    // In the current API, the sluice emulates it by changing the target tile's inflow cap.
    float _currentFlow;
    BlockObject _blockObject;
    SluiceState _sluiceState;
    WaterObstacleController _waterObstacleController;
    Vector3Int _sourceCoordinates;
    bool _obstacleAdded;

    FlowControllerState _flowControllerState;

    public Vector3Int TargetCoordinates { get; private set; }

    public bool IsOpen
    {
        get
        {
            if (!_obstacleAdded)
            {
                return _flowControllerState != FlowControllerState.DecreaseFlow;
            }
            return false;
        }
    }

    public int MinHeight
    {
        get
        {
            if (!_threadSafeWaterMap.TryGetColumnFloor(TargetCoordinates, out var floor))
            {
                return MaxHeight;
            }
            return floor;
        }
    }

    public int MaxHeight => _blockObject.Coordinates.z + 1;

    public float TargetDepth => _threadSafeWaterMap.WaterDepth(TargetCoordinates);

    public float Contamination => _threadSafeWaterMap.ColumnContamination(_sourceCoordinates);

    public Sluice(IWaterService waterService, IThreadSafeWaterMap threadSafeWaterMap)
    {
        _waterService = (WaterService)waterService;
        _threadSafeWaterMap = threadSafeWaterMap;
    }

    public void Awake()
    {
        _blockObject = GetComponent<BlockObject>();
        _sluiceState = GetComponent<SluiceState>();
        _waterObstacleController = GetComponent<WaterObstacleController>();
        DisableComponent();
    }

    public void InitializeEntity()
    {
        // The old sluice checks contamination before the gate and water height after the gate.
        _sourceCoordinates = _blockObject.TransformCoordinates(new Vector3Int(0, -1, 0));
        TargetCoordinates = _blockObject.TransformCoordinates(new Vector3Int(0, 1, 0));
    }

    public override void Tick()
    {
        if (_sluiceState.OutflowLimit < (float)(MinHeight - MaxHeight))
        {
            _sluiceState.SetOutflowLimit(MinHeight - MaxHeight);
        }
        if (_sluiceState.AutoMode)
        {
            UpdateAutoMode();
            return;
        }
        UpdateObstacle(!_sluiceState.IsOpen);
        RemoveFlowControl();
    }

    public void OnEnterFinishedState()
    {
        _waterService.AddDirectionLimiter(_blockObject.Coordinates, _blockObject.Orientation.ToFlowDirection());
        _waterObstacleController.UpdateState(_obstacleAdded);
        // Resume an in-progress automatic adjustment after loading or becoming finished again.
        if (_currentFlow > 0f)
        {
            _waterService.SetInflowLimit(_blockObject.Coordinates, _currentFlow);
        }
        EnableComponent();
    }

    public void OnExitFinishedState()
    {
        RemoveFlowControl();
        _waterService.RemoveDirectionLimiter(_blockObject.Coordinates);
        DisableComponent();
    }

    public void Save(IEntitySaver entitySaver)
    {
        var component = entitySaver.GetComponent(SluiceKey);
        component.Set(ObstacleAddedKey, _obstacleAdded);
        component.Set(FlowControllerStateKey, _flowControllerState);
        // CurrentFlow is needed because flow control is now expressed as an inflow limit.
        component.Set(CurrentFlowKey, _currentFlow);
    }

    public void Load(IEntityLoader entityLoader)
    {
        IObjectLoader component = entityLoader.GetComponent(SluiceKey);
        _obstacleAdded = component.Get(ObstacleAddedKey);
        _flowControllerState = component.Get(FlowControllerStateKey);
        _currentFlow = component.Has(CurrentFlowKey) ? component.Get(CurrentFlowKey) : 0f;
    }

    public void DuplicateFrom(Sluice source)
    {
        _sluiceState.SetStateAndSynchronize(source._sluiceState, MinHeight - MaxHeight);
    }

    void UpdateAutoMode()
    {
        // Contamination auto-close has priority over water-level regulation.
        bool isOpen = !IsAutoClosedByContamination();
        UpdateObstacle(!isOpen);
        if (isOpen && _sluiceState.AutoCloseOnOutflow)
        {
            UpdateOutflowLimit();
        }
        else
        {
            RemoveFlowControl();
        }
    }

    bool IsAutoClosedByContamination()
    {
        if (_sluiceState.AutoCloseOnAbove)
        {
            return GetContamination() > _sluiceState.OnAboveLimit;
        }
        if (_sluiceState.AutoCloseOnBelow)
        {
            return GetContamination() < _sluiceState.OnBelowLimit;
        }
        return false;
    }

    float GetContamination()
    {
        return (float)Math.Round(_threadSafeWaterMap.ColumnContamination(_sourceCoordinates), 2);
    }

    void UpdateOutflowLimit()
    {
        float targetHeight = (float)MaxHeight + _sluiceState.OutflowLimit;
        float heightDelta = _threadSafeWaterMap.WaterHeightOrFloor(TargetCoordinates) - targetHeight;
        // Preserve the old sluice's small hysteresis while translating IncreaseFlow/DecreaseFlow
        // into gradual changes to the new inflow-limit API.
        bool shouldIncreaseFlow = _flowControllerState == FlowControllerState.IncreaseFlow
            ? heightDelta < SluiceOverflowLimit
            : heightDelta < 0f;

        if (shouldIncreaseFlow && _currentFlow < float.MaxValue)
        {
            _currentFlow = _currentFlow > MaxFlow ? float.MaxValue : _currentFlow + OpeningStep;
            _waterService.SetInflowLimit(_blockObject.Coordinates, _currentFlow);
            _flowControllerState = FlowControllerState.IncreaseFlow;
        }
        else if (!shouldIncreaseFlow && _currentFlow > 0f)
        {
            _currentFlow = GetPreviousClosingFlow(_currentFlow);
            _waterService.SetInflowLimit(_blockObject.Coordinates, _currentFlow);
            _flowControllerState = FlowControllerState.DecreaseFlow;
        }
        else if (!shouldIncreaseFlow && _currentFlow == 0f)
        {
            // Once fully throttled, become a physical blocker. This is the closest replacement
            // for the removed simulator-level DecreaseFlow controller.
            RemoveFlowControl();
            UpdateObstacle(add: true);
        }
    }

    void UpdateObstacle(bool add)
    {
        _waterObstacleController.UpdateState(add);
        _obstacleAdded = add;
    }

    void RemoveFlowControl()
    {
        if (_flowControllerState != FlowControllerState.NoControl)
        {
            _waterService.RemoveInflowLimit(_blockObject.Coordinates);
        }
        _flowControllerState = FlowControllerState.NoControl;
        _currentFlow = 0f;
    }

    // Match FillValve's non-linear closing behavior: close quickly at high flows,
    // then step down gently near zero to avoid oscillating around the target height.
    static readonly ImmutableArray<float> ClosingCurve = [ 0f, 0.005f, 0.01f, 0.015f, 0.02f, 0.025f, 0.03f, 0.035f, 0.04f, 0.045f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f, 6f, 7f, 8f, 9f, MaxFlow, float.MaxValue, ];
    static float GetPreviousClosingFlow(float currentFlow)
    {
        for (int i = ClosingCurve.Length - 1; i >= 0; i--)
        {
            float candidate = ClosingCurve[i];
            if (candidate < currentFlow)
            {
                return candidate;
            }
        }
        return 0f;
    }

}
