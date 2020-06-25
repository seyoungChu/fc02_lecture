using FC;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// 사격 기능 : 사격이 가능한지 여부를 체크하는 기능.
/// 발사 키 입력받아서 애니메이션 재생, 이펙트 생성, 충돌 체크 기능
/// UI 관련해서 십자선 표시 기능
/// 발사 속도 조정
/// 캐릭터 상체를 IK를 이용해서 조준 시점에 맞춰 회전.
/// 벽이나 충돌체 총알이 피격되었을경우 피탄이펙트를 생성.
/// 인벤토리 역할. 무기를 소지하고 있는지 확인용.
/// 재장전과 무기 교체 기능까지 포함하고 있어요.
/// </summary>
public class ShootBehaviour : GenericBehaviour
{
    public Texture2D aimCrossHair, shootCrossHair;
    public GameObject muzzleFlash, shot, sparks;
    public Material bulletHole;
    public int MaxBulletHoles = 50;
    public float shootErrorRate = 0.01f;
    public float shootRateFactor = 1f;

    public float armsRotation = 8f;

    public LayerMask shotMask = ~(TagAndLayer.LayerMasking.IgnoreRayCast | TagAndLayer.LayerMasking.IgnoreShot |
        TagAndLayer.LayerMasking.CoverInvisible | TagAndLayer.LayerMasking.Player);
    public LayerMask organicMask = TagAndLayer.LayerMasking.Player | TagAndLayer.LayerMasking.Enemy;
    //짧은 총, 피스톨 같은 총을 들었을때 조준시 왼팔의 위치 보정.
    public Vector3 leftArmShortAim = new Vector3(-4.0f, 0.0f, 2.0f);

    private int activeWeapon = 0;
    //animator value
    private int weaponType;
    private int changeWeaponTrigger;
    private int shootingTrigger;
    private int aimBool, blockedAimBool, reloadBool;

    private List<InteractiveWeapon> weapons; //소지하고 있는 무기들.
    private bool isAiming, isAimBlocked;

    private Transform gunMuzzle;
    private float distToHand;

    private Vector3 castRelativeOrigin;

    private Dictionary<InteractiveWeapon.WeaponType, int> slotMap;

    private Transform hips, spine, chest, rightHand, leftArm;
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Vector3 initialCheckRotation;

    private float shotInterval, originalShoInterval = 0.5f; //
    private List<GameObject> bulletHoles;
    private int bulletHoleSlot = 0;
    private int burstShotCount = 0;
    private AimBehaviour aimBehaviour;
    private Texture2D originalCrossHair;
    private bool isShooting = false;
    private bool isChangingWeapon = false;
    private bool isShotAlive = false;

    private void Start()
    {
        weaponType = Animator.StringToHash(AnimatorKey.Weapon);
        aimBool = Animator.StringToHash(AnimatorKey.Aim);
        blockedAimBool = Animator.StringToHash(AnimatorKey.BlockedAim);
        changeWeaponTrigger = Animator.StringToHash(AnimatorKey.ChangeWeapon);
        shootingTrigger = Animator.StringToHash(AnimatorKey.Shooting);
        reloadBool = Animator.StringToHash(AnimatorKey.Reload);
        weapons = new List<InteractiveWeapon>(new InteractiveWeapon[3]);
        aimBehaviour = GetComponent<AimBehaviour>();
        bulletHoles = new List<GameObject>();

        muzzleFlash.SetActive(false);
        shot.SetActive(false);
        sparks.SetActive(false);

        slotMap = new Dictionary<InteractiveWeapon.WeaponType, int>
        {
            {InteractiveWeapon.WeaponType.SHORT,1},
            {InteractiveWeapon.WeaponType.LONG,2}
        };

        Transform neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Neck);
        if(!neck)
        {
            neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Head).parent;
        }
        hips = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        spine = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        chest = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        leftArm = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;
        initialCheckRotation = chest.localEulerAngles;
        originalCrossHair = aimBehaviour.crossHair;
        shotInterval = originalShoInterval;
        castRelativeOrigin = neck.position - transform.position;
        distToHand = (rightHand.position - neck.position).magnitude * 1.5f;
    }
    //발사 비주얼 담당.
    private void DrawShoot(GameObject weapon, Vector3 destination, Vector3 targetNormal, Transform parent,
        bool placeSparks = true, bool placeBulletHole = true)
    {
        Vector3 origin = gunMuzzle.position - gunMuzzle.right * 0.5f;

        muzzleFlash.SetActive(true);
        muzzleFlash.transform.SetParent(gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.back * 90f;

        GameObject instantShot = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, origin);
        instantShot.SetActive(true);
        instantShot.transform.rotation = Quaternion.LookRotation(destination - origin);
        instantShot.transform.parent = shot.transform.parent;

        if(placeSparks)
        {
            GameObject instantSparks = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, destination);
            instantSparks.SetActive(true);
            instantSparks.transform.parent = sparks.transform.parent;
        }
        if(placeBulletHole)
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, targetNormal);
            GameObject bullet = null;
            if(bulletHoles.Count < MaxBulletHoles)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bullet.GetComponent<MeshRenderer>().material = bulletHole;
                bullet.GetComponent<Collider>().enabled = false;
                bullet.transform.localScale = Vector3.one * 0.07f;
                bullet.name = "BulletHole";
                bulletHoles.Add(bullet);
            }
            else
            {
                bullet = bulletHoles[bulletHoleSlot];
                bulletHoleSlot++;
                bulletHoleSlot %= MaxBulletHoles;
            }
            bullet.transform.position = destination + 0.01f * targetNormal;
            bullet.transform.rotation = hitRotation;
            bullet.transform.SetParent(parent);
        }       

    }

    private void ShootWeapon(int weapon, bool firstShot = true)
    {
        if(!isAiming || isAimBlocked || behaviourController.GetAnimator.GetBool(reloadBool) || 
            !weapons[weapon].Shoot(firstShot))
        {
            return;
        }
        else
        {
            this.burstShotCount++;
            behaviourController.GetAnimator.SetTrigger(shootingTrigger);
            aimBehaviour.crossHair = shootCrossHair;
            behaviourController.GetCamScript.BounceVertical(weapons[weapon].recoilAngle);

            Vector3 imprecision = Random.Range(-shootErrorRate, shootErrorRate) *
                behaviourController.playerCamera.forward;
            Ray ray = new Ray(behaviourController.playerCamera.position,
                behaviourController.playerCamera.forward + imprecision);
            RaycastHit hit = default(RaycastHit);
            if(Physics.Raycast(ray, out hit, 500f, shotMask))
            {
                if(hit.collider.transform != transform)
                {
                    bool isOrganic = (organicMask == (organicMask | (1 << hit.transform.root.gameObject.layer)));
                    DrawShoot(weapons[weapon].gameObject, hit.point, hit.normal, hit.collider.transform,
                        !isOrganic, !isOrganic);
                    if(hit.collider)
                    {
                        hit.collider.SendMessageUpwards("HitCallBack", new HealthBase.DamageInfo(
                            hit.point, ray.direction, weapons[weapon].bulletDamage, hit.collider),
                            SendMessageOptions.DontRequireReceiver);
                    }

                }
            }
            else
            {
                Vector3 destination = (ray.direction * 500f) - ray.origin;
                DrawShoot(weapons[weapon].gameObject, destination, Vector3.up, null, false, false);
            }

            SoundManager.Instance.PlayOneShotEffect((int)weapons[weapon].shotSound, gunMuzzle.position, 5f);
            GameObject gameContoller = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);
            gameContoller.SendMessage("RootAlertNearBy", ray.origin, SendMessageOptions.DontRequireReceiver);
            shotInterval = originalShoInterval;
            isShotAlive = true;

        }
    }
    
    public void EndReloadWeapon()
    {
        behaviourController.GetAnimator.SetBool(reloadBool, false);
        weapons[activeWeapon].EndReload();
    }

    private void SetWeaponCrossHair(bool armed)
    {
        if(armed)
        {
            aimBehaviour.crossHair = aimCrossHair;
        }
        else
        {
            aimBehaviour.crossHair = originalCrossHair;
        }
    }

    private void ShotProgress()
    {
        if(shotInterval > 0.2f)
        {
            shotInterval -= shootRateFactor * Time.deltaTime;
            if(shotInterval <= 0.4f)
            {
                SetWeaponCrossHair(activeWeapon > 0);
                muzzleFlash.SetActive(false);
                if(activeWeapon > 0)
                {
                    behaviourController.GetCamScript.BounceVertical(-weapons[activeWeapon].recoilAngle * 0.1f);

                    if(shotInterval <= (0.4f - 2f * Time.deltaTime))
                    {
                        if(weapons[activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.AUTO &&
                            Input.GetAxisRaw(ButtonName.Shoot) != 0)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else if(weapons[activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.BURST && 
                            burstShotCount < weapons[activeWeapon].burstSize)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else if(weapons[activeWeapon].weaponMode != InteractiveWeapon.WeaponMode.BURST)
                        {
                            burstShotCount = 0;
                        }
                    }

                }
            }

        }
        else
        {
            isShotAlive = false;
            behaviourController.GetCamScript.BounceVertical(0);
            burstShotCount = 0;
        }
    }

    private void ChangeWeapon(int oldWeapon, int newWeapon)
    {
        if(oldWeapon > 0)
        {
            weapons[oldWeapon].gameObject.SetActive(false);
            gunMuzzle = null;
            weapons[oldWeapon].Toggle(false);
        }

        while(weapons[newWeapon] == null && newWeapon > 0)
        {            
            newWeapon = (newWeapon + 1) % weapons.Count;
        }
        if(newWeapon > 0)
        {
            weapons[newWeapon].gameObject.SetActive(true);
            gunMuzzle = weapons[newWeapon].transform.Find("muzzle");
            weapons[newWeapon].Toggle(true);
        }
        activeWeapon = newWeapon;
        if(oldWeapon != newWeapon)
        {
            behaviourController.GetAnimator.SetTrigger(changeWeaponTrigger);
            behaviourController.GetAnimator.SetInteger(weaponType, weapons[newWeapon] ? (int)weapons[newWeapon].weaponType
                : 0);
        }
        SetWeaponCrossHair(newWeapon > 0);
    }

    private void Update()
    {
        float shootTrigger = Mathf.Abs(Input.GetAxisRaw(ButtonName.Shoot));
        if(shootTrigger > Mathf.Epsilon && !isShooting && activeWeapon > 0 && burstShotCount ==  0)
        {
            isShooting = true;
            ShootWeapon(activeWeapon);
        }
        else if(isShooting && shootTrigger < Mathf.Epsilon)
        {
            isShooting = false;
        }
        else if(Input.GetButtonUp(ButtonName.Reload) && activeWeapon > 0)
        {
            if(weapons[activeWeapon].StartReload())
            {
                SoundManager.Instance.PlayOneShotEffect((int)weapons[activeWeapon].reloadSound,
                    gunMuzzle.position, 0.5f);
                behaviourController.GetAnimator.SetBool(reloadBool, true);
            }
        }else if(Input.GetButtonDown(ButtonName.Drop) && activeWeapon > 0)
        {
            EndReloadWeapon();
            int weaponToDrop = activeWeapon;
            ChangeWeapon(activeWeapon, 0);
            weapons[weaponToDrop].Drop();
            weapons[weaponToDrop] = null;

        }
        else
        {
            if((Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) > Mathf.Epsilon && !isChangingWeapon))
            {
                isChangingWeapon = true;
                int nextWeapon = activeWeapon + 1;
                ChangeWeapon(activeWeapon, nextWeapon % weapons.Count);

            }
            else if(Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) < Mathf.Epsilon)
            {
                isChangingWeapon = false;
            }
        }
        if(isShotAlive)
        {
            ShotProgress();
        }
        isAiming = behaviourController.GetAnimator.GetBool(aimBool);
    }

    /// <summary>
    /// 인벤토리역할을 하게될 함수.
    /// </summary>
    /// <param name="weapon"></param>
    public void AddWeapon(InteractiveWeapon newWeapon)
    {
        newWeapon.gameObject.transform.SetParent(rightHand);
        newWeapon.transform.localPosition = newWeapon.rightHandPosition;
        newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.relativeRotation);

        if(weapons[slotMap[newWeapon.weaponType]])
        {
            if(weapons[slotMap[newWeapon.weaponType]].label_weaponName == newWeapon.label_weaponName)
            {
                weapons[slotMap[newWeapon.weaponType]].ResetBulet();
                ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
                Destroy(newWeapon.gameObject);
                return;
            }
            else
            {
                weapons[slotMap[newWeapon.weaponType]].Drop();
            }
        }

        weapons[slotMap[newWeapon.weaponType]] = newWeapon;
        ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
    }

    private bool CheckforBlockedAim()
    {
        isAimBlocked = Physics.SphereCast(transform.position + castRelativeOrigin, 0.1f,
            behaviourController.GetCamScript.transform.forward, out RaycastHit hit, distToHand - 0.1f);
        isAimBlocked = isAimBlocked && hit.collider.transform != transform;
        behaviourController.GetAnimator.SetBool(blockedAimBool, isAimBlocked);
        Debug.DrawRay(transform.position + castRelativeOrigin,
           behaviourController.GetCamScript.transform.forward * distToHand, isAimBlocked ? Color.red : Color.cyan);
        return isAimBlocked;
    }
    public void OnAnimatorIK(int layerIndex)
    {
        if(isAiming && activeWeapon > 0)
        {
            if(CheckforBlockedAim())
            {
                return;
            }
            Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            targetRot *= Quaternion.Euler(initialRootRotation);
            targetRot *= Quaternion.Euler(initialHipsRotation);
            targetRot *= Quaternion.Euler(initialSpineRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Spine,
                Quaternion.Inverse(hips.rotation) * targetRot);

            float xcamRot = Quaternion.LookRotation(behaviourController.playerCamera.forward).eulerAngles.x;
            targetRot = Quaternion.AngleAxis(xcamRot + armsRotation, this.transform.right);
            if(weapons[activeWeapon] && weapons[activeWeapon].weaponType == InteractiveWeapon.WeaponType.LONG)
            {
                targetRot *= Quaternion.AngleAxis(9f, transform.right);
                targetRot *= Quaternion.AngleAxis(20f, transform.up);
            }
            targetRot *= spine.rotation;
            targetRot *= Quaternion.Euler(initialCheckRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Chest,
                Quaternion.Inverse(spine.rotation) * targetRot);
        }
    }

    private void LateUpdate()
    {
        if(isAiming && weapons[activeWeapon] && 
            weapons[activeWeapon].weaponType == InteractiveWeapon.WeaponType.SHORT)
        {
            leftArm.localEulerAngles = leftArm.localEulerAngles + leftArmShortAim;
        }
    }
}
