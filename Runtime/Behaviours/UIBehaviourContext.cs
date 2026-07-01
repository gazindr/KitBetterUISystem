using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    public sealed class UIBehaviourContext
    {
        public Component sourceComponent;
        public GameObject sourceGameObject;
        public UIBehaviourTrigger trigger;
        public PointerEventData pointerEventData;
        public UISelectableState selectedState;
        public bool hasSelectedState;
        public float sliderValue;
        public bool hasSliderValue;
        public float timestamp;
        public UIContainer targetContainer;

        public static UIBehaviourContext Create(Component source, UIBehaviourTrigger trigger, PointerEventData pointerEventData)
        {
            UIBehaviourContext context = new UIBehaviourContext();
            context.sourceComponent = source;
            context.sourceGameObject = source == null ? null : source.gameObject;
            context.trigger = trigger;
            context.pointerEventData = pointerEventData;
            context.timestamp = Time.unscaledTime;
            return context;
        }
    }
}



