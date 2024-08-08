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
    void Update()
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
}
