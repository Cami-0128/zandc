using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("�򥻳]�w")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;
    public int meteorsPerWave = 5;

    [Header("�o�g��m")]
    public Vector2 spawnPosition = new Vector2(10f, 5f);

    [Header("���׳]�w")]
    public float spreadAngle = 60f;
    public float baseAngle = 225f;

    [Header("�k���ݩ�")]
    public float meteorSpeed = 8f;
    public int meteorDamage = 30;

    void Start()
    {
        Debug.Log("�k�۵o�g���Ұ�");

        // �ߧY�o�g�Ĥ@�i�A�M��C3��o�g�@�i
        InvokeRepeating("SpawnMeteorWave", 0f, spawnInterval);
    }

    void SpawnMeteorWave()
    {
        if (meteorPrefab == null)
        {
            Debug.LogError("�S���]�w�k�۹w�s���I");
            return;
        }

        Debug.Log($"�o�g�@�i�k�ۡA�@ {meteorsPerWave} ��");

        for (int i = 0; i < meteorsPerWave; i++)
        {
            SpawnSingleMeteor(i);
        }
    }

    void SpawnSingleMeteor(int index)
    {
        // �p�⨤��
        float angleStep = spreadAngle / (meteorsPerWave - 1);
        float currentAngle = baseAngle - (spreadAngle / 2) + (angleStep * index);

        // �ഫ����V�V�q
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // �Ыعk��
        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

        // �]�w�k���ݩ�
        MeteorController meteorController = meteor.GetComponent<MeteorController>();
        if (meteorController != null)
        {
            meteorController.SetDirection(direction);
            meteorController.moveSpeed = meteorSpeed;
            meteorController.damage = meteorDamage;
        }

        Debug.Log($"�k�� {index + 1} �o�g�A����: {currentAngle} ��");
    }

    // ��ʵo�g����
    [ContextMenu("���յo�g�@�i")]
    void TestSpawn()
    {
        SpawnMeteorWave();
    }
}