using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ImageSequenceManager : MonoBehaviour
{
    [Tooltip("persistentDataPath/image-sequences/?")]
    [SerializeField]
    private string _imgSeqDirectoryName;

    private List<ImageSequence> _imageSequences = new();
    [SerializeField]
    private int _expectedAmountOfSequences;

    private Stopwatch _timeMeasurer;

    private bool _areSequencesFilled = false;

    // Start is called before the first frame update
    void Start()
    {
        _timeMeasurer = new Stopwatch();
        _timeMeasurer.Start();
        UnityEngine.Debug.Log("start loading");
        //LoadImgSequencesWithTasks();
        LoadImgSequencesWithWww();
    }

    // Update is called once per frame
    void Update()
    {
        if (_imageSequences.Count > 0)
        {
            _areSequencesFilled = true;
            _imageSequences.ForEach(imageSequence =>
            {
                if (imageSequence._sequence.Any(s => s == null))
                {
                    _areSequencesFilled = false;
                }
            });
        }
        if (_imageSequences.Count >= _expectedAmountOfSequences && _areSequencesFilled)
        {

            UnityEngine.Debug.Log($"done, took {_timeMeasurer.ElapsedMilliseconds} ms");
            _timeMeasurer.Stop();
        }
    }

    private async void GetImageSequencesWithTasks(string dir, List<ImageSequence> imageSequenceResult)
    {
        var loader = new ImageSequenceLoader();

        imageSequenceResult.Add(await loader.LoadImageSequenceInFolder(dir));
    }

    private void LoadImgSequencesWithTasks()
    {
        string rootFolderPath = Application.persistentDataPath + "/" + "image-sequences" + "/" + _imgSeqDirectoryName;

        string[] subdirectories = Directory.GetDirectories(rootFolderPath);

        foreach (string subdir in subdirectories)
        {
            GetImageSequencesWithTasks(subdir, _imageSequences);
        }
    }

    private void LoadImgSequencesWithWww()
    {
        string rootFolderPath = Application.persistentDataPath + "/" + "image-sequences" + "/" + _imgSeqDirectoryName;

        string[] subdirectories = Directory.GetDirectories(rootFolderPath);

        foreach (string subdir in subdirectories)
        {
            GetImageSequencesWithWww(subdir, _imageSequences);
        }
    }

    private void GetImageSequencesWithWww(string dir, List<ImageSequence> imageSequenceResult)
    {
        string[] filePaths = Directory.GetFiles(dir);

        Texture2D[] imageSequence = new Texture2D[filePaths.Length];


        for (int i = 0; i < filePaths.Length; i++)
        {
            StartCoroutine(LoadImageWithWww(filePaths[i], i, imageSequence));
        }

        imageSequenceResult.Add(new ImageSequence(imageSequence));
    }

    private IEnumerator LoadImageWithWww(string filePath, int i, Texture2D[] imageSequence)
    {
        if (!File.Exists(filePath))
        {
            yield break;
        }
        var www = UnityWebRequestTexture.GetTexture("file://" + filePath);
        yield return www.SendWebRequest();

        var texture = DownloadHandlerTexture.GetContent(www);
        imageSequence[i] = texture;
    }
}
