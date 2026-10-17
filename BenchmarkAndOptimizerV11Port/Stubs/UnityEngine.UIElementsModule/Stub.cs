using System;

namespace UnityEngine.UIElements
{
    public enum ScrollViewMode
    {
        Vertical,
        Horizontal,
        VerticalAndHorizontal
    }

    public enum ScrollerVisibility
    {
        Auto,
        AlwaysVisible,
        Hidden
    }

    public enum Align
    {
        Auto,
        FlexStart,
        Center,
        FlexEnd,
        Stretch
    }
    public enum DisplayStyle { Flex, None }
    public enum Position { Relative, Absolute }
    public readonly struct StyleLength { public static implicit operator StyleLength(float value) => new StyleLength(); }
    public readonly struct StyleEnum<T> where T : struct
    {
        public static implicit operator StyleEnum<T>(T value) => new StyleEnum<T>();
    }
    public interface IStyle
    {
        StyleEnum<DisplayStyle> display { get; set; }
        StyleEnum<Position> position { get; set; }
        StyleLength left { get; set; }
        StyleLength top { get; set; }
    }
    public class EventBase { }
    public class DetachFromPanelEvent : EventBase { }

    public class ChangeEvent<T> : EventBase
    {
        public T newValue { get; set; } = default!;
    }

    public delegate void EventCallback<in TEventType>(TEventType evt) where TEventType : EventBase;

    public class VisualElement
    {
        public IStyle style { get; } = null!;
        public string name { get; set; } = "";
        public VisualElement parent { get; } = null!;
        public System.Collections.Generic.IEnumerable<VisualElement> Children() => System.Array.Empty<VisualElement>();
        public void RemoveFromClassList(string className) { }
        public void Add(VisualElement child) { }
        public void AddToClassList(string className) { }
        public void Clear() { }
        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase { }
    }

    public static class UQueryExtensions
    {
        public static VisualElement Q(this VisualElement element, string name = null, string className = null) => null;
        public static T Q<T>(this VisualElement element, string name = null, string className = null) where T : VisualElement => null;
    }

    public class Label : VisualElement
    {
        public Label() { }
        public Label(string text) { this.text = text; }
        public string text { get; set; } = "";
    }

    public class Toggle : VisualElement
    {
        public Toggle() { }
        public string text { get; set; } = "";
        public bool value { get; set; }
        public void SetValueWithoutNotify(bool value) { this.value = value; }
    }

    public class ScrollView : VisualElement
    {
        public ScrollViewMode mode { get; set; }
        public ScrollerVisibility horizontalScrollerVisibility { get; set; }
        public ScrollerVisibility verticalScrollerVisibility { get; set; }
    }

    public class TextField : VisualElement
    {
        public string value { get; set; } = "";
    }

    public class Button : VisualElement
    {
        public string text { get; set; } = "";
        public event Action? clicked;
    }

    public class SliderInt : VisualElement
    {
        public int lowValue { get; set; }
        public int highValue { get; set; }
        public int value { get; set; }
        public void SetValueWithoutNotify(int value) { this.value = value; }
    }
}
