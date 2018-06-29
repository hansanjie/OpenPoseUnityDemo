using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR_OSX && UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif
using TriLib;
using System;
using System.IO;
[InitializeOnLoad]
public class TriLibCheckPlugins
{
    public const string DebugSymbol = "TRILIB_OUTPUT_MESSAGES";
    public const string DevilSymbol = "TRILIB_USE_DEVIL";
    public const string ZipSymbol = "TRILIB_USE_ZIP";
    public const string DebugEnabledMenuPath = "TriLib/Enable Debug";
    public const string DevilEnabledMenuPath = "TriLib/Enable DevIL Image Library (Disabled on iOS)";
    public const string ZipEnabledMenuPath = "TriLib/Enable Zip loading";
    public const string DevilPrefKey = "DevilRemindAnswer";
    #if UNITY_EDITOR_OSX && UNITY_IOS
    public const string IOSSimulatorSymbol = "USE_IOS_SIMULATOR";
    public const string IOSSimulatorEnabledMenuPath = "TriLib/iOS Simulator Enabled";
    public const string XCodeProjectPath = "Libraries/TriLib/TriLib/Plugins/iOS";
    #endif
    public static bool PluginsLoaded { get; private set; }
    static TriLibCheckPlugins()
    {
        try
        {
            AssimpInterop.ai_IsExtensionSupported(".3ds");
            PluginsLoaded = true;
        }
        catch (Exception exception)
        {
            if (exception is DllNotFoundException)
            {
                if (EditorUtility.DisplayDialog("TriLib plugins not found", "TriLib was unable to find the native plugins.\n\nIf you just imported the package, you will have to restart Unity editor.\n\nIf you click \"Ask to save changes and restart\", you will be prompted to save your changes (if there is any) then Unity editor will restart.\n\nOtherwise, you will have to save your changes and restart Unity editor manually.", "Ask to save changes and restart", "I will do it manually"))
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    var projectPath = Directory.GetParent(Application.dataPath);
                    EditorApplication.OpenProject(projectPath.FullName);
                }
            }
        }
    }

    [MenuItem(DebugEnabledMenuPath)]
    public static void DebugEnabled()
    {
        GenerateSymbolsAndUpdateMenu(DebugEnabledMenuPath, DebugSymbol, true);
    }

    [MenuItem(DebugEnabledMenuPath, true)]
    public static bool DebugEnabledValidate()
    {
        GenerateSymbolsAndUpdateMenu(DebugEnabledMenuPath, DebugSymbol, false);
        return true;
    }

    [MenuItem(DevilEnabledMenuPath)]
    public static void DevilEnabled()
    {
        GenerateSymbolsAndUpdateMenu(DevilEnabledMenuPath, DevilSymbol, true);
    }

    [MenuItem(DevilEnabledMenuPath, true)]
    public static bool DevilEnabledValidate()
    {
        GenerateSymbolsAndUpdateMenu(DevilEnabledMenuPath, DevilSymbol, false);
        return true;
    }

    [MenuItem(ZipEnabledMenuPath)]
    public static void ZipEnabled()
    {
        GenerateSymbolsAndUpdateMenu(ZipEnabledMenuPath, ZipSymbol, true);
    }

    [MenuItem(ZipEnabledMenuPath, true)]
    public static bool ZipEnabledValidate()
    {
        GenerateSymbolsAndUpdateMenu(ZipEnabledMenuPath, ZipSymbol, false);
        return true;
    }

    public static void GenerateSymbolsAndUpdateMenu(string menuPath, string checkingDefineSymbol, bool generateSymbols, bool forceDefinition = false)
    {
        var isDefined = false;
        var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var defineSymbolsArray = defineSymbols.Split(';');
        var newDefineSymbols = generateSymbols ? string.Empty : null;
        foreach (var defineSymbol in defineSymbolsArray)
        {
            var trimmedDefineSymbol = defineSymbol.Trim();
            if (trimmedDefineSymbol == checkingDefineSymbol)
            {
                isDefined = true;
                if (!generateSymbols)
                {
                    break;
                }
                continue;
            }
            if (generateSymbols)
            {
                newDefineSymbols += string.Format("{0};", trimmedDefineSymbol);
            }
        }
        if (generateSymbols)
        {
            if (!isDefined || forceDefinition)
            {
                newDefineSymbols += string.Format("{0};", checkingDefineSymbol);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefineSymbols);
        }
        Menu.SetChecked(menuPath, generateSymbols ? !isDefined : isDefined);
    }

    #if UNITY_EDITOR_OSX && UNITY_IOS
    [MenuItem(IOSSimulatorEnabledMenuPath)]
    public static void IOSSimulatorEnabled()
    {
    GenerateSymbolsAndUpdateMenu(IOSSimulatorEnabledMenuPath, IOSSimulatorSymbol, true);
    }

    [MenuItem(IOSSimulatorEnabledMenuPath, true)]
    public static bool IOSSimulatorEnabledValidate()
    {
    GenerateSymbolsAndUpdateMenu(IOSSimulatorEnabledMenuPath, IOSSimulatorSymbol, false);
    return true;
    }

    [PostProcessBuildAttribute(1000)]
    public static void OnPreProcessBuild(BuildTarget target, string pathToBuiltProject) {
    if (target == BuildTarget.iOS) {
    var path = PBXProject.GetPBXProjectPath(pathToBuiltProject);
    var pbxProject = new PBXProject();
    pbxProject.ReadFromFile(path);
    var targetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName ());
    #if USE_IOS_SIMULATOR
    RemoveFileFromProject(pbxProject, targetGuid, "libassimp.release.a");
    RemoveFileFromProject(pbxProject, targetGuid, "libirrxml.release.a");
    #else
    RemoveFileFromProject(pbxProject, targetGuid, "libassimp.debug.a");
    RemoveFileFromProject(pbxProject, targetGuid, "libirrxml.debug.a");
    pbxProject.SetBuildProperty (targetGuid, "ENABLE_BITCODE", "NO");
    #endif
    pbxProject.WriteToFile (path);
}
}

private static void RemoveFileFromProject(PBXProject pbxProject, string targetGuid, string filename) {
    var path = Path.Combine(XCodeProjectPath, filename);
    var fileGuid = pbxProject.FindFileGuidByProjectPath (path);
    if (fileGuid != null) {
        pbxProject.RemoveFileFromBuild (targetGuid, fileGuid);
        pbxProject.RemoveFile (fileGuid);
    }
}
    #endif
}
