using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;

namespace Splash
{
    [BepInPlugin("PlacidPlasticDucks.Splash", "Splash", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public ConfigEntry<float> explosionRadius;
        public ConfigEntry<float> explosionForce;
        public static Plugin instance;
        void Awake()
        {
            instance = this;
            setupConfig();
            Harmony.CreateAndPatchAll(typeof(Plugin));
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        void setupConfig(){
            explosionRadius = Config.Bind<float>("Splash","Radius",2,"the radius of the splash. larger values will affect ducks farther away.");
            explosionForce = Config.Bind<float>("Splash","Force",11,"the force of the splash. larger values will launch ducks farther away.");
        }
        [HarmonyPatch(typeof(GeneralManager),"Update")]
        [HarmonyPostfix]
        public static void PostUpdate(GeneralManager __instance){
            if (Input.GetMouseButtonDown(0)){
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit HitInfo, Mathf.Infinity, LayerMask.GetMask("Suimono_Water"));
                if (HitInfo.collider.tag == "water"){
                    GameObject splashFX = GameObject.Instantiate(GameObject.Find("/SUIMONO_Module/_particle_effects/BigSplash(Clone)"));
                    splashFX.transform.position = HitInfo.point;
                    ParticleSystem par = splashFX.GetComponent<ParticleSystem>();
                    var main = par.main;
                    main.stopAction = ParticleSystemStopAction.Destroy;
                    par.Play();
                    Collider[] objectsInRange = Physics.OverlapSphere(HitInfo.point, instance.explosionRadius.Value);
                    foreach (Collider c in objectsInRange){
                        Rigidbody rb = c.GetComponent<Rigidbody>();
                        if (rb != null){
                            rb.AddExplosionForce(instance.explosionForce.Value, HitInfo.point, instance.explosionRadius.Value, 0, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }
}
