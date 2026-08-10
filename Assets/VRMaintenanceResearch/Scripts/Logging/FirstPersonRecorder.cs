using System;
using System.IO;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Records the participant's first-person view for one task attempt as motion-JPEG:
    /// every captured frame is a complete JPEG appended to one open FileStream.
    ///
    /// Unity ships no runtime video encoder and this project may not add a package, so a
    /// concatenated JPEG stream is the closest thing to a video file the installed
    /// modules can produce. It is one file per attempt rather than thousands of loose
    /// images, and both ffmpeg and VLC read it directly. The conversion line and the
    /// storage cost are in KNOWN_LIMITATIONS.md.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FirstPersonRecorder : MonoBehaviour
    {
        // These four constants are the whole storage trade-off, so they sit together at
        // the top. 848 x 480 at quality 50 is roughly 35 KB a frame, which at 5 fps is
        // about 10 MB a minute: a six-minute attempt costs tens of megabytes and a
        // 900 s one lands near 155 MB. Doubling the rate or the long edge roughly
        // doubles both figures, and what the footage is for is where the participant
        // looked and what they reached for - neither needs the extra disk.
        const int CaptureWidth = 848;
        const int CaptureHeight = 480;
        const float CaptureFramesPerSecond = 5f;
        const int JpegQuality = 50;
        const string CaptureCameraName = "First-Person Recorder Camera";

        ResearchLogService logger;
        ResearchTaskDefinition definition;
        int attemptId;
        Camera participantCamera;
        Camera captureCamera;
        RenderTexture target;
        Texture2D readback;
        FileStream stream;
        float nextCaptureAt;
        bool errorLogged;

        /// <summary>
        /// The gate, and the only part of this worth checking without a headset: the
        /// proposal (9.11) authorises first-person capture during the two maintenance
        /// tasks and nowhere else, and consent is a precondition of enabling it rather
        /// than a second opinion about it. When this is false nothing is written at all,
        /// not even an empty file.
        /// </summary>
        public static bool ShouldRecord(ResearchTaskId taskId, ResearchSessionConfig config) =>
            config != null && config.firstPersonRecordingConsent && config.firstPersonRecordingEnabled &&
            (taskId == ResearchTaskId.Computer || taskId == ResearchTaskId.Fan);

        /// <summary>
        /// Opens the file for one attempt. The caller decides whether recording is
        /// permitted, with <see cref="ShouldRecord"/>; by the time this runs that
        /// question is settled.
        /// </summary>
        public void Begin(ResearchLogService logService, ResearchSessionConfig config, ResearchTaskDefinition taskDefinition, int attempt)
        {
            // A development reset ends one attempt and starts the next without passing
            // through MaintenanceTaskController.End, so anything still running is closed
            // here rather than left appending attempt 1's frames to attempt 2's file.
            StopRecording();
            logger = logService;
            definition = taskDefinition;
            attemptId = attempt;
            errorLogged = false;
            if (logService == null || taskDefinition == null || config == null)
                return;

            participantCamera = FindParticipantCamera();
            if (participantCamera == null)
            {
                Fail("first_person_recording_no_camera");
                return;
            }

            var folder = logService.CurrentDataFolder;
            if (string.IsNullOrEmpty(folder))
            {
                Fail("first_person_recording_no_session_folder");
                return;
            }

            try
            {
                target = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
                readback = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

                // Our own camera, never the XR one. Pointing the participant's camera at
                // a render texture reaches into the display's render loop, and
                // ScreenCapture reads the composited XR frame for the same reason. This
                // one is disabled, so it renders only when Render() is called below.
                captureCamera = new GameObject(CaptureCameraName).AddComponent<Camera>();
                captureCamera.enabled = false;

                // The stream is opened last so that a failure setting any of the above
                // leaves no file behind at all - a zero-byte .mjpeg next to the CSVs
                // would read as a recording that captured nothing rather than as one
                // that never started.
                var fileName = ResearchIdentifiers.SanitizeForPath(config.participantCode) + "_" + taskDefinition.taskId + "_attempt" + attempt + ".mjpeg";
                stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create, FileAccess.Write);
                nextCaptureAt = Time.unscaledTime;
            }
            catch (Exception exception)
            {
                Fail("first_person_recording_open_failed;" + exception.GetType().Name);
            }
        }

        /// <summary>Closes the attempt's file. Safe to call when nothing is recording.</summary>
        public void StopRecording()
        {
            if (stream != null)
            {
                try
                {
                    stream.Flush();
                    stream.Dispose();
                }
                catch (Exception exception)
                {
                    LogTechnicalError("first_person_recording_close_failed;" + exception.GetType().Name);
                }
            }

            stream = null;
            ReleaseCapture();
        }

        void Update()
        {
            if (stream == null || Time.unscaledTime < nextCaptureAt)
                return;

            nextCaptureAt = Time.unscaledTime + 1f / CaptureFramesPerSecond;
            if (participantCamera == null)
            {
                Fail("first_person_recording_camera_lost");
                return;
            }

            try
            {
                // CopyFrom every capture, because it brings the projection, clear flags
                // and culling mask across so the recording shows what the participant
                // sees rather than a default view - and because it also clears the
                // target texture, which is why that is assigned after it rather than
                // once in Begin. CopyFrom does not move the transform; that is the
                // separate call, and it is what makes the recording follow the head.
                captureCamera.CopyFrom(participantCamera);
                captureCamera.enabled = false;
                captureCamera.stereoTargetEye = StereoTargetEyeMask.None;
                captureCamera.aspect = (float)CaptureWidth / CaptureHeight;
                captureCamera.targetTexture = target;
                captureCamera.transform.SetPositionAndRotation(participantCamera.transform.position, participantCamera.transform.rotation);
                captureCamera.Render();

                var previous = RenderTexture.active;
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0);
                readback.Apply();
                RenderTexture.active = previous;

                var frame = readback.EncodeToJPG(JpegQuality);
                if (frame == null || frame.Length == 0)
                {
                    Fail("first_person_recording_encode_failed");
                    return;
                }

                stream.Write(frame, 0, frame.Length);

                // Flushed every capture. At 5 fps that is cheap, and it means a session
                // that is killed rather than ended still leaves a file that plays up to
                // the last frame instead of one truncated to the last buffer boundary.
                stream.Flush();
            }
            catch (Exception exception)
            {
                Fail("first_person_recording_frame_failed;" + exception.GetType().Name);
            }
        }

        void OnApplicationQuit() => StopRecording();
        void OnDestroy() => StopRecording();

        /// <summary>
        /// The first hard failure ends the recording for this attempt. A fault that
        /// repeats per capture would otherwise write a TechnicalError five times a
        /// second and bury the event stream it is meant to annotate, which is the same
        /// reason ResearchLogService caps the errors it forwards.
        /// </summary>
        void Fail(string detail)
        {
            LogTechnicalError(detail);
            StopRecording();
        }

        void LogTechnicalError(string detail)
        {
            if (errorLogged || logger == null)
                return;

            errorLogged = true;
            var taskId = definition == null ? ResearchTaskId.Session : definition.taskId;
            var taskContext = definition == null ? "session" : definition.taskContext;
            var layoutId = definition == null ? "session" : definition.layoutId;
            logger.LogEvent(taskId, attemptId, taskContext, layoutId, ResearchEventType.TechnicalError, "recording.first-person", "system", "", "", "", "failure", TaskState.Active, "system", Vector3.zero, Quaternion.identity, detail);
        }

        /// <summary>
        /// Camera.main when the rig tags one, otherwise the first camera in the scene -
        /// except our own. Destroy is deferred to the end of the frame, so a recorder
        /// restarted by a development reset can still see the previous attempt's capture
        /// camera in this search, and adopting it would record a still view of whatever
        /// that camera was last pointed at.
        /// </summary>
        static Camera FindParticipantCamera()
        {
            if (Camera.main != null)
                return Camera.main;

            foreach (var camera in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (camera.name != CaptureCameraName)
                    return camera;
            return null;
        }

        void ReleaseCapture()
        {
            if (captureCamera != null)
                Destroy(captureCamera.gameObject);
            if (readback != null)
                Destroy(readback);
            if (target != null)
            {
                target.Release();
                Destroy(target);
            }

            captureCamera = null;
            readback = null;
            target = null;
            participantCamera = null;
        }
    }
}
