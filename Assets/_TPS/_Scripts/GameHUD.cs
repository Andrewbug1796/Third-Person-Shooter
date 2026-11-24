using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [Header("Player References")]
    public PlayerHealth playerHealth;
    public PlayerShoot playerShoot;

    [Header("HUD Text")]
    public TMP_Text healthText;
    public TMP_Text pistolText;
    public TMP_Text rifleText;
    public TMP_Text shotgunText;

    private void Update()
    {
        healthText.text = "HEALTH: " + playerHealth.currentHealth;

        pistolText.text = Marker(PlayerShoot.PISTOL) + "PISTOL: " + playerShoot.pistolAmmo;

        // Show "LOCKED" until the weapon has been collected
        rifleText.text = Marker(PlayerShoot.RIFLE) + "RIFLE: " +
            (playerShoot.hasRifle ? playerShoot.rifleAmmo.ToString() : "LOCKED");

        shotgunText.text = Marker(PlayerShoot.SHOTGUN) + "SHOTGUN: " +
            (playerShoot.hasShotGun ? playerShoot.shotgunAmmo.ToString() : "LOCKED");
    }

    // Puts an arrow next to the weapon currently equipped
    string Marker(int id)
    {
        return playerShoot.weaponID == id ? "> " : "  ";
    }
}
