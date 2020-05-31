using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기를 획득하면 획득한 무기를 UI를 통해보여주고
/// 현재 잔탄량과 전체 소지할수 있는 총알량을 출력.
/// </summary>
public class WeaponUIManager : MonoBehaviour
{
    public Color bulletColor = Color.white;
    public Color emptyBulletColor = Color.black;
    private Color noBulletColor; //투명하게 색깔표시.

    [SerializeField] private Image weaponHUD;
    [SerializeField] private GameObject bulletMag;
    [SerializeField] private Text totalBulletsHUD;


    // Start is called before the first frame update
    void Start()
    {
        noBulletColor = new Color(0f, 0f, 0f, 0f);
        if(weaponHUD == null)
        {
            weaponHUD = transform.Find("WeaponHUD/Weapon").GetComponent<Image>();
        }
        if(bulletMag == null)
        {
            bulletMag = transform.Find("WeaponHUD/Data/Mag").gameObject;
        }
        if(totalBulletsHUD == null)
        {
            totalBulletsHUD = transform.Find("WeaponHUD/Data/bulletAmount").GetComponent<Text>();
        }
        Toggle(false);
    }
    public void Toggle(bool active)
    {
        weaponHUD.transform.parent.gameObject.SetActive(active);
    }

    public void UpdateWeaponHUD(Sprite weaponSprite, int bulletLeft, int fullMag, int ExtraBullets)
    {
        if (weaponSprite != null && weaponHUD.sprite != weaponSprite)
        {
            weaponHUD.sprite = weaponSprite;
            weaponHUD.type = Image.Type.Filled;
            weaponHUD.fillMethod = Image.FillMethod.Horizontal;
        }
        int bulletCount = 0;
        foreach (Transform bullet in bulletMag.transform)
        {
            //잔탄..
            if (bulletCount < bulletLeft)
            {
                bullet.GetComponent<Image>().color = bulletColor;
            }
            else if (bulletCount >= fullMag)
            {   //넘치는 탄.
                bullet.GetComponent<Image>().color = noBulletColor;
            }
            else
            {   //사용한 탄.
                bullet.GetComponent<Image>().color = emptyBulletColor;
            }
            bulletCount++;
        }
        totalBulletsHUD.text = bulletLeft + "/" + ExtraBullets;
    }
}
