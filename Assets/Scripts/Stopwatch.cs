using UnityEngine;

public class Stopwatch
{
    private float startTime = 0f;
    //private float elapsedTime = 0f;

    public float ElapsedTimeSec()
    {
        return Time.time - startTime;
    }

    public void Restart()
    {
        startTime = Time.time;
    }
}