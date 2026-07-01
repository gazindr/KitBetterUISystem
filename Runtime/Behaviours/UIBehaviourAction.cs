using UnityEngine;

namespace Project.UI
{
    public abstract class UIBehaviourAction : ScriptableObject
    {
        public abstract void Execute(UIBehaviourContext context);
    }
}



