using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水花粒子效果系統
/// </summary>
public class WaterSplashEffect : MonoBehaviour
{
    [Header("═══ 粒子效果設定 ═══")]
    public GameObject waterSplashParticlePrefab;
    public float splashForce = 5f;
    public float splashRadius = 1f;
    public int splashParticleCount = 10;

    [Header("═══ 觸發設定 ═══")]
    public float minVelocityForSplash = 2f;  // 最小速度才會產生水花

    private WaterZone waterZone;
    private ParticleSystem particleSystem;

    void Start()
    {
        waterZone = FindObjectOfType<WaterZone>();

        // 如果沒有粒子效果預設物，創建簡單的粒子系統
        if (waterSplashParticlePrefab == null)
        {
            CreateDefaultParticleSystem();
        }

        Debug.Log("[WaterSplashEffect] 初始化完成");
    }

    // ========== 創建默認粒子系統 ==========
    void CreateDefaultParticleSystem()
    {
        GameObject psObj = new GameObject("WaterSplashParticles");
        psObj.transform.SetParent(FindObjectOfType<WaterZone>()?.transform ?? null);
        psObj.transform.position = Vector3.zero;

        particleSystem = psObj.AddComponent<ParticleSystem>();
        Rigidbody2D psRb = psObj.AddComponent<Rigidbody2D>();
        psRb.gravityScale = 0;

        // 配置粒子系統
        var main = particleSystem.main;
        main.maxParticles = 200;
        main.startLifetime = 1f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.3f, 0.7f, 1f, 0.8f);
        main.duration = 2f;
        main.loop = false;

        var emission = particleSystem.emission;
        emission.rateOverTime = 50;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
        velocity.y = new ParticleSystem.MinMaxCurve(1f, 5f);

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // 使用默認材質
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        }

        waterSplashParticlePrefab = psObj;
        Debug.Log("[WaterSplashEffect] 已創建默認粒子系統");
    }

    // ========== 觸發水花效果 ==========

    /// <summary>
    /// 在指定位置產生水花
    /// </summary>
    public void CreateSplash(Vector3 position, float intensity = 1f)
    {
        if (waterZone == null || !IsInWater(position)) return;

        // 產生粒子效果
        if (waterSplashParticlePrefab != null)
        {
            GameObject splash = Instantiate(waterSplashParticlePrefab, position, Quaternion.identity);

            // 調整粒子效果的強度
            ParticleSystem ps = splash.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.rateOverTime = splashParticleCount * intensity;
            }

            // 2秒後銷毀
            Destroy(splash, 2f);
        }

        Debug.Log($"[WaterSplashEffect] 在 {position} 產生水花");
    }

    /// <summary>
    /// 當物體進入水域時產生水花
    /// </summary>
    public void CreateEntrySplash(Rigidbody2D rb)
    {
        if (rb == null) return;

        float velocity = rb.velocity.magnitude;
        if (velocity > minVelocityForSplash)
        {
            CreateSplash(rb.position, velocity / 5f);
        }
    }

    /// <summary>
    /// 當魚被擊殺時產生血色水花
    /// </summary>
    public void CreateBloodSplash(Vector3 position)
    {
        if (waterZone == null || !IsInWater(position)) return;

        // 可以創建不同顏色的粒子效果
        GameObject splash = Instantiate(waterSplashParticlePrefab, position, Quaternion.identity);

        ParticleSystem ps = splash.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = new Color(1f, 0.3f, 0.3f, 0.8f);  // 紅色

            var emission = ps.emission;
            emission.rateOverTime = splashParticleCount * 1.5f;
        }

        Destroy(splash, 2f);
        Debug.Log("[WaterSplashEffect] 產生血色水花");
    }

    /// <summary>
    /// 船隻受損時的水花
    /// </summary>
    public void CreateBoatDamageSplash(Vector3 position)
    {
        // 在船的四周產生多個水花
        // ========== 修復：改用 Vector2 ==========
        for (int i = 0; i < 3; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 splashPos = position + new Vector3(randomOffset.x, randomOffset.y, 0);
            CreateSplash(splashPos, 0.8f);
        }
    }

    /// <summary>
    /// 玩家跳入水中時的大水花
    /// </summary>
    public void CreatePlayerJumpSplash(Vector3 position, float jumpPower)
    {
        if (waterZone == null || !IsInWater(position)) return;

        GameObject splash = Instantiate(waterSplashParticlePrefab, position, Quaternion.identity);

        ParticleSystem ps = splash.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverTime = splashParticleCount * jumpPower;

            var velocity = ps.velocityOverLifetime;
            velocity.x = new ParticleSystem.MinMaxCurve(-3f * jumpPower, 3f * jumpPower);
            velocity.y = new ParticleSystem.MinMaxCurve(2f * jumpPower, 6f * jumpPower);
        }

        Destroy(splash, 2.5f);
        Debug.Log("[WaterSplashEffect] 產生玩家跳水水花");
    }

    // ========== 工具方法 ==========

    bool IsInWater(Vector3 position)
    {
        if (waterZone == null) return false;

        Collider2D waterCollider = waterZone.GetComponent<Collider2D>();
        if (waterCollider == null) return false;

        return waterCollider.bounds.Contains(position);
    }

    // ========== 公開方法 ==========

    public void SetSplashIntensity(float intensity)
    {
        splashParticleCount = Mathf.Max(1, (int)(10 * intensity));
    }

    public void SetMinVelocityForSplash(float velocity)
    {
        minVelocityForSplash = velocity;
    }
}