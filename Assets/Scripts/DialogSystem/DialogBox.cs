using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogBox : MonoBehaviour, IPointerClickHandler
{
    public DialogManager dialogManager;     //����Ի���ʱ������һ���Ի�
    public GameObject dialogPanel;          //�Ի�����
    public bool isSelect;                   //�Ƿ���ѡ����

    public void Awake()
    {
        dialogManager = GameObject.Find("DialogManager").GetComponent<DialogManager>();
        dialogPanel = GameObject.Find("DialogPanel");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("���˶Ի���");
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
