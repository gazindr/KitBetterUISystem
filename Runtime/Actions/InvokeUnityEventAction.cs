using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Actions/Invoke Unity Event", fileName = "InvokeUnityEventAction")]
    public sealed class InvokeUnityEventAction : UIBehaviourAction
    {
        public UnityEvent callback = new UnityEvent();

        public override void Execute(UIBehaviourContext context)
        {
            if (callback != null)
            {
                callback.Invoke();
            }
        }
    }
}



