using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class AdSearch : MonoBehaviour
{
    public Material myMaterial;

    public RawImage myRawImage;

    public string myURL;

    // Backend url
    public string adURL = "http://localhost/ad/"; 

    // Api_key(every game will have one)
    public string apiKey = "66d9601d1d03b7fbc16746c1";

    void Awake() 
    {
        // StartCoroutine(RepeatGet());
        StartCoroutine(Render());
    }


    
    IEnumerator Render()
    {
        while (true)
        {
            // Circularly iterating over list present in AudioRecorder.cs
            if (ResponseWrapper.data.Count > 0)
            {
                int index = 0;
                
                while (index < ResponseWrapper.data.Count)
                {
                    ResponseData currentItem = ResponseWrapper.data[index];
                    
                    StartCoroutine(DownloadImageFromURL(currentItem.image));

                    index++;

                    // Add a 15-second delay to keep the ad on the canvas
                    yield return new WaitForSeconds(15f);
                }
            }
            
            yield return new WaitForSeconds(5f); 
        }
    }


    // Downloading image and rendering it
    IEnumerator DownloadImageFromURL(string url) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success) {
            Texture downloadedTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            myMaterial.mainTexture = downloadedTexture;
            myRawImage.texture = downloadedTexture;

        }else {
            Debug.Log(request.error);
        }
    }

    // public class Ad {
    //     public string image;
    // }

    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    // void Update()
    // {
        
    // }

    // public void RenderImage(string image) {
    //     StartCoroutine(DownloadImageFromURL(image));
    // }

    // IEnumerator RepeatGet()
    // {
    //     while (true)
    //     {
    //         yield return GetAds();
    //         yield return new WaitForSeconds(40);
    //     }
    // }

    // IEnumerator GetAds() {
    //     // Adding query param(api) to the backend url
    //     string url = adURL + "?api_key=" + apiKey;

    //     Debug.Log(url);
    //     using(UnityWebRequest request = UnityWebRequest.Get(url)) {

    //         yield return request.SendWebRequest();
    //         if(request.result != UnityWebRequest.Result.Success) {
    //             Debug.Log(request.error);
    //         }else{
    //             // Debug.Log("Ads downloaded successfully");

    //             var text = request.downloadHandler.text;

    //             Ad img = JsonUtility.FromJson<Ad>(text);
    //             // Debug.Log(img.image);
    //             StartCoroutine(DownloadImageFromURL(img.image));

    //         }
    //     }
    // }
}
