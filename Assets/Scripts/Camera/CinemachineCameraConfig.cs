using UnityEngine;
using Unity.Cinemachine; // Unity 6 Cinemachine v3 namespace
using EndlessRunner.Player;

namespace EndlessRunner.Camera
{
    public class CinemachineCameraConfig : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float sprintFOV = 75f;
        [SerializeField] private float fovTransitionSpeed = 5f;

        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeAmplitude = 1.5f;
        [SerializeField] private float shakeFrequency = 2.0f;

        private float currentShakeTime;
        private CinemachineBasicMultiChannelPerlin noise;

        private void Start()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = GetComponent<CinemachineCamera>();
            }

            if (cinemachineCamera != null)
            {
                // Retrieve or add the noise component for screen shake
                noise = cinemachineCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            }

            if (player != null)
            {
                player.OnStumble += TriggerCameraShake;
            }
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnStumble -= TriggerCameraShake;
            }
        }

        private void Update()
        {
            if (cinemachineCamera == null || player == null) return;

            // FOV transitions during Speed Boost
            float targetFOV = player.IsSpeedBoosting ? sprintFOV : normalFOV;
            
            // Adjust lens properties
            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
            cinemachineCamera.Lens = lens;

            // Handle Camera Shake decay
            if (currentShakeTime > 0)
            {
                currentShakeTime -= Time.deltaTime;
                if (currentShakeTime <= 0)
                {
                    StopShake();
                }
            }
        }

        public void TriggerCameraShake()
        {
            if (noise == null) return;
            noise.AmplitudeGain = shakeAmplitude;
            noise.FrequencyGain = shakeFrequency;
            currentShakeTime = shakeDuration;
        }

        private void StopShake()
        {
            if (noise == null) return;
            noise.AmplitudeGain = 0;
            noise.FrequencyGain = 0;
        }
    }
}
