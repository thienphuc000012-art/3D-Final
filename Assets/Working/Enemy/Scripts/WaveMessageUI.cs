using UnityEngine;
using TMPro; 
using System.Collections;
public class WaveMessageUI : MonoBehaviour
{
    public TMP_Text messageText;     
    public float displayDuration = 3f; 
    public float fadeSpeed = 2f;       

    private Coroutine currentRoutine;

    void Awake()
    {
        if (messageText != null)
        {
            messageText.alpha = 0f; 
        }
    }


    public void ShowMessage(string msg)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(DisplayMessageRoutine(msg));
    }

    private IEnumerator DisplayMessageRoutine(string msg)
    {
        messageText.text = msg;


        while (messageText.alpha < 1f)
        {
            messageText.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }


        yield return new WaitForSeconds(displayDuration);


        while (messageText.alpha > 0f)
        {
            messageText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        currentRoutine = null;
    }
}