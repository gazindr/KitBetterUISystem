using System;
using Sirenix.OdinInspector;

namespace Project.UI
{
    [Serializable]
    public sealed class UIPresetApplyMask
    {
        [EnumToggleButtons]
        public UIPresetApplyMode mode = UIPresetApplyMode.Full;

        [ShowIf(nameof(IsCustom))]
        public bool applyAnimations = true;

        [ShowIf(nameof(IsCustom))]
        public bool applyBehaviours = true;

        [ShowIf(nameof(IsCustom))]
        public bool applyCallbacks = false;

        [ShowIf(nameof(IsCustom))]
        public bool applyTargets = false;

        [ShowIf(nameof(IsCustom))]
        public bool applyStartup = false;

        [ShowIf(nameof(IsCustom))]
        public bool applyBackground = false;

        [ShowIf(nameof(IsCustom))]
        public bool applySettings = true;

        public bool ShouldApplyAnimations
        {
            get { return mode == UIPresetApplyMode.Full || mode == UIPresetApplyMode.OnlyAnimations || (mode == UIPresetApplyMode.Custom && applyAnimations); }
        }

        public bool ShouldApplyBehaviours
        {
            get { return mode == UIPresetApplyMode.Full || mode == UIPresetApplyMode.OnlyBehaviours || (mode == UIPresetApplyMode.Custom && applyBehaviours); }
        }

        public bool ShouldApplyCallbacks
        {
            get { return mode == UIPresetApplyMode.Full || mode == UIPresetApplyMode.OnlyCallbacks || (mode == UIPresetApplyMode.Custom && applyCallbacks); }
        }

        public bool ShouldApplyTargets
        {
            get { return mode == UIPresetApplyMode.Full || (mode == UIPresetApplyMode.Custom && applyTargets); }
        }

        public bool ShouldApplyStartup
        {
            get { return mode == UIPresetApplyMode.Full || (mode == UIPresetApplyMode.Custom && applyStartup); }
        }

        public bool ShouldApplyBackground
        {
            get { return mode == UIPresetApplyMode.Full || (mode == UIPresetApplyMode.Custom && applyBackground); }
        }

        public bool ShouldApplySettings
        {
            get { return mode == UIPresetApplyMode.Full || (mode == UIPresetApplyMode.Custom && applySettings); }
        }

        private bool IsCustom()
        {
            return mode == UIPresetApplyMode.Custom;
        }
    }
}



