using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSkin : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinMats;

    public Texture skinTextureAwal;
    public Texture skinTextureGanti;

    public bool isChangedSkin;

    public Button sdButton;
    public Button smpButton;

    public Sprite useSprite;
    public Sprite selectedSprite;

    private void Update()
    {
        if (isChangedSkin)
        {
            for (int i = 0; i < skinMats.Length; i++)
            {
                skinMats[i].material.mainTexture = skinTextureGanti;
            }
        }
        else
        {
            for (int i = 0; i < skinMats.Length; i++)
            {
                skinMats[i].material.mainTexture = skinTextureAwal;
            }
        }
    }

    public void SkinOn()
    {
        isChangedSkin = true;
        sdButton.GetComponent<Image>().sprite = useSprite;
        smpButton.GetComponent<Image>().sprite = selectedSprite;
    }

    public void SkinOff()
    {
        isChangedSkin = false;
        sdButton.GetComponent<Image>().sprite = selectedSprite;
        smpButton.GetComponent<Image>().sprite = useSprite;
    }
}
