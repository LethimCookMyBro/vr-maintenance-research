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

        /// <summary>Fits an imported model inside a project-owned visual wrapper.</summary>
        public static Transform ImportedVisual(string name, Transform parent, string assetPath, Vector3 localCenter, Vector3 size, Vector3 euler = default)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null)
            {
                Debug.LogError($"[ResearchBuildKit] imported visual missing: {assetPath}");
                return null;
            }

            var wrapper = Group(name, parent, localCenter);
            var scaleRoot = Group("Scale", wrapper);
            var orientation = Group("Orientation", scaleRoot, default, euler);
            var instance = PrefabUtility.InstantiatePrefab(source, orientation) as GameObject;
            if (instance == null)
            {
                Object.DestroyImmediate(wrapper.gameObject);
                Debug.LogError($"[ResearchBuildKit] could not instantiate: {assetPath}");
                return null;
            }

            instance.name = source.name;
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            foreach (var component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Joint>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Animator>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Animation>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Light>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<Camera>(true))
                Object.DestroyImmediate(component);
            foreach (var component in instance.GetComponentsInChildren<AudioSource>(true))
                Object.DestroyImmediate(component);

            if (!TryBounds(orientation, scaleRoot, out var bounds))
            {
                Object.DestroyImmediate(wrapper.gameObject);
                Debug.LogError($"[ResearchBuildKit] imported visual has no renderers: {assetPath}");
                return null;
            }

            orientation.localPosition = -bounds.center;
            scaleRoot.localScale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z);
            return wrapper;
        }

        static bool TryBounds(Transform root, Transform basis, out Bounds bounds)
        {
            bounds = default;
            var found = false;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var local = renderer.localBounds;
                for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var corner = local.center + Vector3.Scale(local.extents, new Vector3(x, y, z));
                    var point = basis.InverseTransformPoint(renderer.transform.TransformPoint(corner));
                    if (found)
                        bounds.Encapsulate(point);
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        found = true;
                    }
                }
            }
            return found && bounds.size.x > 0f && bounds.size.y > 0f && bounds.size.z > 0f;
        }

        /// <summary>Fits a simple XR collider to the visual while retaining the logical wrapper.</summary>
        public static void FitBoxCollider(GameObject go, Vector3 size)
        {
            if (go == null)
                return;
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            foreach (var collider in go.GetComponents<Collider>())
                if (collider != box)
                    Object.DestroyImmediate(collider);
            box.center = Vector3.zero;
            box.size = size;
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
