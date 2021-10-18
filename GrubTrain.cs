using System;
using System.Collections;
using System.Collections.Generic;

using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;


using HutongGames.PlayMaker.Actions;

using static Modding.Logger;
using static Satchel.GameObjectUtils;
using static Satchel.FsmUtil;
using static Satchel.EnemyUtils;

namespace GrubTrain
{
    public class GrubTrain : Mod , ICustomMenuMod,IGlobalSettings<ModSettings>
    {

        internal static GrubTrain Instance;
        public GameObject grubPrefab;
        public List<GameObject> grubs = new List<GameObject>();
        
        public override string GetVersion()
        {
            return "0.2";
        }

        public static ModSettings settings { get; set; } = new ModSettings();
        public void OnLoadGlobal(ModSettings s) => settings = s;
        public ModSettings OnSaveGlobal() => settings;
        public bool ToggleButtonInsideMenu => false;

        public int neededGrubCount = 0;
        public GameObject createGrubCompanion(GameObject ft = null){
            var grub = grubPrefab.createCompanionFromPrefab();
            grub.layer = settings.grubStrats ? 11 : 18;
            //add control and adjust parameters
            var gc = grub.GetAddComponent<CompanionControl>();
            gc.moveSpeed = settings.moveSpeed;
            gc.followDistance = settings.followDistance;
            if(ft != null){
                gc.followTarget = ft;
            }
            //fix up collider size
            var collider = grub.GetAddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f,2.0f);
            collider.offset = new Vector2(0f,-0.4f);
            // add animations
            gc.Animations.Add(State.Idle,"Idle");
            gc.Animations.Add(State.Walk,"Freed Fake");
            gc.Animations.Add(State.Turn,"Cry Turn");
            gc.Animations.Add(State.Teleport,"Idle");
            // extract audios
            var pfsm = grubPrefab.GetComponent<PlayMakerFSM>();
            if(pfsm != null){
                gc.teleport = pfsm.GetAction<AudioPlayRandom>("Hero Close",1).audioClips[0];
                gc.yay = pfsm.GetAction<AudioPlayRandom>("Leave",0).audioClips[0];
                gc.walk = pfsm.GetAction<AudioPlay>("Dig",0).oneShotClip.Value as AudioClip;
            }
            grub.SetActive(true);
            return grub;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            grubPrefab = preloadedObjects["Crossroads_05"]["Grub Bottle/Grub"];
            UnityEngine.Object.DontDestroyOnLoad(grubPrefab);
            ModHooks.HeroUpdateHook += update;
            GameManager.instance.StartCoroutine(updateGrubCount());
        }
       
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Crossroads_05", "Grub Bottle/Grub")
            };   
        }

        
        public void createTrain(){
            if(grubs.Count >= neededGrubCount){ return; }
            GameObject grub;
            if(grubs.Count==0){
                grub = createGrubCompanion();
            } else {
                var followTarget = grubs[grubs.Count-1];
                if(settings.grubGathererMode && UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f && grubs.Count > 4){
                    followTarget = grubs[3];    
                    if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.1f){
                        followTarget = grubs[2];
                    }
                }
                grub = createGrubCompanion(followTarget);
            }
            grubs.Add(grub);
        }

        public IEnumerator updateGrubCount(){
            yield return new WaitWhile(()=> PlayerData.instance == null );
            while(true){
                yield return new WaitForSeconds(2f);
                neededGrubCount = settings.grubBaseCount;
                if(settings.grubGathererMode){
                    // for each freed grub add more grubs
                    var rescuedGrubs = PlayerData.instance.GetVariable<List<string>>("scenesGrubRescued");
                    if(rescuedGrubs != null){
                        neededGrubCount += rescuedGrubs.Count;
                    }
                }
            }
        }
        public void update()
        {
            createTrain();
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            Menu.saveModsMenuScreen(modListMenu);
            return Menu.CreatemenuScreen();
        }
    }

}
