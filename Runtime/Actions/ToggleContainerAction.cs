using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Actions/Toggle Container", fileName = "ToggleContainerAction")]
    public sealed class ToggleContainerAction : UIBehaviourAction
    {
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UIContainer targetContainer;

        public string containerId;

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
                    if (container.State == UIContainerState.Visible || container.State == UIContainerState.Showing)
                    {
                        container.EditorPreviewHideAnimation();
                    }
                    else
                    {
                        container.EditorPreviewShowAnimation();
                    }

                    return;
                }
#endif
                container.Toggle();
                return;
            }

            if (!string.IsNullOrEmpty(containerId))
            {
                UIContainer.Toggle(containerId);
            }
        }
    }
}



