namespace UnityEngine.UIElements;

public static class VisualElementExtensions
{

    extension<T>(T el) where T : VisualElement
    {

        public void SetFullscreen()
        {
            var s = el.style;
            s.position = Position.Absolute;
            s.left = s.right = s.top = s.bottom = 0;
        }

        public T SetBorderRadius(float radius)
        {
            var s = el.style;
            s.borderBottomLeftRadius = s.borderTopLeftRadius = s.borderBottomRightRadius = s.borderTopRightRadius = radius;
            return el;
        }

    }

}
