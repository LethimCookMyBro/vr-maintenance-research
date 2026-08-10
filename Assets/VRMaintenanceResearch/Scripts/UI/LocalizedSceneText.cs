using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Applies the session language to text that was baked into a scene by the
    /// editor builders — bench placards, the training room's printed signage, the
    /// device-test sign and the lab wall notice board.
    ///
    /// Those strings are geometry, authored once in English and saved into the scene
    /// file, because they have to be present in edit mode and in the audit captures.
    /// The session language is not known until a researcher picks it, so something
    /// has to rewrite them at load. This is that something, and it is a scene sweep
    /// rather than a component per sign for one reason: there are roughly forty of
    /// them across four scenes, and attaching, wiring and maintaining forty
    /// components — each of which can be dropped by a rebuild — is more moving parts
    /// than one pass over the text that is already there.
    ///
    /// It matches on the exact English string, so it is safe to run more than once:
    /// a translated string is not a key, so a second pass changes nothing. A string
    /// with no entry in <see cref="ResearchStrings"/> is left in English rather than
    /// blanked.
    ///
    /// It deliberately does **not** handle:
    ///
    ///  - The work order. <see cref="LocalizedTaskBrief"/> owns that, because the two
    ///    briefs differ by task and not only by language.
    ///  - Anything built at run time — the status board, the training board, the
    ///    heads-up display, the information readers. Those construct their own text
    ///    after this has run, so each calls <see cref="ResearchStrings.T(string)"/>
    ///    where it sets the text instead.
    /// </summary>
    public static class LocalizedSceneText
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Apply();
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply();

        /// <summary>
        /// Rewrites every scene label that has a translation. Public so a researcher
        /// tool or a test can force a pass without reloading the scene.
        /// </summary>
        /// <summary>
        /// What each label said in English the first time this saw it.
        ///
        /// Translating whatever is currently on the label only works once: after a
        /// pass, the text is Thai, Thai is not a key, and a second pass can no longer
        /// reach Japanese or get back to English. In a real session the language is
        /// fixed before the scene loads and never changes, so one pass is all that
        /// happens — but a researcher stepping through the languages to check the
        /// build is exactly who would hit it, and they would conclude the Japanese was
        /// missing when it was only unreachable.
        /// </summary>
        static readonly Dictionary<Object, string> Originals = new Dictionary<Object, string>();

        /// <summary>
        /// Rewrites every scene label that has a translation. Public so a researcher
        /// tool or a test can force a pass without reloading the scene.
        /// </summary>
        public static void Apply()
        {
            // The signage is the first Thai or Japanese a participant sees in a scene,
            // and it can be the only text on screen at that moment. Without the
            // fallback registered, the placards render as boxes.
            InformationSourceController.EnsureLocalizedFontFallbacks();

            foreach (var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                label.text = Translate(label, label.text);

            // The information dock's own buttons are legacy TextMesh rather than TMP,
            // so a sweep that looked only at TMP_Text left every Prev, Next and Close
            // in English in an otherwise translated room.
            foreach (var label in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                label.text = Translate(label, label.text);
        }

        static string Translate(Object label, string current)
        {
            if (!Originals.TryGetValue(label, out var english))
            {
                // Nothing to remember for a label this has never translated and never
                // could: keeping only the ones with an entry stops the dictionary
                // growing to the size of every piece of text in the room.
                if (!ResearchStrings.Has(current))
                    return current;

                english = current;
                Originals[label] = english;
            }

            return ResearchStrings.T(english);
        }
    }
}
