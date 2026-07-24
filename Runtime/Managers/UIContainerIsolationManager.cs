using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Keeps one container exclusive: hides other open containers and blocks their Show
    /// until the isolated container becomes Hidden, then restores the suppressed ones.
    /// </summary>
    public static class UIContainerIsolationManager
    {
        private static UIContainer isolated;
        private static readonly List<UIContainer> suppressed = new List<UIContainer>();
        private static bool ending;

        public static UIContainer Isolated
        {
            get { return isolated; }
        }

        public static bool HasActiveIsolation
        {
            get { return isolated != null && isolated.State != UIContainerState.Hidden; }
        }

        public static bool IsIsolated(UIContainer container)
        {
            return container != null && isolated == container;
        }

        public static bool IsBlocked(UIContainer container)
        {
            if (container == null || isolated == null || container == isolated)
            {
                return false;
            }

            return isolated.State != UIContainerState.Hidden;
        }

        public static void Begin(UIContainer container)
        {
            if (container == null || ending)
            {
                return;
            }

            if (isolated == container)
            {
                return;
            }

            if (isolated != null)
            {
                isolated = null;
                suppressed.Clear();
            }

            suppressed.Clear();
            List<UIContainer> all = UIRegistry.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                UIContainer other = all[i];
                if (other == null || other == container)
                {
                    continue;
                }

                if (other.State == UIContainerState.Visible || other.State == UIContainerState.Showing)
                {
                    suppressed.Add(other);
                    other.Hide();
                }
            }

            isolated = container;
        }

        public static void NotifyHidden(UIContainer container)
        {
            if (ending || container == null || isolated != container)
            {
                return;
            }

            EndAndRestore();
        }

        public static void Remove(UIContainer container)
        {
            if (container == null)
            {
                return;
            }

            suppressed.RemoveAll(item => item == container);

            if (isolated == container)
            {
                isolated = null;
                suppressed.Clear();
            }
        }

        public static void Clear()
        {
            isolated = null;
            suppressed.Clear();
            ending = false;
        }

        private static void EndAndRestore()
        {
            ending = true;
            isolated = null;

            List<UIContainer> toRestore = new List<UIContainer>(suppressed);
            suppressed.Clear();

            for (int i = 0; i < toRestore.Count; i++)
            {
                UIContainer container = toRestore[i];
                if (container != null)
                {
                    container.Show();
                }
            }

            ending = false;
        }
    }
}
