using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Container Animation Preset", fileName = "UIContainerAnimationPreset")]
    public sealed class UIContainerAnimationPreset : ScriptableObject
    {
        public UIContainerAnimationProfile animations = UIAnimationDefaults.CreateContainerProfile();

        public void CopyFrom(UIContainerAnimationProfile source)
        {
            if (animations == null)
            {
                animations = new UIContainerAnimationProfile();
            }

            animations.CopyFrom(source);
        }

        public void ApplyTo(UIContainerAnimationProfile target)
        {
            if (target == null || animations == null)
            {
                return;
            }

            target.CopyFrom(animations);
        }
    }
}
