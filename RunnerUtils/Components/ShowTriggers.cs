using System;
using System.Collections.Generic;
using System.Linq;
using Enemy;
using Equipment;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace RunnerUtils.Components;

public static class ShowTriggers
{
    private static Texture2D tex = new Texture2D(1, 1);
    public static Material mat = new Material(Shader.Find("Sprites/Default"));
    private static Color spawnerColor = Color.magenta;
    private static Color placedEquipmentColor = new Color(1, 0, 0.5f, 0.5f);
    private static Dictionary<Type, Color> triggerColors = new Dictionary<Type, Color>();
    private static Dictionary<string, Color> extraColors = new Dictionary<string, Color>();
    private static List<GameObject> registry = new List<GameObject>();
    private static readonly int mainTex = Shader.PropertyToID("_MainTex");
    private static readonly int baseColor = Shader.PropertyToID("_BaseColor");

    static ShowTriggers() {
        mat.SetTexture(mainTex, tex);

        triggerColors.Add(typeof(PlayerOutOfBoundsBox), new Color(1, 0.5f, 0, 0.5f));
        triggerColors.Add(typeof(PlayerTimerStartBox), new Color(0.5f, 0, 1, 0.5f));
        triggerColors.Add(typeof(KillPlayerEventTrigger), new Color(1, 0, 0, 0.5f));
        extraColors.Add("Spawn", new Color(1, 1, 0, 0.5f));
        extraColors.Add("SpawnEnemy", new Color(1, 1, 0, 0.5f));
    }

    public static void ShowAllOf<T>() where T : MonoBehaviour {
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            if (registry[i].TryGetComponent<T>(out _)) registry[i].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public static void HideAllOf<T>() where T : MonoBehaviour {
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            if (registry[i].TryGetComponent<T>(out _)) registry[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public static void ToggleAllOf<T>() where T : MonoBehaviour {
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            if (registry[i].TryGetComponent<T>(out _)) {
                var renderer = registry[i].GetComponent<MeshRenderer>();
                renderer.enabled = !renderer.enabled;
            }
        }
    }

    public static void ShowAll() {
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            foreach (Transform child in registry[i].transform)
                if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(true);
            registry[i].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public static void HideAll(){
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            foreach (Transform child in registry[i].transform)
                if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(false);
            registry[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public static void ToggleAll() {
        for (int i = registry.Count - 1; i >= 0; --i) {
            if (!registry[i]) {
                registry.RemoveAt(i);
                continue;
            }
            var renderer = registry[i].GetComponent<MeshRenderer>();
            foreach (Transform child in registry[i].transform)
                if (child.gameObject.name == "SpawnerTrace") child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
            renderer.enabled = !renderer.enabled;
        }
    }

    public static void RegisterAllOf<T>() where T : MonoBehaviour {
        registry.Clear();
        foreach (var obj in Object.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
            if (obj.gameObject.GetComponent<T>())
                RegisterObj(obj);
        }
    }

    //horrible nested loops . yay
    public static void RegisterAllExcluding(Type[] Ts) {
        registry.Clear();
        foreach (var obj in Object.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
            bool isOfCorrectType = true;
            foreach (var t in Ts) {
                if (obj.gameObject.GetComponent(t)) {
                    isOfCorrectType = false;
                    break;
                }
            }
            if (isOfCorrectType) RegisterObj(obj);
        }
    }

    public static void RegisterAll() {
        registry.Clear();
        foreach (var obj in Object.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
            RegisterObj(obj);
        }
    }

    public static void ExtendRegistry() {
        foreach (var obj in Object.FindObjectsOfType<EventTriggerBoxPlayer>(true)) {
            if (!registry.Contains(obj.gameObject)) {
                RegisterObj(obj);
            }
        }
    }

    private static void MakeVisible(GameObject obj, Color boxColor) {
        if (!obj.TryGetComponent<BoxCollider>(out var collider)) return;

        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        
        if (!obj.TryGetComponent<MeshFilter>(out var filter)) {
            filter = obj.AddComponent<MeshFilter>();
        }

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = cube.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(cube);
        Mesh scaled = Object.Instantiate(cubeMesh);

        Vector3[] verts = scaled.vertices;
        for (int i = 0; i < verts.Length; ++i) {
            verts[i] = Vector3.Scale(verts[i], collider.size) + collider.center;
        }
        scaled.vertices = verts;
        scaled.RecalculateBounds();
        filter.mesh = scaled;

        renderer.material = mat;

        renderer.enabled = false; //disable by default
        renderer.material.color = boxColor;
    }

    private static void RegisterObj(EventTriggerBoxPlayer obj){

        var col = new Color(0, 1, 0, 0.25f);

        UnityEvent onTrigger = obj.onTrigger;
        int eventCount = onTrigger?.GetPersistentEventCount() ?? 0;

        for (int i = 0; i < eventCount; ++i) {

            //add spawner trace objects
            string methodName = onTrigger?.GetPersistentMethodName(i);


            if (methodName is "Spawn" or "SpawnEnemy") {
                var target = (onTrigger.GetPersistentTarget(i));
                if (!target) continue;

                GameObject targetObj;
                try {
                    targetObj = ((MonoBehaviour)target).gameObject;
                } catch {
                    //this exists because ONE trigger on permafrost is wired up wrong
                    continue;
                }

                var spawners = new List<EnemySpawner>(targetObj.GetComponentsInChildren<EnemySpawner>(true));

                foreach (var spawner in spawners) {
                    var child = new GameObject("SpawnerTrace");
                    child.transform.SetParent(obj.gameObject.transform);

                    var line = child.AddComponent<LineRenderer>();
                    line.material = mat;
                    line.startColor = extraColors[methodName];
                    line.endColor = spawnerColor;
                    line.positionCount = 2;
                    line.SetPosition(0, obj.gameObject.transform.position);
                    line.SetPosition(1, spawner.gameObject.transform.position);
                    child.SetActive(false);
                }

            }

            if (methodName != null && extraColors.TryGetValue(methodName, out var color))
                col = color;
        }

        foreach (var kv in triggerColors.Where(kv => obj.gameObject.GetComponent(kv.Key))) {
            col = kv.Value;
        }

        MakeVisible(obj.gameObject, col);
        registry.Add(obj.gameObject);
    }

    [HarmonyPatch(typeof(EnemySpawner))]
    public class EnemySpawnerPatch
    {
        [HarmonyPatch(nameof(EnemySpawner.Start))]
        [HarmonyPostfix]
        public static void ColorAndRegisterSpawners(EnemySpawner __instance) {
            if (__instance.gameObject.TryGetComponent<MeshRenderer>(out var renderer)) {
                renderer.material.SetColor(baseColor, spawnerColor);
            }
            __instance.gameObject.SetActive(true);
            registry.Add(__instance.gameObject);
        }

        [HarmonyPatch(nameof(EnemySpawner.SpawnEnemy))]
        [HarmonyPostfix]
        public static void KeepActiveAfterSpawn(EnemySpawner __instance) {
            __instance.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(PlacedEquipment), nameof(PlacedEquipment.Initialize))]
    public class PlacedEquipmentPatch
    {
        [HarmonyPostfix]
        public static void ShowAndRegisterEquipment(PlacedEquipment __instance) {
            MakeVisible(__instance.gameObject, placedEquipmentColor);
            registry.Add(__instance.gameObject);
        }
    }
}