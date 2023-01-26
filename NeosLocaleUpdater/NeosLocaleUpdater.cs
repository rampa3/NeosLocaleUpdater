using FrooxEngine;
using LibGit2Sharp;
using NeosModLoader;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace NeosLocaleUpdater
{
    public class NeosLocaleUpdater : NeosMod
    {
        public override string Name => "NeosLocaleUpdater";
        public override string Author => "rampa3";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/rampa3/NeosLocaleUpdater/";
        private static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("ModEnabled", "Enabled (Requires restart)", () => true);

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<string> WORK_DIR = new ModConfigurationKey<string>("WorkDir", "Working directory for cloning the locale repository", () => "LocaleUpdaterClone");

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<string> LOCALE_REPO = new ModConfigurationKey<string>("LocaleRepo", "Locale repository to update from", () => "https://github.com/Neos-Metaverse/NeosLocale.git");

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> UPDATE_NOW = new ModConfigurationKey<bool>("UpdateNow", "Update now (Toggling on will start an update; will self-disable)", () => false);

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> AUTO_UPDATE = new ModConfigurationKey<bool>("AutoUpdate", "Auto update on Neos startup", () => true);
        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);
            if (Config.GetValue(MOD_ENABLED))
            {
                ModConfiguration.OnAnyConfigurationChanged += OnConfigurationChanged;
                if (Config.GetValue(AUTO_UPDATE))
                {
                    Engine.Current.OnReady += () => {
                        update();
                    };
                }
            }
        }

        private void OnConfigurationChanged(ConfigurationChangedEvent @event)
        {
            if ((@event.Config.Owner.Name == Name) && (@event.Key.Name == UPDATE_NOW.Name) && Config.GetValue(UPDATE_NOW)) {
                Config.Set(UPDATE_NOW, false);
                Config.Save();
                update();
            }
        }

        private static async void update()
        {
            string localeSettingPath = "Interface.Locale";
            string defaultFallback = "en";
            string currentLocale = Settings.ReadValue<string>(localeSettingPath, defaultFallback);

            Msg("Locale update started.");
            await updateClone();
            await updateGameData();
            Msg("Refreshing localization by switching.");
            if (currentLocale == defaultFallback)
            {
                Settings.WriteValue<string>(localeSettingPath, "en-gb");
                Thread.Sleep(2000);
                Settings.WriteValue<string>(localeSettingPath, currentLocale);
            }
            else
            {
                Settings.WriteValue<string>(localeSettingPath, "en");
                Thread.Sleep(2000);
                Settings.WriteValue<string>(localeSettingPath, currentLocale);
            }
            Msg("Locale update complete!");
        }

        private static async Task updateGameData()
        {
            string workDir = getWorkDir();
            string neosLocaleDir = "Locale";

            DirectoryInfo workDirInfo = new DirectoryInfo(workDir);

            foreach (FileInfo file in workDirInfo.GetFiles())
            {
                file.CopyTo(Path.Combine(neosLocaleDir, file.Name), true);
            }

            Msg("Game data updated.");
        }

        private static async Task updateClone()
        {
            string workDir = getWorkDir();


            DirectoryInfo workDirInfo = new DirectoryInfo(workDir);

            foreach (FileInfo file in workDirInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in workDirInfo.GetDirectories())
            {
                DelDir(Path.Combine(workDir, dir.Name));
            }

            Repository.Clone(Config.GetValue(LOCALE_REPO), @Path.GetFullPath(workDir));
            Msg("Clone updated.");
        }

        private static string getWorkDir()
        {
            string dirString = Config.GetValue(WORK_DIR);

            if (dirString.Length == 0)
            {
                Msg("No working directory was specified, using default.");
                if (!Directory.Exists("LocaleUpdaterClone"))
                {
                    Directory.CreateDirectory("LocaleUpdaterClone");
                    Msg("Default working directory did not existed and was created.");
                }
                return "LocaleUpdaterClone";
            }
            else
            {
                if (Directory.Exists(dirString))
                {
                    Msg("Working directory \"" + dirString + "\" found successfully.");
                    return dirString;
                }
                else
                {
                    Msg("Directory \"" + dirString + "\" didn't existed and was created.");
                    Directory.CreateDirectory(dirString);
                    return dirString;
                }
            }
        }

        //Directory copying code from example: https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void CopyDir(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDir(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void DelDir(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DelDir(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}