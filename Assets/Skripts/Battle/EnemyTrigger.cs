using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    public EnemyData enemyData;
    public string battleScene = "BattleScene";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestManager.Instance != null && GameManager.Instance != null)
            {
                QuestManager.Instance.nextBattleEnemy = enemyData;
                GameManager.Instance.LoadScene(battleScene);
            }
        }
    }
}