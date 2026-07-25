using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Button Animation Preset", fileName = "UIButtonAnimationPreset")]
    public sealed class UIButtonAnimationPreset : ScriptableObject
    {
        public UISelectableAnimationProfile stateAnimations = UIAnimationDefaults.CreateButtonProfile();

        public void CopyFrom(UISelectableAnimationProfile source)
        {
            if (stateAnimations == null)
            {
                stateAnimations = new UISelectableAnimationProfile();
            }

            stateAnimations.CopyFrom(source);
        }

        public void ApplyTo(UISelectableAnimationProfile target)
        {
            if (target == null || stateAnimations == null)
            {
                return;
            }

            target.CopyFrom(stateAnimations);
        }
    }
}
