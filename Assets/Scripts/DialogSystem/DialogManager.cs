using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    [Header("对话系统相关")]
    public TextAsset dialogDataFile;    //对话文本文件
    private int dialogIndex;            //当前对话的索引值
    public string[] dialogRows;        //对话文本按行分割

    //对话界面 - UI
    private GameObject dialogPanel;     //对话界面
    private DialogBox dialogBox;        //对话框
    private Image imageLeft;            //左侧立绘
    private Image imageMiddle;          //中间立绘
    private Image imageRight;           //右侧立绘

    private GameObject optionButton;    //选项按钮的预制体
    private Transform buttonGroup;      //选项按钮的父节点

    //文本控制 - UI
    private TMP_Text nameText;
    private TMP_Text dialogText;

    [Header("角色图片列表")]
    public List<Sprite> characterSprites = new List<Sprite>();
    Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>(); //角色名字对应图片的字典

    [Header("文本字体滚动")]
    public bool isScrolling;                                                //对话框当前状态 - 是否正在滚动
    public float textSpeed = 0.05f;                                         //字体滚动速度

    [Header("立绘淡入淡出")]
    public float waitTimes = 10f;                                           //淡入淡出时间
    private char lastNameMiddle = ' ';                                      //上一个图片的名字的第一个字，用于检测是否需要淡入淡出，初始化为第一次打开都为空
    private char lastNameRight = ' ';

    public void Awake()
    {
        //获取组件
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
        ReadText(dialogDataFile);                   //读取解析表格
        InitImage();                                //初始化立绘参数
        InitState();                                //初始化对话界面

        dialogIndex = 0;
        ShowDialogRow();
    }

    public void ReadText(TextAsset _textAsset)
    {
        dialogRows = _textAsset.text.Split('\n');   //用换行符来分割开文本里的每一行，存入dialogRows中
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
        imageLeft.sprite = imageDic["透明"];
        imageMiddle.sprite = imageDic["透明"];
        imageRight.sprite = imageDic["透明"];
    }

    public void OnEnable()
    {
        ShowDialogRow();//对话界面被激活时，更新对话行
    }

    public IEnumerator UpdateImage(string _name, string location)
    {

        switch (location)
        {
            case "左":
                imageLeft.sprite = imageDic[_name]; //直接换图 (主角是同一个人)

                imageLeft.color = Color.white;      //高亮当前正在说话的人
                imageRight.color = Color.gray;
                imageMiddle.color = Color.gray;

                break;
            case "中":
                if (_name[0] != lastNameMiddle)//跟刚刚说话的不是同一个人
                {
                    yield return StartCoroutine(FadeToClear(imageMiddle));//先淡入
                    imageMiddle.sprite = imageDic[_name];                 //再换图

                    imageRight.color = Color.gray;                        
                    imageLeft.color = Color.gray;

                    yield return StartCoroutine(FadeToWhite(imageMiddle));//再淡出

                    imageMiddle.color = Color.white;
                }
                else
                {
                    //跟刚刚说话的是同一个人，直接换图
                    imageMiddle.sprite = imageDic[_name];

                    imageRight.color = Color.gray;
                    imageLeft.color = Color.gray;
                    imageMiddle.color = Color.white;
                }

                lastNameMiddle = _name[0];
                break;
            case "右":
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
            case "旁":
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
            //用逗号分割每一行的每一个单元格
            string[] cells = dialogRows[i].Split(',');

            //如果索引匹配上，那么就执行这一行
            if(int.Parse(cells[1]) == dialogIndex)
            {
                if (cells[0] == "#")//如果是普通的顺序对话
                {
                    StartCoroutine(ScrollingText(cells[2], cells[5]));          //更新文本
                    StartCoroutine(UpdateImage(cells[2] + cells[3], cells[4])); //更新人物立绘
                    dialogIndex = int.Parse(cells[6]);                          //更新当前索引
                    dialogBox.isSelect = false;                                 //更新当前文本状态为顺序对话(非选择)
                    
                }
                else if (cells[0] == "&")//如果是选择项
                {
                    dialogBox.isSelect = true;                                  //跳转到选项界面 -> 更新状态为选择项
                    GenerateOption(i);                                          //生成选项
                }
                else if (cells[0] == "END")//如果是对话结束处
                {
                    dialogIndex = int.Parse(cells[6]);                          //更新当前索引
                    dialogPanel.SetActive(false);                               //关闭对话界面
                    InitState();                                                //初始化对话界面
                }

                break;                                                          //找到要执行的行之后跳出循环
            }

        }
    }

    //传入行数生成选项按钮
    public void GenerateOption(int _index)
    {
        string[] cells = dialogRows[_index].Split(',');
        if (cells[0] == "&")
        {
            GameObject button = Instantiate(optionButton, buttonGroup);

            //绑定点击事件
            button.GetComponentInChildren<TMP_Text>().text = cells[5];
            button.GetComponent<Button>().onClick.AddListener
            (
                //新建一个委托
                delegate
                {
                    //传入下一个跳转的对话，执行跳转
                    OnOptionClick(int.Parse(cells[6]));
                }
            );

            //生成完之后判断下一行是不是也是选项按钮，是的话就继续生成
            GenerateOption(_index + 1);
        }

    }

    //当点击选项以后
    public void OnOptionClick(int _id)
    {
        dialogIndex = _id;  //更新下一行索引
        ShowDialogRow();    //展示下一行

        //销毁按钮
        for (int i = 0; i < buttonGroup.childCount; i++)
        {
            Destroy(buttonGroup.GetChild(i).gameObject);
        }
    }

    //角色立绘渐隐   
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
            yield return null;//协程会在这里被暂停，直到下一帧被唤醒。
        }
    }

    //角色立绘渐显 
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
            yield return null;//协程会在这里被暂停，直到下一帧被唤醒。
        }
    }

    //实现文字滚动
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
