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
    public class GrubTrain : Mod
    {

        internal static GrubTrain Instance;
        public GameObject grubPrefab;
        public List<GameObject> grubs = new List<GameObject>();
        
        public override string GetVersion()
        {
            return "0.1";
        }


        public GameObject createGrubCompanion(GameObject ft = null){
            var grub = grubPrefab.createCompanionFromPrefab();
            //add control and adjust parameters
            var gc = grub.GetAddComponent<CompanionControl>();
            gc.moveSpeed = 13f;
            gc.followDistance = 3f;
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
            var pfsm = grub.GetComponent<PlayMakerFSM>();
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
        }
       
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Crossroads_05", "Grub Bottle/Grub")
            };   
        }

        
        public void createTrain(){
            if(grubs.Count > 3){ return; }
            GameObject grub;
            if(grubs.Count==0){
                grub = createGrubCompanion();
            } else {
                grub = createGrubCompanion(grubs[grubs.Count-1]);
            }
            grubs.Add(grub);
        }
        public void update()
        {
            createTrain();
        }

    }

}
