using UnityEngine;
using UnityEngine.Video;

public class FreezeOnLastFrame : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        // Subscribe to the loopPointReached event
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Instead of letting it stop/reset, we pause it 
        // This keeps the last frame rendered on the Render Texture
        vp.Pause();
    }
}