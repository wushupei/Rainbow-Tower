using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 底座管理类,挂Pedestals上
/// </summary>
public class PedestalManage : MonoBehaviour
{
    public List<Transform> pedestals = new List<Transform>();//创建一个集合用来管理所有底座
    float timer = 0.1f;
    void Start()
    {
        pedestals.Add(GameObject.Find("TopFloor").transform);//首先将顶层添加进集合       
    }
    //生成新顶层和边角料(大小和位置受原顶层的大小和移动方块的位置影响)
    public GameObject CreateNewFoundationAndCffcut(GameObject top, GameObject moveCube)
    {
        //获取顶层位置
        Vector3 tPos = top.transform.position;
        //获取顶层大小
        Vector3 tSize = top.transform.localScale;

        //获取移动方块位置
        Vector3 mPos = moveCube.transform.position;
        //获取移动方块颜色
        Color mColor = moveCube.GetComponent<MeshRenderer>().material.color;
        //获取移动方块的状态
        CubeState state = moveCube.GetComponent<CubeMove>().cubeState;

        //创建一个Cube做新顶层
        GameObject newTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //加载一个边角料预制件
        GameObject cffcut = Instantiate(Resources.Load("Prefab/Cffcut") as GameObject);
        //新顶层和边角料的颜色均来自移动方块
        newTop.GetComponent<MeshRenderer>().material.color = new Color(mColor.r, mColor.g, mColor.b);
        cffcut.GetComponent<MeshRenderer>().material.color = new Color(mColor.r, mColor.g, mColor.b);

        //获取移动方块与原顶层z轴和x轴的位置偏差
        float z_Offset = Mathf.Abs(mPos.z - tPos.z);
        float x_Offset = Mathf.Abs(mPos.x - tPos.x);

        //新顶层大小(原顶层-偏差)
        newTop.transform.localScale = new Vector3(Mathf.Abs(tSize.x - x_Offset), tSize.y, Mathf.Abs(tSize.z - z_Offset));
        //新顶层位置和边角料大小及位置(受移动状态影响)
        switch (state)
        {
            case CubeState.Z_Move:
                //边角料大小(原顶层x值,原顶层厚度,偏差值)
                cffcut.transform.localScale = new Vector3(tSize.x, tSize.y, z_Offset);
                if (mPos.z < tPos.z) //如果z轴值小于原顶层z轴
                {
                    //新顶层位置(原顶层x值, 原顶层y值 + 原顶层厚度, 原顶层z值 - 偏差值/2)
                    newTop.transform.position = new Vector3(tPos.x, tPos.y + tSize.y, tPos.z - z_Offset / 2);
                    //边角料位置(原顶层x值,原顶层y值 + 原顶层厚度,原顶层z值-(原顶层z值/2+偏差值/2))
                    cffcut.transform.position = new Vector3(tPos.x, tPos.y + tSize.y, tPos.z - (tSize.z / 2 + z_Offset / 2));
                }
                else//否则
                {
                    //新底座位置(原顶层x值, 原顶层y值 + 原顶层厚度, 原顶层z值 + 偏差值/2)
                    newTop.transform.position = new Vector3(tPos.x, tPos.y + tSize.y, tPos.z + z_Offset / 2);
                    //边角料位置(原顶层x值,原顶层y值 + 原顶层厚度,原顶层z值+(原顶层z值/2+偏差值/2))
                    cffcut.transform.position = new Vector3(tPos.x, tPos.y + tSize.y, tPos.z + (tSize.z / 2 + z_Offset / 2));
                }
                break;
            case CubeState.X_Move:
                //边角料大小(偏差值,原顶层厚度,原顶层z值)
                cffcut.transform.localScale = new Vector3(x_Offset, tSize.y, tSize.z); //边角料大小
                if (mPos.x < tPos.x) //如果x轴值小于原顶层x轴
                {
                    //新底座位置(原顶层x值-偏差值/2,原顶层y值+原顶层厚度,原顶层z值)
                    newTop.transform.position = new Vector3(tPos.x - x_Offset / 2, tPos.y + tSize.y, tPos.z);
                    //边角料位置(原顶层x值-(原顶层x值/2+偏差值/2),原顶层y值 + 原顶层厚度,原顶层z值)
                    cffcut.transform.position = new Vector3(tPos.x - (tSize.x / 2 + x_Offset / 2), tPos.y + tSize.y, tPos.z);
                }
                else //否则
                {
                    //新底座位置(原顶层x值+偏差值/2,原顶层y值+原顶层厚度,原顶层z值)
                    newTop.transform.position = new Vector3(tPos.x + x_Offset / 2, tPos.y + tSize.y, tPos.z);
                    //边角料位置(原顶层x值+(原顶层x值/2+偏差值/2),原顶层y值 + 原顶层厚度,原顶层z值)
                    cffcut.transform.position = new Vector3(tPos.x + (tSize.x / 2 + x_Offset / 2), tPos.y + tSize.y, tPos.z);
                }
                break;
        }
        AddScore(mColor); //加分
        pedestals.Add(newTop.transform); //将新顶层也添加进集合
        newTop.transform.parent = transform; //将新顶层作为自身子物体

        return newTop;
    }
    void AddScore(Color cubeColor) //加分方法
    {
        Text score = GameObject.Find("Scroe").GetComponent<Text>();
        score.text = (System.Convert.ToInt32(score.text) + 1).ToString();
        score.color = new Color(cubeColor.r, cubeColor.g, cubeColor.b); //分数颜色随移动cube
    }
    public void AllPedestalBoom() //所有底座爆炸
    {
        timer += Time.deltaTime; //0.05秒执行一次
        if (timer > 0.05f)
            if (pedestals.Count > 0)
            {
                Transform top = pedestals[pedestals.Count - 1]; //获取顶层
                if (pedestals.Count > 1) //如果底座有两层以上
                    pedestals[pedestals.Count - 2].name = top.name;//把顶层名字给下一层

                //在顶层位置创建爆炸特效
                GameObject boomEffect = Resources.Load("Prefab/Boom3") as GameObject;
                boomEffect = Instantiate(boomEffect);
                boomEffect.transform.position = top.position;

                //将顶层从集合中移除并销毁
                pedestals.Remove(top);
                Destroy(top.gameObject);
                timer = 0; //计时清零
            }
    }
}
