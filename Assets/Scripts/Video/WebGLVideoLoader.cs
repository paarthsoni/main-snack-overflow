using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class WebGLVideoLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "Instructions_Clip.mp4"; // Name of your video file

    void Start()
    {
        // Build full path to StreamingAssets folder
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        Debug.Log("Loading video from: " + videoPath);

        // Optional: autoplay when ready
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (VideoPlayer vp) => { vp.Play(); };
    }
}