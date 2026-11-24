using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

[System.Serializable]

public class WeaponSlot
{
    public GameObject weaponModel;
    public Transform leftHandIK;
}


public class PlayerShoot : MonoBehaviour
{
    Tps_Inputs tpsInputs;
    PlayerMove playerMove;
    [SerializeField] Animator anim;

    // Weapon IDs match the order of the _weapons array in the Inspector
    public const int PISTOL = 0;
    public const int SHOTGUN = 1;
    public const int RIFLE = 2;

    [Header("Set Weapon Type")]
    public int weaponID = PISTOL;

    [Header("Weapon Flags")]
    public bool hasRifle = false;
    public bool hasShotGun = false;

    [Header("Ammo")]
    public int pistolAmmo = 30;
    public int rifleAmmo = 0;
    public int shotgunAmmo = 0;

    [Header("HUD Elements")]
    public GameObject aimPanel;
    public GameObject scopePanel;
    public Image crosshair;
    public Sprite pistolCrossHair;
    public Sprite shotgunCrossHair;

    [Header("Projectile Effects")]
    public Rigidbody bulletTrail;
    public GameObject bulletDecal;
    public GameObject bulletImpact;
    

    [Header("Spawn Points")]
    public Transform gunSpawnPoint;
    public Transform pistolSpawn;
    public Transform shotgunSpawn;
    public Transform rifleSpawn;
    public Transform aimDebugObject;

    [Header("Damage")]
    // Pistol Damage
    public float pistolHeadDamage = 30;
    public float pistolBodyDamage = 10;
    public float pistolLegDamage = 10;

    //Shotgun Damage
    public float shotHeadDamage = 30;
    public float shotBodyDamage = 10;
    public float shotLegDamage = 10;

    // Rifle Damage
    public float rifleHeadDamage = 30;
    public float rifleBodyDamage = 10;
    public float rifleLegDamage = 10;

    [Header("Layer Mask")]
    public LayerMask aimCollisionMask = new LayerMask();
    public LayerMask environment = new LayerMask();

    [Header("Ray Distance")]
    public float pistolDistance = 100;
    public float shotDistance = 15;
    public float rifleDistance = 999;

    [Header("Weapon Speed")]
    public float pistolSpeed = 0.25f;
    public float shotSpeed = 1f;
    public float rifleSpeed = 1f;
    [SerializeField] private float gunCoolDown;

    [Header("Shot Amount")]
    public int shotAmount = 7;
    public float randomAccuracy = 5;

    [Header("Weapon Switching")]
    public WeaponSlot[] _weapons;
    public RigBuilder rigBuilder;
    public TwoBoneIKConstraint leftHandConstraint;
    public Rig rig;
    public MultiAimConstraint bodyAim;
    public Vector3 pistolOffset;
    public Vector3 longGunOffset;
    

    private void Awake()
    {
        tpsInputs = new Tps_Inputs();
        playerMove = GetComponent<PlayerMove>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable() => tpsInputs.Enable();
    private void OnDisable() => tpsInputs.Disable();

    private void Start()
    {
        EquipWeapon(weaponID);
    }

    private void Update()
    {
        rig.weight = playerMove.isAiming ? 1 : 0;
        aimPanel.SetActive(playerMove.isAiming ? true : false);
        scopePanel.SetActive(playerMove.isAiming && playerMove.isScoped ? true : false);

        if(gunCoolDown > 0) gunCoolDown -= Time.deltaTime;
        else gunCoolDown = 0;

        // Weapon switching - rifle and shotgun only work once they've been collected
        if (tpsInputs.Player.Weapon1.triggered)
            SwitchWeapon(PISTOL);
        if (tpsInputs.Player.Weapon2.triggered && hasRifle)
            SwitchWeapon(RIFLE);
        if (tpsInputs.Player.Weapon3.triggered && hasShotGun)
            SwitchWeapon(SHOTGUN);

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (tpsInputs.Player.Fire.triggered && playerMove.isAiming && gunCoolDown <= 0)
        {
            switch(weaponID)
            {
                case PISTOL:
                    if (pistolAmmo <= 0) break;
                    pistolAmmo--;
                    gunCoolDown = pistolSpeed;
                    FirePistol();
                    break;
                case RIFLE:
                    if (rifleAmmo <= 0) break;
                    rifleAmmo--;
                    gunCoolDown = rifleSpeed;
                    FireRifle();
                    break;
                case SHOTGUN:
                    if (shotgunAmmo <= 0) break;
                    shotgunAmmo--;
                    gunCoolDown = shotSpeed;
                    FireShotgun();
                    break;
            }
        }
            

        if(Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimCollisionMask))
        {
            aimDebugObject.position = rayCastHit.point;
            gunSpawnPoint.LookAt(rayCastHit.point);
            pistolSpawn.LookAt(rayCastHit.point);
            rifleSpawn.LookAt(rayCastHit.point);
            shotgunSpawn.LookAt(rayCastHit.point);
                
        }
    }

    public void LateUpdate()
    {
        Vector3 _offeset = bodyAim.data.offset;
        if(playerMove.isAiming)
        {
            if (weaponID == PISTOL)
                _offeset = pistolOffset;
            else
                _offeset = longGunOffset;
        }
        bodyAim.data.offset = _offeset;
    }

    void SwitchWeapon(int id)
    {
        if (weaponID == id) return;
        weaponID = id;
        EquipWeapon(id);
    }

    // Called by WeaponPickup when the player walks over a weapon
    public void CollectWeapon(WeaponType type, int ammo)
    {
        switch (type)
        {
            case WeaponType.Pistol:
                pistolAmmo += ammo;
                break;
            case WeaponType.Rifle:
                hasRifle = true;
                rifleAmmo += ammo;
                break;
            case WeaponType.Shotgun:
                hasShotGun = true;
                shotgunAmmo += ammo;
                break;
        }
    }

    void EquipWeapon(int id)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            _weapons[i].weaponModel.SetActive(i == id);
        }

        var slot = _weapons[id];
        var data = leftHandConstraint.data;
        data.target = slot.leftHandIK;
        leftHandConstraint.data = data;

        rigBuilder.Build();
    }

    void FirePistol()
    {
        SoundManager.Instance.PlaySound2D("PistolFire");
        anim.SetTrigger("Pistol");
        Rigidbody _trail;
        _trail = Instantiate(bulletTrail, pistolSpawn.position, 
            pistolSpawn.rotation) as Rigidbody;
        _trail.AddForce(pistolSpawn.forward * 300, ForceMode.Impulse);

        if(Physics.Raycast(pistolSpawn.position, pistolSpawn.forward, 
            out RaycastHit hit, pistolDistance))
        {
            WeaponHit(hit, (int) pistolHeadDamage, (int) pistolBodyDamage, (int) pistolLegDamage);
        }
    }

    void FireRifle()
    {
        SoundManager.Instance.PlaySound2D("Rifle");
        Rigidbody _trail;
        _trail = Instantiate(bulletTrail, rifleSpawn.position,
            rifleSpawn.rotation) as Rigidbody;
        _trail.AddForce(rifleSpawn.forward * 300, ForceMode.Impulse);

        if (Physics.Raycast(rifleSpawn.position, rifleSpawn.forward,
            out RaycastHit hit, pistolDistance))
        {
            WeaponHit(hit, (int) rifleHeadDamage, (int)rifleBodyDamage, (int)rifleLegDamage);
        }

    }

    void FireShotgun()
    {
        SoundManager.Instance.PlaySound2D("ShotGun");
        for (int i = 0; i < shotAmount; i++)
        {
            Vector3 shotAngle = GetShotDirection();
            Rigidbody _trail;
            _trail = Instantiate(bulletTrail, gunSpawnPoint.position,
                gunSpawnPoint.rotation) as Rigidbody;

            _trail.AddForce(shotAngle * 300, ForceMode.Impulse);

            if (Physics.Raycast(gunSpawnPoint.position, shotAngle,
                out RaycastHit hit, pistolDistance))
            {
                WeaponHit(hit, (int) shotHeadDamage, (int) shotBodyDamage, (int) shotLegDamage);
            }
        }
    }

    Vector3 GetShotDirection()
    {
        Vector3 targetPos = gunSpawnPoint.position + gunSpawnPoint.forward * 30;

        targetPos = new Vector3(targetPos.x + Random.Range(-randomAccuracy, randomAccuracy),
            targetPos.y + Random.Range(-randomAccuracy, randomAccuracy),
            targetPos.z + Random.Range(- randomAccuracy, randomAccuracy));

        Vector3 _direction = targetPos - gunSpawnPoint.position;
        return _direction.normalized;
    }

    void WeaponHit(RaycastHit hit, int headDmg, int bodyDmg, int legDmg)
    {
        Debug.Log("You just hit " + hit.collider.name);

        if ((environment.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            Instantiate(bulletDecal, hit.point, Quaternion.LookRotation(hit.normal));
            Instantiate (bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
        }
        if(hit.collider.tag == "Enemy")
        {
            ZombieHitZone zone = hit.collider.GetComponent<ZombieHitZone>();
            if (zone != null)
            {
                zone.ZombieShoot(headDmg, bodyDmg, legDmg);
            }
        }
    } 
}
