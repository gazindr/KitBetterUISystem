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

    public enum UIBackgroundAttachMode
    {
        BehindContainer = 0,
        InsideContainer = 1
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
        OffsetFromCurrent,
        Direction
    }

    public enum UIAnimationDirection
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3,
        TopLeft = 4,
        TopRight = 5,
        BottomLeft = 6,
        BottomRight = 7
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



