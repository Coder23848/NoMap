using System;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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

            IL.HUD.ExpeditionHUD.Update += ExpeditionHUD_Update;
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

        // Remove the "open map" background effect on the sleep/death screen.
        private bool SleepAndDeathScreen_get_RevealMap(Func<Menu.SleepAndDeathScreen, bool> orig, Menu.SleepAndDeathScreen self)
        {
            return false;
        }

        // Change the criteria for displaying the expedition challenge list to not be dependent on the map being visible.
        private void ExpeditionHUD_Update(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HUD.ExpeditionHUD>(nameof(HUD.ExpeditionHUD.pendingUpdates))))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                static bool ExpeditionHUD_UpdateDelegate(bool orig, HUD.ExpeditionHUD self) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return orig || self.hud.owner.RevealMap;
                };
                cursor.EmitDelegate(ExpeditionHUD_UpdateDelegate);
            }
            else
            {
                Logger.LogError("Failed to hook ExpeditionHUD.Update: no match found.");
            }
        }
    }
}