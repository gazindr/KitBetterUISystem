using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Actions/Set GameObject Active", fileName = "SetGameObjectActiveAction")]
    public sealed class SetGameObjectActiveAction : UIBehaviourAction
    {
        public GameObject target;

        public bool active = true;

        public bool useSourceIfTargetMissing;

        public override void Execute(UIBehaviourContext context)
        {
            GameObject objectToSet = target;
            if (objectToSet == null && useSourceIfTargetMissing && context != null)
            {
                objectToSet = context.sourceGameObject;
            }

            if (objectToSet != null)
            {
                objectToSet.SetActive(active);
            }
        }
    }
}



