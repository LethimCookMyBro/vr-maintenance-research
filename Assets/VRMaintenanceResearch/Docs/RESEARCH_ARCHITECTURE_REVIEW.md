# Research Architecture Review

**Status:** design-control review; it does not claim research scenes, logging, or recording are implemented. The review began before `Assets/VRMaintenanceResearch/` existed.

## Validity risks and controls

| Risk | Required control |
|---|---|
| The Computer and Fan tasks measure different opportunities rather than task context. | Use one task-definition model, common event vocabulary, the same source set, recoverable errors, natural device tests, and the same completion/stop semantics. Analyse `taskId` and `taskOrder`; do not treat the two device contexts as interchangeable. |
| Practice, fatigue, training, or researcher behaviour is mistaken for a site/group difference. | Preassign and log task order, training status, researcher code, site, language, and all researcher actions. Use the same researcher script and transition procedure at both sites. |
| Source choice is confounded by information coverage, translation, or physical access. | Freeze a reviewed equivalence matrix per task/language/version; use preassigned documented slots with equivalent visibility, reach, and interaction depth. |
| Simulator/headset, application, or layout drift changes observations. | Log platform, headset, simulator mode, Unity/XRI/application/Git versions, content/configuration versions, and all layout IDs. Treat simulator data as simulator data. |
| Derived labels overwrite evidence. | Keep append-only, ordered raw events as the source of truth. Version every derivation; retain raw logs and record technical failures separately. |
| Focus or low activity is over-interpreted. | Log `controller-ray hover`, `object/UI hover`, `head-ray dwell`, and `LowActivityPeriod` with method/context. Do not label them eye gaze, attention, confusion, or cognitive load. |

## Task-equivalence contract

Computer and Fan are different maintenance contexts, not matched replicas. They are comparable only when both expose the same **observable choice structure**:

1. Inspect device/external/internal parts; open any of the four sources at any time; act before or after information; test; make recoverable incorrect attempts; correct and retry; complete, timeout, abort, or safety-stop.
2. Use common definitions for `firstMeaningfulAction`, unsuccessful action, retry, device test, source return after error, completion, timeout, abort, and low activity. Define each from a versioned raw-event rule; hover, scene load, and automatic UI state do not count as a meaningful action.
3. Record the same action categories in both tasks: device/cover/case, component, cable/wiring, tool, information source, test, error, retry, and terminal state. Task-specific object IDs remain valid only within their task/content version.
4. Keep the task rules configurable, then freeze an approved task-content version before collection: fault, available tools/components/replacements, validation/completion rules, maximum time, workspace layout, and source layout.
5. Pilot each approved pair with the same training/procedure and compare completion status/time, successful and unsuccessful attempts, source availability, event coverage, and log completeness. Revise the configuration version rather than silently changing task behaviour.

## Information-source neutrality

All four sources—Product Manual, Text Troubleshooting Guide, Instructional Video, and Visual Step-by-Step Guide—must be concurrently reachable. No source auto-opens, is recommended, highlighted, nearer, more visible, or the sole holder of an essential fact.

Maintain `INFORMATION_EQUIVALENCE_MATRIX.md` for every task, language, and content version. For every essential fact, show its location in every source and document coverage, word count, number of steps, visual detail, video duration, navigation depth, and access path. Localized content needs meaning-level review, not merely literal translation. Source labels, open/close affordances, font legibility, panel size, controls, audio defaults, and load time must be reviewed as potential modality advantages.

Use only researcher-selected/preassigned source layouts with stable layout IDs. Each layout must document source slots, reach distance, viewing angle/occlusion, and interaction method. Counterbalance layouts only after the advisor approves an assignment schedule; never use uncontrolled runtime randomization.

## Session configuration and privacy

Create one immutable session manifest at start, then log any researcher intervention as an event. Require a non-empty `participantCode`; generate/sanitize `sessionId`; reject direct identifiers (name, phone, email, student ID) and free-text personal information. Default first-person recording to off and permit it only when consent is true; do not claim recording exists until it is implemented.

Required configuration: `participantGroup`, `language`, `taskOrder`, `experimentSite`, `researcherCode`, start time, headset/platform, application build, task-content/configuration versions, Computer/Fan/source-layout IDs, development mode, recording consent/enabled state, and training requirement. Prefer fixed choices/enums. Persist exactly one session configuration across scenes; send development sessions to a separate output root and mark them `developmentMode=true` and `simulatorMode` explicitly.

## Logging variables and reproducibility

- **Manifest/provenance:** session configuration above plus Unity version, XRI version, Git commit, movement sample frequency, completion state, and non-identifying technical notes.
- **Raw event row:** participant/session/task/context/order/group/language; layout and source-layout IDs; task-relative and ISO-8601 absolute timestamps; stable sequence number; event type; stable object/source IDs and source type/slot; action result/task state/measurement method; optional position/rotation; additional value; build and content versions.
- **Minimum raw events:** session/task lifecycle; source hover/open/close/page/video play-pause-stop-seek-complete; device/component/tool/UI hover; ray/dwell; grab/release/place; tool/component operations; unsuccessful actions; device tests; retry; researcher action; low-activity begin/end; technical error.
- **Movement row:** task/session/time/sample sequence; `Headset`, `LeftController`, or `RightController`; position; quaternion; tracking validity; simulator flag; configured sample frequency; build/content versions. Pause sampling outside a task and handle absent controller tracking without inventing a pose.
- **Derived summary:** derive first meaningful action/source, action-before-information, source return after error, source switches/repeats/durations, unsuccessful actions, retries, tests, low-activity counts/duration, and terminal timing/status from raw events. Store a `derivationVersion`; do not replace the source rows.

Use UTF-8, invariant numerics, a fixed column order, append-only ordered writes, regular flush/safe closure, and separate technical-error reporting. A write failure must be visible but must not create duplicate lifecycle events or stop the participant task. Store under `Application.persistentDataPath/VRMaintenanceResearchData/{Development|Sessions}/{sessionId}/`.

## Cross-site consistency gate

Before each site collects data, verify and record the approved task-content/configuration package, source-language set, participant/researcher instructions, stopping rules, source-layout assignment, workspace layout, stable object-ID map, event schema, and derivation version. The Computer layout need only match other Computer layouts; the Fan layout need only match other Fan layouts; within a task/content version every research-relevant object ID must match across builds/sites.

Run the same scripted development session at both sites/build targets and compare: manifest fields, event order/types, required columns, source-layout IDs, object IDs, derived summary, and output separation. Resolve any mismatch by a new approved configuration/content version. Windows simulation and Quest builds may differ technically, but their platform/headset/simulator provenance must make that difference analysable.

## Advisor decisions kept configurable

| Decision | Configuration/record needed |
|---|---|
| Final Computer and Fan faults, available tools/parts, validation/completion/stopping rules | Versioned task definition and content version |
| Maximum task time and timer visibility | Versioned task configuration; log timeout/stop reason |
| Task-order and source-layout assignment/counterbalance | Preassigned schedule with stable order/layout IDs |
| Training required, skip permission, and researcher transition checklist | Session configuration plus researcher-action events |
| Language releases and equivalence approval | Language/content version and approved matrix |
| Movement sample frequency and low-activity threshold | Configuration version, method/context, and sampling rate in manifest/events |
| First meaningful-action and derivation rules | Data-dictionary/derivation version with reproducible tests |
| First-person recording procedure | Consent-gated configuration; leave disabled until implementation is verified |

Do not make the exact final faults, counterbalance schedule, task limits, or source-language release appear final until advisor approval. Any approved change increments the relevant content/configuration/derivation version and is recorded in the protocol change log.

## Evidence inspected

- `C:\Users\User\.codex\attachments\2267ef83-6583-4e97-b075-256ebb75f8ed\pasted-text-1.txt` — current controlled-study requirements, configuration, logging, source-equivalence, session flow, and test scenarios.
- `ProjectSettings\ProjectVersion.txt` — Unity `6000.3.20f1`.
- `Packages\manifest.json` — XR Interaction Toolkit `3.4.0`, Input System `1.19.0`, OpenXR `1.16.1`.
- `Assets\Samples\XR Interaction Toolkit\3.4.0\XR Interaction Simulator\` and `Assets\Samples\XR Interaction Toolkit\3.4.0\Starter Assets\` — installed simulator and default-input sample material.
- `Assets\XRI_Examples\Scenes\XRI_Examples_Main.unity`, `Documentation\Focus.md`, and `Documentation\Gaze.md` — stock example/focus-gaze behaviour. No custom research directory existed when this review began.
