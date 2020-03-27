# BailOutMode
With this plugin enabled, your energy will reset when you fail and allow you to continue playing the song. You won't submit a score if you are bailed out.

## Installation
* Download the zip and extract it to your Beat Saber folder.

## Setup
The following settings can be configured in-game:
* **Enabled**: Whether or not BailOutMode is enabled.
* **Show Fail Effect**: Shows the 'Level Failed' effect when you get bailed out.
* **Repeat Fail Effect**: If enabled, shows the 'Level Failed' effect every time you get bailed out. If disabled, only shows on the first bail out for a song.
* **Fail Effect Duration**: How long the 'Level Failed' effect is shown.
* **Energy Reset Amount**: How much energy you are reset to after a bail out (30 to 100).
* **Counter Text Position**: The position of the bail out counter text. Must be in the form **#,#,#** (x, y, z).
* **Counter Text Size**: Size of the bail out counter text (default is 15).

## Dependencies
* <a href="https://github.com/Kylemc1413/Beat-Saber-Utils">BS_Utils</a> by Kyle1413
* <a href="https://github.com/monkeymanboy/BeatSaberMarkupLanguage">BSML</a> by MonkeyManBoy
* <a href="https://github.com/pardeike/Harmony">Harmony v2</a> by Pardeike

# Build
* To resolve reference paths you can do one of the following:
  * Drag your Beat Saber game folder onto CreateJunctions.bat to create directory junctions inside the References folder.
  * Create a BailOutMode.csproj.user file that sets the *BeatSaberDir* project property to the full path of your Beat Saber game folder.
* Project should be ready to build.
