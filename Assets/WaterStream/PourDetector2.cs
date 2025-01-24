using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PourDetector2 : MonoBehaviour
{
    public float pourThreshold = 45f;
    public Transform origin = null;
    public ParticleSystem streamPrefab = null;
    public Image WaterGauge;
    public float minWater = 0.0f;
    public float maxWater = 100f; 
    public static float Water;
    public Button beanWaterButton;
    public GameObject beanObject; // Bean 태그를 가진 오브젝트
    public float activationDistance = 0.7f; // 버튼이 눌리는 거리
    public float pourDuration = 5.0f;

    private bool isPouring = false;
    private bool hasPouredOnBean = false; // 현재 물이 나온 상태에서 버튼이 눌렸는지 여부
    private ParticleSystem currentStream = null;
    private Coroutine pourCoroutine = null;

    void Start()
    {
        WaterGauge = GetComponent<Image>();
        Water = minWater;
    }

    private void Update()
    {
        WaterGauge.fillAmount = Water/100f;
        float pourAngle = CalculatePourAngle();
        float distanceToBean = Vector3.Distance(transform.position, beanObject.transform.position);

        if (pourAngle > pourThreshold && !isPouring)
        {
            StartPour();
        }
        else if (pourAngle <= pourThreshold && isPouring)
        {
            EndPour();
        }

        if (isPouring && distanceToBean <= activationDistance && !hasPouredOnBean)
        {
            
            //beanWaterButton.onClick.Invoke();
            hasPouredOnBean = true; // 현재 물이 나온 상태에서 버튼이 눌렸음을 기록
            Debug.Log("Button clicked due to proximity and pouring.");
            if(pourCoroutine == null)
            {
                pourCoroutine = StartCoroutine(FillWaterGauge());
            }
        }
        else if (!isPouring)
        {
            hasPouredOnBean = false; // 물이 멈추면 다시 초기화
        }
    }

    private void StartPour()
    {
        isPouring = true;
        currentStream = CreateStream();
        currentStream.Play();
        Debug.Log("Start pouring.");
    }

    private void EndPour()
    {
        isPouring = false;
        if (currentStream != null)
        {
            currentStream.Stop();
            Destroy(currentStream.gameObject, currentStream.main.duration);
            currentStream = null;
        }
        if (pourCoroutine != null)
        {
            StopCoroutine(pourCoroutine);
            pourCoroutine = null;
        }
        Debug.Log("End pouring.");
    }

    private float CalculatePourAngle()
    {
        // Z축 기준으로 회전한 각도를 반환
        float angle = transform.rotation.eulerAngles.z;
        // 각도가 180도 이상일 경우 보정
        if (angle > 180)
        {
            angle = 360 - angle;
        }
        return angle;
    }

    private ParticleSystem CreateStream()
    {
        var stream = Instantiate(streamPrefab, origin.position, Quaternion.identity, transform);
        var collisionModule = stream.collision;
        collisionModule.enabled = true;
        collisionModule.type = ParticleSystemCollisionType.World;
        collisionModule.sendCollisionMessages = true;
        return stream;
    }
    private IEnumerator FillWaterGauge()
    {
        float elapsed = 0.0f; //경과 시간

        while(elapsed < pourDuration)
        {

            Water = Mathf.Lerp(minWater, maxWater, elapsed / pourDuration);
            elapsed += Time.deltaTime; // 매 프레임 시간 추가
            yield return null; //다음 프레임까지 대기
        }

        Water = maxWater;
        Debug.Log("water fully filled");
    }
}