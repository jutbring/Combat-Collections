using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera mainCamera = null;
    [SerializeField] TMP_Text fPSText = null;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 0, 0);
    [SerializeField] float cameraRecoverySpeed = 1f;
    [SerializeField] float shakePowerReduce = 1f;
    [SerializeField] float shakeTime = 1f;
    [SerializeField] float shakePowerMod = 1f;
    [SerializeField] float shakeRotationMod = 1f;
    [SerializeField] bool displaysFps = true;

    Vector3 shakePosition = new Vector3(0, 0, 0);
    Vector3 nextShakePosition = new Vector3(0, 0, 0);
    Vector3 targetPosition = new Vector3(0, 0, 0);
    float targetRotation = 0f;
    float shakeRotation = 0f;

    List<float> deltaTimes = new List<float>();
    float fPSUpdateSpeed = 1f;
    float fPSUpdateTime = 0.5f;
    float fPSUpdateTimer = 0f;

    float lastPower = 0f;
    bool canShake = true;

    void Update()
    {
        GetFrameRate();
        UpdateTargetPosition();
        UpdateCamera();
    }
    void GetFrameRate()
    {
        if (displaysFps)
        {
            deltaTimes.Add(Time.unscaledDeltaTime);
            fPSUpdateTimer = Mathf.Max(fPSUpdateTimer - Time.unscaledDeltaTime, 0);
            float fPSUpdateSpeedScaled = fPSUpdateSpeed / Time.unscaledDeltaTime;
            if (deltaTimes.Count > fPSUpdateSpeedScaled)
            {
                for (int i = 0; i < deltaTimes.Count - fPSUpdateSpeedScaled; i++)
                {
                    deltaTimes.RemoveAt(0);
                }
            }
            if (fPSUpdateTimer == 0)
            {
                fPSUpdateTimer = fPSUpdateTime;
                float frameRate = 0f;
                for (int i = 0; i < deltaTimes.Count; i++)
                {
                    frameRate += 1 / deltaTimes[i];
                }
                frameRate /= deltaTimes.Count;
                if (frameRate <= 999)
                {
                    fPSText.text = frameRate.ToString("###") + " FPS";
                }
                else
                {
                    fPSText.text = "999+ FPS";
                }
            }
        }
        else
        {
            fPSText.text = "";
        }
    }
    void UpdateTargetPosition()
    {
        shakePosition = Vector2.Lerp(shakePosition, nextShakePosition, cameraRecoverySpeed * GameSettings.defaultTimeScale);
        targetPosition = shakePosition + cameraOffset;
        targetRotation = Mathf.Lerp(targetRotation, shakeRotation, cameraRecoverySpeed * GameSettings.defaultTimeScale);
    }
    void UpdateCamera()
    {
        mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, targetPosition, cameraRecoverySpeed * Time.deltaTime * 100 * GameSettings.defaultTimeScale);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(0, 0, targetRotation), cameraRecoverySpeed * Time.deltaTime * 100 * GameSettings.defaultTimeScale);
    }
    public void shakeCamera(float power)
    {
        float scaledPower = power * shakePowerMod;
        if (scaledPower > lastPower && canShake)
        {
            StopAllCoroutines();
            StartCoroutine(Shake(power, scaledPower));
        }
    }
    IEnumerator Shake(float power, float scaledPower)
    {
        int shakeAmount = (int)(scaledPower * 10);
        for (int i = 0; i < shakeAmount; i++)
        {
            if (i == 0)
            {
                canShake = false;
            }
            lastPower = scaledPower;
            float shakeTimeScaled = shakeTime * ((1 + (scaledPower / power) / shakePowerMod));
            float posX = UnityEngine.Random.Range(-1f, 1f);
            float posY = UnityEngine.Random.Range(-1f, 1f);
            float rot = UnityEngine.Random.Range(-1f, 1f);
            nextShakePosition = new Vector2(posX, posY).normalized * scaledPower;
            shakeRotation = rot * scaledPower * shakeRotationMod;
            yield return new WaitForSeconds(shakeTimeScaled);

            nextShakePosition = new Vector2(0, 0);
            shakeRotation = 0;
            yield return new WaitForSeconds(shakeTimeScaled);

            canShake = true;
            scaledPower *= shakePowerReduce;
        }
    }
}
