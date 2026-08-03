using TMPro;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Primitive helpers for the scene builders. Every builder object is created
    /// collider-free so it can never intercept an XR ray meant for a task object;
    /// the interactable roots keep their own colliders.
    /// </summary>
    public static class ResearchBuildKit
    {
        public static GameObject Box(string name, Transform parent, Vector3 localPos, Vector3 size, string material, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = size;
            Paint(go, material);
            return go;
        }

        public static GameObject Cyl(string name, Transform parent, Vector3 localPos, Vector3 size, string material, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = size;
            Paint(go, material);
            return go;
        }

        public static GameObject Sphere(string name, Transform parent, Vector3 localPos, float diameter, string material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * diameter;
            Paint(go, material);
            return go;
        }

        public static void Paint(GameObject go, string material)
        {
            var mat = ResearchMaterialPalette.Load(material);
            if (mat == null)
                return;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = mat;
        }

        /// <summary>Engraved-looking bench label. Faces -Z (toward the participant) by default.</summary>
        public static TextMeshPro Label(string name, Transform parent, Vector3 localPos, string text, float size, string colorHex, Vector3 euler = default, float boxWidth = 0.3f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);

            var label = go.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = size;
            label.color = ResearchMaterialPalette.Hex(colorHex);
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.rectTransform.sizeDelta = new Vector2(boxWidth, size * 0.02f + 0.02f);
            return label;
        }

        /// <summary>Removes previously generated children so builders stay idempotent.</summary>
        public static void ClearGenerated(Transform parent, string prefix)
        {
            if (parent == null)
                return;
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.name.StartsWith(prefix))
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        /// <summary>True if this subtree holds anything the research runtime depends on.</summary>
        public static bool CarriesTaskLogic(Transform t)
        {
            return t.GetComponentInChildren<ResearchInteractable>(true) != null
                || t.GetComponentInChildren<InformationSourceController>(true) != null;
        }

        public static GameObject Root(string name, Vector3 position, Vector3 euler = default)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
                Object.DestroyImmediate(existing);
            var go = new GameObject(name);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            return go;
        }

        public static Transform Group(string name, Transform parent, Vector3 localPos = default, Vector3 euler = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            return go.transform;
        }

        /// <summary>
        /// Replaces the "Visual X" child of a task interactable without touching the
        /// interactable itself.
        ///
        /// Children that carry task logic are detached, never destroyed. Builders
        /// reparent interactables under their device (so local coordinates stay
        /// readable), which meant a second run of a builder deleted them outright.
        /// </summary>
        public static Transform ResetVisual(string interactableName, out GameObject interactable)
        {
            interactable = GameObject.Find(interactableName);
            if (interactable == null)
                return null;

            for (var i = interactable.transform.childCount - 1; i >= 0; i--)
            {
                var child = interactable.transform.GetChild(i);
                if (CarriesTaskLogic(child))
                {
                    child.SetParent(null, true);
                    continue;
                }
                Object.DestroyImmediate(child.gameObject);
            }

            // Hide the placeholder's own primitive mesh; the rebuilt child carries
            // the look. Any Renderer counts: a placeholder left with an enabled
            // cylinder mesh becomes a 1x2 m column once its scale is normalised.
            foreach (var own in interactable.GetComponents<Renderer>())
                own.enabled = false;

            return Group("Visual", interactable.transform);
        }
    }
}
