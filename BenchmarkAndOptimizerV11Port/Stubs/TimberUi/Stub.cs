using System; using System.Collections.Generic; using Bindito.Core;
namespace UiBuilder { public enum GameLabelSize { Normal, Big } public enum GameLabelColor { Default, Yellow } public enum ToggleStyle { Settings, GamePanel } public enum GameButtonSize { Medium, Small, Large } }
namespace TimberUi.CommonUi
{
    public readonly struct SliderValues<T> where T:IComparable<T>
    {
        public SliderValues(T low, T high, T @default) { Low=low; High=high; Default=@default; }
        public T Low { get; }
        public T High { get; }
        public T Default { get; }
    }
    public class GameSliderInt : UnityEngine.UIElements.VisualElement
    {
        public int Value { get; set; }
        public GameSliderInt AddEndLabel(Func<int,string> f)=>this;
        public GameSliderInt RegisterChange(Action<int> a)=>this;
        public GameSliderInt SetValueWithoutNotify(int v){Value=v;return this;}
    }
    public class NineSliceTextField : UnityEngine.UIElements.TextField { }
    public class NineSliceButton : UnityEngine.UIElements.Button { }
}
namespace Bindito.Core
{
    public static class UiBuilderExtensions
    {
        public static Configurator BindSingleton<T>(this Configurator c) where T:class => c;
        public static Configurator MultiBindSingleton<T,TImpl>(this Configurator c) where T:class where TImpl:class,T => c;
    }
}
namespace UnityEngine.UIElements
{
    public static class UiBuilderExtensions
    {
        public static T SetPadding<T>(this T e,float padding) where T:VisualElement=>e;
        public static T SetMargin<T>(this T e,float top,float right,float bottom,float left) where T:VisualElement=>e;
        public static T SetMarginBottom<T>(this T e,float margin=10f) where T:VisualElement=>e;
        public static T SetMarginRight<T>(this T e,float margin=10f) where T:VisualElement=>e;
        public static T SetWidthPercent<T>(this T e,float p) where T:VisualElement=>e;
        public static T SetMaxHeight<T>(this T e,float h) where T:VisualElement=>e;
        public static T SetMinSize<T>(this T e,float? minW,float? minH) where T:VisualElement=>e;
        public static T SetFlexGrow<T>(this T e,float flexGrow=1) where T:VisualElement=>e;
        public static T AlignItems<T>(this T e,Align align=Align.Center) where T:VisualElement=>e;
        public static T Initialize<T>(this T e,Timberborn.CoreUI.VisualElementInitializer initializer) where T:VisualElement=>e;
        public static VisualElement AddChild(this VisualElement p,Type? type=null,string? name=null,IEnumerable<string>? classes=null)=>new VisualElement();
        public static T AddChild<T>(this VisualElement p,string? name=null,IEnumerable<string>? classes=null) where T:VisualElement,new()=>new T();
        public static VisualElement AddRow(this VisualElement p,string? name=null)=>new VisualElement();
        public static Label AddGameLabel(this VisualElement p,string? text=null,string? name=null,IEnumerable<string>? additionalClasses=null,UiBuilder.GameLabelSize size=default,UiBuilder.GameLabelColor? color=default,bool bold=default,bool centered=default)=>new Label{ text=text??"" };
        public static Toggle AddToggle(this VisualElement p,string? text=null,string? name=null,IEnumerable<string>? additionalClasses=null,Action<bool>? onValueChanged=null,UiBuilder.ToggleStyle style=default)=>new Toggle{text=text??""};
        public static TimberUi.CommonUi.GameSliderInt AddSliderInt(this VisualElement p,string? label=null,string? name=null,IEnumerable<string>? additionalClasses=null,in TimberUi.CommonUi.SliderValues<int>? values=default)=>new TimberUi.CommonUi.GameSliderInt();
        public static Timberborn.CoreUI.NineSliceButton AddMenuButton(this VisualElement p,string? text=null,Action? onClick=null,string? name=null,IEnumerable<string>? additionalClasses=null,UiBuilder.GameButtonSize? size=default,bool stretched=false)=>new Timberborn.CoreUI.NineSliceButton{text=text??""};
        public static Timberborn.CoreUI.NineSliceTextField AddTextField(this VisualElement p,string? name=null,Action<string>? changeCallback=null,IEnumerable<string>? additionalClasses=null)=>new Timberborn.CoreUI.NineSliceTextField();
        public static ScrollView AddScrollView(this VisualElement p,string? name=null,IEnumerable<string>? additionalClasses=null,bool greenDecorated=true)=>new ScrollView();
        public static VisualElement InsertSelfAfter(this VisualElement element, VisualElement target)=>element;
        public static T SetWidth<T>(this T element, float width) where T : VisualElement => element;
    }
}
