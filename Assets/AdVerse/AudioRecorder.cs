using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

// Json response expecting from the backend
[System.Serializable]
public class ResponseData
{
    public string id; 
    public string name;
    public string image;
}

// AdSearch.cs is accessing this list
public class ResponseWrapper
{
    public static List<ResponseData> data = new List<ResponseData>(); // Initialize the list
}

public class AudioRecorder : MonoBehaviour
{
    private string backendUrl = "http://127.249.0.1:5000/save-record-unity";
    private AudioClip audioClip;
    private string filePath;
    AdSearch ad;

    void Start()
    {
        ad = FindObjectOfType<AdSearch>();
        // Schedule recording and sending every 5 seconds
        InvokeRepeating("RecordAndSendAudio", 0f, 5f);
    }

    void RecordAndSendAudio()
    {
        Debug.Log("Recording has started");
        // Start recording
        audioClip = Microphone.Start(null, false, 5, 44100); // Record for 5 seconds
        filePath = Path.Combine(Application.persistentDataPath, "audio.wav");

        // Stop recording after 5 seconds and proceed to process the file
        Invoke("StopRecording", 5f);
    }

    void StopRecording()
    {
        // Stop recording
        Microphone.End(null);

        // Export to .wav file (you need to implement or import this functionality)
        byte[] wavFileData = WavUtility.FromAudioClip(audioClip); // Custom method to create a .wav file byte array
        File.WriteAllBytes(filePath, wavFileData);

        Debug.Log("Saved audio file to: " + filePath);

        // Send the audio file to the backend
        StartCoroutine(SendAudioToBackend(filePath));
    }

    IEnumerator SendAudioToBackend(string filePath)
    {
        // Create a form and add the file data
        WWWForm form = new WWWForm();

        // Read the file as bytes
        byte[] fileData = File.ReadAllBytes(filePath);
        form.AddBinaryData("file", fileData, "audio.wav", "audio/wav");

        // Send the request to the backend
        using (UnityWebRequest www = UnityWebRequest.Post(backendUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Audio file upload complete!");

                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                if (jsonResponse != "[]")
                {
                    // Parse the JSON data
                    List<ResponseData> responseArray = JsonConvert.DeserializeObject<List<ResponseData>>(jsonResponse);

                    foreach (var item in responseArray)
                    {
                        Debug.Log($"Name: {item.name}");
                        ResponseWrapper.data.Add(item); // Corrected from 'add' to 'Add'

                        // Ensure ad is not null before using
                        // if (ad != null)
                        // {
                        //     ad.RenderImage(item.image);
                        // }
                    }
                }
            }
        }
    }
}

public static class WavUtility
{
    // Convert an AudioClip to WAV byte array
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            WriteWavFile(stream, clip);
            return stream.ToArray();
        }
    }

    // Write WAV file data to a stream
    private static void WriteWavFile(Stream stream, AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        int byteRate = clip.frequency * clip.channels * 2; // 16-bit audio
        int headerSize = 44; // Standard WAV header size

        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // Write WAV header
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(36 + sampleCount * 2); // Chunk size
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Subchunk1 size
            writer.Write((short)1); // Audio format (1 = PCM)
            writer.Write((short)clip.channels); // Number of channels
            writer.Write(clip.frequency); // Sample rate
            writer.Write(byteRate); // Byte rate
            writer.Write((short)(clip.channels * 2)); // Block align
            writer.Write((short)16); // Bits per sample
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(sampleCount * 2); // Data chunk size

            // Write audio data
            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);
            foreach (float sample in samples)
            {
                short intSample = (short)(sample * short.MaxValue);
                writer.Write(intSample);
            }
        }
    }
}
