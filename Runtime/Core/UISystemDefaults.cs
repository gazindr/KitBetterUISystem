using System;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Package defaults: default full presets for new UIContainer / UIButton instances.
    /// Place an asset named UISystemDefaults in a Resources folder.
    /// </summary>
    public sealed class UISystemDefaults : ScriptableObject
    {
        public const string ResourcesName = "UISystemDefaults";

        public UIContainerPreset defaultContainerPreset;
        public UIButtonPreset defaultButtonPreset;

        private static UISystemDefaults cached;

        public static UISystemDefaults Instance
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<UISystemDefaults>(ResourcesName);
                }

                return cached;
            }
        }

#if UNITY_EDITOR
        public static void ClearCache()
        {
            cached = null;
        }
#endif
    }
}
