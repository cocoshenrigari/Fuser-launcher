using UnityEngine;

public class OpenWebsite : MonoBehaviour
{
    public string url = "https://www.example.com";

    public void OpenSite() // must be public and return void
    {
        Application.OpenURL(url);
    }
}
