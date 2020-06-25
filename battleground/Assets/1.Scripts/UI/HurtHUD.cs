using Boo.Lang;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class HurtHUD : MonoBehaviour
{
    struct HurtData
    {
        public Transform shotOrigin;
        public Image hurtImg;
    }

    private Transform canvas;
    private GameObject hurtPrefab;
    private float decayFactor = 0.8f;
    private Dictionary<int, HurtData> hurtUIData;
    private Transform player, cam;

    public void Setup(Transform canvas, GameObject hurtPrefab, float decayFactor, Transform player)
    {
        hurtUIData = new Dictionary<int, HurtData>();
        this.canvas = canvas;
        this.hurtPrefab = hurtPrefab;
        this.decayFactor = decayFactor;
        this.player = player;
        cam = Camera.main.transform;
    }

    private void SetRotation(Image hurtUI, Vector3 orientation, Vector3 shotDirecion)
    {
        orientation.y = 0;
        shotDirecion.y = 0;
        float rotation = Vector3.SignedAngle(shotDirecion, orientation, Vector3.up);

        Vector3 newRotation = hurtUI.rectTransform.rotation.eulerAngles;
        newRotation.z = rotation;
        Image hurtImg = hurtUI.GetComponent<Image>();
        hurtImg.rectTransform.rotation = Quaternion.Euler(newRotation);
    }

    private Color GetUpdatedAlpha(Color currentColor, bool reset = false)
    {
        if(reset)
        {
            currentColor.a = 1f;
        }
        else
        {
            currentColor.a -= decayFactor * Time.deltaTime;
        }
        return currentColor;
    }

    public void DrawHurtUI(Transform shotOrigin, int hashID)
    {
        if(hurtUIData.ContainsKey(hashID))
        {
            hurtUIData[hashID].hurtImg.color = GetUpdatedAlpha(hurtUIData[hashID].hurtImg.color, true);
        }else
        {
            GameObject hurtUI = Instantiate(hurtPrefab, canvas);
            SetRotation(hurtUI.GetComponent<Image>(), cam.forward, shotOrigin.position - player.position);
            HurtData data;
            data.shotOrigin = shotOrigin;
            data.hurtImg = hurtUI.GetComponent<Image>();
            hurtUIData.Add(hashID, data);
        }
    }


    private void Update()
    {
        System.Collections.Generic.List<int> toRemoveKeys = new System.Collections.Generic.List<int>();
        foreach(int key in hurtUIData.Keys)
        {
            SetRotation(hurtUIData[key].hurtImg, cam.forward, hurtUIData[key].shotOrigin.position - player.position);
            hurtUIData[key].hurtImg.color = GetUpdatedAlpha(hurtUIData[key].hurtImg.color);
            if(hurtUIData[key].hurtImg.color.a <= 0f)
            {
                toRemoveKeys.Add(key);
            }
        }
        for(int i = 0; i< toRemoveKeys.Count;i++)
        {
            Destroy(hurtUIData[toRemoveKeys[i]].hurtImg.transform.gameObject);
            hurtUIData.Remove(toRemoveKeys[i]);
        }
    }

}
