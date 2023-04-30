using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePip : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI pipText;

    Vector3 endPosition;

    float zPos = -0.5f;  // keep the pip in front of everything

    public bool running = false;

    float timeoutTimer = 0.0f;
    public float timeoutWaitTime = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeSelf && !running)
        {
            // broken state, time it out.
            // timeout
            timeoutTimer += Time.deltaTime;
            if (timeoutTimer > timeoutWaitTime)
            {
                running = false;
                timeoutTimer = timeoutTimer - timeoutWaitTime;
            }
        }
    }

    public ScorePip score(Vector3 start, string score)
    {
        transform.position = new Vector3(start.x, start.y, zPos);
        endPosition = start + new Vector3(0, 1, 0);
        pipText.SetText(score.ToString());
        return this;
    }

    public ScorePip run()
    {
        running = true;
        StartCoroutine(runPip());
        return this;
    }

    private IEnumerator runPip()
    {
        while(running)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, 1 * Time.deltaTime);
            if (transform.position == endPosition)
            {
                pipText.SetText("");
                running = false;
            }
            yield return null;
        }
        yield return null;
    }
}
