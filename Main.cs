using MelonLoader;
using Harmony;
using NKHook6.Api;
using Assets.Scripts.Unity.UI_New.InGame.Races;
using Assets.Scripts.Simulation.Towers.Weapons;
using NKHook6;
using Assets.Scripts.Simulation;
using Assets.Scripts.Unity.UI_New.InGame;
using NKHook6.Api.Extensions;
using Assets.Scripts.Unity.UI_New.Main;
using NKHook6.Api.Events;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Models.Towers;
using NKHook6.Api.Utilities;
using Assets.Scripts.Unity;

using NKHook6.Api.Enums;

using static NKHook6.Api.Events._Towers.TowerEvents;
using Assets.Scripts.Simulation.Towers;

using static NKHook6.Api.Events._Weapons.WeaponEvents;
using Assets.Scripts.Utils;

using static NKHook6.Api.Events._TimeManager.TimeManagerEvents;
using Il2CppSystem.Collections;
using NKHook6.Api.Events._Bloons;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Bloons;
using Il2CppSystem.Collections.Generic;
using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Weapons;
using NKHook6.Api.Events._Weapons;
using NKHook6.Api.Events._Projectile;
using Assets.Scripts.Models.Rounds;
using UnhollowerBaseLib;
using System.Linq;
using Assets.Scripts.Unity.UI_New.Races;
using Assets.Scripts.Models.Store;
using Assets.Scripts.Models.Store.Loot;
using Assets.Scripts.Unity.Gift;
using System.Text.RegularExpressions;

namespace twitchcontrols
{
    public class Main : MelonMod
    {
        public static string[] effects = {
            "Give 500 cash",
            "Send next round early",
            "Send next 3 rounds early",
            "Lose 40% of cash",
            "Sell random tower",
            "Sell half of the towers",
            "lose 10% of cash",
            "move a random tower by 20 to 60 units",
            "move all towers by 20 to 60 units",
            "each type of bloon gets a random speed",
            "make bloons 3x as fast for 30 seconds",
            "make bloons 0.5x as fast for 55 seconds",
            "towers spin for 55 seconds",
            "nothing",
            "reset all cooldowns",
            "change all tower targeting to last",
            "delete all bloons on screen",
            "all bloons are camgrow fortified for 60s",
            "bloons randomly upgrade or take no dmg (30s)",
            "reset lives",
            "new towers turn to cave monkeys (40s)",
            "new towers turn to cold sentries (40s)",
            "new towers turn to energising totems (20s)",
            "new towers turn to portable lakes (20s)"
        };
        public static int prevChat = 0;

        public static string prevEffect = "nothing";
        public static string[] options = { "nothing", "Give 500 cash", "Send next round early" };
        public static int[] votes = { 0, 0, 0 };
        public static float voteTimer = 60;
        public static float voteTimerMax = 60;

        public static float getChatTimer = 0;
        public static System.Random random = new System.Random();
        static string chatFile = @"C:\Program Files (x86)\Steam\steamapps\common\BloonsTD6\twitchchat.txt";

        string[] chat = { "" };

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            EventRegistry.subscriber.register(this.GetType());
            Logger.Log("twitchcontrols mod loaded");
            try
            {
                System.IO.File.WriteAllText(chatFile, "");
            }
            catch
            {

            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (InGame.instance != null && InGame.instance.bridge != null)
            {
                voteTimer += UnityEngine.Time.deltaTime;
                getChatTimer += UnityEngine.Time.deltaTime;
            }
            else
            {
                Logger.Log("get in a game to start the chaos mod");
                prevEffect = "none";
                options[0] = "nothing";
                voteTimer = voteTimerMax;
            }

            //towers spin
            if (prevEffect == effects[12] && voteTimer < (voteTimerMax - 5))
            {
                var towers = InGame.instance.bridge.GetAllTowers();
                foreach (var tower in towers)
                {
                    tower.tower.RotateTower(5, false);
                }
            }
            //reset 3x speed
            if (prevEffect == effects[10] && voteTimer > (voteTimerMax - 30))
            {
                var models = InGame.instance.bridge.Model.bloons;
                foreach (var model in models)
                {
                    model.Speed /= 3;
                }
                prevEffect += " (done)";
            }
            //reset 0.5x speed
            if (prevEffect == effects[11] && voteTimer > (voteTimerMax - 5))
            {
                var models = InGame.instance.bridge.Model.bloons;
                foreach (var model in models)
                {
                    model.Speed /= 0.5f;
                }
                prevEffect += " (done)";
            }

            if (getChatTimer > 0.3)
            {

                try
                {
                    chat = System.IO.File.ReadAllLines(chatFile);
                }
                catch
                {

                }

                for (var i = prevChat; i < chat.Length; i++)
                {
                    var line = chat[i];
                    if (line == "1") votes[0]++;
                    if (line == "2") votes[1]++;
                    if (line == "3") votes[2]++;
                }
                prevChat = chat.Length;

                if (voteTimer > voteTimerMax)
                {
                    if (votes[0] >= votes[1] && votes[0] >= votes[2])
                    {
                        prevEffect = options[0];
                    }
                    else if (votes[1] >= votes[0] && votes[1] >= votes[2])
                    {
                        prevEffect = options[1];
                    }
                    else
                    {
                        prevEffect = options[2];
                    }
                    //one time effects go here
                    try
                    {
                        //add 500 cash
                        if (prevEffect == effects[0])
                        {
                            InGame.instance.addCash(500);
                        }
                        //start next round
                        if (prevEffect == effects[1])
                        {
                            InGame.instance.bridge.StartRaceRound();
                        }
                        //start next 3 rounds
                        if (prevEffect == effects[2])
                        {
                            InGame.instance.bridge.StartRaceRound();
                            InGame.instance.bridge.StartRaceRound();
                            InGame.instance.bridge.StartRaceRound();
                        }
                        //cash * 0.6
                        if (prevEffect == effects[3])
                        {
                            var cash = InGame.instance.getCash() * 0.6;
                            InGame.instance.setCash(cash);
                        }
                        //sell random tower
                        if (prevEffect == effects[4])
                        {
                            var towers = InGame.instance.bridge.GetAllTowers();
                            if (towers.Count > 0)
                                InGame.instance.SellTower(towers[random.Next(0, towers.Count)]);
                        }
                        //sell half of the towers
                        if (prevEffect == effects[5])
                        {
                            var towers = InGame.instance.bridge.GetAllTowers();
                            if (towers.Count > 0)
                                for (int i = 0; i < towers.Count * 0.5; i++)
                                {
                                    var randomtower = towers[random.Next(0, towers.Count)];
                                    InGame.instance.SellTower(randomtower);
                                }
                        }
                        //cash * 0.9
                        if (prevEffect == effects[6])
                        {
                            var cash = InGame.instance.getCash() * 0.9;
                            InGame.instance.setCash(cash);
                        }
                        //move random tower
                        if (prevEffect == effects[7])
                        {
                            var towers = InGame.instance.bridge.GetAllTowers();
                            var tower = towers[random.Next(0, towers.Count)];
                            float x = tower.position.x + (float)((random.NextDouble() > 0.5 ? 1 : -1) * (20 + (random.NextDouble() * 40)));
                            float y = tower.position.y + (float)((random.NextDouble() > 0.5 ? 1 : -1) * (20 + (random.NextDouble() * 40)));
                            tower.tower.PositionTower(new Assets.Scripts.Simulation.SMath.Vector2(x, y));
                        }
                        //move all towers
                        if (prevEffect == effects[8])
                        {
                            var towers = InGame.instance.bridge.GetAllTowers();
                            foreach (var tower in towers)
                            {
                                float x = tower.position.x + (float)((random.NextDouble() > 0.5 ? 1 : -1) * (20 + (random.NextDouble() * 40)));
                                float y = tower.position.y + (float)((random.NextDouble() > 0.5 ? 1 : -1) * (20 + (random.NextDouble() * 40)));
                                tower.tower.PositionTower(new Assets.Scripts.Simulation.SMath.Vector2(x, y));
                            }
                        }
                        //each bloon gets a random speed
                        if (prevEffect == effects[9])
                        {
                            var models = InGame.instance.bridge.Model.bloons;
                            foreach (var model in models)
                            {
                                model.Speed = (((float)random.NextDouble()) * 95) + 40;
                            }
                        }
                        //3x speed
                        if (prevEffect == effects[10])
                        {
                            var models = InGame.instance.bridge.Model.bloons;
                            foreach (var model in models)
                            {
                                model.Speed *= 3;
                            }
                        }
                        //half speed
                        if (prevEffect == effects[11])
                        {
                            var models = InGame.instance.bridge.Model.bloons;
                            foreach (var model in models)
                            {
                                model.Speed *= 0.5f;
                            }
                        }
                        //towers spin happens each frame
                        if (prevEffect == effects[12])
                        {

                        }
                        //nothing effect
                        if (prevEffect == effects[13])
                        {

                        }
                        //reset all cooldowns
                        if (prevEffect == effects[14])
                        {
                            InGame.instance.bridge.ResetAbilityCooldowns(false);
                        }
                        //targeting to last
                        if (prevEffect == effects[15])
                        {
                            foreach (var t in InGame.instance.bridge.GetAllTowers())
                            {
                                string[] valid = new string[] {
                                    "Dart",
                                    "Boomerang",
                                    "BombShooter",
                                    "GlueGunner",
                                    "Sniper",
                                    "Sub",
                                    "Buccaneer",
                                    "Wizard",
                                    "Super",
                                    "Ninja",
                                    "Alchemist",
                                    "Druid",
                                    "Engineer",
                                };
                                var name = Regex.Replace(t.tower.namedMonkeyKey, @"\d+", "");
                                name = name.Replace("Monkey", "");
                                Logger.Log(name);
                                if (valid.Contains(name))
                                    t.tower.SetTargetType(TargetType.last);
                            }

                        }
                        //delete all bloons on screen
                        if (prevEffect == effects[16])
                        {
                            InGame.instance.DeleteAllBloons();
                        }
                        //camgrow fortified
                        if (prevEffect == effects[17])
                        {

                        }
                        //bloons randomly upgrade or take no dmg (30s)
                        if (prevEffect == effects[18])
                        {

                        }
                        //reset lives
                        if (prevEffect == effects[19])
                        {
                            InGame.instance.bridge.ResetHealth();
                        }
                        //effects 20 to 23 change towers


                        if (prevEffect == "Spawn Ceramic")
                        {
                            Il2CppReferenceArray<BloonEmissionModel> bme = new Il2CppReferenceArray<BloonEmissionModel>(1);
                            bme[0] = (new BloonEmissionModel("Ceramic", 1, "Ceramic"));
                            InGame.instance.bridge.SpawnBloons(bme, 38, 0);
                        }
                        if (prevEffect == "Spawn MOAB")
                        {
                            Il2CppReferenceArray<BloonEmissionModel> bme = new Il2CppReferenceArray<BloonEmissionModel>(1);
                            bme[0] = (new BloonEmissionModel("Moab", 1, "Moab"));
                            InGame.instance.bridge.SpawnBloons(bme, 40, 0);
                        }
                        if (prevEffect == "Spawn BFB")
                        {
                            Il2CppReferenceArray<BloonEmissionModel> bme = new Il2CppReferenceArray<BloonEmissionModel>(1);
                            bme[0] = (new BloonEmissionModel("Bfb", 1, "Bfb"));
                            InGame.instance.bridge.SpawnBloons(bme, 60, 0);
                        }
                        if (prevEffect == "Spawn ZOMG")
                        {
                            Il2CppReferenceArray<BloonEmissionModel> bme = new Il2CppReferenceArray<BloonEmissionModel>(1);
                            bme[0] = (new BloonEmissionModel("Zomg", 1, "Zomg"));
                            InGame.instance.bridge.SpawnBloons(bme, 80, 0);
                        }
                    }
                    catch
                    {
                        prevEffect += " (error)";
                    }



                    voteTimer = 0;
                    votes = new int[] { 0, 0, 0 };

                    string[] temp = new string[effects.Length];
                    effects.CopyTo(temp, 0);
                    var tempEffects = temp.ToList();
                    int round = InGame.instance.bridge.GetCurrentRound();
                    if (round > 10 && round < 40)
                        tempEffects.Add("Spawn Ceramic");
                    if (round > 20)
                        tempEffects.Add("Spawn MOAB");
                    if (round > 40)
                        tempEffects.Add("Spawn BFB");
                    if (round > 60)
                        tempEffects.Add("Spawn ZOMG");

                    string[] r = tempEffects.OrderBy(x => random.Next()).ToArray();
                    options[0] = r[0];
                    options[1] = r[1];
                    options[2] = r[2];
                }


                Logger.Log(" \n \n \n  \n  \n  \n  \n  \n  \n  \n  \n  \n   \n  \n  \n  \n  \n  \n  \n   \n  \n  \n  \n  \n  \n  \n ");
                Logger.Log("Last effect: " + prevEffect);
                Logger.Log("Option 1: " + options[0] + " (" + votes[0] + " votes)");
                Logger.Log("Option 2: " + options[1] + " (" + votes[1] + " votes)");
                Logger.Log("Option 3: " + options[2] + " (" + votes[2] + " votes)");
                Logger.Log("seconds left to vote: " + (voteTimerMax - voteTimer));


                getChatTimer = 0;

            }





        }



        [EventAttribute("KeyPressEvent")]
        public static void onEvent(KeyEvent e)
        {

            string key = e.key + "";

            //if (key == "Alpha5")
            //{
            //    options[0] = effects[20];
            //}
            //if (key == "Alpha6")
            //{
            //    options[0] = effects[21];
            //}
            //if (key == "Alpha7")
            //{
            //    options[0] = effects[22];
            //}
            //if (key == "Alpha8")
            //{
            //    options[0] = effects[23];
            //}
            //if (key == "Alpha9")
            //{
            //    options[0] = effects[18];
            //}
            //if (key == "Alpha0")
            //{
            //    options[0] = effects[19];
            //}

        }

        [HarmonyPatch(typeof(Bloon), "Initialise")]
        public class BloonInitialise_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(Bloon __instance, ref Model modelToUse)
            {
                //camgrow fortified
                if (prevEffect == effects[17])
                {
                    modelToUse = BloonUtils.SetBloonStatus(modelToUse.name, true, true, true);
                }
                //upgrade bloons randomly
                if (prevEffect == effects[18] && voteTimer < 30)
                {
                    modelToUse = GetNextBloon(modelToUse.name);
                }

                return true;
            }

            public static BloonModel GetNextBloon(string currentBloon)
            {
                var allBloonTypes = BloonUtils.GetAllBloonTypes();
                int num1 = random.Next(0, 4);
                int num2 = num1 == 0 ? 0 : 1;
                int num3 = BloonUtils.GetBloonIdNum(currentBloon);
                //so the bad doesn't turn to an invis bloon
                if (num3 + num2 > allBloonTypes.Count - 2)
                {
                    num3 = allBloonTypes.Count - 2;
                }
                else
                {
                    num3 += num2;
                }
                return BloonUtils.GetNextStrongestBloon(allBloonTypes[num3], false, false, false, true);
            }


        }

        [HarmonyPatch(typeof(Tower), "Initialise")]
        public class TowerInitialise_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(Tower __instance, ref Model modelToUse)
            {

                if (prevEffect == effects[20] && voteTimer < 40)
                    modelToUse = TowerUtils.GetTower(TowerType.CaveMonkey);

                if (prevEffect == effects[21] && voteTimer < 40)
                    modelToUse = TowerUtils.GetTower(TowerType.SentryCold);

                if (prevEffect == effects[22] && voteTimer < 20)
                    modelToUse = TowerUtils.GetTower(TowerType.EnergisingTotem);

                if (prevEffect == effects[23] && voteTimer < 20)
                    modelToUse = TowerUtils.GetTower(TowerType.PortableLake);



                return true;
            }
        }

    }

}