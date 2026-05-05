using UnityEngine;
using System.Linq;

public class TempleSpawnManager : MonoBehaviour
{
    [Header("Spawn Point")]
    public Transform templeSpawnPoint;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public TempleCameraIntro templeCameraIntro;

    private void Start()
    {
        // =========================
        // QUEST PROGRESS
        // =========================
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.visitedTemple = true;
            Debug.Log("Quest Progress: Temple besucht!");
        }

        // =========================
        // PLAYER SUCHEN
        // =========================
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("Player nicht gefunden!");
            return;
        }

        // =========================
        // PLAYER POSITION SETZEN
        // =========================
        if (templeSpawnPoint != null)
        {
            playerObject.transform.position = templeSpawnPoint.position;

            Debug.Log("Player korrekt im Tempel gespawnt.");
        }
        else
        {
            Debug.LogError("TempleSpawnPoint fehlt!");
        }

        // =========================
        // CAMERA FOLLOW FIX
        // =========================
        if (cameraFollow == null)
        {
            cameraFollow = FindFirstObjectByType<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            
            Debug.Log("CameraFollow erfolgreich verbunden.");
        }
        else
        {
            Debug.LogError("CameraFollow fehlt!");
        }

        // =========================
        // TEMPLE CAMERA INTRO FIX
        // =========================
        if (templeCameraIntro == null)
        {
            templeCameraIntro = FindFirstObjectByType<TempleCameraIntro>();
        }

        if (templeCameraIntro != null)
        {
            templeCameraIntro.player = playerObject.transform;

            Debug.Log("TempleCameraIntro erfolgreich verbunden.");
        }
        else
        {
            Debug.LogError("TempleCameraIntro fehlt!");
        }

        // =========================
        // UI FIX
        // =========================
        MyUIManager uiManager = FindFirstObjectByType<MyUIManager>();

        if (uiManager != null)
        {
            uiManager.ReconnectUIFromGameManager();

            Debug.Log("UI neu verbunden.");
        }
        else
        {
            Debug.LogError("MyUIManager nicht gefunden!");
        }

        // =========================
        // ATTRIBUTE UI FIX
        // =========================
        AttributeUI attributeUI = Resources.FindObjectsOfTypeAll<AttributeUI>()
            .FirstOrDefault();

        if (attributeUI != null)
        {
            attributeUI.playerStats = PlayerStats.Instance;
            attributeUI.SetupAllButtons();

            Debug.Log("PlayerStats erfolgreich verbunden.");
        }
        else
        {
            Debug.LogError("AttributeUI nicht gefunden!");
        }
    }
}