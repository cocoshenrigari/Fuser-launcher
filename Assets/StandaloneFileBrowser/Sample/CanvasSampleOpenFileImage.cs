using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SFB; // StandaloneFileBrowser
using UnityEngine.Networking; // For UnityWebRequestTexture

public class CanvasSampleOpenFileImage : MonoBehaviour
{
    public RawImage displayImage;
    public Text infoText;

    public void OnOpenButtonClicked()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
            new ExtensionFilter("All Files", "*")
        };

        // Open file panel asynchronously
        StandaloneFileBrowser.OpenFilePanelAsync("Open Image(s)", "", extensions, true, OnFilesSelected);
    }

    private void OnFilesSelected(string[] paths)
    {
        if (paths.Length == 0)
        {
            infoText.text = "No images selected.";
            return;
        }

        infoText.text = "Selected images:\n";
        foreach (var path in paths)
        {
            infoText.text += path + "\n";
            StartCoroutine(LoadImage(path));
        }
    }

    private IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading image: " + uwr.error);
            }
            else
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (displayImage != null)
                {
                    displayImage.texture = tex;
                    displayImage.SetNativeSize(); // Optional: resize to original texture size
                }
            }
        }
    }
}
