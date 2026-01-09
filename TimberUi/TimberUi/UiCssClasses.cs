namespace TimberUi;

public static partial class UiCssClasses
{

    // Buttons
    public static readonly ImmutableArray<string> ButtonText = ["text--default", "text--bold"];
    public const string ButtonMenu = "menu-button";
    public const string ButtonWideMenu = "wide-menu-button";
    public const string ButtonBottomBar = "bottom-bar-button";
    public const string ButtonDevPanel = "dev-panel-button";
    public const string ButtonGame = "button-game";
    public const string ButtonEntityFragment = "entity-fragment__button";
    public static readonly ImmutableArray<string> ButtonPlusClasses = [ButtonPlus, "button-plus--margin"];
    public const string ButtonPlus = "button-plus";
    public const string ButtonMinus = "button-minus";

    public const string ButtonSquare = "button-square";

    public const string ButtonTopBarPrefix = "square-large--";

    public const string ButtonPfStretched = "--stretched";
    public const string ButtonPfSmall = "--small";
    public const string ButtonPfMedium = "--medium";
    public const string ButtonPfLarge = "--large";
    public const string ButtonPfClickable = "--clickable";

    public const string BottomBarButtonRed = "bottom-bar-button--red";

    // Dialogs
    public const string CloseButton = "close-button";

    // Dropdown
    public const string DropDownMenuClass = "settings-dropdown";

    // Toggle
    public static readonly ImmutableArray<string> ToggleSettings = ["settings-element", SettingsText, "settings-toggle"];

    // List/Scroll View
    public const string ScrollViewClass = "panel-list-view";
    public const string ListViewClass = "panel-list-view";
    public const string ScrollGreenDecorated = "scroll--green-decorated";
    public const string GameScrollView = "game-scroll-view";

    // Labels
    public const string LabelGamePrefix = "text--";
    public const string LabelGameTextNormal = "game-text-normal";
    public const string LabelGameTextBig = "game-text-big";
    public const string LabelGameTextBold = "text--bold";
    public const string LabelGameTextCentered = "text--centered";
    public const string LabelEntityPanelText = "entity-panel__text";
    public const string SettingsText = "settings-text";

    // Panel Fragment
    public const string FragmentClass = "entity-sub-panel";
    public const string FragmentBgPrefix = "bg-sub-box--";

    public const string TopRightItemClass = "top-right-item";

    public const string BgStripedGreen = "bg-striped--green";

    // Progress
    public const string ProgressBarClass = "progress-bar";

    // Textfields
    public const string TextFieldClass = "text-field";

    // Common colors
    public const string Brown = "brown";
    public const string Green = "green";
    public const string Blue = "blue";
    public const string PurpleStriped = "purple-striped";
    public const string PalePurple = "pale-purple";
    public const string RedStriped = "red-striped";
    public const string Frame = "frame";
    public const string Yellow = "yellow";
    public const string Red = "red";
    public const string Teal = "teal";

    // Sliders
    public static readonly ImmutableArray<string> SliderContainerClasses = ["settings-slider", SettingsText];
    public static readonly ImmutableArray<string> SliderClasses = ["settings-slider__slider"];
    public static readonly ImmutableArray<string> SliderEndLabelClasses = ["settings-slider__end-label"];

}