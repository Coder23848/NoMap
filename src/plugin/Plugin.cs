using BepInEx;

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
            On.HUD.Map.Draw += Map_Draw;
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;
            On.Menu.FastTravelScreen.Update += FastTravelScreen_Update;
            On.RWInput.PlayerInput_int += RWInput_PlayerInput_int;
        }

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
        bool suppressMapButton = false;
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
            self.fade = 0;
        }

        // Remove the prompts in the Regions/Passage screen telling you to open the map.
        private void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            orig(self, manager, ID);
            self.mapButtonPrompt.text = "";
            self.mapButtonPrompt.label.Redraw(false, false);
        }

        // Prevents the map from showing up.
        private void Map_Draw(On.HUD.Map.orig_Draw orig, HUD.Map self, float timeStacker)
        {
            // Nothing
        }
    }
}