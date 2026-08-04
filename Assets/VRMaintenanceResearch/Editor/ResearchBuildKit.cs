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

        /// <summary>
        /// Scene lookup that also sees deactivated objects.
        ///
        /// Builders stow parts by deactivating them rather than deleting them, so the
        /// StableObjectId, the ResearchInteractable and the collider all survive. Plain
        /// GameObject.Find skips inactive objects, so a second run of a builder would
        /// lose every part it had stowed on the first and start warning about missing
        /// objects.
        /// </summary>
        public static GameObject FindAny(string name)
        {
            var active = GameObject.Find(name);
            if (active != null)
                return active;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name)
                    return t.gameObject;
            return null;
        }

        /// <summary>
        /// Takes a part off the bench without taking it out of the scene.
        ///
        /// Both tasks are diagnostic: the machine is already assembled and the
        /// participant has to find one fault. Spares that are not part of that repair
        /// read as an assembly kit, so they are stowed. Deactivating rather than
        /// deleting keeps the id in the scene for the task definition and leaves one
        /// checkbox to put the part back.
        /// </summary>
        public static void Stow(string name)
        {
            var go = FindAny(name);
            if (go == null)
            {
                Debug.LogWarning($"[ResearchBuildKit] cannot stow missing {name}");
                return;
            }
            go.SetActive(false);
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
        /// Drops an imported model into a project-owned visual wrapper, scaled to sit
        /// inside <paramref name="fitBox"/>.
        ///
        /// The scale is deliberately uniform. The first integration fitted each axis
        /// independently, which let a 0.248 x 0.022 x 0.305 m motherboard be asked for
        /// at 0.085 x 0.255 x 0.265 — scale factors of 0.28, 11.6 and 1.07 on one mesh.
        /// Every part came out sheared, which is what read on screen as parts floating,
        /// clipping and disagreeing about scale. Fitting inside the box on the tightest
        /// axis can distort nothing; a part that ends up smaller than its slot is a
        /// placement number to fix, not silent shear.
        ///
        /// The wrapper is Wrapper > Scale > Orientation > model, so the caller's
        /// position, the fit and the model's own axes never fight for one transform.
        /// </summary>
        /// <param name="drop">
        /// Child objects to delete before the model is measured. Some sources carry
        /// parts that are not the thing being modelled: the power supply has a loose
        /// screw sitting 16 units out from a body only 10 deep, which would both float
        /// outside the case and shrink the unit by inflating the bounds it is fitted to,
        /// and the storage kit is two drives when the bench needs one.
        /// </param>
        public static Transform ImportedVisual(string name, Transform parent, string assetPath, Vector3 localCenter, Vector3 fitBox, Vector3 euler = default, string[] drop = null)
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
            StripImportedExtras(instance);

            if (drop != null)
            {
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                {
                    if (child == null || child == instance.transform)
                        continue;
                    if (System.Array.IndexOf(drop, child.name) >= 0)
                        Object.DestroyImmediate(child.gameObject);
                }
            }

            if (!TryBounds(orientation, scaleRoot, out var bounds))
            {
                Object.DestroyImmediate(wrapper.gameObject);
                Debug.LogError($"[ResearchBuildKit] imported visual has no renderers: {assetPath}");
                return null;
            }

            var fit = Mathf.Min(fitBox.x / bounds.size.x, fitBox.y / bounds.size.y, fitBox.z / bounds.size.z);
            orientation.localPosition = -bounds.center;
            scaleRoot.localScale = Vector3.one * fit;
            return wrapper;
        }

        /// <summary>
        /// Everything an imported file brings that the scene must not inherit: its own
        /// cameras and lights would fight the research lighting rig, its scripts and
        /// animation would run at play time, and its colliders would intercept XR rays
        /// meant for the task interactable that owns this visual.
        /// </summary>
        static void StripImportedExtras(GameObject instance)
        {
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
        }

        /// <summary>Renderer bounds of <paramref name="root"/> expressed in <paramref name="basis"/>' local space.</summary>
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
            // FindAny, not Find: a part stowed on the previous run is inactive, and it
            // still has to be rebuilt so a later run can put it back unchanged.
            interactable = FindAny(interactableName);
            if (interactable == null)
                return null;

            // Rebuild from the visible state every time; the builder decides at the end
            // of the pass what gets stowed.
            interactable.SetActive(true);

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
