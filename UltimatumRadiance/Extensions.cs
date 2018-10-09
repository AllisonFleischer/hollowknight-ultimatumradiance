using System.Linq;
using UnityEngine;

namespace UltimatumRadiance
{
    public static class Extensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name) => gameObject == null
            ? null
            : (gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).Select(t => t.gameObject)).FirstOrDefault();

        public static Transform FindTransformInChildren(this GameObject gameObject, string name) => gameObject?.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    }
}