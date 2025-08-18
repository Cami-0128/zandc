using UnityEngine;
public class MeteorSpawner2 : MonoBehaviour
{
    [Header("�򥻳]�w")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;

    [Header("�o�g��m")]
    [Tooltip("�O�_�ϥΦ۩w�q�o�g��m�A���Ŀ�h�q�������m�o�g")]
    public bool useCustomSpawnPosition = false;
    public Vector2 customSpawnPosition = new Vector2(10f, 5f);

    [Header("���׳]�w")]
    public float startAngle = 10f;      // �_�l����
    public float endAngle = 360f;       // ��������
    public float angleStep = 10f;       // ���׶��j

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

        // �p��k���`��
        int meteorCount = Mathf.RoundToInt((endAngle - startAngle) / angleStep) + 1;
        Debug.Log($"�o�g�@�i�k�ۡA�@ {meteorCount} ��");

        int index = 0;
        for (float angle = startAngle; angle <= endAngle; angle += angleStep)
        {
            SpawnSingleMeteor(angle, index);
            index++;
        }
    }

    void SpawnSingleMeteor(float angle, int index)
    {
        // �M�w�o�g��m
        Vector2 spawnPos = useCustomSpawnPosition ? customSpawnPosition : (Vector2)transform.position;

        // �ഫ����V�V�q
        float angleInRadians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // �Ыعk��
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);

        // �]�w�k���ݩ�
        MeteorController meteorController = meteor.GetComponent<MeteorController>();
        if (meteorController != null)
        {
            meteorController.SetDirection(direction);
            meteorController.moveSpeed = meteorSpeed;
            meteorController.damage = meteorDamage;
        }

        Debug.Log($"�k�� {index + 1} �o�g�A����: {angle} �סA��m: {spawnPos}");
    }

    // ��ʵo�g����
    [ContextMenu("���յo�g�@�i")]
    void TestSpawn()
    {
        SpawnMeteorWave();
    }
}