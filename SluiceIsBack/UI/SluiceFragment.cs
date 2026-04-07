using System.Globalization;

namespace SluiceIsBack.UI;

#nullable disable

public class SluiceFragment : IEntityPanelFragment
{
    static readonly float WaterLevelChangeStep = 0.05f;

    static readonly float ContaminationChangeStep = 0.01f;

    static readonly string AutoClosedLocKey = "Building.Sluice.Mode.AutoClosed";

    static readonly string AutoOpenKey = "Building.Sluice.Mode.AutoOpen";

    static readonly string ClosedLocKey = "Building.Sluice.Mode.Closed";

    static readonly string OpenLocKey = "Building.Sluice.Mode.Open";

    static readonly string MaximumDepthToggleLocKey = "Building.Sluice.MaximumDepthToggle";

    static readonly string DownstreamDepthLocKey = "Building.Sluice.DownstreamDepth";

    readonly VisualElementLoader _visualElementLoader;

    readonly SluiceToggleFactory _sluiceToggleFactory;

    readonly ILoc _loc;

    VisualElement _root;

    SluiceToggle _modeToggle;

    Label _modeLabel;

    Toggle _waterLevelToggle;

    PreciseSlider _waterLevelSlider;

    Toggle _aboveContaminationToggle;

    PreciseSlider _aboveContaminationSlider;

    Toggle _belowContaminationToggle;

    PreciseSlider _belowContaminationSlider;

    Toggle _synchronizeToggle;

    Label _depthLabel;

    Label _contaminationLabel;

    Sluice _sluice;

    SluiceState _sluiceState;

    bool _sliderInitialization;

    readonly Phrase _contaminationPhrase = Phrase.New("Building.Sluice.Contamination").FormatPercentRounded();

    readonly Phrase _aboveContaminationPhrase = Phrase.New("Building.Sluice.AboveContaminationToggle").FormatPercentRounded();

    readonly Phrase _belowContaminationPhrase = Phrase.New("Building.Sluice.BelowContaminationToggle").FormatPercentRounded();

    int Range => _sluice.MaxHeight - _sluice.MinHeight;

    float WaterLevelSliderValue => (float)Range + _sluiceState.OutflowLimit;

    public SluiceFragment(VisualElementLoader visualElementLoader, SluiceToggleFactory sluiceToggleFactory, ILoc loc)
    {
        _visualElementLoader = visualElementLoader;
        _sluiceToggleFactory = sluiceToggleFactory;
        _loc = loc;
    }

    public VisualElement InitializeFragment()
    {
        _root = _visualElementLoader.LoadVisualElement("Game/EntityPanel/SluiceFragment");
        _modeToggle = _sluiceToggleFactory.Create(_root.Q<VisualElement>("ModeToggle"));
        _modeLabel = _root.Q<Label>("Mode");
        _waterLevelToggle = _root.Q<Toggle>("WaterLevelToggle");
        _waterLevelSlider = _root.Q<PreciseSlider>("WaterLevelSlider");
        _aboveContaminationToggle = _root.Q<Toggle>("AboveContaminationToggle");
        _aboveContaminationSlider = _root.Q<PreciseSlider>("AboveContaminationSlider");
        _belowContaminationToggle = _root.Q<Toggle>("BelowContaminationToggle");
        _belowContaminationSlider = _root.Q<PreciseSlider>("BelowContaminationSlider");
        _synchronizeToggle = _root.Q<Toggle>("Synchronize");
        _depthLabel = _root.Q<Label>("DepthLabel");
        _contaminationLabel = _root.Q<Label>("ContaminationLabel");
        _waterLevelToggle.RegisterValueChangedCallback(OnWaterLevelToggleChanged);
        _waterLevelSlider.SetValueChangedCallback(OnWaterLevelSliderChanged);
        _waterLevelSlider.SetStepWithoutNotify(WaterLevelChangeStep);
        _aboveContaminationToggle.RegisterValueChangedCallback(OnAboveContaminationToggleChanged);
        _aboveContaminationSlider.SetValueChangedCallback(OnAboveContaminationSliderChanged);
        _aboveContaminationSlider.SetStepWithoutNotify(ContaminationChangeStep);
        _belowContaminationToggle.RegisterValueChangedCallback(OnBelowContaminationToggleChanged);
        _belowContaminationSlider.SetValueChangedCallback(OnBelowContaminationSliderChanged);
        _belowContaminationSlider.SetStepWithoutNotify(ContaminationChangeStep);
        _synchronizeToggle.RegisterValueChangedCallback(ToggleSynchronization);
        _root.ToggleDisplayStyle(visible: false);
        return _root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        _sluice = entity.GetComponent<Sluice>();
        if ((bool)_sluice)
        {
            _sluiceState = _sluice.GetComponent<SluiceState>();
            _modeToggle.Show(_sluiceState);
            _sliderInitialization = true;
            _waterLevelToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnOutflow);
            _waterLevelSlider.UpdateValuesWithoutNotify(WaterLevelSliderValue, Range);
            _aboveContaminationToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnAbove);
            _aboveContaminationSlider.UpdateValuesWithoutNotify(_sluiceState.OnAboveLimit, 1f);
            _belowContaminationToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnBelow);
            _belowContaminationSlider.UpdateValuesWithoutNotify(_sluiceState.OnBelowLimit, 1f);
            _sliderInitialization = false;
        }
    }

    public void ClearFragment()
    {
        _sluice = null;
        _sluiceState = null;
        _modeToggle.Clear();
        _root.ToggleDisplayStyle(visible: false);
    }

    public void UpdateFragment()
    {
        if ((bool)_sluice)
        {
            UpdateModeToggle();
            UpdateAutomation();
            _synchronizeToggle.SetValueWithoutNotify(_sluiceState.IsSynchronized);
            _depthLabel.text = _loc.T(DownstreamDepthLocKey, FormatValue(_sluice.TargetDepth));
            _contaminationLabel.text = _loc.T(_contaminationPhrase, _sluice.Contamination);
            _root.ToggleDisplayStyle(visible: true);
        }
        else
        {
            _root.ToggleDisplayStyle(visible: false);
        }
    }

    void ToggleSynchronization(ChangeEvent<bool> evt)
    {
        _sluiceState.ToggleSynchronization(evt.newValue);
        _waterLevelSlider.SetValueWithoutNotify(WaterLevelSliderValue);
    }

    void OnWaterLevelToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            _sluiceState.EnableAutoCloseOnOutflow();
        }
        else
        {
            _sluiceState.DisableAutoCloseOnOutflow();
        }
    }

    void OnWaterLevelSliderChanged(float newValue)
    {
        if (!_sliderInitialization)
        {
            ChangeFlow(newValue);
        }
    }

    void OnAboveContaminationToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            _sluiceState.EnableAutoCloseOnAbove();
        }
        else
        {
            _sluiceState.DisableAutoCloseOnAbove();
        }
    }

    void OnAboveContaminationSliderChanged(float newValue)
    {
        if (!_sliderInitialization)
        {
            ChangeAboveContaminationLimit(newValue);
        }
    }

    void OnBelowContaminationToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            _sluiceState.EnableAutoCloseOnBelow();
        }
        else
        {
            _sluiceState.DisableAutoCloseOnBelow();
        }
    }

    void OnBelowContaminationSliderChanged(float newValue)
    {
        if (!_sliderInitialization)
        {
            ChangeBelowContaminationLimit(newValue);
        }
    }

    void UpdateModeToggle()
    {
        _modeToggle.Update();
        if (_sluiceState.AutoMode && _sluice.IsOpen)
        {
            _modeLabel.text = _loc.T(AutoOpenKey);
        }
        else if (_sluiceState.AutoMode && !_sluice.IsOpen)
        {
            _modeLabel.text = _loc.T(AutoClosedLocKey);
        }
        else if (!_sluiceState.AutoMode && _sluiceState.IsOpen)
        {
            _modeLabel.text = _loc.T(OpenLocKey);
        }
        else
        {
            _modeLabel.text = _loc.T(ClosedLocKey);
        }
    }

    void UpdateAutomation()
    {
        _waterLevelSlider.UpdateValuesWithoutNotify(WaterLevelSliderValue, Range);
        _waterLevelToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnOutflow);
        _waterLevelToggle.text = _loc.T(MaximumDepthToggleLocKey, FormatValue(_waterLevelSlider.Value));
        _aboveContaminationSlider.SetValueWithoutNotify(_sluiceState.OnAboveLimit);
        _aboveContaminationToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnAbove);
        _aboveContaminationToggle.text = _loc.T(_aboveContaminationPhrase, _aboveContaminationSlider.Value);
        _belowContaminationSlider.SetValueWithoutNotify(_sluiceState.OnBelowLimit);
        _belowContaminationToggle.SetValueWithoutNotify(_sluiceState.AutoCloseOnBelow);
        _belowContaminationToggle.text = _loc.T(_belowContaminationPhrase, _belowContaminationSlider.Value);
    }

    void ChangeFlow(float newHeight)
    {
        if (WaterLevelSliderValue != newHeight)
        {
            _sluiceState.SetOutflowLimit(newHeight - (float)Range);
        }
    }

    void ChangeAboveContaminationLimit(float newValue)
    {
        if (_sluiceState.OnAboveLimit != newValue)
        {
            _sluiceState.SetAboveContaminationLimit(newValue);
        }
    }

    void ChangeBelowContaminationLimit(float newValue)
    {
        if (_sluiceState.OnBelowLimit != newValue)
        {
            _sluiceState.SetBelowContaminationLimit(newValue);
        }
    }

    static string FormatValue(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
