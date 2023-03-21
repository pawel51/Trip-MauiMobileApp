﻿using Android.Content;
using Android.Speech;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripaui.Abstractions;

namespace Tripaui.Platforms;
public class SpeechToText : ISpeechToText
{
    private SpeechRecognitionListener listener;
    private SpeechRecognizer speechRecognizer;

    public async Task<string> Listen(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken)
    {
        var taskResult = new TaskCompletionSource<string>();
        listener = new SpeechRecognitionListener
        {
            Error = ex => taskResult.TrySetException(new Exception("Failure in speech engine - " + ex)),
            PartialResults = sentence =>
            {
                recognitionResult?.Report(sentence);
            },
            Results = sentence => taskResult.TrySetResult(sentence)
        };
        speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
        if (speechRecognizer is null)
        {
            throw new ArgumentException("Speech recognizer is not available");
        }

        speechRecognizer.SetRecognitionListener(listener);
        speechRecognizer.StartListening(CreateSpeechIntent(culture));
        await using (cancellationToken.Register(() =>
        {
            StopRecording();
            taskResult.TrySetCanceled();
        }))
        {
            return await taskResult.Task;
        }
    }

    private void StopRecording()
    {
        speechRecognizer?.StopListening();
        speechRecognizer?.Destroy();
    }

    private Intent CreateSpeechIntent(CultureInfo culture)
    {
        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, Java.Util.Locale.Default);
        var javaLocale = Java.Util.Locale.ForLanguageTag(culture.Name);
        intent.PutExtra(RecognizerIntent.ExtraLanguage, javaLocale);
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Android.App.Application.Context.PackageName);
        intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);

        //intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 0);
        //intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 0);
        //intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 0);
        //intent.PutExtra(RecognizerIntent.ExtraMaxResults, 0);

        return intent;
    }

    public async Task<bool> RequestPermissions()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        var isAvaiable = SpeechRecognizer.IsRecognitionAvailable(Android.App.Application.Context);
        return status == PermissionStatus.Granted && isAvaiable;
    }
}
