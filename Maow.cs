using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Elements.Core;
namespace NovrXPatches
{
    
    public class Maow
    {
        public static void log(string msg) { ResoniteMod.Msg(msg); }

        public static void Error(string  msg) { ResoniteMod.Error(msg); }

        public static void Init(Harmony harmony)
        {
            ResoniteMod.Msg("hello! welcome to NovrX!");
            ResoniteMod.Msg("i'm mavrik, and i'm here to help log infomation to the console. ");
            ResoniteMod.Msg("please look out for me because i might actualy give infomation you might Really need.");
            ResoniteMod.Msg("you can find me around the logs with [mavrik], and sometimes with the games logging aswell.");
            ResoniteMod.Msg("hope you have fun!");
            ResoniteMod.Msg("[mavrik] Patching!!!");
            harmony.PatchAll();
            ResoniteMod.Msg("Patched! have fun!");
        }
    }
}
