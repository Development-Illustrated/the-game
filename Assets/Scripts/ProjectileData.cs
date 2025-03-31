using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "Game/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Basic Properties")]
    [Tooltip("How much damage this projectile deals")]
    public float damage = 10f;

    [Tooltip("How fast the projectile moves")]
    public float velocity = 10f;

    [Tooltip("How long the projectile exists before auto-destroying (seconds)")]
    public float lifetime = 5f;

    [Header("Physics")]
    [Tooltip("Gravity scale for the projectile (0 = no gravity)")]
    public float gravityScale = 0f;

    [Header("Hit Behavior")]
    [Tooltip("Destroy the projectile when it hits something")]
    public bool destroyOnHit = true;

    [Tooltip("How many times the projectile can hit each object (0 = unlimited)")]
    public int hitMaxTimes = 1;

    [Tooltip("Cooldown between hits if hitting the same object multiple times")]
    public float hitCooldown = 0.5f;
}
