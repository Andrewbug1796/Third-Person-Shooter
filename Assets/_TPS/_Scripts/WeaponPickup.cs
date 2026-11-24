using UnityEngine;

public enum WeaponType { Pistol, Rifle, Shotgun }

public class WeaponPickup : MonoBehaviour
{
    public WeaponType weaponType = WeaponType.Rifle;
    public int ammoAmount = 10;
    public float spinSpeed = 90;

    private void Update()
    {
        // Spin so the player notices it
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
            if (playerShoot != null)
            {
                playerShoot.CollectWeapon(weaponType, ammoAmount);
                Debug.Log("Collected " + weaponType);
                Destroy(gameObject);
            }
        }
    }
}
