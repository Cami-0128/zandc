using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水域系統 - 修復版：正確的深度判定
/// 
/// 三個層級說明：
/// 红線(水面線) ─────────────────
/// 黄線(半浸沒底) ─────────────────
/// 蓝線(全水下線) ─────────────────
/// 
/// Surface: 紅線上方 (浮在水面)
/// Partial: 紅線和黃線之間 (半浸沒)
/// Submerged: 黃線和藍線之間 (完全浸沒，會扣血)
/// </summary>
public class WaterZone : MonoBehaviour
{
    [Header("═══ 水域邊界設定 ═══")]
    private BoxCollider2D waterCollider;
    private Vector2 boundsMin;
    private Vector2 boundsMax;
    private float waterSurfaceY;

    [Header("═══ 深度層級設定 ═══")]
    [Tooltip("水面高度相對於水域頂部的位置")]
    public float surfaceDepthOffset = 0.2f;
    [Tooltip("半浸沒層的厚度")]
    public float partialSubmergedThickness = 0.5f;

    private float surfaceBottomY;
    private float partialBottomY;

    [Header("═══ 玩家物理設定 ═══")]
    public float playerSwimInputForce = 8f;
    public float playerSwimDrag = 0.85f;
    public float playerSinkAcceleration = 2f;
    public float playerWaterDrag = 0.6f;

    [Header("═══ 船隻物理設定 ═══")]
    public float boatSurfaceDamping = 0.9f;
    public float boatWaterDragMultiplier = 0.8f;

    [Header("═══ 魚類物理設定 ═══")]
    public float fishNeutralBuoyancy = 1f;
    public float fishBoundaryAvoidanceForce = 5f;

    [Header("═══ 溺水傷害設定 ═══")]
    public int[] drownDamageThresholds = { 15, 30, 45, 60, 75, 90 };
    public int[] drownDamageAmounts = { 2, 5, 10, 20, 30, 40 };
    private Dictionary<Rigidbody2D, float> timeFullySubmerged = new Dictionary<Rigidbody2D, float>();
    private Dictionary<Rigidbody2D, int> lastDamageIndex = new Dictionary<Rigidbody2D, int>();

    [Header("═══ 視覺效果 ═══")]
    public bool enableWaveAnimation = true;
    public float waveAmplitude = 0.2f;
    public float waveFrequency = 2f;
    private float timeOffset = 0f;

    [Header("═══ 特殊設定 ═══")]
    public string playerTag = "Player";
    public string boatTag = "Boat";
    public string fishTag = "Fish";

    [Header("═══ Debug 設定 ═══")]
    public bool enableDetailedDebug = true;

    private List<Rigidbody2D> objectsInWater = new List<Rigidbody2D>();
    private Dictionary<Rigidbody2D, string> objectTypes = new Dictionary<Rigidbody2D, string>();

    void Start()
    {
        waterCollider = GetComponent<BoxCollider2D>();
        if (waterCollider == null)
        {
            Debug.LogError("[WaterZone] 必須有 BoxCollider2D (IsTrigger=true)!");
            return;
        }

        CalculateWaterBounds();
        Debug.Log("[WaterZone] ✅ 初始化完成");
    }

    void Update()
    {
        if (enableWaveAnimation)
        {
            timeOffset += Time.deltaTime;
            waterSurfaceY = boundsMax.y - surfaceDepthOffset + Mathf.Sin(timeOffset * waveFrequency) * waveAmplitude;
            surfaceBottomY = waterSurfaceY - partialSubmergedThickness;
            partialBottomY = boundsMin.y;
        }
    }

    void FixedUpdate()
    {
        for (int i = objectsInWater.Count - 1; i >= 0; i--)
        {
            Rigidbody2D rb = objectsInWater[i];
            if (rb == null)
            {
                objectsInWater.RemoveAt(i);
                objectTypes.Remove(rb);
                continue;
            }

            if (!objectTypes.ContainsKey(rb))
            {
                DetermineObjectType(rb);
            }

            string objType = objectTypes[rb];

            EnforceWaterBoundary(rb);

            switch (objType)
            {
                case "Player":
                    ApplyPlayerPhysics(rb);
                    break;
                case "Boat":
                    ApplyBoatPhysics(rb);
                    break;
                case "Fish":
                    ApplyFishPhysics(rb);
                    break;
            }

            // 溺水傷害
            if (objType == "Player")
            {
                UpdateDrownDamage(rb);
            }
        }
    }

    // ========== 水域邊界計算 ==========
    void CalculateWaterBounds()
    {
        Bounds bounds = waterCollider.bounds;
        boundsMin = bounds.min;
        boundsMax = bounds.max;

        waterSurfaceY = boundsMax.y - surfaceDepthOffset;
        surfaceBottomY = waterSurfaceY - partialSubmergedThickness;
        partialBottomY = boundsMin.y;

        Debug.Log($"[WaterZone] 邊界 Y: {boundsMin.y:F2} ~ {boundsMax.y:F2}");
        Debug.Log($"[WaterZone] 水面線（紅線）: {waterSurfaceY:F2}");
        Debug.Log($"[WaterZone] 半浸沒底（黃線）: {surfaceBottomY:F2}");
        Debug.Log($"[WaterZone] 全水下線（藍線）: {partialBottomY:F2}");
    }

    // ========== 邊界強制推回 ==========
    void EnforceWaterBoundary(Rigidbody2D rb)
    {
        if (!rb.CompareTag(fishTag))
            return;

        Vector2 pos = rb.position;
        Vector2 newPos = pos;
        bool outOfBounds = false;

        if (pos.x < boundsMin.x)
        {
            newPos.x = boundsMin.x;
            outOfBounds = true;
        }
        else if (pos.x > boundsMax.x)
        {
            newPos.x = boundsMax.x;
            outOfBounds = true;
        }

        if (pos.y < boundsMin.y)
        {
            newPos.y = boundsMin.y;
            outOfBounds = true;
        }
        else if (pos.y > boundsMax.y)
        {
            newPos.y = boundsMax.y;
            outOfBounds = true;
        }

        if (outOfBounds)
        {
            rb.position = newPos;
            Vector2 vel = rb.velocity;
            if (pos.x != newPos.x) vel.x *= -0.5f;
            if (pos.y != newPos.y) vel.y *= -0.5f;
            rb.velocity = vel;
        }
    }

    // ========== 深度檢測 ✅ 修復版 ==========
    public enum DepthState { Surface, Partial, Submerged }

    /// <summary>
    /// 深度判定：
    /// Surface: Y > 紅線（水面線）
    /// Partial: 黃線 ≤ Y ≤ 紅線（半浸沒）
    /// Submerged: 藍線 ≤ Y < 黃線（完全浸沒，會扣血）
    /// </summary>
    public DepthState GetDepthState(Vector2 position)
    {
        if (position.y > waterSurfaceY)
        {
            // 在紅線上方 = Surface（浮在水面）
            return DepthState.Surface;
        }
        else if (position.y > surfaceBottomY)
        {
            // 在紅線和黃線之間 = Partial（半浸沒）
            return DepthState.Partial;
        }
        else if (position.y > partialBottomY)
        {
            // 在黃線和藍線之間 = Submerged（完全浸沒，會扣血）
            return DepthState.Submerged;
        }
        else
        {
            // 在藍線下方 = Submerged（完全浸沒）
            return DepthState.Submerged;
        }
    }

    public float GetSubmergedRatio(Collider2D collider)
    {
        if (collider == null) return 0f;

        Bounds objBounds = collider.bounds;
        float submergedAmount = Mathf.Max(0, waterSurfaceY - objBounds.min.y) / objBounds.size.y;
        return Mathf.Clamp01(submergedAmount);
    }

    // ========== 玩家物理 ==========
    void ApplyPlayerPhysics(Rigidbody2D rb)
    {
        PlayerController2D player = rb.GetComponent<PlayerController2D>();
        if (player == null) return;

        DepthState depth = GetDepthState(rb.position);

        InvincibilityController invincibility = player.GetComponent<InvincibilityController>();
        bool isInvincible = invincibility != null && invincibility.IsInvincible();

        switch (depth)
        {
            case DepthState.Surface:
                ApplyPlayerMovementDrag(rb, 0.3f);
                break;

            case DepthState.Partial:
                // ✅ 在無敵時不應用減速
                if (!isInvincible)
                {
                    ApplyPlayerMovementDrag(rb, playerWaterDrag);
                    ApplyPlayerSwimPhysics(rb, 0.5f);
                }
                break;

            case DepthState.Submerged:
                // ✅ 在無敵時不應用減速
                if (!isInvincible)
                {
                    ApplyPlayerMovementDrag(rb, playerWaterDrag);
                    ApplyPlayerSwimPhysics(rb, 1f);
                }
                break;
        }
    }

    void ApplyPlayerMovementDrag(Rigidbody2D rb, float dragAmount)
    {
        rb.velocity *= (1f - dragAmount * Time.fixedDeltaTime);
    }

    void ApplyPlayerSwimPhysics(Rigidbody2D rb, float submergedRatio)
    {
        PlayerController2D player = rb.GetComponent<PlayerController2D>();
        if (player == null) return;

        bool isSwimmingUp = player.isInWater && rb.velocity.y > 0.1f;

        if (!isSwimmingUp)
        {
            rb.velocity -= Vector2.up * playerSinkAcceleration * submergedRatio * Time.fixedDeltaTime;
        }

        rb.velocity *= playerSwimDrag;
    }

    // ========== 船隻物理 ==========
    void ApplyBoatPhysics(Rigidbody2D rb)
    {
        Boat boat = rb.GetComponent<Boat>();
        if (boat == null) return;

        float boatCenterY = rb.position.y;
        float targetY = waterSurfaceY;

        float buoyancy = (targetY - boatCenterY) * 10f;
        rb.velocity = new Vector2(rb.velocity.x, buoyancy);

        float submergedRatio = GetSubmergedRatio(rb.GetComponent<Collider2D>());
        if (submergedRatio > 0.1f)
        {
            rb.velocity *= (1f - submergedRatio * boatWaterDragMultiplier * Time.fixedDeltaTime);
        }
    }

    // ========== 魚類物理 ==========
    void ApplyFishPhysics(Rigidbody2D rb)
    {
        float gravityForce = rb.mass * Physics2D.gravity.y;
        float neutralBuoyancy = -gravityForce * fishNeutralBuoyancy;

        rb.velocity = new Vector2(
            rb.velocity.x,
            rb.velocity.y + (neutralBuoyancy / rb.mass) * Time.fixedDeltaTime
        );

        rb.velocity *= 0.99f;

        Vector2 pos = rb.position;
        Vector2 boundaryPushForce = Vector2.zero;
        float boundaryDistance = 0.5f;

        if (pos.x < boundsMin.x + boundaryDistance)
        {
            boundaryPushForce.x = fishBoundaryAvoidanceForce;
        }
        else if (pos.x > boundsMax.x - boundaryDistance)
        {
            boundaryPushForce.x = -fishBoundaryAvoidanceForce;
        }

        if (pos.y < boundsMin.y + boundaryDistance)
        {
            boundaryPushForce.y = fishBoundaryAvoidanceForce;
        }
        else if (pos.y > boundsMax.y - boundaryDistance)
        {
            boundaryPushForce.y = -fishBoundaryAvoidanceForce;
        }

        if (boundaryPushForce.magnitude > 0)
        {
            rb.AddForce(boundaryPushForce, ForceMode2D.Force);
        }
    }

    // ========== 溺水傷害 ==========
    void UpdateDrownDamage(Rigidbody2D rb)
    {
        PlayerController2D player = rb.GetComponent<PlayerController2D>();
        if (player == null || player.isDead) return;

        DepthState depth = GetDepthState(rb.position);

        InvincibilityController invincibility = player.GetComponent<InvincibilityController>();
        bool isInvincible = invincibility != null && invincibility.IsInvincible();

        // ✅ 只有 Submerged 狀態才扣血
        if (depth != DepthState.Submerged)
        {
            if (timeFullySubmerged.ContainsKey(rb))
            {
                timeFullySubmerged[rb] = 0f;
                lastDamageIndex[rb] = 0;
            }
            return;
        }

        if (isInvincible)
        {
            if (timeFullySubmerged.ContainsKey(rb))
            {
                timeFullySubmerged[rb] = 0f;
                lastDamageIndex[rb] = 0;
            }
            return;
        }

        // 初始化
        if (!timeFullySubmerged.ContainsKey(rb))
        {
            timeFullySubmerged[rb] = 0f;
            lastDamageIndex[rb] = 0;
            Debug.Log($"[WaterZone] 🌊 玩家開始完全浸沒（Y={rb.position.y:F2}）");
        }

        // 累積浸沒時間
        timeFullySubmerged[rb] += Time.fixedDeltaTime;
        float timeInSeconds = timeFullySubmerged[rb];

        // 每 5 秒打印一次
        if (Mathf.FloorToInt(timeInSeconds) % 5 == 0 && Mathf.FloorToInt(timeInSeconds) != Mathf.FloorToInt(timeInSeconds - Time.fixedDeltaTime))
        {
            Debug.Log($"[WaterZone] ⏱️ 浸沒時間: {timeInSeconds:F1}秒");
        }

        // 檢查傷害時間點
        for (int i = lastDamageIndex[rb]; i < drownDamageThresholds.Length; i++)
        {
            if (timeInSeconds >= drownDamageThresholds[i])
            {
                if (!player.isDead)
                {
                    int damage = drownDamageAmounts[i];
                    player.TakeDamage(damage);
                    Debug.Log($"[WaterZone] 💀 溺水傷害 -{damage} (浸沒 {timeInSeconds:F1}秒，血量: {player.GetCurrentHealth()}/{player.GetMaxHealth()})");
                }

                lastDamageIndex[rb] = i + 1;
            }
        }
    }

    // ========== 進出水域事件 ==========
    void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null && !objectsInWater.Contains(rb))
        {
            objectsInWater.Add(rb);
            DetermineObjectType(rb);

            if (!timeFullySubmerged.ContainsKey(rb))
            {
                timeFullySubmerged[rb] = 0f;
                lastDamageIndex[rb] = 0;
            }

            PlayerController2D player = rb.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.isInWater = true;
                Debug.Log($"[WaterZone] 💧 玩家進入水域");
            }

            Debug.Log($"[WaterZone] {collision.gameObject.name} 進入水域");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            objectsInWater.Remove(rb);
            objectTypes.Remove(rb);
            timeFullySubmerged.Remove(rb);
            lastDamageIndex.Remove(rb);

            PlayerController2D player = rb.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.isInWater = false;
                Debug.Log($"[WaterZone] 🏖️ 玩家離開水域");
            }

            Debug.Log($"[WaterZone] {collision.gameObject.name} 離開水域");
        }
    }

    // ========== 物件類型判定 ==========
    void DetermineObjectType(Rigidbody2D rb)
    {
        if (rb.CompareTag(playerTag))
            objectTypes[rb] = "Player";
        else if (rb.CompareTag(boatTag))
            objectTypes[rb] = "Boat";
        else if (rb.CompareTag(fishTag))
            objectTypes[rb] = "Fish";
        else
            objectTypes[rb] = "Other";
    }

    // ========== 調試 Gizmos ==========
    void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Bounds bounds = col.bounds;

        // 整個水域
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.2f);
        Gizmos.DrawCube(bounds.center, bounds.size);

        // 紅線：水面線（Surface/Partial 邊界）
        Gizmos.color = Color.red;
        float surfaceY = bounds.max.y - surfaceDepthOffset;
        Gizmos.DrawLine(
            new Vector3(bounds.min.x, surfaceY, 0),
            new Vector3(bounds.max.x, surfaceY, 0)
        );

        // 黃線：半浸沒底（Partial/Submerged 邊界）
        Gizmos.color = Color.yellow;
        float partialBottomY = surfaceY - partialSubmergedThickness;
        Gizmos.DrawLine(
            new Vector3(bounds.min.x, partialBottomY, 0),
            new Vector3(bounds.max.x, partialBottomY, 0)
        );

        // 藍線：全水下線（Submerged 下限）
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(bounds.min.x, bounds.min.y, 0),
            new Vector3(bounds.max.x, bounds.min.y, 0)
        );
    }

    public Vector2 GetWaterBoundsMin() => boundsMin;
    public Vector2 GetWaterBoundsMax() => boundsMax;
    public float GetWaterSurfaceY() => waterSurfaceY;
}