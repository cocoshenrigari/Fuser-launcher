using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using SFB; // from StandaloneFileBrowser

public class FolderManager : MonoBehaviour
{
    [SerializeField] private Button setFolderButton;
    [SerializeField] private Button openFolderButton;

    private const string FolderPrefKey = "SavedFolderPath";
    private string folderPath;

    void Start()
    {
        folderPath = PlayerPrefs.GetString(FolderPrefKey, "");

        if (setFolderButton != null)
            setFolderButton.onClick.AddListener(SetFolder);

        if (openFolderButton != null)
            openFolderButton.onClick.AddListener(OpenFolder);
    }

    void SetFolder()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            folderPath = paths[0];
            PlayerPrefs.SetString(FolderPrefKey, folderPath);
            PlayerPrefs.Save();
            UnityEngine.Debug.Log($"Folder saved: {folderPath}");
        }
    }

    void OpenFolder()
    {
        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
        {
            Process.Start("explorer.exe", folderPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("No valid folder set yet. Please set it first.");
        }
    }
}
