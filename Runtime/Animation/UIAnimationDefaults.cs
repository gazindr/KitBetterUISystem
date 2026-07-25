using UnityEngine;

namespace Project.UI
{
    public static class UIAnimationDefaults
    {
        public const float DefaultDuration = 0.2f;
        public const float ButtonScaleHighlight = 1.1f;
        public const float ButtonScalePressed = 0.9f;
        public const float ButtonFadeDisabled = 0.8f;

        public static UIContainerAnimationProfile CreateContainerProfile()
        {
            UIContainerAnimationProfile profile = new UIContainerAnimationProfile();
            profile.show = CreateContainerShow();
            profile.hide = CreateContainerHide();
            return profile;
        }

        public static UIAnimationState CreateContainerShow()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();

            ConfigureMoveDirection(state.move, UIAnimationDirection.Top, true);
            state.move.enabled = false;

            state.scale.enabled = true;
            state.scale.duration = DefaultDuration;
            state.scale.fromMode = UIValueMode.CustomValue;
            state.scale.customFromVector = Vector3.zero;
            state.scale.toMode = UIValueMode.CurrentValue;
            state.scale.customToVector = Vector3.one;

            state.fade.enabled = false;
            state.fade.duration = DefaultDuration;
            state.fade.fromMode = UIValueMode.CustomValue;
            state.fade.customFromFloat = 0f;
            state.fade.toMode = UIValueMode.CurrentValue;
            state.fade.customToFloat = 1f;

            return state;
        }

        public static UIAnimationState CreateContainerHide()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();

            ConfigureMoveDirection(state.move, UIAnimationDirection.Top, false);
            state.move.enabled = false;

            state.scale.enabled = true;
            state.scale.duration = DefaultDuration;
            state.scale.fromMode = UIValueMode.CurrentValue;
            state.scale.customFromVector = Vector3.one;
            state.scale.toMode = UIValueMode.CustomValue;
            state.scale.customToVector = Vector3.zero;

            state.fade.enabled = false;
            state.fade.duration = DefaultDuration;
            state.fade.fromMode = UIValueMode.CurrentValue;
            state.fade.customFromFloat = 1f;
            state.fade.toMode = UIValueMode.CustomValue;
            state.fade.customToFloat = 0f;

            return state;
        }

        public static UISelectableAnimationProfile CreateButtonProfile()
        {
            UISelectableAnimationProfile profile = new UISelectableAnimationProfile();
            profile.normal = CreateButtonNormal();
            profile.highlighted = CreateButtonHighlighted();
            profile.pressed = CreateButtonPressed();
            profile.selected = CreateButtonSelected();
            profile.disabled = CreateButtonDisabled();
            return profile;
        }

        public static UIAnimationState CreateButtonNormal()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();
            ConfigureFadeTo(state.fade, 1f);
            return state;
        }

        public static UIAnimationState CreateButtonHighlighted()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();
            ConfigureScaleTo(state.scale, Vector3.one * ButtonScaleHighlight);
            return state;
        }

        public static UIAnimationState CreateButtonPressed()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();
            ConfigureScaleTo(state.scale, Vector3.one * ButtonScalePressed);
            return state;
        }

        public static UIAnimationState CreateButtonSelected()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();
            ConfigureFadeTo(state.fade, 1f);
            return state;
        }

        public static UIAnimationState CreateButtonDisabled()
        {
            UIAnimationState state = new UIAnimationState();
            state.EnsureTypes();
            ConfigureFadeTo(state.fade, ButtonFadeDisabled);
            return state;
        }

        public static void ConfigureMoveDirection(UIAnimationSettings move, UIAnimationDirection direction, bool isShow)
        {
            if (move == null)
            {
                return;
            }

            move.SetType(UIAnimationType.Move);
            move.duration = DefaultDuration;
            if (isShow)
            {
                move.fromMode = UIValueMode.Direction;
                move.fromDirection = direction;
                move.toMode = UIValueMode.StartValue;
            }
            else
            {
                move.fromMode = UIValueMode.StartValue;
                move.toMode = UIValueMode.Direction;
                move.toDirection = direction;
            }
        }

        private static void ConfigureScaleTo(UIAnimationSettings scale, Vector3 to)
        {
            scale.enabled = true;
            scale.duration = DefaultDuration;
            scale.fromMode = UIValueMode.CurrentValue;
            scale.toMode = UIValueMode.CustomValue;
            scale.customToVector = to;
        }

        private static void ConfigureFadeTo(UIAnimationSettings fade, float to)
        {
            fade.enabled = true;
            fade.duration = DefaultDuration;
            fade.fromMode = UIValueMode.CurrentValue;
            fade.toMode = UIValueMode.CustomValue;
            fade.customToFloat = to;
        }
    }
}
