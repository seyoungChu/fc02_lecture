using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///숨을 만한 곳을 찾아주는 컴포넌트
///플레이어보다 멀리있는 건 제외.
/// </summary>
public class CoverLookUp : MonoBehaviour
{
    private List<Vector3[]> allCoverSpots;
    private GameObject[] covers;
    private List<int> coverHashCodes; //cover unity ID;
    private Dictionary<float, Vector3> filteredSpots; //

    private GameObject[] GetObjectsInLayerMask(int layerMask)
    {
        List<GameObject> ret = new List<GameObject>();

        foreach(GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if(go.activeInHierarchy && layerMask == (layerMask | (1 << go.layer)))
            {
                ret.Add(go);
            }
        }
        return ret.ToArray();
    }
    private void ProcessPoint(List<Vector3> vector3s, Vector3 nativePoint, float range)
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(nativePoint, out hit, range, NavMesh.AllAreas))
        {
            vector3s.Add(hit.position);
        }
    }

    private Vector3[] GetSpots(GameObject go, LayerMask obstacleMask)
    {
        List<Vector3> bounds = new List<Vector3>();
        foreach(Collider col in go.GetComponents<Collider>())
        {
            float baseHeight = (col.bounds.center - col.bounds.extents).y;
            float range = 2 * col.bounds.extents.y;

            Vector3 deslocalForward = go.transform.forward * go.transform.localScale.z * 0.5f;
            Vector3 deslocalRight = go.transform.right * go.transform.localScale.x * 0.5f;

            if(go.GetComponent<MeshCollider>())
            {
                float maxBounds = go.GetComponent<MeshCollider>().bounds.extents.z +
                    go.GetComponent<MeshCollider>().bounds.extents.x;
                Vector3 originForward = col.bounds.center + go.transform.forward * maxBounds;
                Vector3 originRight = col.bounds.center + go.transform.right * maxBounds;
                if(Physics.Raycast(originForward, col.bounds.center - originForward, out RaycastHit hit,
                    maxBounds, obstacleMask))
                {
                    deslocalForward = hit.point - col.bounds.center;
                }
                if(Physics.Raycast(originRight, col.bounds.center - originRight, out hit, maxBounds,
                    obstacleMask))
                {
                    deslocalRight = hit.point - col.bounds.center;
                }
            }
            else if(Vector3.Equals(go.transform.localScale, Vector3.one))
            {
                deslocalForward = go.transform.forward * col.bounds.extents.z;
                deslocalRight = go.transform.right * col.bounds.extents.x;
            }
            float edgeFactor = 0.75f;
            ProcessPoint(bounds, col.bounds.center + deslocalRight + deslocalForward * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center + deslocalForward + deslocalRight * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center + deslocalForward, range);
            ProcessPoint(bounds, col.bounds.center + deslocalForward - deslocalRight * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center - deslocalRight + deslocalForward * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center + deslocalRight, range);
            ProcessPoint(bounds, col.bounds.center + deslocalRight - deslocalForward * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center - deslocalForward + deslocalRight * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center - deslocalForward, range);
            ProcessPoint(bounds, col.bounds.center - deslocalForward - deslocalRight * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center - deslocalRight - deslocalForward * edgeFactor, range);
            ProcessPoint(bounds, col.bounds.center - deslocalRight, range);

        }
        return bounds.ToArray();
    }

    public void Setup(LayerMask coverMask)
    {
        covers = GetObjectsInLayerMask(coverMask);

        coverHashCodes = new List<int>();
        allCoverSpots = new List<Vector3[]>();
        foreach(GameObject cover in covers)
        {
            allCoverSpots.Add(GetSpots(cover, coverMask));
            coverHashCodes.Add(cover.GetHashCode());
        }
    }
    //목표물이 경로에 있는 확인, 대상이 각도 안에 있고 지점보다 가까이 있느냐?
    private bool TargetInPath(Vector3 origin, Vector3 spot, Vector3 target, float angle)
    {
        Vector3 dirToTarget = (target - origin).normalized;
        Vector3 dirToSpot = (spot - origin).normalized;

        if(Vector3.Angle(dirToSpot, dirToTarget) <= angle)
        {
            float targetDist = (target - origin).sqrMagnitude;
            float spotDist = (spot - origin).sqrMagnitude;
            return (targetDist <= spotDist);
        }
        return false;
    }
    //가장 가까운 유효한 지점을 찾아줍니다. 거리도 같이 주자.
    private ArrayList FilterSpots(StateController controller)
    {
        float minDist = Mathf.Infinity;
        filteredSpots = new Dictionary<float, Vector3>();
        int nextCoverHash = -1;
        for(int i = 0; i < allCoverSpots.Count; i++)
        {
            if(!covers[i].activeSelf || coverHashCodes[i] == controller.coverHash)
            {
                continue;
            }
            foreach(Vector3 spot in allCoverSpots[i])
            {
                Vector3 vectorDist = controller.personalTarget - spot;
                float searchDist = (controller.transform.position - spot).sqrMagnitude;

                if(vectorDist.sqrMagnitude <= controller.viewRadius * controller.viewRadius && 
                    Physics.Raycast(spot, vectorDist, out RaycastHit hit, vectorDist.sqrMagnitude,
                    controller.generalStats.coverMask))
                {
                        //플레이어가 npc와 스팟 사이에 있지 않은지 확인하고, 보이는 각도의 1/4각을 사용
                        //타겟보다 멀리 있는 건 거른다.
                    if(hit.collider == covers[i].GetComponent<Collider>() &&
                        !TargetInPath(controller.transform.position, spot, controller.personalTarget,
                        controller.viewAngle / 4))
                    {
                        if (!filteredSpots.ContainsKey(searchDist))
                        {
                            filteredSpots.Add(searchDist, spot);
                        }
                        else
                            continue;

                        if(minDist > searchDist)
                        {
                            minDist = searchDist;
                            nextCoverHash = coverHashCodes[i];
                        }
                    }
                }
            }
        }

        ArrayList returnArray = new ArrayList();
        returnArray.Add(nextCoverHash);
        returnArray.Add(minDist);
        return returnArray;
    }

    public ArrayList GetBestCoverSpot(StateController controller)
    {
        ArrayList nextCoverData = FilterSpots(controller);
        int nextCoverHash = (int)nextCoverData[0];
        float minDist = (float)nextCoverData[1];

        ArrayList returnArray = new ArrayList();
        if(filteredSpots.Count == 0)
        {
            returnArray.Add(-1);
            returnArray.Add(Vector3.positiveInfinity);
        }
        else
        {
            returnArray.Add(nextCoverHash);
            returnArray.Add(filteredSpots[minDist]);
        }

        return returnArray;
    }

}
