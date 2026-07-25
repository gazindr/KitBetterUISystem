using System;
using System.Reflection;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Soft bridge to project CursorLocker. Safe when CursorLocker is missing.
    /// </summary>
    public static class UICursorBridge
    {
        private const string TypeName = "CursorLocker";
        private static bool resolved;
        private static MethodInfo unlockMethod;
        private static MethodInfo lockMethod;

        public static void Apply(bool showCursor)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureResolved();
            MethodInfo method = showCursor ? unlockMethod : lockMethod;
            if (method == null)
            {
                return;
            }

            try
            {
                method.Invoke(null, null);
            }
            catch (Exception)
            {
                // Ignore missing/broken CursorLocker bindings.
            }
        }

        private static void EnsureResolved()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            Type type = FindType(TypeName);
            if (type == null)
            {
                return;
            }

            unlockMethod = type.GetMethod("UnlockCursor", BindingFlags.Public | BindingFlags.Static);
            lockMethod = type.GetMethod("LockCursor", BindingFlags.Public | BindingFlags.Static);
        }

        private static Type FindType(string typeName)
        {
            Type direct = Type.GetType(typeName);
            if (direct != null)
            {
                return direct;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
