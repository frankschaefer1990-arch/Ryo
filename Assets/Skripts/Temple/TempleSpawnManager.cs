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
            Debug.Log("Quest Progress: Temple betreten.");
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
        }
        }