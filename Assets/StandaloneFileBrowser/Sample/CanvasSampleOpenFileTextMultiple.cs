using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SFB; // StandaloneFileBrowser
using UnityEngine.Networking; // Needed for UnityWebRequest

public class CanvasSampleOpenFileTextMultiple : MonoBehaviour
{
    public Text infoText;

    public void OnOpenButtonClicked()
    {
        var extensions = new[] {
            new ExtensionFilter("Text Files", "txt"),
            new ExtensionFilter("All Files", "*" )
        };

        StandaloneFileBrowser.OpenFilePanelAsync("Open File(s)", "", extensions, true, OnFilesSelected);
    }

    // Callback when files are selected
    private void OnFilesSelected(string[] paths)
    {
        if (paths.Length == 0)
        {
            infoText.text = "No files selected.";
            return;
        }

        infoText.text = "Selected files:\n";

        foreach (var path in paths)
        {
            infoText.text += path + "\n";
            // Start coroutine to read file contents
            StartCoroutine(LoadFileContent(path));
        }
    }

    private IEnumerator LoadFileContent(string path)
    {
        // UnityWebRequest works for local files as well
        using (UnityWebRequest www = UnityWebRequest.Get("file:///" + path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error reading file: " + www.error);
            }
            else
            {
                string fileContents = www.downloadHandler.text;
                Debug.Log($"Contents of {path}:\n{fileContents}");
            }
        }
    }
}
