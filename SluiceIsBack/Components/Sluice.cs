namespace SluiceIsBack.Components;

#nullable disable

public class Sluice : TickableComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity, IDuplicable<Sluice>, IDuplicable
{
    static readonly ComponentKey SluiceKey = new("Sluice");
    static readonly PropertyKey<bool> ObstacleAddedKey = new("ObstacleAdded");
    static readonly PropertyKey<FlowControllerState> FlowControllerStateKey = new("FlowControllerState");
    static readonly float SluiceOverflowLimit = 0.02f;

    readonly IWaterService _waterService;
    readonly IThreadSafeWaterMap _threadSafeWaterMap;

    BlockObject _blockObject;

    SluiceState _sluiceState;

    WaterObstacleController _waterObstacleController;

    Vector3Int _sourceCoordinates;

    FlowDirection _flowDirection;

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
        _waterService = waterService;
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
        RemoveFlowController();
    }

    public void OnEnterFinishedState()
    {
        _waterService.AddDirectionLimiter(_blockObject.Coordinates, _blockObject.Orientation.ToFlowDirection());
        _waterObstacleController.UpdateState(_obstacleAdded);
        if (_flowControllerState == FlowControllerState.IncreaseFlow)
        {
            _waterService.SetControllerToIncreaseFlow(_blockObject.Coordinates);
        }
        else if (_flowControllerState == FlowControllerState.DecreaseFlow)
        {
            _waterService.SetControllerToDecreaseFlow(_blockObject.Coordinates);
        }
        EnableComponent();
    }

    public void OnExitFinishedState()
    {
        _waterService.RemoveFlowController(_blockObject.Coordinates);
        _waterService.RemoveDirectionLimiter(_blockObject.Coordinates);
        DisableComponent();
    }

    public void Save(IEntitySaver entitySaver)
    {
        IObjectSaver component = entitySaver.GetComponent(SluiceKey);
        component.Set(ObstacleAddedKey, _obstacleAdded);
        component.Set(FlowControllerStateKey, _flowControllerState);
    }

    public void Load(IEntityLoader entityLoader)
    {
        IObjectLoader component = entityLoader.GetComponent(SluiceKey);
        _obstacleAdded = component.Get(ObstacleAddedKey);
        _flowControllerState = component.Get(FlowControllerStateKey);
    }

    public void DuplicateFrom(Sluice source)
    {
        _sluiceState.SetStateAndSynchronize(source._sluiceState, MinHeight - MaxHeight);
    }

    void UpdateAutoMode()
    {
        bool flag = !IsAutoClosedByContamination();
        UpdateObstacle(!flag);
        if (flag && _sluiceState.AutoCloseOnOutflow)
        {
            UpdateOutflowLimit();
        }
        else
        {
            RemoveFlowController();
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
        float num = _threadSafeWaterMap.WaterHeightOrFloor(TargetCoordinates) - ((float)MaxHeight + _sluiceState.OutflowLimit);
        bool flag = _flowControllerState == FlowControllerState.IncreaseFlow;
        bool flag2 = (flag ? (num < SluiceOverflowLimit) : (num < 0f));
        if (flag2 != flag || _flowControllerState == FlowControllerState.NoControl)
        {
            if (flag2)
            {
                _waterService.SetControllerToIncreaseFlow(_blockObject.Coordinates);
                _flowControllerState = FlowControllerState.IncreaseFlow;
            }
            else
            {
                _waterService.SetControllerToDecreaseFlow(_blockObject.Coordinates);
                _flowControllerState = FlowControllerState.DecreaseFlow;
            }
        }
    }

    void RemoveFlowController()
    {
        if (_flowControllerState != FlowControllerState.NoControl)
        {
            _waterService.RemoveFlowController(_blockObject.Coordinates);
        }
        _flowControllerState = FlowControllerState.NoControl;
    }

    void UpdateObstacle(bool add)
    {
        _waterObstacleController.UpdateState(add);
        _obstacleAdded = add;
    }
}
