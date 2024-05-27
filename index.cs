using HarmonyLib; // HarmonyLib comes included with a NeosModLoader install
using NovrX;
using System;
using System.Reflection;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.Tutorials;
using FrooxEngine.CommonAvatar;
using BaseX;
using CloudX;
using CodeX;
using CommandX;
using PostX;
using QuantityX;
using NeosModLoader;
namespace NovrX
{
    public class NovrXinitClass : NeosMod
    {



        public override string Name => "NovrXInitPlugin";
        public override string Author => "charlie-sans";
        public override string Version => "1.0.0";
        public override string Link => "your mom"; // this line is optional and can be omitted

        private static bool _first_trigger = false;
        public static void log(string msg)
        {
            Msg(msg);
        }
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("com.NovrX.initmodpluginthing");
            // do whatever LibHarmony patching you need
            harmony.PatchAll();
            for (int i = 0; i < 10; i++)
            {
                {
                    Msg("I'M LOADED DAMMIT");
                }
                //Debug("a debug log");
                //Msg("a regular log");
                //Warn("a warn log");
                //Error("an error log");
            }
            //[HarmonyPatch(typeof(NovrXinitClass))]
            //public override 
            //{

            //}


        }
    }
}