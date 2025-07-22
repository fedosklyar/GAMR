using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Microsoft.MixedReality.Toolkit.Windows.Utilities;

public class SpeechToTextManager : MonoBehaviour
{
    public TMP_Text comment;
    public GameObject commentUI;

    public GameObject startbtn;
    public GameObject stopbtn;
    bool isRecording = false;

    Vector3 notePos;
    public GameObject noteBtn;

    DictationRecognizer dictationRecognizer;

    public MessageBehavior startmessage;
    public MessageBehavior stopmessage;

    void Awake()
    {
        // comment.text = "Press start and say something to add a note...";
        // The confidence level is Medium be dafault
        // Specified the Low level to increase probability of speech recognition (if the confidence level term understood correctly) 
        dictationRecognizer = new DictationRecognizer(ConfidenceLevel.Low);

        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

        //Introduce the handler of an error
        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        commentUI.SetActive(false);
        startbtn.SetActive(true);
        stopbtn.SetActive(false);
    }

    private void DictationRecognizer_DictationHypothesis(string text)
    {
        Debug.Log("Text within dictation hypothesis --> " + text); 
        // this.comment.text = this.comment.text + text;
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("The DictationResult event is triggered");
        Debug.Log("The text within the event is" + text);
        if (this.comment.text == "Press start and say something to add a note...")
        {
            comment.text = "";
        }
        this.comment.text = this.comment.text + text + " ";
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        Debug.Log("The DictationRecognizer is completed");
        dictationRecognizer.Stop();
    }

    public void SpeechToText()
    {
        // PhraseRecognitionSystem.Shutdown();
        // commentUI.SetActive(true);
        
        // Stops replaying of the recording
        ReplaySystem.rs.StopReplay();
    }

    public void StartSpeechToText()
    {
        Debug.Log("Speech to text entered");
        Debug.Log("Value of isRecording is " + isRecording);
        if (!isRecording)
        {
            Debug.Log("Log within the if of StartSpeecToText");
            // Display start recording message
            startmessage.FadeOut();

            PhraseRecognitionSystem.Shutdown();
            startbtn.SetActive(false);
            stopbtn.SetActive(true);

            this.comment.text = "";

            // RecordIndicator.recordindicator.StartBlink();
            dictationRecognizer.Start();
            Debug.Log("Speech to text started (respective method of the class)");
        }

        isRecording = true;
    }

    public void StopSpeechToText()
    {
        Debug.Log("StopSpeechToTextIsEntered");
        if (isRecording)
        {
            // Display stop recording message
            stopmessage.FadeOut();

            // Restarts replaying of the recording
            ReplaySystem.rs.Replay();

            // RecordIndicator.recordindicator.StopBlink();
            dictationRecognizer.Stop();

            notePos = Camera.main.transform.position;
            GameObject note = Instantiate(noteBtn, new Vector3(notePos.x, 0.05f, notePos.z), Quaternion.identity);
            Debug.Log("The text within comment is --> " + this.comment.text);
            note.GetComponentInChildren<Note>().note = this.comment.text;

            commentUI.SetActive(false);
            startbtn.SetActive(true);
            stopbtn.SetActive(false);
        }

        isRecording = false;
    }
}
