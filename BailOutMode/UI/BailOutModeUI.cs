using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomUI.GameplaySettings;
using CustomUI.Settings;

namespace BailOutMode.UI
{
    class BailOutModeUI
    {
        public static void CreateUI()
        {
            CreateSettingsUI();
        }



        public static void CreateSettingsUI()
        {
            //This will create a menu tab in the settings menu for your plugin
            var pluginSettingsSubmenu = SettingsUI.CreateSubMenu("BailOut Mode");

            var enableToggle = pluginSettingsSubmenu.AddBool("Enable", "Keep playing songs if you fail, won't post scores if you do");
            enableToggle.GetValue += delegate { return Plugin.IsEnabled; };
            enableToggle.SetValue += delegate (bool value) {
                Plugin.IsEnabled = value;
            };
            var failTextToggle = pluginSettingsSubmenu.AddBool("Show Fail Text", "Show the fail text effect when you would've failed");
            failTextToggle.GetValue += delegate { return Plugin.ShowFailText; };
            failTextToggle.SetValue += delegate (bool value) {
                Plugin.ShowFailText = value;
            };
            
            int durationMax = (Plugin.FailTextDuration <= 10) ? 10 : Plugin.FailTextDuration; // If duration setting is more than 10, increase the max to match
            durationMax = (durationMax >= 0) ? durationMax : 0; // If the duration setting is negative, set to 0
            var failTextDurationOption = pluginSettingsSubmenu.AddInt("Fail Text Duration","How long the fail text effect lingers, 0 to show it forever", 0, durationMax, 1);
            failTextDurationOption.GetValue += delegate { return Plugin.FailTextDuration; };
            failTextDurationOption.SetValue += delegate (int value) {
                Plugin.FailTextDuration = value;
            };
            

        }

        public static void CreateGameplayOptionsUI()
        {

            //Example submenu option
            var pluginSubmenu = GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.ModifiersLeft, "Plugin Name", "MainMenu", "pluginMenu1", "You can keep all your plugin's gameplay options nested within this one button");

            //Example Toggle Option within a submenu
            var exampleOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.ModifiersLeft, "Example Toggle", "pluginMenu1", "Put a toggle for a setting you want easily accessible in game here.");
            exampleOption.GetValue = /* Fetch the initial value for the option here*/ false;
            exampleOption.OnToggle += (value) => { /*  You can execute whatever you want to occur when the value is toggled here, usually that would include updating wherever the value is pulled from   */};
            exampleOption.AddConflict("Conflicting Option Name"); //You can add conflicts with other gameplay options settings here, preventing both from being active at the same time, including that of other mods



        }





    }
}
