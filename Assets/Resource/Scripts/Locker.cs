using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Locker : MonoBehaviour
{
    public Puddinget_Manager mainManager;
    public Image thisIMG;
    public Sprite locking;
    public Sprite unLocking;
    public Sprite locking_dark;
    public Sprite unLocking_dark;

    private static Locker _instance;
    public static Locker Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(Locker)) as Locker;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    public void Change_Sprite(ThemeMod mod)
    {
        if (mod == ThemeMod.Light)
        {
            if (mainManager.positionLock)
            {
                thisIMG.sprite = locking;
            }
            else
            {
                thisIMG.sprite = unLocking;
            }
        }
        else
        {
            if (mainManager.positionLock)
            {
                thisIMG.sprite = locking_dark;
            }
            else
            {
                thisIMG.sprite = unLocking_dark;
            }
        }
    }
}
