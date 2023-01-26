# Neos Locale Updater

A [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader) mod for [Neos VR](https://neos.com/) that updates the game's locale files without need for full game update by fetching them straight from user set Git repository (default is official [NeosLocale repository](https://github.com/Neos-Metaverse/NeosLocale)).

## Installation
1. Install [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader).
1. Place [NeosLocaleUpdater.dll and its dependencies](https://github.com/rampa3/NeosLocaleUpdater/releases/latest/download/NeosLocaleUpdater.zip) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## Used libraries
- [libgit2](https://github.com/libgit2/libgit2) (GNU GPL 2 with linking exception)
- [libgit2sharp](https://github.com/libgit2/libgit2sharp) (MIT)