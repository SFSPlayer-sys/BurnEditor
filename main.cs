using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SFS.Builds;
using SFS.UI;

namespace BurnEditor
{
    public class Main : Mod
    {
        public override string ModNameID => "BurnEditor";
        public override string DisplayName => "Burn Editor";
        public override string Author => "SFSGamer";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v1.0.0";
        public override string Description => "Adds a burn planning interface to the build menu.";

        static Harmony patcher;

        public override void Load()
        {
            patcher = new Harmony("BurnEditor.Main");
            patcher.PatchAll();
        }

        public override void Early_Load()
        {

        }
    }
} 