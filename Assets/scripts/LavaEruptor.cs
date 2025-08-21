// === 2. ���߼Q�o���]�O�d�즳���Q�o�w�]�p�^ ===
using System.Collections;
using UnityEngine;

public class LavaEruptor : MonoBehaviour
{
    [Header("���߼Q�o�]�w")]
    public GameObject lavaDropPrefab;
    public Transform spawnPoint;
    public float eruptionForce = 15f;
    public float eruptionInterval = 2f;
    public int dropsPerEruption = 8;
    public float spreadAngle = 45f;
    public bool isActive = true;

    [Header("�Q�o�Ҧ�")]
    public EruptionMode mode = EruptionMode.Continuous;
    public float burstDuration = 3f;
    public float cooldownTime = 5f;

    [Header("�wĵ�t��")]
    public float warningTime = 1f;
    public GameObject warningEffect;

    [Header("���ĻP�S��")]
    public AudioSource audioSource;
    public AudioClip eruptionSound;
    public AudioClip warningSound;
    public ParticleSystem smokeEffect;
    public ParticleSystem sparkEffect;

    private float nextEruptionTime;
    private bool isCoolingDown = false;
    private float burstStartTime;

    public enum EruptionMode
    {
        Continuous,
        Burst,
        Random
    }

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        nextEruptionTime = Time.time + eruptionInterval;

        if (smokeEffect != null && isActive)
            smokeEffect.Play();
    }

    void Update()
    {
        if (!isActive) return;

        switch (mode)
        {
            case EruptionMode.Continuous:
                HandleContinuousEruption();
                break;
            case EruptionMode.Burst:
                HandleBurstEruption();
                break;
            case EruptionMode.Random:
                HandleRandomEruption();
                break;
        }
    }

    void HandleContinuousEruption()
    {
        if (Time.time >= nextEruptionTime - warningTime && Time.time < nextEruptionTime - warningTime + 0.1f)
        {
            ShowWarning();
        }

        if (Time.time >= nextEruptionTime)
        {
            Erupt();
            nextEruptionTime = Time.time + eruptionInterval;
        }
    }

    void HandleBurstEruption()
    {
        if (!isCoolingDown)
        {
            if (burstStartTime == 0)
                burstStartTime = Time.time;

            if (Time.time - burstStartTime < burstDuration)
            {
                if (Time.time >= nextEruptionTime)
                {
                    Erupt();
                    nextEruptionTime = Time.time + 0.3f;
                }
            }
            else
            {
                isCoolingDown = true;
                nextEruptionTime = Time.time + cooldownTime;
                if (smokeEffect != null)
                    smokeEffect.Stop();
            }
        }
        else
        {
            if (Time.time >= nextEruptionTime)
            {
                isCoolingDown = false;
                burstStartTime = Time.time;
                if (smokeEffect != null)
                    smokeEffect.Play();
            }
        }
    }

    void HandleRandomEruption()
    {
        if (Time.time >= nextEruptionTime)
        {
            if (Random.Range(0f, 1f) < 0.6f)
            {
                Erupt();
            }
            nextEruptionTime = Time.time + Random.Range(eruptionInterval * 0.5f, eruptionInterval * 2f);
        }
    }

    void ShowWarning()
    {
        if (warningEffect != null)
        {
            warningEffect.SetActive(true);
            Invoke(nameof(HideWarning), warningTime);
        }

        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
    }

    void HideWarning()
    {
        if (warningEffect != null)
            warningEffect.SetActive(false);
    }

    void Erupt()
    {
        if (audioSource != null && eruptionSound != null)
        {
            audioSource.PlayOneShot(eruptionSound);
        }

        if (sparkEffect != null)
        {
            sparkEffect.Play();
        }

        for (int i = 0; i < dropsPerEruption; i++)
        {
            SpawnLavaDrop();
        }

        Debug.Log("���߼Q�o�I");
    }

    void SpawnLavaDrop()
    {
        if (lavaDropPrefab == null) return;

        float randomAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
        Vector2 direction = new Vector2(
            Mathf.Sin(randomAngle * Mathf.Deg2Rad),
            Mathf.Cos(randomAngle * Mathf.Deg2Rad)
        ).normalized;

        GameObject lavaDrop = Instantiate(lavaDropPrefab, spawnPoint.position, Quaternion.identity);

        Rigidbody2D rb = lavaDrop.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomForce = Random.Range(eruptionForce * 0.8f, eruptionForce * 1.2f);
            rb.velocity = direction * randomForce;
        }
    }

    public void TriggerEruption()
    {
        if (isActive)
            Erupt();
    }
}