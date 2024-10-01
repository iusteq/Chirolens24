using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using InputDevice = UnityEngine.XR.InputDevice;

namespace MagicLeap.Examples
{
    [Serializable]
    public class EyeTrackingData
    {
        public string timestamp;
        public EyeData leftEye;
        public EyeData rightEye;
        public Vector3 fixationPoint;
        public float fixationConfidence;
    }

    [Serializable]
    public class EyeData
    {
        public Vector3 position;
        public Vector3 gazeDirection;
        public float confidence;
        public float openness;
    }

    public class SaveEyeTrackingData : MonoBehaviour
    {
        [SerializeField, Tooltip("Left Eye Statistic Panel")]
        private TMP_Text leftEyeTextStatic;
        [SerializeField, Tooltip("Right Eye Statistic Panel")]
        private TMP_Text rightEyeTextStatic;
        [SerializeField, Tooltip("Both Eyes Statistic Panel")]
        private TMP_Text bothEyesTextStatic;
        [SerializeField, Tooltip("Fixation Point marker")]
        private Transform eyesFixationPoint;

        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.EyesActions eyesActions;
        private InputDevice eyesDevice;
        private bool permissionGranted = false;
        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        public string currentImageName; // The current image being tracked
        private int sessionCount;
        private bool isTracking = false; // Flag to track if we're currently tracking an image
        private string currentSessionFilePath; // Path for the current session's data file
        private string currentCsvFilePath; // New variable for the CSV file path

        private void Awake()
        {
            permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
        }

        private void Start()
        {
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            MLPermissions.RequestPermission(MLPermission.EyeTracking, permissionCallbacks);

            sessionCount = PlayerPrefs.GetInt("AppStartCount", 0);
        }

        private void OnDestroy()
        {
            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

            mlInputs.Disable();
            mlInputs.Dispose();
            InputSubsystem.Extensions.MLEyes.StopTracking();
        }

        private void Update()
        {
            if (!permissionGranted || !eyesDevice.isValid)
            {
                this.eyesDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice);
                return;
            }

            if (!isTracking)
            {
                return; // If we're not tracking, don't update the eye data
            }

            // Capture eye data
            var eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();
            InputSubsystem.Extensions.TryGetEyeTrackingState(eyesDevice, out var trackingState);

            // Left Eye Data
            var leftEyeForwardGaze = eyes.leftEyeRotation * Vector3.forward;
            string leftEyeText = $"Center:\n({eyes.leftEyePosition.x:F2}, {eyes.leftEyePosition.y:F2}, {eyes.leftEyePosition.z:F2})\n" +
                                 $"Gaze:\n({leftEyeForwardGaze.x:F2}, {leftEyeForwardGaze.y:F2}, {leftEyeForwardGaze.z:F2})\n" +
                                 $"Confidence:\n{trackingState.LeftCenterConfidence:F2}\n" +
                                 $"Openness:\n{eyes.leftEyeOpenAmount:F2}";

            leftEyeTextStatic.text = leftEyeText;

            // Right Eye Data
            var rightEyeForwardGaze = eyes.rightEyeRotation * Vector3.forward;
            string rightEyeText = $"Center:\n({eyes.rightEyePosition.x:F2}, {eyes.rightEyePosition.y:F2}, {eyes.rightEyePosition.z:F2})\n" +
                                  $"Gaze:\n({rightEyeForwardGaze.x:F2}, {rightEyeForwardGaze.y:F2}, {rightEyeForwardGaze.z:F2})\n" +
                                  $"Confidence:\n{trackingState.RightCenterConfidence:F2}\n" +
                                  $"Openness:\n{eyes.rightEyeOpenAmount:F2}";

            rightEyeTextStatic.text = rightEyeText;

            // Both Eyes Data
            string bothEyesText = $"Fixation Point:\n({eyes.fixationPoint.x:F2}, {eyes.fixationPoint.y:F2}, {eyes.fixationPoint.z:F2})\n" +
                                  $"Confidence:\n{trackingState.FixationConfidence:F2}";
            bothEyesTextStatic.text = bothEyesText;

            // Log Blink Detection
            if (trackingState.RightBlink || trackingState.LeftBlink)
            {
                Debug.Log($"Eye Tracking Blink Registered Right Eye Blink: {trackingState.RightBlink} Left Eye Blink: {trackingState.LeftBlink}");
            }

            // Collect all the data into a class and save it to JSON
            EyeTrackingData eyeTrackingData = new EyeTrackingData
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // ISO 8601 format
                leftEye = new EyeData
                {
                    position = eyes.leftEyePosition,
                    gazeDirection = leftEyeForwardGaze,
                    confidence = trackingState.LeftCenterConfidence,
                    openness = eyes.leftEyeOpenAmount
                },
                rightEye = new EyeData
                {
                    position = eyes.rightEyePosition,
                    gazeDirection = rightEyeForwardGaze,
                    confidence = trackingState.RightCenterConfidence,
                    openness = eyes.rightEyeOpenAmount
                },
                fixationPoint = eyes.fixationPoint,
                fixationConfidence = trackingState.FixationConfidence
            };

            // Convert data to JSON
            string jsonData = JsonUtility.ToJson(eyeTrackingData, true);

            // Save JSON to a file
            File.AppendAllText(currentSessionFilePath, jsonData + Environment.NewLine); // Append new data to the file

            SaveDataAsCSV(eyeTrackingData);
        }

        private void SaveDataAsCSV(EyeTrackingData data)
        {
            // If the CSV file doesn't exist, create it and add headers
            if (!File.Exists(currentCsvFilePath))
            {
                string header = "Timestamp,LeftEyePosX,LeftEyePosY,LeftEyePosZ,LeftGazeDirX,LeftGazeDirY,LeftGazeDirZ,LeftConfidence,LeftOpenness," +
                                "RightEyePosX,RightEyePosY,RightEyePosZ,RightGazeDirX,RightGazeDirY,RightGazeDirZ,RightConfidence,RightOpenness," +
                                "FixationPointX,FixationPointY,FixationPointZ,FixationConfidence";
                File.AppendAllText(currentCsvFilePath, header + Environment.NewLine);
            }

            // Append the data row
            string dataRow = $"{data.timestamp}," +
                             $"{data.leftEye.position.x},{data.leftEye.position.y},{data.leftEye.position.z}," +
                             $"{data.leftEye.gazeDirection.x},{data.leftEye.gazeDirection.y},{data.leftEye.gazeDirection.z}," +
                             $"{data.leftEye.confidence},{data.leftEye.openness}," +
                             $"{data.rightEye.position.x},{data.rightEye.position.y},{data.rightEye.position.z}," +
                             $"{data.rightEye.gazeDirection.x},{data.rightEye.gazeDirection.y},{data.rightEye.gazeDirection.z}," +
                             $"{data.rightEye.confidence},{data.rightEye.openness}," +
                             $"{data.fixationPoint.x},{data.fixationPoint.y},{data.fixationPoint.z}," +
                             $"{data.fixationConfidence}";

            File.AppendAllText(currentCsvFilePath, dataRow + Environment.NewLine);
            Debug.Log($"Appended data to CSV file at path: {currentCsvFilePath}");
        }

        // Call this method to start tracking a new image
        public void StartTrackingNewImage(string imageName)
        {
            currentImageName = imageName;

            // Retrieve the session count for the specific painting from PlayerPrefs
            sessionCount = PlayerPrefs.GetInt($"{currentImageName}_SessionCount", 0);
            sessionCount++; // Increment the session count for this specific image

            // Save the updated session count for this painting back to PlayerPrefs
            PlayerPrefs.SetInt($"{currentImageName}_SessionCount", sessionCount);
            PlayerPrefs.Save(); // Make sure PlayerPrefs are saved to disk

            isTracking = true;

            // Create a new file for this image and session for JSON
            currentSessionFilePath = Path.Combine(Application.persistentDataPath, $"{currentImageName}_Session_{sessionCount}_EyeTrackingData.json");

            // Create a new file for this image and session for CSV
            currentCsvFilePath = Path.Combine(Application.persistentDataPath, $"{currentImageName}_Session_{sessionCount}_EyeTrackingData.csv");

            Debug.Log($"Started tracking for image: {currentImageName}, session: {sessionCount}");
        }

        // Call this method to stop tracking the current image
        public void StopTracking()
        {
            isTracking = false;
            Debug.Log($"Stopped tracking for image: {currentImageName}, session: {sessionCount}");
        }

        private void OnPermissionDenied(string permission)
        {
            MLPluginLog.Error($"{permission} denied, example won't function.");
        }

        private void OnPermissionGranted(string permission)
        {
            InputSubsystem.Extensions.MLEyes.StartTracking();
            eyesActions = new MagicLeapInputs.EyesActions(mlInputs);
            permissionGranted = true;
        }
    }
}