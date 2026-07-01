using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Actions/Show Container", fileName = "ShowContainerAction")]
    public sealed class ShowContainerAction : UIBehaviourAction
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
                    container.EditorPreviewShowAnimation();
                    return;
                }
#endif
                if (instant)
                {
                    container.InstantShow();
                }
                else
                {
                    container.Show();
                }

                return;
            }

            if (!string.IsNullOrEmpty(containerId))
            {
                if (instant)
                {
                    UIContainer.InstantShow(containerId);
                }
                else
                {
                    UIContainer.Show(containerId);
                }
            }
        }
    }
}



