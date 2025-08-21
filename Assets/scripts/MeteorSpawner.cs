using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Basic Settings")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;
    public int meteorsPerWave = 5;

    [Header("Spawn Position")]
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    [Header("Direction")]
    public Direction shootDirection = Direction.LeftDown;

    [Header("Meteor Attributes")]
    public float meteorSpeed = 8f;
    public int meteorDamage = 30;

    [Header("Sound Settings")]
    public AudioClip shootSFX;
    private AudioSource audioSource;

    public enum Direction
    {
        LeftDown,
        RightUp,
        RightDown,
        LeftUp
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InvokeRepeating("SpawnMeteorWave", 0f, spawnInterval);
    }

    void SpawnMeteorWave()
    {
        if (meteorPrefab == null) return;

        if (shootSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSFX);
        }

        float baseAngle;
        switch (shootDirection)
        {
            case Direction.LeftDown: baseAngle = 225f; break;
            case Direction.RightUp: baseAngle = 45f; break;
            case Direction.RightDown: baseAngle = 315f; break;
            case Direction.LeftUp: baseAngle = 135f; break;
            default: baseAngle = 225f; break;
        }

        float spreadRange = 60f;
        for (int i = 0; i < meteorsPerWave; i++)
        {
            float angleOffset = (spreadRange / (meteorsPerWave - 1)) * i - (spreadRange / 2);
            float finalAngle = baseAngle + angleOffset;

            Vector2 direction = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            );

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
