using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [Serializable]
    public sealed class UIAnimationProfile
    {
        [Required]
        public RectTransform target;

        [InlineProperty]
        [HideLabel]
        public UIAnimationState animation = new UIAnimationState();
    }

    [Serializable]
    public sealed class UISelectableAnimationProfile
    {
        [TabGroup("Normal")]
        [HideLabel]
        public UIAnimationState normal = new UIAnimationState();

        [TabGroup("Highlighted")]
        [HideLabel]
        public UIAnimationState highlighted = new UIAnimationState();

        [TabGroup("Pressed")]
        [HideLabel]
        public UIAnimationState pressed = new UIAnimationState();

        [TabGroup("Selected")]
        [HideLabel]
        public UIAnimationState selected = new UIAnimationState();

        [TabGroup("Disabled")]
        [HideLabel]
        public UIAnimationState disabled = new UIAnimationState();

        public UIAnimationState GetState(UISelectableState state)
        {
            switch (state)
            {
                case UISelectableState.Highlighted:
                    return highlighted;
                case UISelectableState.Pressed:
                    return pressed;
                case UISelectableState.Selected:
                    return selected;
                case UISelectableState.Disabled:
                    return disabled;
                default:
                    return normal;
            }
        }

        public void CopyFrom(UISelectableAnimationProfile source)
        {
            if (source == null)
            {
                return;
            }

            normal.CopyFrom(source.normal);
            highlighted.CopyFrom(source.highlighted);
            pressed.CopyFrom(source.pressed);
            selected.CopyFrom(source.selected);
            disabled.CopyFrom(source.disabled);
        }
    }

    [Serializable]
    public sealed class UIContainerAnimationProfile
    {
        [TabGroup("Show")]
        [HideLabel]
        public UIAnimationState show = new UIAnimationState();

        [TabGroup("Hide")]
        [HideLabel]
        public UIAnimationState hide = new UIAnimationState();

        public void CopyFrom(UIContainerAnimationProfile source)
        {
            if (source == null)
            {
                return;
            }

            show.CopyFrom(source.show);
            hide.CopyFrom(source.hide);
        }
    }
}



