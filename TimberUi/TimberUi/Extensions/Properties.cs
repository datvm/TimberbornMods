namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    extension<T>(T element) where T : VisualElement
    {
        public T AddClass(string className)
        {
            element.classList.Add(className);
            return element;
        }

        public T AddClasses(params IEnumerable<string> classNames)
        {
            element.classList.AddRange(classNames);
            return element;
        }

        public T SetMargin(float margin) => element.SetMargin(margin, margin);

        public T SetMargin(float marginX = 0, float marginY = 0) => element.SetMargin(marginY, marginX, marginY, marginX);

        public T SetMargin(float top = 0, float right = 0, float bottom = 0, float left = 0)
        {
            element.style.marginTop = top;
            element.style.marginRight = right;
            element.style.marginBottom = bottom;
            element.style.marginLeft = left;
            return element;
        }

        public T SetMarginBottom(float margin = 10f)
        {
            element.style.marginBottom = margin;
            return element;
        }

        public T SetMarginRight(float margin = 10f)
        {
            element.style.marginRight = margin;
            return element;
        }

        public T SetMarginLeftAuto()
        {
            element.style.marginLeft = new StyleLength(StyleKeyword.Auto);
            return element;
        }

        public T SetPadding(float padding) => element.SetPadding(padding, padding);

        public T SetPadding(float paddingX = 0, float paddingY = 0) => element.SetPadding(paddingY, paddingX, paddingY, paddingX);

        public T SetPadding(float top = 0, float right = 0, float bottom = 0, float left = 0)
        {
            element.style.paddingTop = top;
            element.style.paddingRight = right;
            element.style.paddingBottom = bottom;
            element.style.paddingLeft = left;
            return element;
        }

        public T SetWidth(float width) => element.SetSize(width, null);
        public T SetHeight(float height) => element.SetSize(null, height);
        public T SetSize(float? width = default, float? height = default)
        {
            if (width is not null)
            {
                element.style.width = width.Value;
            }

            if (height is not null)
            {
                element.style.height = height.Value;
            }

            return element;
        }

        public T SetSize(float widthAndHeight) => element.SetSize(widthAndHeight, widthAndHeight);

        public T SetAsRow()
        {
            element.style.flexDirection = FlexDirection.Row;
            return element;
        }

        public T SetWrap(bool wrap = true)
        {
            element.style.flexWrap = wrap ? Wrap.Wrap : Wrap.NoWrap;
            return element;
        }

        public T SetMaxWidth(float maxWidth) => element.SetMaxSize(maxWidth, null);

        public T SetMaxHeight(float maxHeight) => element.SetMaxSize(null, maxHeight);

        public T SetMaxSize(float maxWH) => element.SetMaxSize(maxWH, maxWH);

        public T SetMaxSize(float? maxW, float? maxH)
        {
            if (maxW is not null)
            {
                element.style.maxWidth = maxW.Value;
            }
            if (maxH is not null)
            {
                element.style.maxHeight = maxH.Value;
            }

            return element;
        }

        public T SetMaxSizePercent(float? maxW, float? maxH)
        {
            var s = element.style;
            if (maxW is not null)
            {
                s.maxWidth = new Length(maxW.Value, LengthUnit.Percent);
            }
            if (maxH is not null)
            {
                s.maxHeight = new Length(maxH.Value, LengthUnit.Percent);
            }
            return element;
        }

        public T SetMinSize(float minWH) => element.SetMinSize(minWH, minWH);

        public T SetMinSize(float? minW, float? minH)
        {
            if (minW is not null)
            {
                element.style.minWidth = minW.Value;
            }
            if (minH is not null)
            {
                element.style.minHeight = minH.Value;
            }
            return element;
        }

        public T SetMinMaxSize(float? w, float? h)
        {
            var s = element.style;

            if (w is not null)
            {
                s.minWidth = s.maxWidth = s.width = w.Value;
            }
            if (h is not null)
            {
                s.minHeight = s.maxHeight = s.height = h.Value;
            }
            return element;
        }

        public T SetMinMaxSizePercent(float? w, float? h)
        {
            var s = element.style;
            if (w is not null)
            {
                s.minWidth = s.maxWidth = s.width = new Length(w.Value, LengthUnit.Percent);
            }
            if (h is not null)
            {
                s.minHeight = s.maxHeight = s.height = new Length(h.Value, LengthUnit.Percent);
            }
            return element;
        }

        public T SetWidthPercent(float percent) => element.SetSizePercent(percent, null);

        public T SetHeightPercent(float percent) => element.SetSizePercent(null, percent);

        public T SetSizePercent(float? w, float? h)
        {
            var s = element.style;
            if (w is not null)
            {
                s.width = new Length(w.Value, LengthUnit.Percent);
            }
            if (h is not null)
            {
                s.height = new Length(h.Value, LengthUnit.Percent);
            }
            return element;
        }

        public T SetFlexGrow(float flexGrow = 1)
        {
            element.style.flexGrow = flexGrow;
            return element;
        }

        public T SetFlexShrink(float flexShrink = 1)
        {
            element.style.flexShrink = flexShrink;
            return element;
        }

        public T SetDisplay(bool display)
        {
            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            return element;
        }

        public T AlignItems(Align align = Align.Center)
        {
            element.style.alignItems = align;
            return element;
        }

        public T JustifyContent(Justify justify = Justify.Center)
        {
            element.style.justifyContent = justify;
            return element;
        }

        public T SetBorder(Color? color = null, float? width = null)
        {
            var s = element.style;

            if (width is not null)
            {
                s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = s.borderLeftWidth = width.Value;
            }

            if (color is not null)
            {
                s.borderTopColor = s.borderRightColor = s.borderBottomColor = s.borderLeftColor = color.Value;
            }

            return element;
        }

        public T SetPosition(bool absolute = true)
        {
            element.style.position = absolute ? Position.Absolute : Position.Relative;
            return element;
        }
    }

    extension<T>(T element) where T : TextElement
    {
        public T AddLabelClasses(GameLabelStyle style, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default)
        {
            element.classList.AddRange(GetClasses(style, size, color, bold));
            return element;
        }
    }
}