using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    public EnemyData enemyData;
    public string battleScene = "BattleScene";

    private Vector2 spawnPos;

    private void Start()
    {
        spawnPos = transform.position;
        if (GameManager.Instance != null && GameManager.Instance.defeatedEnemiesInCurrentScene.Contains(GetID()))
        {
            gameObject.SetActive(false);
            Debug.Log($"EnemyTrigger: {gameObject.name} deactivated (already defeated this visit).");
        }
    }

    private string GetID()
    {
        // Use spawnPos instead of current position for the ID
        return gameObject.scene.name + "_" + gameObject.name + "_" + Mathf.RoundToInt(spawnPos.x) + "_" + Mathf.RoundToInt(spawnPos.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestManager.Instance != null && GameManager.Instance != null)
            {
                GameManager.Instance.lastEnemyTriggerID = GetID();
                QuestManager.Instance.nextBattleEnemy = enemyData;
                GameManager.Instance.LoadScene(battleScene);
            }
        }
    }
}