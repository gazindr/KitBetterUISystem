namespace Project.UI
{
    public enum UIAnimationType
    {
        Move,
        Rotate,
        Scale,
        Fade
    }

    public enum UISelectableState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    public enum UIContainerState
    {
        Hidden,
        Hiding,
        Showing,
        Visible
    }

    public enum UIContainerStartupMode
    {
        InstantHide,
        InstantShow,
        Hide,
        Show
    }

    public enum UIBehaviourTrigger
    {
        PointerEnter,
        PointerExit,
        PointerDown,
        PointerUp,
        PointerDoubleClick,
        PointerLongClick,
        PointerLeftClick,
        PointerMiddleClick,
        PointerRightClick,
        Selected,
        Deselected,
        Submit,
        ValueChanged,
        MultipleSelect
    }

    public enum UIValueMode
    {
        CurrentValue,
        StartValue,
        CustomValue,
        OffsetFromStart,
        OffsetFromCurrent
    }

    public enum UIPresetApplyMode
    {
        Full,
        OnlyAnimations,
        OnlyBehaviours,
        OnlyCallbacks,
        Custom
    }

    public enum UIAnimationPlayMode
    {
        Once,
        Loop,
        PingPong
    }

    public enum UIEaseMode
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        OutBack,
        CustomCurve
    }
}



