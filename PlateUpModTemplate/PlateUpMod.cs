using HarmonyLib;
using UnityEngine;
using KitchenMods;
using PlateUpEmotesApi;

namespace PlateUpModTemplate;

public class PlateUpMod : IModInitializer
{
    public const string AUTHOR = "AUTHOR";
    public const string MOD_NAME = "MOD_NAME";
    public const string MOD_ID = $"com.{AUTHOR}.{MOD_NAME}";
    
    public void PostActivate(Mod mod)
    {
        Harmony.DEBUG = true;
        Harmony harmony = new Harmony(MOD_ID);
        
        harmony.PatchAll();
    }

    public void PreInject()
    {
        
    }

    public void PostInject()
    {
        Assets.PopulateAssets();
        PlateUpEmotesManager.AddCustomAnimation(Assets.Load<AnimationClip>("@ExampleEmotePlugin_caramelldansen:assets/animationreplacements/caramelldansen.anim"), false, "Play_Caramell", "Stop_Caramell", dimWhenClose: true, syncAnim: true, syncAudio: true);
    }
}