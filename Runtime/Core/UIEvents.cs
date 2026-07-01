using System;
using UnityEngine.Events;

namespace Project.UI
{
    [Serializable]
    public sealed class UIBoolEvent : UnityEvent<bool>
    {
    }

    [Serializable]
    public sealed class UIFloatEvent : UnityEvent<float>
    {
    }
}



