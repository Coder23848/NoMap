using System;
using BepInEx;
using MonoMod.RuntimeDetour;

namespace NoMap
{
    [BepInPlugin("com.coder23848.nomap", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable IDE0051 // Visual Studio is whiny
        private void OnEnable()
#pragma warning restore IDE0051
        {
            On.HUD.Map.Update += Map_Update;
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;
            On.Menu.FastTravelScreen.Update += FastTravelScreen_Update;
            On.RWInput.PlayerInput_int += RWInput_PlayerInput_int;
            _ = new Hook(typeof(Menu.SleepAndDeathScreen).GetMethod("get_RevealMap"), SleepAndDeathScreen_get_RevealMap);
        }

        static bool suppressMapButton = false;
        private Player.InputPackage RWInput_PlayerInput_int(On.RWInput.orig_PlayerInput_int orig, int playerNumber)
        {
            Player.InputPackage result = orig(playerNumber);
            if (suppressMapButton)
            {
                result.mp = false;
            }
            return result;
        }

        // Suppress the map button entirely while on the region/Passage screen.
        private void FastTravelScreen_Update(On.Menu.FastTravelScreen.orig_Update orig, Menu.FastTravelScreen self)
        {
            suppressMapButton = true;
            orig(self);
            suppressMapButton = false;
        }

        // Forcing map fade to 0 removes the "open map" background effects in select/shelter/region/Passage screens.
        private void Map_Update(On.HUD.Map.orig_Update orig, HUD.Map self)
        {
            orig(self);
            self.fadeCounter = 0;
            self.fade = 0;
            self.lastFade = 0;
            self.visible = false;
        }

        // Remove the prompts in the Regions/Passage screen telling you to open the map.
        private void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            orig(self, manager, ID);
            self.mapButtonPrompt.text = "";
            self.mapButtonPrompt.label.Redraw(false, false);
        }

        // Removes the "open map" background effect on the sleep/death screen.
        private bool SleepAndDeathScreen_get_RevealMap(Func<Menu.SleepAndDeathScreen, bool> orig, Menu.SleepAndDeathScreen self)
        {
            return false;
        }
    }
}