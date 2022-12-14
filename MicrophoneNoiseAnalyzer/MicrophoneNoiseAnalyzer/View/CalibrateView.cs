﻿using MicrophoneNoiseAnalyzer.Domain;
using Sharprompt;

namespace MicrophoneNoiseAnalyzer.View;

public class CalibrateView : MicrophoneView, ICalibrateView
{
    public bool ConfirmInvoke()
    {
        Console.WriteLine();
        Console.WriteLine("マイクのキャリブレーションを開始します。5秒間、一定の音量の発声をしてください。");
        return Prompt.Confirm("開始してよろしいですか？");
    }

    public void NotifyCalibrated(IMicrophones microphones)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("マイクのキャリブレーションを完了しました。");

        for (int i = 0; i < microphones.Devices.Count; i++)
        {
            var microphone = microphones.Devices[i];
            Console.WriteLine($"{i + 1} = {microphone.Name} 入力レベル：{microphone.MasterVolumeLevelScalar}");
        }
    }
}