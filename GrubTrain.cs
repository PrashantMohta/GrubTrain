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
using static Satchel.SpriteUtils;
using static Satchel.TextureUtils;

namespace GrubTrain
{
    public class GrubTrain : Mod , ICustomMenuMod,IGlobalSettings<ModSettings>
    {

        public static GrubTrain Instance;
        public GameObject grubPrefab;
        public List<GameObject> grubs = new List<GameObject>();
        public List<GameObject> MenuGos = new List<GameObject>();
        
        public GrubTrain(){
            On.MenuStyleTitle.SetTitle += MenuScreenGrubs;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void MenuScreenGrubs(On.MenuStyleTitle.orig_SetTitle orig, MenuStyleTitle self, int index){
            var floor = new GameObject();
            floor.layer = 8; 
            floor.transform.position = new Vector3(0f,0.2f,-10f);
            var boxC = floor.AddComponent<BoxCollider2D>();
            boxC.size = new Vector2(100f,1f);
            
            MenuGos.ForEach(g => GameObject.Destroy(g));
            MenuGos = new List<GameObject>();

            var dummy = new GameObject();
            dummy.transform.position = new Vector3(14.6f, 2.7f, -18.1f);
            dummy.GetAddComponent<MouseFollow>();
            MenuGos.Add(dummy);
            MenuGos.Add(createGrubCompanion(dummy));
            for(var i = 2; i < 4 ; i++){
                MenuGos.Add(createGrubCompanion(MenuGos[i-1]));
            }

            orig(self, index);
        }
        public override string GetVersion()
        {
            return "0.4";
        }

        public static ModSettings settings { get; set; } = new ModSettings();
        public void OnLoadGlobal(ModSettings s) { 
            destroyTrain();
            settings = s;
            Menu.refreshMenuOptions();
        }
        public ModSettings OnSaveGlobal() => settings;
        public bool ToggleButtonInsideMenu => false;

        public int neededGrubCount = 0;
        public GameObject createGrubCompanion(GameObject ft = null){
            if(grubPrefab == null) { return null; }
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
            ModHooks.AfterSavegameLoadHook += LoadSaveGame;
            CoroutineHelper.GetRunner().StartCoroutine(updateGrubCount());
            CoroutineHelper.GetRunner().StartCoroutine(RefreshMenuOptions());
        }
       
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Crossroads_05", "Grub Bottle/Grub")
            };   
        }

        
        public void createTrain(){
            grubs.RemoveAll(item => item == null);
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
            
            var sr = grub.GetComponent<MeshRenderer>();
            sr.sortingOrder = grubs.Count;

            if(settings.cursedStrats){
                var grubCollider = grub.GetComponent<BoxCollider2D>();
                var terrain = new GameObject("terrain");
                terrain.layer = 8;
                terrain.GetAddComponent<BoxCollider2D>().size = grubCollider.size - new Vector2(0.5f,0.5f);
                terrain.transform.position = grub.transform.position - new Vector3(0.3f,0.5f,0f);
                terrain.transform.SetParent(grub.transform);
            }
            grubs.Add(grub);
        }

        public void destroyTrain(){
            foreach(var grub in grubs){
                GameObject.Destroy(grub);
            }
            grubs = new List<GameObject>();
            foreach(var grub in MenuGos){
                GameObject.Destroy(grub);
            }
            MenuGos = new List<GameObject>();
        }

        public IEnumerator updateGrubCount(){
            while(true){
                yield return new WaitWhile(()=> PlayerData.instance == null );
                yield return new WaitForSeconds(2f);
                neededGrubCount = settings.grubBaseCount;
                if(settings.grubGathererMode){
                    // for each freed grub add more grubs
                    var rescuedGrubs = PlayerData.instance.GetInt(nameof(PlayerData.grubsCollected));
                    neededGrubCount += (rescuedGrubs - settings.returnedCount);
                }
            }
        }
        
        public void OnSceneChanged(Scene from, Scene to){
            if(to.name == "Crossroads_38"){            
                settings.returnedCount = PlayerData.instance.GetInt(nameof(PlayerData.grubsCollected));
                neededGrubCount = settings.grubBaseCount;
                destroyTrain();                        
            }
        }
        public IEnumerator RefreshMenuOptions(){
           yield return null;
           yield return new WaitForSeconds(0.1f);
           Menu.refreshMenuOptions();
        }
        public void update()
        {
            createTrain();
        }
        
        public void LoadSaveGame(SaveGameData data){
            destroyTrain();
        }
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            Menu.saveModsMenuScreen(modListMenu);
            return Menu.CreatemenuScreen();
        }
    }

}
