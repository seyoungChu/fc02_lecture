using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카메라 속성중 중요 속성 하나는 카메라로부터 위치 오프셋 벡터, 피봇 오프셋 벡터
//위치 오프셋 벡터는 충돌 처리용으로 사용하고 피봇 오프셋 벡터는 시선이동에 사용하도록
//충돌체크 : 이중 충돌 체크 기능 ( 캐릭터로부터 카메라 , 카메라로부터 캐릭터사이 )
//사격 반동을 위한 기능
//FOV 변경 기능.
[RequireComponent(typeof(Camera))]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    public Transform player; //player transform 
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f; // 카메라 반응속도.
    public float horizontalAimingSpeed = 6.0f;// 수평 회전 속도.
    public float verticalAimingSpeed = 6.0f; //수적 회전 속도.
    public float maxVerticalAngle = 30.0f; //카메라의 수직 최대 각도.
    public float minVerticalAngle = -60.0f; //카메라의 수직 최소 각도.
    public float recoilAngleBounce = 5.0f;//사격 반동 바운스 값.
    private float angleH = 0.0f; //마우스 이동에 따른 카메라 수평이동 수치.
    private float angleV = 0.0f; //마우스 이동에 따른 카메라 수직 이동 수치.
    private Transform cameraTransform; //트랜스폼 캐싱.
    private Camera myCamera;
    private Vector3 relCameraPos; //플레이어로부터 카메라까지의 벡터.
    private float relCameraPosMag; //플레이어로부터 카메라사이의 거리.
    private Vector3 smoothPivotOffset; //카메라 피봇용 보간용 벡터.
    private Vector3 smoothCamOffset; //카메라 위치용 보간용 벡터.
    private Vector3 targetPivotOffset; //카메라 피봇용 보간용 벡터.
    private Vector3 targetCamOffset; //카메라 위치용 보간용 벡터.
    private float defaultFOV; //기본 시야값.
    private float targetFOV; //타겟 시야값.
    private float targetMaxVerticleAngle;//카메라 수직 최대 각도.
    private float recoilAngle = 0f;//사격 반동 각도.

    public float GetH
    {
        get
        {
            return angleH;
        }
    }

    private void Awake()
    {
        //캐싱
        cameraTransform = transform;
        myCamera = cameraTransform.GetComponent<Camera>();
        //카메라기본 포지션 세팅.
        cameraTransform.position = player.position + Quaternion.identity * pivotOffset +
            Quaternion.identity * camOffset;
        cameraTransform.rotation = Quaternion.identity;

        //카메라와 플레이어간의 상대 벡터, 충돌체크에 사용하기 위함.
        relCameraPos = cameraTransform.position - player.position;
        relCameraPosMag = relCameraPos.magnitude - 0.5f;

        //기본 세팅.
        smoothPivotOffset = pivotOffset;
        smoothCamOffset = camOffset;
        defaultFOV = myCamera.fieldOfView;
        angleH = player.eulerAngles.y;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();
    }
    public void ResetTargetOffsets()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }
    public void ResetFOV()
    {
        this.targetFOV = defaultFOV;
    }
    public void ResetMaxVerticalAngle()
    {
        targetMaxVerticleAngle = maxVerticalAngle;
    }
    public void BounceVertical(float degree)
    {
        recoilAngle = degree;
    }
    public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }

    public void SetFOV(float customFOV)
    {
        this.targetFOV = customFOV;
    }

    bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
    {
        Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);
        if(Physics.SphereCast(checkPos, 0.2f,target - checkPos,out RaycastHit hit, 
            relCameraPosMag))
        {
            if(hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }
    bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight,float maxDistance)
    {
        Vector3 origin = player.position + (Vector3.up * deltaPlayerHeight);
        if(Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
        {
            if(hit.transform != player && hit.transform != transform &&
                !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
    {
        float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
        return ViewingPosCheck(checkPos, playerFocusHeight) &&
            ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
    }
    private void Update()
    {
        //마우스 이동 값.
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
        angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;
        //수직 이동 제한.
        angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticleAngle);
        //수직 카메라 바운스.
        angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);

        //카메라 회전.
        Quaternion camYRotation = Quaternion.Euler(0.0f, angleH, 0.0f);
        Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0.0f);
        cameraTransform.rotation = aimRotation;

        //set FOV
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset; //조존할때 카메라의 오프셋값, 조준할때와평소와다르다.
        for(float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f)
        {
            noCollisionOffset.z = zOffset;
            if(DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, 
                Mathf.Abs(zOffset)) || zOffset == 0f)
            {
                break;
            }
        }
        //Reposition Camera
        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, 
            smooth * Time.deltaTime);
        smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset,
            smooth * Time.deltaTime);

        cameraTransform.position = player.position + camYRotation * smoothPivotOffset +
            aimRotation * smoothCamOffset;

        if(recoilAngle > 0.0f)
        {
            recoilAngle -= recoilAngleBounce * Time.deltaTime;
        }else if(recoilAngle < 0.0f)
        {
            recoilAngle += recoilAngleBounce * Time.deltaTime;
        }

    }

    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }

}
