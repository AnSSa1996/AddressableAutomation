using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AddressablesAutomation : EditorWindow
{
    [MenuItem("Tools/Register All Assets in Addressables")]
    public static void RegisterAllAssets()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        var folderPath = "Assets/AllResources";

        // 하위 폴더 포함 모든 파일 검색
        RegisterAssetsRecursively(settings, folderPath);

        // 변경 사항 저장
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    private static void RegisterAssetsRecursively(AddressableAssetSettings settings, string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        var newLabels = new HashSet<string>();
        var existingLabels = CollectAllLabels(settings);

        foreach (var file in files)
        {
            string assetPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                var address = Path.GetFileNameWithoutExtension(assetPath);
                var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), settings.DefaultGroup);
                entry.address = address;

                var relativePath = Path.GetDirectoryName(assetPath).Replace("Assets/", "").Replace("/", "_");
                entry.labels.Add(relativePath);
                newLabels.Add(relativePath);
            }
        }

        // 기존 라벨들을 검사하고 새로운 라벨만 추가
        foreach (var label in newLabels)
        {
            if (existingLabels.Contains(label) == false)
            {
                settings.AddLabel(label);
            }
        }
    }

    private static HashSet<string> CollectAllLabels(AddressableAssetSettings settings)
    {
        var labels = new HashSet<string>();
        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                foreach (var label in entry.labels)
                {
                    labels.Add(label);
                }
            }
        }

        return labels;
    }
}