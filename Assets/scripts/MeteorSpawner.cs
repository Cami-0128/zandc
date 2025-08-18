using System.Collections;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("�򥻳]�w")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;
    public int meteorsPerWave = 5;

    [Header("�o�g��m")]
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    [Header("�o�g��V���")]
    public Direction shootDirection = Direction.LeftDown;

    [Header("�k���ݩ�")]
    public float meteorSpeed = 8f;
    public int meteorDamage = 30;

    // �o�g��V�T�| - �]�t�Ҧ�4�Ӥ��
    public enum Direction
    {
        LeftDown,   // �¥��U�� (�쥻��)
        RightUp,    // �¥k�W��
        RightDown,  // �¥k�U��
        LeftUp      // �¥��W��
    }

    void Start()
    {
        InvokeRepeating("SpawnMeteorWave", 0f, spawnInterval);
    }

    void SpawnMeteorWave()
    {
        if (meteorPrefab == null) return;

        // �ھڤ�V�]�w�o�g����
        float baseAngle;

        switch (shootDirection)
        {
            case Direction.LeftDown:
                baseAngle = 225f;                    // �¥��U��V (�쥻��)
                break;

            case Direction.RightUp:
                baseAngle = 45f;                     // �¥k�W��V
                break;

            case Direction.RightDown:
                baseAngle = 315f;                    // �¥k�U��V
                break;

            case Direction.LeftUp:
                baseAngle = 135f;                    // �¥��W��V
                break;

            default:
                baseAngle = 225f;                    // �w�]�¥��U
                break;
        }

        // �o�g�@�i�k��
        for (int i = 0; i < meteorsPerWave; i++)
        {
            // �p�ⴲ������ (�����b60�׽d��)
            float spreadRange = 60f; // �`��������
            float angleOffset = (spreadRange / (meteorsPerWave - 1)) * i - (spreadRange / 2);
            float finalAngle = baseAngle + angleOffset;

            // �p��o�g��V
            Vector2 direction = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            );

            // �ͦ��k�ۡ]�ϥΦ۳]�w���o�g��m�^
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