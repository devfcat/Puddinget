using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 테마의 모드 (추가 가능)
/// </summary>
public enum ThemeMod
{
    Light = 1,
    Dark = 0,
}

/// <summary>
/// UI의 테마 색상을 변경하는 기능이 포함된 클래스
/// </summary>
public class ThemeSwitcher : MonoBehaviour
{
    [Header("UIs")]
    public List<Image> images_UI; // 이미지를 사용하는 UI
    public List<GameObject> text_UI_Light; // 텍스트 오브젝트 (라이트)
    public List<GameObject> text_UI_Dark; // 텍스트 오브젝트 (다크)

    [Header("Sprites")]
    public List<Sprite> sprites_UI_A;     
    public List<Sprite> sprites_UI_B;
    
    [Header("State")]
    public ThemeMod themeMod;

    private static ThemeSwitcher _instance;
    public static ThemeSwitcher Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(ThemeSwitcher)) as ThemeSwitcher;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    public void Awake()
    {
        themeMod = Load_ThemeMod();
        Switing_Theme(themeMod);
    }

    // 기존에 라이트 모드였다면 눌렀을 때 다크모드로,
    // 다크 모드였다면 라이트모드로 전환하는 토글 버튼 기능
    public void Onclick_ToggleTheme()
    {
        ThemeMod thisMod = themeMod;
        if (thisMod == ThemeMod.Light)
        {
            Switing_Theme(ThemeMod.Dark);
        }
        else
        {
            Switing_Theme(ThemeMod.Light);
        }
    }

    public void Onclick_LightMod()
    {
        Switing_Theme(ThemeMod.Light);
    }

    public void Onclick_DarkMod()
    {
        Switing_Theme(ThemeMod.Dark);
    }

    public void Switing_Theme(ThemeMod mod)
    {
        themeMod = mod;

        Locker.Instance.Change_Sprite(themeMod);

        for (int i = 0; images_UI.Count > i; i++)
        {
            if (themeMod == ThemeMod.Light)
            {
                Save_ThemeMod(1);
                images_UI[i].sprite = sprites_UI_B[i];
                //images_UI[i].sprite = sprites_UI_A[i];
            }
            else
            {
                Save_ThemeMod(0);
                images_UI[i].sprite = sprites_UI_A[i];
                //images_UI[i].sprite = sprites_UI_B[i];
            }
        }
        for (int i = 0; text_UI_Light.Count > i; i++)
        {
            if (themeMod == ThemeMod.Light)
            {
                text_UI_Light[i].SetActive(false);
                text_UI_Dark[i].SetActive(true);
            }
            else
            {
                text_UI_Light[i].SetActive(true);
                text_UI_Dark[i].SetActive(false);
            }
        }
    }

    // UI 테마 모드값을 저장함
    public void Save_ThemeMod(int index)
    {
        PlayerPrefs.SetInt("themeMod", index);
        PlayerPrefs.Save();
    }

    // 저장된 UI 테마 모드값을 가져옴
    // 가져온 모드값을 반환함 
    public ThemeMod Load_ThemeMod()
    {
        int mod = PlayerPrefs.GetInt("themeMod");
        if (mod == 0)
        {
            return ThemeMod.Dark;
        }
        else
        {
            return ThemeMod.Light;
        }
    }

}
