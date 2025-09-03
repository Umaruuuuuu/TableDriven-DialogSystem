using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    [Header("�Ի�ϵͳ���")]
    public TextAsset dialogDataFile;    //�Ի��ı��ļ�
    private int dialogIndex;            //��ǰ�Ի�������ֵ
    public string[] dialogRows;        //�Ի��ı����зָ�

    //�Ի����� - UI
    private GameObject dialogPanel;     //�Ի�����
    private DialogBox dialogBox;        //�Ի���
    private Image imageLeft;            //�������
    private Image imageMiddle;          //�м�����
    private Image imageRight;           //�Ҳ�����

    private GameObject optionButton;    //ѡ�ť��Ԥ����
    private Transform buttonGroup;      //ѡ�ť�ĸ��ڵ�

    //�ı����� - UI
    private TMP_Text nameText;
    private TMP_Text dialogText;

    [Header("��ɫͼƬ�б�")]
    public List<Sprite> characterSprites = new List<Sprite>();
    Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>(); //��ɫ���ֶ�ӦͼƬ���ֵ�

    [Header("�ı��������")]
    public bool isScrolling;                                                //�Ի���ǰ״̬ - �Ƿ����ڹ���
    public float textSpeed = 0.05f;                                         //��������ٶ�

    [Header("���浭�뵭��")]
    public float waitTimes = 10f;                                           //���뵭��ʱ��
    private char lastNameMiddle = ' ';                                      //��һ��ͼƬ�����ֵĵ�һ���֣����ڼ���Ƿ���Ҫ���뵭������ʼ��Ϊ��һ�δ򿪶�Ϊ��
    private char lastNameRight = ' ';

    public void Awake()
    {
        //��ȡ���
        dialogPanel = GameObject.Find("DialogPanel");
        dialogBox = GameObject.Find("DialogBox").GetComponent<DialogBox>();
        imageLeft = dialogPanel.transform.Find("ImageLeft").GetComponent<Image>();
        imageMiddle = dialogPanel.transform.Find("ImageMiddle").GetComponent<Image>();
        imageRight = dialogPanel.transform.Find("ImageRight").GetComponent<Image>();

        optionButton = Resources.Load<GameObject>("Prefab/OptionButton");
        buttonGroup = dialogPanel.transform.Find("SelectButtonGroups");

        nameText = dialogPanel.transform.Find("DialogBox/NameBox/Name").GetComponent<TMP_Text>();
        dialogText = dialogPanel.transform.Find("DialogBox/DialogTxt").GetComponent<TMP_Text>();
    }

    public void Start()
    {
        ReadText(dialogDataFile);                   //��ȡ�������
        InitImage();                                //��ʼ���������
        InitState();                                //��ʼ���Ի�����

        dialogIndex = 0;
        ShowDialogRow();
    }

    public void ReadText(TextAsset _textAsset)
    {
        dialogRows = _textAsset.text.Split('\n');   //�û��з����ָ�ı����ÿһ�У�����dialogRows��
    }

    public void InitImage()
    {
        imageDic.Clear();

        foreach (Sprite sprite in characterSprites)
        {
            if (sprite != null)
            {
                imageDic[sprite.name] = sprite;
            }
        }
    }

    public void InitState()
    {
        nameText.text = "";
        dialogText.text = "";
        imageLeft.sprite = imageDic["͸��"];
        imageMiddle.sprite = imageDic["͸��"];
        imageRight.sprite = imageDic["͸��"];
    }

    public void OnEnable()
    {
        ShowDialogRow();//�Ի����汻����ʱ�����¶Ի���
    }

    public IEnumerator UpdateImage(string _name, string location)
    {

        switch (location)
        {
            case "��":
                imageLeft.sprite = imageDic[_name]; //ֱ�ӻ�ͼ (������ͬһ����)

                imageLeft.color = Color.white;      //������ǰ����˵������
                imageRight.color = Color.gray;
                imageMiddle.color = Color.gray;

                break;
            case "��":
                if (_name[0] != lastNameMiddle)//���ո�˵���Ĳ���ͬһ����
                {
                    yield return StartCoroutine(FadeToClear(imageMiddle));//�ȵ���
                    imageMiddle.sprite = imageDic[_name];                 //�ٻ�ͼ

                    imageRight.color = Color.gray;                        
                    imageLeft.color = Color.gray;

                    yield return StartCoroutine(FadeToWhite(imageMiddle));//�ٵ���

                    imageMiddle.color = Color.white;
                }
                else
                {
                    //���ո�˵������ͬһ���ˣ�ֱ�ӻ�ͼ
                    imageMiddle.sprite = imageDic[_name];

                    imageRight.color = Color.gray;
                    imageLeft.color = Color.gray;
                    imageMiddle.color = Color.white;
                }

                lastNameMiddle = _name[0];
                break;
            case "��":
                if (_name[0] != lastNameRight)
                {
                    yield return StartCoroutine(FadeToClear(imageRight));
                    imageRight.sprite = imageDic[_name];

                    imageLeft.color = Color.gray;
                    imageMiddle.color = Color.gray;

                    yield return StartCoroutine(FadeToWhite(imageRight));

                    imageRight.color = Color.white;
                }
                else
                {
                    imageRight.sprite = imageDic[_name];

                    imageLeft.color = Color.gray;
                    imageMiddle.color = Color.gray;
                    imageRight.color = Color.white;
                }

                lastNameRight = _name[0];
                break;
            case "��":
                imageLeft.color = Color.gray;
                imageMiddle.color = Color.gray;
                imageRight.color = Color.gray;
                break;
        }

    }

    public void ShowDialogRow()
    {
        for (int i = 1; i < dialogRows.Length; i++)
        {
            //�ö��ŷָ�ÿһ�е�ÿһ����Ԫ��
            string[] cells = dialogRows[i].Split(',');

            //�������ƥ���ϣ���ô��ִ����һ��
            if(int.Parse(cells[1]) == dialogIndex)
            {
                if (cells[0] == "#")//�������ͨ��˳��Ի�
                {
                    StartCoroutine(ScrollingText(cells[2], cells[5]));          //�����ı�
                    StartCoroutine(UpdateImage(cells[2] + cells[3], cells[4])); //������������
                    dialogIndex = int.Parse(cells[6]);                          //���µ�ǰ����
                    dialogBox.isSelect = false;                                 //���µ�ǰ�ı�״̬Ϊ˳��Ի�(��ѡ��)
                    
                }
                else if (cells[0] == "&")//�����ѡ����
                {
                    dialogBox.isSelect = true;                                  //��ת��ѡ����� -> ����״̬Ϊѡ����
                    GenerateOption(i);                                          //����ѡ��
                }
                else if (cells[0] == "END")//����ǶԻ�������
                {
                    dialogIndex = int.Parse(cells[6]);                          //���µ�ǰ����
                    dialogPanel.SetActive(false);                               //�رնԻ�����
                    InitState();                                                //��ʼ���Ի�����
                }

                break;                                                          //�ҵ�Ҫִ�е���֮������ѭ��
            }

        }
    }

    //������������ѡ�ť
    public void GenerateOption(int _index)
    {
        string[] cells = dialogRows[_index].Split(',');
        if (cells[0] == "&")
        {
            GameObject button = Instantiate(optionButton, buttonGroup);

            //�󶨵���¼�
            button.GetComponentInChildren<TMP_Text>().text = cells[5];
            button.GetComponent<Button>().onClick.AddListener
            (
                //�½�һ��ί��
                delegate
                {
                    //������һ����ת�ĶԻ���ִ����ת
                    OnOptionClick(int.Parse(cells[6]));
                }
            );

            //������֮���ж���һ���ǲ���Ҳ��ѡ�ť���ǵĻ��ͼ�������
            GenerateOption(_index + 1);
        }

    }

    //�����ѡ���Ժ�
    public void OnOptionClick(int _id)
    {
        dialogIndex = _id;  //������һ������
        ShowDialogRow();    //չʾ��һ��

        //���ٰ�ť
        for (int i = 0; i < buttonGroup.childCount; i++)
        {
            Destroy(buttonGroup.GetChild(i).gameObject);
        }
    }

    //��ɫ���潥��   
    public IEnumerator FadeToClear(Image characterIma)
    {
        float alpha;
        if (characterIma.color.a == 0f || characterIma.sprite.name == "touming")
        {
            alpha = 0f;
        }
        else
        {
            alpha = 1f;
        }

        while (alpha >= 0f)
        {
            alpha -= Time.deltaTime * waitTimes;
            Color c = characterIma.color;
            c.a = alpha;
            characterIma.color = c;
            yield return null;//Э�̻������ﱻ��ͣ��ֱ����һ֡�����ѡ�
        }
    }

    //��ɫ���潥�� 
    public IEnumerator FadeToWhite(Image liHui)
    {
        float alpha;
        if (liHui.color.a == 1f)
        {
            alpha = 1f;
        }
        else
        {
            alpha = 0f;
        }

        while (alpha <= 1f)
        {
            alpha += Time.deltaTime * waitTimes;
            Color c = liHui.color;
            c.a = alpha;
            liHui.color = c;
            yield return null;//Э�̻������ﱻ��ͣ��ֱ����һ֡�����ѡ�
        }
    }

    //ʵ�����ֹ���
    private IEnumerator ScrollingText(string _name, string _text)
    {
        isScrolling = true;

        nameText.text = _name;
        dialogText.text = "";

        foreach (char letter in _text.ToCharArray())
        {
            if (!isScrolling)
            {
                dialogText.text = _text;
                break;
            }
            dialogText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isScrolling = false;
    }
}
