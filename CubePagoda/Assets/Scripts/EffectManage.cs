using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 不用挂,外部来调用它的方法即可
/// </summary>
public class EffectManage : MonoBehaviour
{
    public void CreateSpecialEffect(GameObject topFloor) //生成落下特效(生成在顶层下面)
    {
        //获取顶层的坐标
        Vector3 pos = topFloor.transform.position;
        //获取顶层的大小
        Vector3 size = topFloor.transform.localScale;
        //获取顶层的颜色
        Color color = topFloor.GetComponent<MeshRenderer>().material.color;
        //获取顶层的平面对角线长度
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2));

        //加载创建特效,位置大小颜色随顶层
        GameObject specialEffect = Resources.Load("Prefab/SpecialEffect") as GameObject;
        specialEffect = Instantiate(specialEffect, pos + Vector3.down * size.y / 2, Quaternion.identity);
        specialEffect.GetComponent<ParticleSystem>().startColor = color;
        specialEffect.GetComponent<ParticleSystem>().startSize = diagonal * 2;
    }
    public void CreateNaissanceEffect(GameObject moveCube)//生成诞生特效
    {
        //获取移动方块坐标
        Vector3 pos = moveCube.transform.position;
        //获取移动方块大小 
        Vector3 size = moveCube.transform.localScale;
        //获取移动方块对角线长度
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2) + Mathf.Pow(size.z, 2));

        //加载诞生特效(尺寸受移动方块尺寸影响)
        GameObject naissanceEffect = Resources.Load("Prefab/NaissanceEffect2") as GameObject;
        naissanceEffect = Instantiate(naissanceEffect, pos, Quaternion.identity);    
        naissanceEffect.GetComponent<ParticleSystem>().startSize = diagonal * 2;
    }
    public void CtreateAT_Field(GameObject moveCube, Vector3 direction, float Offset, float angle, float axis)//生成AT力场特效
    {
        //获取移动方块坐标
        Vector3 pos = moveCube.transform.position;
        //获取移动方块大小 
        Vector3 size = moveCube.transform.localScale;
        //获取移动方块的前面对角线长度
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.y, 2) + Mathf.Pow(axis, 2));

        //加载生成AT力场特效(尺寸受移动方块尺寸影响)
        GameObject aT_Field = Instantiate(Resources.Load("Prefab/AT_Field") as GameObject);
        //生成位置
        aT_Field.transform.position = pos + direction * Offset / 2;
        //旋转方向(把角度转为弧度)
        aT_Field.GetComponent<ParticleSystem>().startRotation3D = new Vector3(0, angle * Mathf.Deg2Rad, 0);
        //尺寸    
        aT_Field.GetComponent<ParticleSystem>().startSize = diagonal * 2;
    }
}
