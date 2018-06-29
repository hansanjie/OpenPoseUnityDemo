using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ILCheckPlugins {
    static ILCheckPlugins ()
    {
        #if !TRILIB_USE_DEVIL
        if (!EditorPrefs.GetBool(TriLibCheckPlugins.DevilPrefKey)) {
            var result = EditorUtility.DisplayDialogComplex("TriLib - DevIL Image Library disabled", "Would you like to enable DevIL Image Library?\nThis library provides support for many different image formats (like DDS, TGA and TIFF) on all platforms TriLib uses.", "Yes", "No", "No (don't ask anymore)");
            switch (result) {
                case 0:
                    TriLibCheckPlugins.GenerateSymbolsAndUpdateMenu(TriLibCheckPlugins.DevilEnabledMenuPath, TriLibCheckPlugins.DevilSymbol, true, true);
                    break;
                case 2:
                    EditorUtility.DisplayDialog("TriLib", "You will be able to enable DevIL later using \"TriLib/Enable DevIL Image Library\" menu", "Ok");
                    TriLibCheckPlugins.GenerateSymbolsAndUpdateMenu(TriLibCheckPlugins.DevilEnabledMenuPath, TriLibCheckPlugins.DevilSymbol, true, true);
                    EditorPrefs.SetBool(TriLibCheckPlugins.DevilPrefKey, true);
                    break;
                default:
                    break;
            }
        }
        #endif
    }
}
