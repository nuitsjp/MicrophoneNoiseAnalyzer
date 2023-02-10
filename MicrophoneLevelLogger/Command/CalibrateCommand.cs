﻿using MicrophoneLevelLogger.Domain;

namespace MicrophoneLevelLogger.Command;

public class CalibrateCommand : ICommand
{
    private const string DirectoryName = "Calibrate";

    private readonly IAudioInterfaceProvider _audioInterfaceProvider;
    private readonly ICalibrateView _view;

    public CalibrateCommand(IAudioInterfaceProvider audioInterfaceProvider, ICalibrateView view)
    {
        _audioInterfaceProvider = audioInterfaceProvider;
        _view = view;
    }

    public string Name => "Calibrate            : マイクの入力レベルを調整する。";

    public Task ExecuteAsync()
    {
        // すべてのマイクを取得する。
        using var microphones = _audioInterfaceProvider.Resolve();

        // 起動時情報を通知する。
        _view.NotifyMicrophonesInformation(microphones);

        // リファレンスマイクを選択する
        var reference = _view.SelectReference(microphones);

        // 調整対象のマイクを選択する
        var target = _view.SelectTarget(microphones, reference);

        // Recordingディレクトリを作成する
        if (Directory.Exists(DirectoryName))
        {
            Directory.Delete(DirectoryName, true);
        }

        Directory.CreateDirectory(DirectoryName);

        // マイクを有効化する
        microphones.ActivateMicrophones();

        // 画面に入力レベルを通知する。
        //_view.StartNotifyMasterPeakValue(microphones);

        // マイクレベルを順番にキャリブレーションする
        Calibrate(reference, target);

        // 画面の入力レベル通知を停止する。
        //_view.StopNotifyMasterPeakValue();

        _view.NotifyCalibrated(microphones);

        // マイクを無効化する
        microphones.DeactivateMicrophones();

        return Task.CompletedTask;
    }

    private void Calibrate(IMicrophone reference, IMicrophone target)
    {
        // ボリューム調整していくステップ
        MasterVolumeLevelScalar step = new(0.005f);

        Console.WriteLine(target);

        // ターゲットの入力レベルをMaxにする
        target.MasterVolumeLevelScalar = MasterVolumeLevelScalar.Maximum;

        // ターゲット側の入力レベルを少しずつ下げていきながら
        // リファレンスと同程度の音量になるように調整していく。
        var high = 1d;
        for (; MasterVolumeLevelScalar.Minimum < target.MasterVolumeLevelScalar; target.MasterVolumeLevelScalar -= step)
        {
            // レコーディング開始
            reference.StartRecording(DirectoryName);
            target.StartRecording(DirectoryName);

            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            // レコーディング停止
            var referenceLevel = reference.StopRecording().PeakValues.Average();
            var targetLevel = target.StopRecording().PeakValues.Average();

            if (targetLevel <= referenceLevel)
            {
                // キャリブレーション対象のレベルがリファレンスより小さくなったら調整を終了する

                // リファレンスより小さくなった際の値と、リファレンスより大きかった際の値を比較する
                // 小さくなった際の方が誤差が小さかった場合、
                if (!(referenceLevel - targetLevel < high - referenceLevel)) return;


                // 大きかった時(high)の方が誤差が小さかった場合、入力レベルをステップ分戻す
                if (target.MasterVolumeLevelScalar < MasterVolumeLevelScalar.Maximum)
                {
                    target.MasterVolumeLevelScalar += step;
                }

                return;
            }

            high = targetLevel;
        }
    }
}