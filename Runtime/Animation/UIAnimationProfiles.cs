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
        public UIAnimationState normal = UIAnimationDefaults.CreateButtonNormal();

        [TabGroup("Highlighted")]
        [HideLabel]
        public UIAnimationState highlighted = UIAnimationDefaults.CreateButtonHighlighted();

        [TabGroup("Pressed")]
        [HideLabel]
        public UIAnimationState pressed = UIAnimationDefaults.CreateButtonPressed();

        [TabGroup("Selected")]
        [HideLabel]
        public UIAnimationState selected = UIAnimationDefaults.CreateButtonSelected();

        [TabGroup("Disabled")]
        [HideLabel]
        public UIAnimationState disabled = UIAnimationDefaults.CreateButtonDisabled();

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
        public UIAnimationState show = UIAnimationDefaults.CreateContainerShow();

        [TabGroup("Hide")]
        [HideLabel]
        public UIAnimationState hide = UIAnimationDefaults.CreateContainerHide();

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
