using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using QuantityX;
using SharpDX;
using HarmonyLib;
using Leap;
using ResoniteModLoader;
using NovrXCore;
namespace NovrXPatches
{
    // welcome to NovrX's main class! this is where the magic happens!

    // stuff gets initialized here, and the mod gets loaded into the game.
    public class Init : ResoniteMod
    {
        public override string Name => "Novrx";
        public override string Author => "Valhala/OpenStudio";
        public override string Version => "0.1.0";
        public override string Link => "https://github.com/charlie-sans/novrx";
        public override void OnEngineInit()
        {
            Maow.log("maow");
            Harmony harmony = new Harmony("NovrX.maow.com.au");
            Maow.Init(harmony);
            Start_Plugin_patch.init(); 
           
        }
    }
}
