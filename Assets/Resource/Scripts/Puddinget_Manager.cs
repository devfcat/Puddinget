using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Device;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// 레퍼런스: https://wizemean.tistory.com/14
/// </summary>

#region 상태 정의
public enum appState
{
    Menu = 0,
    Processing = 1,
    Error,
}

public enum animation_List
{
    Default = 0,
    Rotation = 1,
    Jumping = 2
}
#endregion

public class Puddinget_Manager : MonoBehaviour
{
    public void Awake()
    {
        UnityEngine.Application.runInBackground = true;
        UnityEngine.Application.targetFrameRate = 30;
        Init();
    }

    public struct MARGINS
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }
    
    OpenFileDialog OpenDialog;
    Stream openStream = null;

 
    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
 
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
 
    [DllImport("user32.dll")]
    public static extern int BringWindowToTop(IntPtr hwnd);
 
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
 
    [DllImport("Dwmapi.dll")]
    public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
 
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
 
    IntPtr hWnd;
    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;
 
    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSTPARENT = 0x00000020;
 
    void Start()
    {
        screenWidth = UnityEngine.Screen.width;
        screenHeight = UnityEngine.Screen.height;

        /*
        hWnd = GetActiveWindow();
 
        MARGINS margins = new MARGINS { leftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
 
        BringWindowToTop(hWnd);
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
        */
    }

#region 구동에 필요한 변수들

    [Header("제어 변수")]
    public bool isError = false;
    public bool positionLock = true; 
    public bool isDrag = false;
    public float minSize = 0.1f;
    public float maxSize = 3f;
    public float nowSize = 1f;
    public int nowAnimation = 0;


    [Header("커스텀 이미지")]
    public Image puddinget_IMG = null;
    public Sprite default_IMG; // 기본 이미지 (푸딩젯)
    public Sprite main_Texture; // 실제 위젯에 적용할 이미지
    public string url_IMG; // 이미지 경로


    [Header("오브젝트")]
    public GameObject puddinget; // 푸딩젯 메인 오브젝트
    public RectTransform puddinget_RectTransform; // 푸딩젯 트랜스폼
    public GameObject puddinget_Main;
    public RectTransform puddinget_MainRectTransform;

    [Header("설정창")]
    public Slider sizeSlider;
    public int isCustomImageUsed = 0;

    [Header("사용자 환경변수")]
    private Vector2 mousePos;

    public GameObject panel_menu;
    public GameObject panel_process;

    public RectTransform screen_UI;

    public float screenWidth;
    public float screenHeight;

#endregion

    public void Init()
    {
        if (screen_UI.anchoredPosition.y != screenHeight || screen_UI.anchoredPosition.x != screenWidth)
        {
#if !UNITY_EDITOR
            isError = true;
            return;
#endif
        }
        
        SetState(appState.Menu);
        
        LoadIMGFileInfo();
        SetIMG_Init();
        Load_Slider();
        Load_Scale();
        Load_Animation();
        puddinget_RectTransform.anchoredPosition = Vector2.zero;
    }

    public void Update()
    {

#region 이미지 오버레이
        //BringWindowToTop(hWnd);
        //SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
        //SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSTPARENT);
#endregion

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.Application.Quit();
        }
#endif

        // 에러 처리
        if (isError)
        {
            SetState(appState.Error);
        }

        // 포지션 락을 풀고 드래그하면 이미지 위치 조정 가능
        if (isDrag && !positionLock)
        {
            mousePos = Input.mousePosition;
            puddinget_RectTransform.anchoredPosition 
                = new Vector2(mousePos.x - screenWidth*0.5f, mousePos.y - screenHeight*0.5f);
        }

        /*
        if (Input.GetKeyUp(KeyCode.Space))
        {
            toggle = !toggle;
 
            BringWindowToTop(hWnd);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
            
            if (toggle)
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
            }
            else
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSTPARENT);
            }
        }
        */
    }

    public void Onclick_Start()
    {
        SetState(appState.Processing);
    }

#region 이미지 불러오기

    public void Onclick_ImportIMG()
    {
        OpenDialog = new OpenFileDialog();
        OpenDialog.Filter = "Custom Image (*.jpg, *.png, *.gif) | *.jpg; *.png; *.gif; | All files  (*.*)|*.*";
        OpenDialog.FilterIndex = 1;
        OpenDialog.Title = "Image Dialog";

        url_IMG = FileOpen();
        if (!string.IsNullOrEmpty(url_IMG))
        {
            SaveIMGFile();
            SetIMG();
        }
    }

    public string FileOpen()
    {
        if (OpenDialog.ShowDialog() == DialogResult.OK)
        {
            if ((openStream = OpenDialog.OpenFile()) != null)
            {
            	openStream.Close();
                return OpenDialog.FileName;
            }
        }
        return null;
    }

    public void SetIMG()
    {
        byte[] byteTexture2D = System.IO.File.ReadAllBytes(url_IMG);
        if (byteTexture2D.Length > 0)
        {
            int width; int height;
            width = 4096; height = 4096;
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(byteTexture2D);
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            main_Texture = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            SetIMG_custom();
        }
        else
        {
            SetIMG_default();
        }
    }

    public void SetIMG_Init()
    {
        if (isCustomImageUsed != 1)
        {
            SetIMG_default();
        }
        else
        {
            SetIMG();
        }
    }

    public void SetIMG_custom()
    {
        if (main_Texture != null)
        {
            puddinget_IMG.sprite = main_Texture;
            Debug.Log("Imported Image exists");
            PlayerPrefs.SetInt("customIMGUsed", 1);
            isCustomImageUsed = PlayerPrefs.GetInt("customIMGUsed");
        }
        else
        {
            SetIMG_default();
        }
    }

    public void SetIMG_default()
    {
        puddinget_IMG.sprite = default_IMG;
        PlayerPrefs.SetInt("customIMGUsed", 0);
        isCustomImageUsed = PlayerPrefs.GetInt("customIMGUsed");
        Resizing_Reset();
    }

    public void LoadIMGFileInfo()
    {
        string url;
        try
        {
            url = PlayerPrefs.GetString("url_file");
            isCustomImageUsed = PlayerPrefs.GetInt("customIMGUsed");
        }
        catch
        {
            url = "";
            isCustomImageUsed = 0;
        }

        if (url != "" && url != null)
        {
            url_IMG = url;
        }
        else
        {
            url_IMG = "";
        }
        Debug.Log(url_IMG);
    }

    public void SaveIMGFile()
    {
        string url = url_IMG;
        Debug.Log(url_IMG);
        PlayerPrefs.SetString("url_file", url);
    }
#endregion

#region 앱 상태 관리
    public void SetState(appState state)
    {
        switch (state)
        {
            case appState.Menu:
                panel_menu.SetActive(true);
                panel_process.SetActive(true);
                break;
            case appState.Processing:
                panel_menu.SetActive(false);
                panel_process.SetActive(true);
                break;
            case appState.Error:
#if UNITY_EDITOR
                SetState(appState.Menu);
#else
                panel_menu.SetActive(false);
                panel_process.SetActive(false);
#endif
                break;
            default:
                panel_menu.SetActive(true);
                panel_process.SetActive(true);
                break;
        }
    }
#endregion

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
    }

#region 이미지 사이즈 조절
    public void Load_Slider()
    {
        sizeSlider.value = nowSize;
        sizeSlider.minValue = minSize;
        sizeSlider.maxValue = maxSize;
    }

    public void Resizing()
    {
        nowSize = sizeSlider.value;
        puddinget_MainRectTransform.localScale = new Vector2(sizeSlider.value, sizeSlider.value);
        Save_scale();
    }

    public void Resizing_Reset()
    {
        nowSize = 1f;
        puddinget_MainRectTransform.localScale = new Vector2(1f, 1f);
        Load_Slider();
        Save_scale();
    }

    public void Save_scale()
    {
        PlayerPrefs.SetFloat("scale", nowSize);
    }

    public void Load_Scale()
    {
        float scale; 

        try
        {
            scale = PlayerPrefs.GetFloat("scale");
        }
        catch
        {
            scale = 1f;
        }

        if (scale < 0.1f)
        {
            scale = 1f;
        }

        nowSize = scale;
        puddinget_MainRectTransform.localScale = new Vector2(nowSize, nowSize);
    }
#endregion

#region 이미지 애니메이션 설정
    public void Select_Animation(animation_List index)
    {
        switch (index)
        {
            case (animation_List)0:
                break;
            case (animation_List)1:
                break;
            case (animation_List)2:
                break;
            default:
                break;
        }
    }

    public void Save_Animation()
    {
        PlayerPrefs.SetInt("animation", nowAnimation);
    }

    public void Load_Animation()
    {
        int animation;
        try
        {
            animation = PlayerPrefs.GetInt("animation");
        }
        catch
        {
            animation = 0;
        }

        Select_Animation((animation_List)animation);
    }
#endregion

#region 설정 관련 메서드
    // 메인 메뉴 패널과 설정이 통합됨 (20240806)
    /*
    public void Toggle_Popup()
    {
        popup_Setting.SetActive(!popup_Setting.activeSelf);

        if (popup_Setting.activeSelf)
        {
            //Load_Slider(Slider sizeSlider);
        }
    }
    */

    public void Toggle_Lock()
    {
        positionLock = !positionLock;
    
        if (positionLock)
        {
            // 자물쇠 스프라이트 활성화
            Debug.Log("Position Lock");
        }
        else
        {
            // 자물쇠 열림 스프라이트 활성화
            Debug.Log("Position UnLock");
        }
    }

    public void Onclick_MainMenu()
    {
        Load_Slider();
        SetState(appState.Menu);
    }

    void Exit_Program()
    {
        UnityEngine.Application.Quit();
    }
#endregion

}
