using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogBox : MonoBehaviour, IPointerClickHandler
{
    public DialogManager dialogManager;     //点击对话框时出现下一条对话
    public GameObject dialogPanel;          //对话界面
    public bool isSelect;                   //是否在选择项

    public void Awake()
    {
        dialogManager = GameObject.Find("DialogManager").GetComponent<DialogManager>();
        dialogPanel = GameObject.Find("DialogPanel");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("点了对话框");
        if (!isSelect && !dialogManager.isScrolling)
        {
            dialogManager.ShowDialogRow();
        }
        else if (!isSelect && dialogManager.isScrolling)
        {
            dialogManager.isScrolling = false;
        }
    }
}
