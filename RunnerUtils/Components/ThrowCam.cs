using AimAssist;
using HarmonyLib;
using RunnerUtils.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace RunnerUtils.Components;

public static class ThrowCam
{
    private static Camera m_cam;
    private static Camera m_oldCam;
    private static GameObject m_obj;
    private static Vector3 m_velocity;

    public static bool cameraAvailable;

    private static void UpdatePos(Transform anchor, Vector3 velocity) {
        m_obj.transform.position = (anchor.position-(velocity*Configs.ThrowCamRangeScale));
        if (Configs.ThrowCamUnlockCameraEnabled) {
            m_obj.transform.rotation = GameManager.instance.cameraManager.GetArmCamera().transform.rotation;
        } else {
            m_obj.transform.LookAt(anchor);
        }
    }

    public static void Reset() {
        cameraAvailable = false;
        GameManager.instance.player.GetHUD().GetReticle().gameObject.SetActive(true);
        if (m_cam) {
            m_cam.enabled = false;
            m_oldCam.enabled = true;
            Object.Destroy(m_obj);
        }
    }

    public static void ToggleCam() {
        m_oldCam.enabled = !m_oldCam.enabled;
        m_cam.enabled = !m_cam.enabled;
        var reticle = GameManager.instance.player.GetHUD().GetReticle().gameObject;
        reticle.SetActive(!reticle.activeInHierarchy);
    }

    private static void SetupCam() {
        m_oldCam = GameManager.instance.cameraManager.GetManagersCamera();
        m_obj = new GameObject();

        m_cam = m_obj.AddComponent<Camera>();
        m_cam.GetUniversalAdditionalCameraData().cameraStack.Add(m_oldCam.GetUniversalAdditionalCameraData().cameraStack[1]);
        m_cam.enabled = false;

        cameraAvailable = true;
        if (Configs.ThrowCamAutoSwitchEnabled) {
            ToggleCam();
        }
    }

    [HarmonyPatch(typeof(PlayerWeaponToss))]
    public static class PlayerWeaponTossPatch
    {
        [HarmonyPatch(nameof(PlayerWeaponToss.Initialize), typeof(WeaponPickup), typeof(AimTarget))]
        [HarmonyPostfix]
        public static void SetupCamOnInit(ref TossedEquipment ___tossedEquipment) {
            if (cameraAvailable) Reset();
            SetupCam();
        }

        [HarmonyPatch(nameof(PlayerWeaponToss.Update))]
        [HarmonyPostfix]
        public static void UpdateCamPosition(ref bool ___hitSurface, ref Transform ___tiltAnchor, ref Transform ___spinAnchor) {
            if (___hitSurface || !cameraAvailable) return;
            UpdatePos(___tiltAnchor, m_velocity);
        }

        [HarmonyPatch(nameof(PlayerWeaponToss.OnCollisionEnter))]
        [HarmonyPostfix]
        public static void ResetOnCollision() {
            Reset();
        }

        [HarmonyPatch(nameof(PlayerWeaponToss.FixedUpdate))]
        [HarmonyPostfix]
        public static void UpdateVelocity(float ___speed, float ___gravity, Transform ___tiltAnchor) {
            var newVelocity = ___tiltAnchor.parent.transform.forward * ___speed;
            newVelocity += Vector3.down * ___gravity;
            m_velocity = newVelocity;
        }
    }


    [HarmonyPatch(typeof(TossedEquipment))]
    public static class TossedEquipmentPatch
    {
        [HarmonyPatch(nameof(TossedEquipment.Initialize))]
        [HarmonyPostfix]
        public static void SetupCamOnInit() {
            if (cameraAvailable) Reset();
            SetupCam();
        }

        [HarmonyPatch(nameof(TossedEquipment.Update))]
        [HarmonyPostfix]
        public static void UpdateCamPosition(ref bool ___collided, ref Rigidbody ___rb) {
            if (___collided || !cameraAvailable) return;
            UpdatePos(___rb.gameObject.transform, ___rb.velocity);
        }

        [HarmonyPatch(nameof(TossedEquipment.OnCollisionEnter))]
        [HarmonyPostfix]
        public static void ResetOnCollision() {
            Reset();
        }
    }
}