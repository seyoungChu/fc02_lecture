using FC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
/// <summary>
/// 마우스 오른쪽버튼으로 조준. 다른 동작을 대체해서 동작하게 됩니다.
/// 마우스 휠버튼으로 좌우 카메라 변경
/// 벽의 모서리에서 조준할때 상체를 살짝 기울여주는 기능.
/// </summary>
public class AimBehaviour : GenericBehaviour
{
    public Texture2D crossHair; //십자선 이미지.
    public float aimTurnSmoothing = 0.15f; //카메라를 향하도록 조준할때 회전속도.
    public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0.0f);
    public Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f);

    private int aimBool; //애니메이터 패러메터. 조준.
    private bool aim; //조준중이냐?.
    private int cornerBool; //애니메이터 관련. 코너.
    private bool peekCorner; //플레이어가 코너 모서리에 있는지 여부.
    private Vector3 initialRootRotation;//루트 본 로컬 회전값.
    private Vector3 initialHipRotation; //
    private Vector3 initialSpineRotation;
    private Transform myTransform;
    private void Start()
    {
        myTransform = transform;
        //setup
        aimBool = Animator.StringToHash(AnimatorKey.Aim);
        cornerBool = Animator.StringToHash(AnimatorKey.Corner);

        //value.
        Transform hips = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipRotation = hips.localEulerAngles;
        initialSpineRotation = behaviourController.GetAnimator.
            GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;

    }
    //카메라에 따라 플레이어를 올바른 방향으로 회전.
    void Rotating()
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f,
            behaviourController.GetCamScript.GetH, 0.0f);
        float minSpeed = Quaternion.Angle(myTransform.rotation, targetRotation) * aimTurnSmoothing;

        if(peekCorner)
        {
            //조준 중일때 플레이어 상체만 살짝 기울여 주기 위함.
            myTransform.rotation = Quaternion.LookRotation(-behaviourController.GetLastDirection());
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipRotation);
            targetRotation *= Quaternion.Euler(initialSpineRotation);
            Transform spine = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation;
        }
        else
        {
            behaviourController.SetLastDirection(forward);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation,
                minSpeed * Time.deltaTime);
        }
    }
    //조준중일때를 관리하는 함수.
    void AimManagement()
    {
        Rotating();
    }
    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);
        //조준이 불가능한 상태일때에 대한 예외처리.
        if(behaviourController.GetTempLockStatus(this.behaviourCode) ||
            behaviourController.IsOverriding(this))
        {
            yield return false;
        }
        else
        {
            aim = true;
            int signal = 1;
            if(peekCorner)
            {
                signal = (int)Mathf.Sign(behaviourController.GetH);
            }
            aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
            yield return new WaitForSeconds(0.1f);
            behaviourController.GetAnimator.SetFloat(speedFloat, 0.0f);
            behaviourController.OverrideWithBehaviour(this);
        }

    }

    private IEnumerator ToggleAimOff()
    {
        aim = false;
        yield return new WaitForSeconds(0.3f);
        behaviourController.GetCamScript.ResetTargetOffsets();
        behaviourController.GetCamScript.ResetMaxVerticalAngle();
        yield return new WaitForSeconds(0.1f);
        behaviourController.RevokeOverridingBehaviour(this);
            
    }
    public override void LocalFixedUpdate()
    {
        if(aim)
        {
            behaviourController.GetCamScript.SetTargetOffset(aimPivotOffset, aimCamOffset);
        }
    }
    public override void LocalLateUpdate()
    {
        AimManagement();
    }
    private void Update()
    {
        peekCorner = behaviourController.GetAnimator.GetBool(cornerBool);
        
        if(Input.GetAxisRaw(ButtonName.Aim) != 0 && !aim)
        {
            StartCoroutine(ToggleAimOn());
        }else if(aim && Input.GetAxisRaw(ButtonName.Aim) == 0)
        {
            StartCoroutine(ToggleAimOff());
        }
        //조준중일때는 달리기를 하지 않습니다.
        canSprint = !aim;
        if(aim && Input.GetButtonDown(ButtonName.Shoulder) && !peekCorner)
        {
            aimCamOffset.x = aimCamOffset.x * (-1);
            aimPivotOffset.x = aimPivotOffset.x * (-1);
        }
        behaviourController.GetAnimator.SetBool(aimBool, aim);
    }
    private void OnGUI()
    {
        if(crossHair != null)
        {
            float length = behaviourController.GetCamScript.
                GetCurrentPivotMagnitude(aimPivotOffset);
            if(length < 0.05f)
            {
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - (crossHair.width * 0.5f),
                    Screen.height * 0.5f - (crossHair.height * 0.5f),
                    crossHair.width, crossHair.height), crossHair);
            }
        }
    }


}
