﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Valheim.exe")]
    public partial class Plugin : BaseUnityPlugin {
        private readonly Harmony harmony = new("zAlweNy26.AlweStats");
        protected static string statsFilePath;
        protected static string[] statsFileLines;
        protected static bool isEditing = false;
        public static ConfigEntry<bool> enableGameStats, enableWorldStats, enableWorldStatsInSelection, enableWorldClock, twelveHourFormat;
        public static ConfigEntry<int> gameStatsSize, worldStatsSize, worldClockSize;
        public static ConfigEntry<KeyCode> toggleEditMode;
        public static ConfigEntry<string> 
            gameStatsColor, gameStatsAlign, gameStatsPosition, gameStatsMargin, 
            worldStatsColor, worldStatsAlign, worldStatsPosition, worldStatsMargin,
            worldClockColor, worldClockAlign, worldClockPosition, worldClockMargin;

        public void Awake() {
            toggleEditMode = Config.Bind("General", "EditModeKey", KeyCode.F9, "Key to toggle hud editing mode");

            enableGameStats = Config.Bind("GameStats", "Enable", true, "Whether or not to show fps and ping counters in game");
            enableWorldStats = Config.Bind("WorldStats", "Enable", true, "Whether or not to show days passed and time played counters in game");
            enableWorldStatsInSelection = Config.Bind("WorldStats", "DaysInWorldList", true, "Whether or not to show days passed counter in world selection");
            enableWorldClock = Config.Bind("WorldClock", "Enable", true, "Whether or not to show a clock in game");

            twelveHourFormat = Config.Bind("WorldClock", "TwelveHourFormat", false, "Whether or not to show the clock in the 12h format with AM and PM");

            gameStatsColor = Config.Bind("GameStats", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            gameStatsSize = Config.Bind("GameStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            gameStatsAlign = Config.Bind("GameStats", "Align", "LowerLeft",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            gameStatsPosition = Config.Bind("GameStats", "Position", "0, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            gameStatsMargin = Config.Bind("GameStats", "Margin", "10, 10",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldStatsColor = Config.Bind("WorldStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldStatsSize = Config.Bind("WorldStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldStatsAlign = Config.Bind("WorldStats", "Align", "LowerRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            worldStatsPosition = Config.Bind("WorldStats", "Position", "1, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldStatsMargin = Config.Bind("WorldStats", "Margin", "-10, 10",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldClockColor = Config.Bind("WorldClock", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldClockSize = Config.Bind("WorldClock", "Size", 24, 
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldClockAlign = Config.Bind("WorldClock", "Align", "UpperCenter", 
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            worldClockPosition = Config.Bind("WorldClock", "Position", "0.5, 1", 
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldClockMargin = Config.Bind("WorldClock", "Margin", "0, 0", 
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            Logger.LogInfo($"AlweStats loaded successfully !");

            statsFilePath = Path.Combine(Paths.PluginPath, "Alwe.stats");
        }

        public void Start() { harmony.PatchAll(); }
        
        public void OnDestroy() { harmony.UnpatchSelf(); }

        [HarmonyPatch(typeof(ZNetScene), "Update")]
        static class PatchGameStats {
            private static GameObject statsObj = null;
            private static float frameTimer;
            private static int frameSamples;

            [HarmonyPostfix]
            static void Postfix() {
                if (!enableGameStats.Value) return;
                Hud hud = Hud.instance;
                MessageHud msgHud = MessageHud.instance;
                string fps = GetFPS();
                ZNet.instance.GetNetStats(out var localQuality, out var remoteQuality, out var ping, out var outByteSec, out var inByteSec);
                if (fps != "0") {
                    //Debug.Log($"FPS : {fps} | Ping : {ping:0} ms");
                    Text statsText;
                    if (statsObj == null) {
                        statsObj = new GameObject("GameStats");
                        statsObj.transform.SetParent(hud.m_statusEffectListRoot.transform.parent);
                        statsObj.AddComponent<RectTransform>();
                        statsText = statsObj.AddComponent<Text>();
                        string[] colors = Regex.Replace(gameStatsColor.Value, @"\s+", "").Split(',');
                        statsText.color = new(
                            Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
                        );
                        statsText.font = msgHud.m_messageCenterText.font;
                        statsText.fontSize = gameStatsSize.Value;
                        statsText.enabled = true;
                        Enum.TryParse(gameStatsAlign.Value, out TextAnchor textAlignment);
                        statsText.alignment = textAlignment;
                        statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    } else statsText = statsObj.GetComponent<Text>();
                    if (ping != 0) statsText.text = $"Ping : {ping:0} ms\nFPS : {fps}";
                    else statsText.text = $"FPS : {fps}";

                    RectTransform statsRect = statsObj.GetComponent<RectTransform>();

                    string[] positions = Regex.Replace(gameStatsPosition.Value, @"\s+", "").Split(',');
                    statsRect.anchorMax = statsRect.anchorMin = new(float.Parse(
                        positions[0], CultureInfo.InvariantCulture),
                        float.Parse(positions[1], CultureInfo.InvariantCulture)
                    );
                    Vector2 shiftDirection = new(0.5f - statsRect.anchorMax.x, 0.5f - statsRect.anchorMax.y);

                    string[] margins = Regex.Replace(gameStatsMargin.Value, @"\s+", "").Split(',');
                    statsRect.anchoredPosition = shiftDirection * statsRect.rect.size + new Vector2(
                        float.Parse(margins[0], CultureInfo.InvariantCulture),
                        float.Parse(margins[1], CultureInfo.InvariantCulture)
                    );

                    statsObj.SetActive(true);
                }
            }

            private static string GetFPS() {
                string fpsCalculated = "0";
                frameTimer += Time.deltaTime;
                frameSamples++;
                if (frameTimer > 1f) {
                    fpsCalculated = Math.Round(1f / (frameTimer / frameSamples)).ToString();
                    frameSamples = 0;
                    frameTimer = 0f;
                }
                return fpsCalculated;
            }
        }

        [HarmonyPatch(typeof(EnvMan), "FixedUpdate")]
        static class PatchWorldStatsAndClock {
            private static GameObject statsObj = null, clockObj = null;
            [HarmonyPostfix]
            static void Postfix(ref EnvMan __instance) {
                if (!enableWorldStats.Value && !enableWorldClock.Value) return;
                Hud hud = Hud.instance;
                MessageHud msgHud = MessageHud.instance;
                if (enableWorldStats.Value) {
                    double timePlayed = ZNet.instance.GetTimeSeconds();
                    int daysPlayed = (int)Math.Floor(timePlayed / __instance.m_dayLengthSec);
                    double minutesPlayed = timePlayed / 60;
                    double hoursPlayed = minutesPlayed / 60;
                    //Debug.Log($"Days : {days} | Hours played : {hoursPlayed:0.00}");
                    Text statsText;
                    if (statsObj == null) {
                        statsObj = new GameObject("WorldStats");
                        statsObj.transform.SetParent(hud.m_statusEffectListRoot.transform.parent);
                        statsObj.AddComponent<RectTransform>();
                        statsText = statsObj.AddComponent<Text>();
                        string[] colors = Regex.Replace(worldStatsColor.Value, @"\s+", "").Split(',');
                        statsText.color = new Color(
                            Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                            Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
                        );
                        statsText.font = msgHud.m_messageCenterText.font;
                        statsText.fontSize = worldStatsSize.Value;
                        statsText.enabled = true;
                        Enum.TryParse(worldStatsAlign.Value, out TextAnchor textAlignment);
                        statsText.alignment = textAlignment;
                        statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    } else statsText = statsObj.GetComponent<Text>();

                    if (hoursPlayed < 1) statsText.text = $"Days passed : {daysPlayed}\nTime played : {minutesPlayed:0.00} m";
                    else statsText.text = $"Days passed : {daysPlayed}\nTime played : {hoursPlayed:0.00} h";

                    RectTransform statsRect = statsObj.GetComponent<RectTransform>();

                    string[] positions = Regex.Replace(worldStatsPosition.Value, @"\s+", "").Split(',');
                    statsRect.anchorMax = statsRect.anchorMin = new(
                        float.Parse(positions[0], CultureInfo.InvariantCulture), 
                        float.Parse(positions[1], CultureInfo.InvariantCulture)
                    );
                    string[] margins = Regex.Replace(worldStatsMargin.Value, @"\s+", "").Split(',');
                    statsRect.anchoredPosition = new Vector2(0.5f - statsRect.anchorMax.x, 0.5f - statsRect.anchorMax.y) * statsRect.rect.size + new Vector2(
                        float.Parse(margins[0], CultureInfo.InvariantCulture), 
                        float.Parse(margins[1], CultureInfo.InvariantCulture)
                    );

                    statsObj.SetActive(true);
                }
                if (enableWorldClock.Value) {
                    Text clockText;
                    if (clockObj == null) {
                        clockObj = new GameObject("WorldClock");
                        clockObj.transform.SetParent(hud.m_statusEffectListRoot.transform.parent);
                        clockObj.AddComponent<RectTransform>();
                        clockText = clockObj.AddComponent<Text>();
                        string[] colors = Regex.Replace(worldClockColor.Value, "\\s+", "").Split(',');
                        clockText.color = new(
                            Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f), 
                            Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f), 
                            Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f), 
                            Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
                        );
                        clockText.font = msgHud.m_messageCenterText.font;
                        clockText.fontSize = worldClockSize.Value;
                        clockText.enabled = true;
                        Enum.TryParse(worldClockAlign.Value, out TextAnchor textAlignment);
                        clockText.alignment = textAlignment;
                        clockText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    } else clockText = clockObj.GetComponent<Text>();
                    string format12h = "";
                    float minuteFraction = Mathf.Lerp(0f, 24f, __instance.GetDayFraction());
                    float floor24h = Mathf.Floor(minuteFraction);
                    int hours = Mathf.FloorToInt(floor24h);
                    int minutes = Mathf.FloorToInt(Mathf.Lerp(0f, 60f, minuteFraction - floor24h));
                    if (twelveHourFormat.Value) {
                        format12h = hours < 12 ? "AM" : "PM";
                        if (hours > 12) hours -= 12;
                    }
                    clockText.text = $"{(hours < 10 ? "0" : "")}{hours}:{(minutes < 10 ? "0" : "")}{minutes} {format12h}";
                    RectTransform clockRect = clockObj.GetComponent<RectTransform>();
                    string[] positions = Regex.Replace(worldClockPosition.Value, "\\s+", "").Split(',');
                    clockRect.anchorMax = clockRect.anchorMin = new(
                        float.Parse(positions[0], CultureInfo.InvariantCulture), 
                        float.Parse(positions[1], CultureInfo.InvariantCulture)
                    );
                    string[] margins = Regex.Replace(worldClockMargin.Value, "\\s+", "").Split(',');
                    clockRect.anchoredPosition = new Vector2(0.5f - clockRect.anchorMax.x, 0.5f - clockRect.anchorMax.y) * clockRect.rect.size + new Vector2(
                        float.Parse(margins[0], CultureInfo.InvariantCulture), 
                        float.Parse(margins[1], CultureInfo.InvariantCulture)
                    );
                    clockObj.SetActive(true);
                }
            }
        }

        static void GetWorldStats(FejdStartup instance) {
            List<string> worlds = new();
            if (File.Exists(statsFilePath)) statsFileLines = File.ReadAllLines(statsFilePath);
            foreach (Transform t in instance.m_worldListRoot) {
                Transform days = Instantiate(t.Find("name"));
                days.name = "days";
                days.SetParent(t.Find("name").parent);
                days.GetComponent<RectTransform>().localPosition = new(325f, -14f, 0f);
                string worldName = t.Find("name").GetComponent<Text>().text;
                string dBPath = $"{Utils.GetSaveDataPath()}/worlds/{worldName}.db";
                if (File.Exists(dBPath)) {
                    using FileStream fs = File.OpenRead(dBPath);
                    using BinaryReader br = new(fs);
                    int worldVersion = br.ReadInt32();
                    if (worldVersion >= 4) {
                        double timePlayed = br.ReadDouble();
                        long daySeconds = 1200L;
                        if (statsFileLines != null) {
                            string str = statsFileLines.FirstOrDefault(s => s.Contains(worldName));
                            if (str != null) daySeconds = long.Parse(statsFileLines[Array.IndexOf(statsFileLines, str)].Split(':')[1]);
                        }
                        int daysPlayed = (int)Math.Floor(timePlayed / daySeconds);
                        //Debug.Log($"{worldName} : {timePlayed} seconds | {daysPlayed} days");
                        days.GetComponent<Text>().text = $"{daysPlayed} days";
                        if (!worlds.Contains(worldName)) worlds.Add(worldName);
                    }
                } else days.GetComponent<Text>().text = "0 days";
                days.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "ShowStartGame")]
        static class PatchWorldList {
            [HarmonyPostfix]
            static void Postfix(ref FejdStartup __instance) {
                if (!enableWorldStatsInSelection.Value) return;
                GetWorldStats(__instance);
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "OnSelectWorld")]
        static class PatchWorldSelection {
            [HarmonyPostfix]
            static void Postfix(ref FejdStartup __instance) {
                if (!enableWorldStatsInSelection.Value) return;
                GetWorldStats(__instance);
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        static class PatchWorldExit {
            [HarmonyPrefix]
            static void Prefix(ref ZNet __instance) {
                if (!enableWorldStatsInSelection.Value) return;
                string worldName = __instance.GetWorldName();
                if (File.Exists(statsFilePath)) {
                    statsFileLines = File.ReadAllLines(statsFilePath);
                    string str = statsFileLines.FirstOrDefault(s => s.Contains(worldName));
                    if (str != null) {
                        statsFileLines[Array.IndexOf(statsFileLines, str)] = $"{worldName}:{EnvMan.instance.m_dayLengthSec}";
                        File.WriteAllLines(statsFilePath, statsFileLines);
                    } else File.AppendAllText(statsFilePath, $"{Environment.NewLine}{worldName}:{EnvMan.instance.m_dayLengthSec}");
                } else File.AppendAllText(statsFilePath, $"{worldName}:{EnvMan.instance.m_dayLengthSec}");
            }
        }
    }
}
