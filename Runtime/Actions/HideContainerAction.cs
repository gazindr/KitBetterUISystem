using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Actions/Hide Container", fileName = "HideContainerAction")]
    public sealed class HideContainerAction : UIBehaviourAction
    {
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UIContainer targetContainer;

        public string containerId;

        public bool instant;

        public override void Execute(UIBehaviourContext context)
        {
            UIContainer container = targetContainer;
            if (container == null && context != null)
            {
                container = context.targetContainer;
            }

            if (container != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    container.EditorPreviewHideAnimation();
                    return;
                }
#endif
                if (instant)
                {
                    container.InstantHide();
                }
                else
                {
                    container.Hide();
                }

                return;
            }

            if (!string.IsNullOrEmpty(containerId))
            {
                if (instant)
                {
                    UIContainer.InstantHide(containerId);
                }
                else
                {
                    UIContainer.Hide(containerId);
                }
            }
        }
    }
}



