using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // For VideoPlayer
using UnityEngine.EventSystems; // For EventTrigger
using SFB; // StandaloneFileBrowser namespace
using System.Diagnostics; // For Process.Start
using System.IO; // For Directory.Exists, File.Exists, File.Delete
using System.Collections; // For IEnumerator and WaitForSeconds
using System; // For Exception
using TMPro; // For TextMeshProUGUI

public class LauncherManager : MonoBehaviour
{
    private const string GamePathKey = "GameInstallPath";
    [SerializeField] private Button openCustomSongsButton; // Assign in Inspector for opening custom_songs folder
    [SerializeField] private Button reloadAndLaunchButton; // Assign in Inspector for reload + launch button
    [SerializeField] private Button resetGamePathButton; // Assign in Inspector for resetting game path
    [SerializeField] private Button quitButton; // Assign in Inspector for quitting application
    [SerializeField] private VideoPlayer videoPlayer; // Assign in Inspector for video background
    [SerializeField] private AudioSource audioSource; // Assign in Inspector for sound effects
    [SerializeField] private AudioClip hoverSound; // Assign in Inspector (e.g., hover.wav)
    [SerializeField] private AudioClip clickSound; // Assign in Inspector (e.g., click.wav)
    [SerializeField] private TextMeshProUGUI gamePathText; // Assign in Inspector for game path display
    [SerializeField] private TextMeshProUGUI statusText; // Assign in Inspector for loading status

    void Start()
    {
        // Initialize video
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            UnityEngine.Debug.Log("Video background started.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("VideoPlayer not assigned in Inspector!");
        }

        // Validate AudioSource and clips
        if (audioSource == null)
        {
            UnityEngine.Debug.LogError("AudioSource not assigned in Inspector!");
        }
        else
        {
            audioSource.priority = 0; // Highest priority for UI sounds
            UnityEngine.Debug.Log("AudioSource assigned, priority set to 0.");
        }
        if (hoverSound == null) UnityEngine.Debug.LogWarning("Hover sound not assigned in Inspector!");
        else UnityEngine.Debug.Log($"Hover sound assigned: {hoverSound.name}");
        if (clickSound == null) UnityEngine.Debug.LogWarning("Click sound not assigned in Inspector!");
        else UnityEngine.Debug.Log($"Click sound assigned: {clickSound.name}");

        // Preload audio clips
        if (audioSource != null && hoverSound != null && clickSound != null)
        {
            audioSource.PlayOneShot(hoverSound, 0f); // Preload
            audioSource.PlayOneShot(clickSound, 0f); // Preload
            UnityEngine.Debug.Log("Audio clips preloaded.");
        }

        // Validate TextMeshProUGUI
        if (gamePathText == null)
        {
            UnityEngine.Debug.LogError("GamePathText (TextMeshProUGUI) not assigned in Inspector!");
        }
        if (statusText == null)
        {
            UnityEngine.Debug.LogWarning("StatusText (TextMeshProUGUI) not assigned in Inspector!");
        }

        // Update game path text
        UpdateGamePathText();

        // Validate ResetGamePathButton
        if (resetGamePathButton != null)
        {
            resetGamePathButton.onClick.AddListener(() => {
                PlayClickSound();
                ResetGamePath();
            });
            UnityEngine.Debug.Log("ResetGamePathButton listener assigned.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("ResetGamePathButton not assigned in Inspector!");
        }

        // Validate QuitButton
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                PlayClickSound();
                QuitApplication();
            });
            UnityEngine.Debug.Log("QuitButton listener assigned.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("QuitButton not assigned in Inspector!");
        }

        // First-launch path selection
        if (!PlayerPrefs.HasKey(GamePathKey))
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Game Installation Folder", "", false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string selectedPath = paths[0];
                PlayerPrefs.SetString(GamePathKey, selectedPath);
                PlayerPrefs.Save();
                UnityEngine.Debug.Log("Game install path set to: " + selectedPath);
                UpdateGamePathText();
            }
            else
            {
                UnityEngine.Debug.LogWarning("No folder selected. Please select a valid game folder.");
            }
        }
        else
        {
            string storedPath = PlayerPrefs.GetString(GamePathKey);
            UnityEngine.Debug.Log("Game install path loaded: " + storedPath);
        }

        // Hook up buttons
        if (openCustomSongsButton != null)
        {
            openCustomSongsButton.onClick.AddListener(() => {
                PlayClickSound();
                OpenCustomSongsFolder();
            });
            UnityEngine.Debug.Log("OpenCustomSongsButton listener assigned.");
        }
        else
        {
            UnityEngine.Debug.LogError("Open Custom Songs Button not assigned in Inspector!");
        }

        if (reloadAndLaunchButton != null)
        {
            reloadAndLaunchButton.onClick.AddListener(() => {
                PlayClickSound();
                StartCoroutine(ReloadAndLaunchGame());
            });
            UnityEngine.Debug.Log("ReloadAndLaunchButton listener assigned.");
        }
        else
        {
            UnityEngine.Debug.LogError("Reload and Launch Button not assigned in Inspector!");
        }
    }

    // Reset game path in PlayerPrefs and update text
    private void ResetGamePath()
    {
        PlayerPrefs.DeleteKey(GamePathKey);
        PlayerPrefs.Save();
        UpdateGamePathText();
        UnityEngine.Debug.Log("Game path reset. Will prompt for new path on next action.");
        if (statusText != null) statusText.text = "";
    }

    // Update TextMeshProUGUI with the current game path
    private void UpdateGamePathText()
    {
        if (gamePathText != null)
        {
            if (PlayerPrefs.HasKey(GamePathKey))
            {
                gamePathText.text = "Game Path: " + PlayerPrefs.GetString(GamePathKey);
            }
            else
            {
                gamePathText.text = "Game Path: Not Set";
            }
            UnityEngine.Debug.Log("Game path text updated: " + gamePathText.text);
        }
    }

    // Play hover sound (called by EventTrigger)
    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            float startTime = Time.realtimeSinceStartup;
            audioSource.PlayOneShot(hoverSound);
            float delay = (Time.realtimeSinceStartup - startTime) * 1000;
            UnityEngine.Debug.Log($"Hover sound played. Delay: {delay:F2} ms");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Hover sound failed: AudioSource {(audioSource == null ? "null" : "assigned")}, HoverSound {(hoverSound == null ? "null" : hoverSound.name)}");
        }
    }

    // Play click sound
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            float startTime = Time.realtimeSinceStartup;
            audioSource.PlayOneShot(clickSound);
            float delay = (Time.realtimeSinceStartup - startTime) * 1000;
            UnityEngine.Debug.Log($"Click sound played. Delay: {delay:F2} ms");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Click sound failed: AudioSource {(audioSource == null ? "null" : "assigned")}, ClickSound {(clickSound == null ? "null" : clickSound.name)}");
        }
    }

    // Quit the application
    private void QuitApplication()
    {
        UnityEngine.Debug.Log("Quit button pressed. Closing launcher.");
        if (statusText != null) statusText.text = "Closing...";
        try
        {
            Application.Quit();
            UnityEngine.Debug.Log("Application.Quit() called successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to quit application: " + e.Message);
        }
#if !UNITY_EDITOR
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        UnityEngine.Debug.Log("Forcefully terminated process.");
#endif
    }

    // Open custom_songs folder
    void OpenCustomSongsFolder()
    {
        if (PlayerPrefs.HasKey(GamePathKey))
        {
            string gamePath = PlayerPrefs.GetString(GamePathKey);
            string customSongsPath = Path.Combine(gamePath, "Fuser", "Content", "Paks", "custom_songs");
            if (Directory.Exists(customSongsPath))
            {
                Process.Start(customSongsPath);
                UnityEngine.Debug.Log("Opened folder: " + customSongsPath);
                if (statusText != null) statusText.text = "Opened custom songs folder";
            }
            else
            {
                UnityEngine.Debug.LogWarning("Custom songs folder does not exist at: " + customSongsPath);
                if (statusText != null) statusText.text = "Custom songs folder not found";
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("No game path stored. Please select the game installation folder.");
            if (statusText != null) statusText.text = "Select game folder";
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Game Installation Folder", "", false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                PlayerPrefs.SetString(GamePathKey, paths[0]);
                PlayerPrefs.Save();
                UpdateGamePathText();
                OpenCustomSongsFolder();
            }
        }
    }

    // Coroutine: Delete files, launch game, close launcher
    private IEnumerator ReloadAndLaunchGame()
    {
        float startTime = Time.realtimeSinceStartup;
        if (statusText != null) statusText.text = "Preparing to launch...";
        UnityEngine.Debug.Log("Starting ReloadAndLaunchGame...");

        string gamePath;
        if (!PlayerPrefs.HasKey(GamePathKey))
        {
            UnityEngine.Debug.LogWarning("No game path stored. Prompting for selection.");
            if (statusText != null) statusText.text = "Select game folder";
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Game Installation Folder", "", false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                gamePath = paths[0];
                PlayerPrefs.SetString(GamePathKey, gamePath);
                PlayerPrefs.Save();
                UpdateGamePathText();
            }
            else
            {
                UnityEngine.Debug.LogWarning("No folder selected. Cannot proceed.");
                if (statusText != null) statusText.text = "No folder selected";
                yield break;
            }
        }
        else
        {
            gamePath = PlayerPrefs.GetString(GamePathKey);
        }
        UnityEngine.Debug.Log($"Game path check completed in {(Time.realtimeSinceStartup - startTime) * 1000:F2} ms");

        string paksDir = Path.Combine(gamePath, "Fuser", "Content", "Paks");
        if (!Directory.Exists(paksDir))
        {
            UnityEngine.Debug.LogError("Paks directory not found: " + paksDir + ". Verify game path.");
            if (statusText != null) statusText.text = "Paks directory not found";
            yield break;
        }

        float fileDeleteStart = Time.realtimeSinceStartup;
        string pakFile = Path.Combine(paksDir, "customSongsUnlocked_P.pak");
        string sigFile = Path.Combine(paksDir, "customSongsUnlocked_P.sig");
        bool filesDeleted = false;
        int maxAttempts = 3;
        int attempt = 1;

        while (!filesDeleted && attempt <= maxAttempts)
        {
            bool attemptFailed = false;
            string errorMessage = "";
            try
            {
                if (File.Exists(pakFile))
                {
                    File.Delete(pakFile);
                    UnityEngine.Debug.Log($"Attempt {attempt}: Deleted {pakFile}");
                }
                if (File.Exists(sigFile))
                {
                    File.Delete(sigFile);
                    UnityEngine.Debug.Log($"Attempt {attempt}: Deleted {sigFile}");
                }
            }
            catch (Exception e)
            {
                attemptFailed = true;
                errorMessage = e.Message;
                UnityEngine.Debug.LogWarning($"Attempt {attempt}: Failed to delete files: {errorMessage}");
            }

            // Verify deletion
            if (!attemptFailed && !File.Exists(pakFile) && !File.Exists(sigFile))
            {
                filesDeleted = true;
                UnityEngine.Debug.Log("File deletion successful.");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Attempt {attempt}: Files still exist or deletion failed.");
                if (statusText != null) statusText.text = $"Retrying file deletion ({attempt}/{maxAttempts})...";
                if (attempt < maxAttempts)
                {
                    yield return new WaitForSeconds(0.5f); // Wait before retry
                }
            }
            attempt++;
        }

        if (!filesDeleted)
        {
            UnityEngine.Debug.LogError("Failed to delete files after maximum attempts. Aborting launch.");
            if (statusText != null) statusText.text = "Failed to delete files";
            yield break;
        }
        UnityEngine.Debug.Log($"File deletion took {(Time.realtimeSinceStartup - fileDeleteStart) * 1000:F2} ms");

        if (statusText != null) statusText.text = "Library Reloaded";
        yield return new WaitForSeconds(1f); // Wait for file system consistency

        float gameLaunchStart = Time.realtimeSinceStartup;
        string exeDir = Path.Combine(gamePath, "Fuser", "Binaries", "Win64");
        string exePath = Path.Combine(exeDir, "FuserEOS-Win64-Shipping.exe");
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("Game executable not found: " + exePath + ". This may be a Steam version (try Fuser-Win64-Shipping.exe) or incorrect path.");
            if (statusText != null) statusText.text = "Game executable not found";
            yield break;
        }

        try
        {
            string launchArguments = "-windowed -AUTH_LOGIN=unused -AUTH_PASSWORD=901dbe79901dbe79901dbe79901dbe79 -AUTH_TYPE=exchangecode -epicapp=2939f4752d4b4ace95a8e1b16e79d3f5 -epicenv=Prod -EpicPortal -epicusername=\"Arbys\" -epicuserid=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa -epiclocale=en-US";
            Process.Start(exePath, launchArguments);
            UnityEngine.Debug.Log("Launched Fuser with reload arguments from: " + exePath);
            if (statusText != null) statusText.text = "Launching Fuser...";
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to launch Fuser: " + e.Message);
            if (statusText != null) statusText.text = "Failed to launch Fuser";
            yield break;
        }
        UnityEngine.Debug.Log($"Game launch took {(Time.realtimeSinceStartup - gameLaunchStart) * 1000:F2} ms");

        UnityEngine.Debug.Log($"Total ReloadAndLaunchGame took {(Time.realtimeSinceStartup - startTime) * 1000:F2} ms");
        UnityEngine.Debug.Log("All operations completed. Closing launcher.");
        try
        {
            Application.Quit();
            UnityEngine.Debug.Log("Application.Quit() called successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to quit application: " + e.Message);
        }
#if !UNITY_EDITOR
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        UnityEngine.Debug.Log("Forcefully terminated process.");
#endif
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (openCustomSongsButton != null)
        {
            openCustomSongsButton.onClick.RemoveListener(OpenCustomSongsFolder);
        }
        if (reloadAndLaunchButton != null)
        {
            reloadAndLaunchButton.onClick.RemoveAllListeners();
        }
        if (resetGamePathButton != null)
        {
            resetGamePathButton.onClick.RemoveAllListeners();
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
        }

        // Stop video and audio
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            UnityEngine.Debug.Log("VideoPlayer stopped in OnDestroy.");
        }
        if (audioSource != null)
        {
            audioSource.Stop();
            UnityEngine.Debug.Log("AudioSource stopped in OnDestroy.");
        }
    }

    void OnApplicationQuit()
    {
        UnityEngine.Debug.Log("OnApplicationQuit called. Application is closing.");
    }
}