using System.Collections;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("基本設定")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;
    public int meteorsPerWave = 5;

    [Header("發射位置")]
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    [Header("發射方向選擇")]
    public Direction shootDirection = Direction.LeftDown;

    [Header("隕石屬性")]
    public float meteorSpeed = 8f;
    public int meteorDamage = 30;

    // 發射方向枚舉 - 包含所有4個方位
    public enum Direction
    {
        LeftDown,   // 朝左下角 (原本的)
        RightUp,    // 朝右上角
        RightDown,  // 朝右下角
        LeftUp      // 朝左上角
    }

    void Start()
    {
        InvokeRepeating("SpawnMeteorWave", 0f, spawnInterval);
    }

    void SpawnMeteorWave()
    {
        if (meteorPrefab == null) return;

        // 根據方向設定發射角度
        float baseAngle;

        switch (shootDirection)
        {
            case Direction.LeftDown:
                baseAngle = 225f;                    // 朝左下方向 (原本的)
                break;

            case Direction.RightUp:
                baseAngle = 45f;                     // 朝右上方向
                break;

            case Direction.RightDown:
                baseAngle = 315f;                    // 朝右下方向
                break;

            case Direction.LeftUp:
                baseAngle = 135f;                    // 朝左上方向
                break;

            default:
                baseAngle = 225f;                    // 預設朝左下
                break;
        }

        // 發射一波隕石
        for (int i = 0; i < meteorsPerWave; i++)
        {
            // 計算散布角度 (集中在60度範圍內)
            float spreadRange = 60f; // 總散布角度
            float angleOffset = (spreadRange / (meteorsPerWave - 1)) * i - (spreadRange / 2);
            float finalAngle = baseAngle + angleOffset;

            // 計算發射方向
            Vector2 direction = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            );

            // 生成隕石（使用自設定的發射位置）
            GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
            MeteorController controller = meteor.GetComponent<MeteorController>();

            if (controller != null)
            {
                controller.SetDirection(direction);
                controller.SetSpeed(meteorSpeed);
                controller.SetDamage(meteorDamage);
            }
        }
    }
}