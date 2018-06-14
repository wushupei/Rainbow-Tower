using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 不用挂,移动方块被创建后会自动添加上
/// </summary>
public enum CubeState
{
    Z_Move, //z轴上移动
    X_Move, //x轴上移动
    Die, //死亡
}
public class CubeMove : MonoBehaviour
{
    CubeState cubeState = CubeState.Z_Move; //初始状态在Z轴移动
    Vector3 initialPos; //初始位置
    bool forward; //是否往前走
    Vector3 size; //声明自身尺寸
    [HideInInspector]
    public float speed; //平移速度

    GameObject topFloor; //顶层
    Vector3 pSize; //顶层大小
    Vector3 pPos; //顶层坐标

    float[] rgb = { 1, 1, 1 }; //声明rgb颜色数组
    int[] rgbIndex = { 0, 1, 2 }; //创建颜色索引数组
    bool reduce = true; //颜色递减

    Text score;//声明分数UI
    private void OnEnable()
    {
        score = GameObject.Find("Scroe").GetComponent<Text>();
        Initialization(); //初始化   
        rgbIndex = RandomIndex(); //将索引顺序打乱
    }
    void Update()
    {
        switch (cubeState)
        {
            case CubeState.Die: //如果变为死亡状态
                GameOver(); //执行死亡方法
                break;
            default:
                TranslationFun(); //平移
                if (Input.GetMouseButtonDown(0))
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
                    if (Mathf.Abs(transform.position.z - initialPos.z) >= 20)
                    {
                        CtreateAT_Field(Vector3.forward, size.z, 0);
                        forward = !forward; //变为后退状态                     
                    }
                }
                else //如果往后退
                {
                    transform.Translate(0, 0, -Time.deltaTime * speed); //后退
                    if (transform.position.z <= initialPos.z)  //如果退回到初始位置
                    {
                        CtreateAT_Field(Vector3.back, size.z, 0);
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
                        CtreateAT_Field(Vector3.right, size.x, 90);
                        forward = !forward; //变为后退状态
                    }
                }
                else //后退状态时
                {
                    transform.Translate(-Time.deltaTime * speed, 0, 0); //后退                                                            
                    if (transform.position.x <= initialPos.x) //如果退回到初始位置
                    {
                        CtreateAT_Field(Vector3.left, size.x, 90);
                        forward = !forward; //又往前走
                    }
                }
                break;
        }
    }
    void FallingFun() //落下方法
    {
        switch (cubeState)
        {
            case CubeState.Z_Move:
                if (Mathf.Abs(transform.position.z - topFloor.transform.position.z) < size.z)//如果和顶层z轴偏差小于自身z轴尺寸,说明落在了顶层上
                    CreateNewFoundationAndCffcut(); //生成新顶层和边角料
                else //没落上去立即狗带
                    cubeState = CubeState.Die;
                break;
            case CubeState.X_Move:
                if (Mathf.Abs(transform.position.x - topFloor.transform.position.x) < size.x)//如果和顶层x轴偏差小于自身x轴尺寸,说明落在了顶层上
                    CreateNewFoundationAndCffcut(); //生成新顶层和边角料
                else
                    cubeState = CubeState.Die;
                break;
        }
    }
    //生成新顶层和边角料(大小和位置受Cube和原顶层的大小位置影响)
    void CreateNewFoundationAndCffcut()
    {
        //加分
        score.text = (System.Convert.ToInt32(score.text) + 1).ToString();
        //创建一个Cube做新顶层
        GameObject newTopFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //加载一个边角料预制件
        GameObject cffcut = Instantiate(Resources.Load("Prefab/Cffcut") as GameObject);
        cffcut.AddComponent<DeleteCffcut>(); //添加边角料脚本
        //新顶层和边角料以及分数UI的颜色来自移动方块
        Color color = GetComponent<MeshRenderer>().material.color;
        newTopFloor.GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g, color.b);
        cffcut.GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g, color.b);
        score.color = new Color(color.r, color.g, color.b);

        //获取移动方块与原顶层z轴和x轴的位置偏差
        float z_Offset = Mathf.Abs(transform.position.z - pPos.z);
        float x_Offset = Mathf.Abs(transform.position.x - pPos.x);

        //新顶层大小(原顶层-偏差)
        newTopFloor.transform.localScale = new Vector3(Mathf.Abs(pSize.x - x_Offset), pSize.y, Mathf.Abs(pSize.z - z_Offset));
        //新顶层位置和边角料大小及位置(受移动状态影响)
        switch (cubeState)
        {
            case CubeState.Z_Move:
                //边角料大小(原顶层x值,原顶层厚度,偏差值)
                cffcut.transform.localScale = new Vector3(pSize.x, pSize.y, z_Offset);
                if (transform.position.z < pPos.z) //如果z轴值小于原顶层z轴
                {
                    //新顶层位置(原顶层x值, 原顶层y值 + 原顶层厚度, 原顶层z值 - 偏差值/2)
                    newTopFloor.transform.position = new Vector3(pPos.x, pPos.y + pSize.y, pPos.z - z_Offset / 2);
                    //边角料位置(原顶层x值,原顶层y值 + 原顶层厚度,原顶层z值-(原顶层z值/2+偏差值/2))
                    cffcut.transform.position = new Vector3(pPos.x, pPos.y + pSize.y, pPos.z - (pSize.z / 2 + z_Offset / 2));
                }
                else//否则
                {
                    //新底座位置(原顶层x值, 原顶层y值 + 原顶层厚度, 原顶层z值 + 偏差值/2)
                    newTopFloor.transform.position = new Vector3(pPos.x, pPos.y + pSize.y, pPos.z + z_Offset / 2);
                    //边角料位置(原顶层x值,原顶层y值 + 原顶层厚度,原顶层z值+(原顶层z值/2+偏差值/2))
                    cffcut.transform.position = new Vector3(pPos.x, pPos.y + pSize.y, pPos.z + (pSize.z / 2 + z_Offset / 2));
                }
                break;
            case CubeState.X_Move:
                //边角料大小(偏差值,原顶层厚度,原顶层z值)
                cffcut.transform.localScale = new Vector3(x_Offset, pSize.y, pSize.z); //边角料大小
                if (transform.position.x < pPos.x) //如果x轴值小于原顶层x轴
                {
                    //新底座位置(原顶层x值-偏差值/2,原顶层y值+原顶层厚度,原顶层z值)
                    newTopFloor.transform.position = new Vector3(pPos.x - x_Offset / 2, pPos.y + pSize.y, pPos.z);
                    //边角料位置(原顶层x值-(原顶层x值/2+偏差值/2),原顶层y值 + 原顶层厚度,原顶层z值)
                    cffcut.transform.position = new Vector3(pPos.x - (pSize.x / 2 + x_Offset / 2), pPos.y + pSize.y, pPos.z);
                }
                else //否则
                {
                    //新底座位置(原顶层x值+偏差值/2,原顶层y值+原顶层厚度,原顶层z值)
                    newTopFloor.transform.position = new Vector3(pPos.x + x_Offset / 2, pPos.y + pSize.y, pPos.z);
                    //边角料位置(原顶层x值+(原顶层x值/2+偏差值/2),原顶层y值 + 原顶层厚度,原顶层z值)
                    cffcut.transform.position = new Vector3(pPos.x + (pSize.x / 2 + x_Offset / 2), pPos.y + pSize.y, pPos.z);
                }
                break;
        }
        ColorGradualChange(); //颜色渐变
        Reset(newTopFloor); //重置
        CreateSpecialEffect(); //生成特效
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

    void Reset(GameObject newTopFloor) //重置
    {
        //切换状态
        if (cubeState == CubeState.Z_Move)
            cubeState = CubeState.X_Move;
        else if (cubeState == CubeState.X_Move)
            cubeState = CubeState.Z_Move;

        //启动新顶层(将原顶层的名字给它,原顶层取名为"Pedestal")
        string name = topFloor.name;
        topFloor.name = "Pedestal";
        newTopFloor.name = name;

        //初始化方块
        Initialization();
    }
    void Initialization() //初始化移动方块
    {
        //获取顶层,及顶层的大小和位置
        topFloor = GameObject.Find("TopFloor");
        pSize = topFloor.transform.localScale;
        pPos = topFloor.transform.position;
        //将顶层的大小赋予自身
        transform.localScale = pSize;
        switch (cubeState) //根据状态确定初始位置
        {
            case CubeState.Z_Move:
                transform.position = new Vector3(pPos.x, pPos.y + pSize.y, pPos.z - 10);

                break;
            case CubeState.X_Move:
                transform.position = new Vector3(pPos.x - 10, pPos.y + pSize.y, pPos.z);
                break;
        }


        forward = true; //往前走
        initialPos = transform.position; //记录初始位置
        size = transform.localScale; //记录自身大小
        GetComponent<MeshRenderer>().material.color = new Color(rgb[0], rgb[1], rgb[2]); //实时rgb值
        CreateNaissanceEffect();//生成诞生特效

    }

    void CreateSpecialEffect() //生成落下特效(生成在顶层下面)
    {
        //获取顶层的坐标
        Vector3 pos = topFloor.transform.position;
        //获取顶层的大小
        Vector3 size = topFloor.transform.localScale;
        //获取顶层的颜色
        Color color = topFloor.GetComponent<MeshRenderer>().material.color;
        //获取顶层的平面对角线长度
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2));
        //加载创建特效
        GameObject specialEffect = Resources.Load("Prefab/SpecialEffect") as GameObject;
        specialEffect = Instantiate(specialEffect, pos + Vector3.down * size.y / 2, Quaternion.identity);
        specialEffect.GetComponent<ParticleSystem>().startColor = color;
        specialEffect.GetComponent<ParticleSystem>().startSize = diagonal * 2;
    }
    void CreateNaissanceEffect()//生成诞生特效
    {
        //加载诞生特效(尺寸受移动方块尺寸影响)
        GameObject naissanceEffect = Resources.Load("Prefab/NaissanceEffect2") as GameObject;
        naissanceEffect = Instantiate(naissanceEffect, transform.position, Quaternion.identity);
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2));//获取移动方块的平面对角线长度
        naissanceEffect.GetComponent<ParticleSystem>().startSize = diagonal * 1.5f;
    }
    void CtreateAT_Field(Vector3 direction, float Offset, float angle)//生成AT力场特效
    {
        //加载生成AT力场特效(尺寸受移动方块尺寸影响)
        GameObject aT_Field = Instantiate(Resources.Load("Prefab/AT_Field") as GameObject);
        //生成位置
        aT_Field.transform.position = transform.position + direction * Offset / 2;
        //旋转方向(把角度转为弧度)
        aT_Field.GetComponent<ParticleSystem>().startRotation3D = new Vector3(0, angle * Mathf.Deg2Rad, 0);
        //尺寸
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2));//获取移动方块的平面对角线长度
        aT_Field.GetComponent<ParticleSystem>().startSize = diagonal * 1.5f;
    }
    void GameOver()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
            print("死亡");
        }
    }

}
