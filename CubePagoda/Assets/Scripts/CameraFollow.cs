using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 挂摄像机上,时刻盯紧顶层
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public float height; //摄像机与顶层相对高度
    public float followSpeed; //摄像机跟随顶层速度
    public float rotateSpeed; //绕顶层旋转速度  
    public float zoomSpeed; //镜头缩放速度   
    void Update()
    {
        if (GameObject.Find("TopFloor")) //如果场景中有该物体,始终跟随
        {
            //获取顶层
            Transform topFloor = GameObject.Find("TopFloor").transform;
            //摄像机始终指向顶层
            Quaternion dir = Quaternion.LookRotation(topFloor.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, dir, Time.deltaTime * 5);

            //摄像机跟随顶层高度
            Vector3 cameraPos = transform.position; //获取摄像机坐标
            cameraPos.y = topFloor.position.y + height; //摄像机时刻比顶层高出一定高度(跟着顶层上升)
            transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * followSpeed); //摄像机渐变到指定高度

            //按住鼠标右键移动进行绕顶层旋转
            if (Input.GetMouseButton(1))
            {
                float h = Input.GetAxis("Mouse X");//获取鼠标水平移动
                transform.RotateAround(topFloor.position, Vector3.up, h * Time.deltaTime * rotateSpeed);
            }

            //使用鼠标滑条拉近拉远镜头
            float slider = Input.GetAxis("Mouse ScrollWheel"); //获取滑条滚动信息
            GetComponent<Camera>().fieldOfView -= slider * Time.deltaTime * zoomSpeed;
        }
    }
}
