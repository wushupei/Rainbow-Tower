using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// 挂移动方块上
/// </summary>
public enum CubeState
{
    Z_Move, //z轴上移动
    X_Move, //x轴上移动
    Pause, //暂停
    Die, //死亡
}
public class CubeMove : MonoBehaviour
{
    bool pause; //是否暂停
    //记录暂停的一瞬间自身信息
    Vector3 pos; //位置
    Vector3 local; //尺寸
    Vector3 euler; //旋转
    CubeState state; //状态

    PedestalManage pedestal; //声明底座管理类
    EffectManage effect = new EffectManage(); //实例化特效管理类,方便生成
    public CubeState cubeState = CubeState.Z_Move; //初始状态在Z轴移动
    Vector3 initialPos; //初始位置
    bool forward; //是否往前走
    Vector3 size; //声明自身尺寸
    public float speed; //平移速度
    Color color; //移动方块颜色

    GameObject top; //声明顶层

    float[] rgb = { 1, 1, 1 }; //声明rgb颜色数组
    int[] rgbIndex = { 0, 1, 2 }; //创建颜色索引数组
    bool reduce = true; //颜色递减

    public Transform mainCamera; //把主摄像机拖进去
    public GameObject texter; //文字显示

    private void Start()
    {
        pedestal = FindObjectOfType<PedestalManage>(); //找到底座管理脚本
        Initialization(); //初始化   
        rgbIndex = RandomIndex(); //将颜色索引顺序打乱
    }
    void Update()
    {
        GamePause(); //暂停方法
        switch (cubeState)
        {
            case CubeState.Pause: //暂停状态时
                if (pause) //pause=true时,往摄像机面前转换
                    Transformation(mainCamera.position + mainCamera.forward * 3, new Vector3(1, 0.1f, 0.5f), new Vector3(50, 45, 0), new Color(1, 1, 1));
                else //pause=false时
                    Transformation(pos, local, euler, color); //往原位置转换
                break;
            case CubeState.Die: //如果变为死亡状态
                GameOver(); //执行死亡方法
                break;
            default:
                TranslationFun(); //平移
                if (Input.GetMouseButtonDown(0)) //按下鼠标左键,移动方块下落
                    FallingFun();
                break;
        }
    }
    void TranslationFun() //平移方法
    {
        switch (cubeState) //根据状态确定移动方式
        {
            case CubeState.Z_Move:
                if (forward) //如果确定往前走
                {
                    transform.Translate(0, 0, Time.deltaTime * speed); //前进
                    if (Mathf.Abs(transform.position.z - initialPos.z) >= 20) //前进超过20m
                    {
                        effect.CtreateAT_Field(gameObject, Vector3.forward, size.z, 0, size.x); //生成特效
                        forward = !forward; //变为后退状态                     
                    }
                }
                else //如果往后退
                {
                    transform.Translate(0, 0, -Time.deltaTime * speed); //后退
                    if (transform.position.z <= initialPos.z)  //如果退回到初始位置
                    { 
                        effect.CtreateAT_Field(gameObject, Vector3.back, size.z, 0, size.x); //生成特效
                        forward = !forward; //又往前走
                    }
                }
                break;

            case CubeState.X_Move:
                if (forward) //如果确定往前走
                {
                    transform.Translate(Time.deltaTime * speed, 0, 0); //前进
                    if (Mathf.Abs(transform.position.x - initialPos.x) >= 20)
                    {
                        effect.CtreateAT_Field(gameObject, Vector3.right, size.x, 90, size.z); //生成特效
                        forward = !forward; //变为后退状态
                    }
                }
                else //后退状态时
                {
                    transform.Translate(-Time.deltaTime * speed, 0, 0); //后退                                                            
                    if (transform.position.x <= initialPos.x) //如果退回到初始位置
                    {
                        effect.CtreateAT_Field(gameObject, Vector3.left, size.x, 90, size.z); //生成特效
                        forward = !forward; //又往前走
                    }
                }
                break;
        }
    }
    void FallingFun() //落下方法
    {
        //如果x轴偏差与z轴其中一个偏差大于自身尺寸,说明没落在顶层上
        if (Mathf.Abs(transform.position.z - top.transform.position.z) >= size.z || Mathf.Abs(transform.position.x - top.transform.position.x) >= size.x)
            cubeState = CubeState.Die; //变成死亡状态
        else
        {
            GameObject newTop = pedestal.CreateNewFoundationAndCffcut(top, gameObject); //生成新顶层和边角料         
            ColorGradualChange(); //颜色渐变
            Reset(newTop); //重置移动方块位置
            effect.CreateSpecialEffect(top); //生成底座特效
        }
    }
    void ColorGradualChange() //颜色渐变
    {
        //逐渐减少r,g,b的值
        if (reduce)
        {
            if (rgb[rgbIndex[0]] > 0)
                rgb[rgbIndex[0]] -= 0.1f;
            else if (rgb[rgbIndex[1]] > 0)
                rgb[rgbIndex[1]] -= 0.1f;
            else if (rgb[rgbIndex[2]] > 0)
                rgb[rgbIndex[2]] -= 0.1f;
            else //当三个值都小于0时,变为逐渐增加
            {
                reduce = !reduce;
                rgbIndex = RandomIndex(); //再次打乱索引顺序
            }
        }
        else //逐渐增加r,g,b的值
        {
            if (rgb[rgbIndex[0]] < 1)
                rgb[rgbIndex[0]] += 0.1f;
            else if (rgb[rgbIndex[1]] < 1)
                rgb[rgbIndex[1]] += 0.1f;
            else if (rgb[rgbIndex[2]] < 1)
                rgb[rgbIndex[2]] += 0.1f;
            else //当三个值都大于1时,变为逐渐减少
            {
                reduce = !reduce;
                rgbIndex = RandomIndex(); //再次打乱索引顺序
            }
        }
    }
    int[] RandomIndex() //将数组内元素顺序随机打乱
    {
        //创建一个集合1,将索引数组的值给它
        List<int> list1 = new List<int>();
        for (int i = 0; i < rgbIndex.Length; i++)
        {
            list1.Add(rgbIndex[i]);
        }
        //再创建一个集合2,把集合1的元素顺序随机打乱
        List<int> list2 = new List<int>();
        for (int i = list1.Count; i > 0; i--)
        {
            int index;
            index = Random.Range(0, i);
            list2.Add(list1[index]);
            list1.Remove(list1[index]);
        }
        //将打乱后的索引成员重新赋值给索引数组
        return list2.ToArray();
    }
    void Reset(GameObject newTop) //重置
    {
        //切换状态
        if (cubeState == CubeState.Z_Move)
            cubeState = CubeState.X_Move;
        else if (cubeState == CubeState.X_Move)
            cubeState = CubeState.Z_Move;

        //启动新顶层(将原顶层的名字给它,原顶层取名为"Pedestal")
        string name = top.name;
        top.name = "Pedestal";
        newTop.name = name;

        //初始化方块
        Initialization();
    }
    void Initialization() //初始化移动方块
    {
        //获取顶层,及顶层的位置
        top = GameObject.Find("TopFloor");
        Vector3 pos = top.transform.position;
        //将顶层的大小赋予自身
        transform.localScale = top.transform.localScale;
        size = transform.localScale; //记录自身大小
        switch (cubeState) //根据状态确定初始位置
        {
            case CubeState.Z_Move:
                transform.position = new Vector3(pos.x, pos.y + size.y, pos.z - 10);
                break;
            case CubeState.X_Move:
                transform.position = new Vector3(pos.x - 10, pos.y + size.y, pos.z);
                break;
        }
        effect.CreateNaissanceEffect(gameObject);//生成诞生特效

        forward = true; //往前走
        initialPos = transform.position; //记录初始位置
        color = new Color(rgb[0], rgb[1], rgb[2]); //实时刷新rgb值
        GetComponent<MeshRenderer>().material.color = color;
    }
    void GamePause() //暂停方法
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //按下Esc键
        {
            if (!pause) //如果pause=false
            {
                if (cubeState != CubeState.Pause) //未进入暂停状态时
                {
                    //显示文字
                    if (!texter.activeInHierarchy)
                        texter.SetActive(true);
                    //记录自身信息(位置,尺寸,旋转,状态)
                    pos = transform.position;
                    local = transform.localScale;
                    euler = transform.eulerAngles;
                    state = cubeState;
                }
                //暂停      
                pause = !pause;
                cubeState = CubeState.Pause;
            }
            else
                pause = !pause;
        }
    }
    void GameOver() //死亡方法
    {
        //添加重力组件
        if (!gameObject.GetComponent<Rigidbody>())
            gameObject.AddComponent<Rigidbody>();
        //炸毁所有底座
        pedestal.AllPedestalBoom();
        //当底座炸完了
        if (pedestal.pedestals.Count == 0)
        {
            //显示文字
            if (!texter.activeInHierarchy)
                texter.SetActive(true);
            //往摄像机面前转换
            Transformation(mainCamera.position + mainCamera.forward * 3, new Vector3(1, 0.1f, 0.5f), new Vector3(50, 45, 0), new Color(1, 1, 1));
        }
    }
    void Transformation(Vector3 pos, Vector3 local, Vector3 euler, Color color) //转换
    {
        //如果有重力组件则删除
        if (gameObject.GetComponent<Rigidbody>())
            Destroy(gameObject.GetComponent<Rigidbody>());
        //移动方块往该处飞行转换
        transform.position = Vector3.Lerp(transform.position, pos, 0.1f);
        transform.localScale = Vector3.Lerp(transform.localScale, local, 0.1f);
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, euler, 0.1f);
        GetComponent<MeshRenderer>().material.color = Color.Lerp(GetComponent<MeshRenderer>().material.color, color, 0.1f);
        //如果快飞到了
        if ((transform.position - pos).magnitude < 0.01f)
        {
            //确定位置尺寸旋转和颜色
            transform.position = pos;
            transform.localScale = local;
            transform.eulerAngles = euler;
            GetComponent<MeshRenderer>().material.color = color;
            //往回转换时,禁用文字还原状态
            if (pause == false && cubeState == CubeState.Pause)
            {
                texter.SetActive(false);
                cubeState = state;               
            }
        }
    }
    public void ComeAgain() //重新开始游戏
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
