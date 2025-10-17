using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SFB; // StandaloneFileBrowser
using UnityEngine.Networking; // For UnityWebRequest

public class CanvasSampleOpenFileText : MonoBehaviour
{
    public Text infoText;

    public void OnOpenButtonClicked()
    {
        var extensions = new[] {
            new ExtensionFilter("Text Files", "txt"),
            new ExtensionFilter("All Files", "*")
        };

        StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, OnFileSelected);
    }

    private void OnFileSelected(string[] paths)
    {
        if (paths.Length == 0)
        {
            infoText.text = "No file selected.";
            return;
        }

        string path = paths[0];
        infoText.text = "Selected file:\n" + path;
        StartCoroutine(LoadFileContent(path));
    }

    private IEnumerator LoadFileContent(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get("file:///" + path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error reading file: " + uwr.error);
            }
            else
            {
                string fileContents = uwr.downloadHandler.text;
                Debug.Log($"Contents of {path}:\n{fileContents}");
            }
        }
    }
}
