using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    public abstract class UIPresetBase : ScriptableObject
    {
        [HideLabel]
        public UIPresetApplyMask defaultApplyMask = new UIPresetApplyMask();

        protected UIPresetApplyMask ResolveMask(UIPresetApplyMask overrideMask)
        {
            return overrideMask ?? defaultApplyMask;
        }
    }
}



