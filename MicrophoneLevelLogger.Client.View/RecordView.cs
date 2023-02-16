﻿using FluentTextTable;
using MicrophoneLevelLogger.Client.Controller.Record;
using Sharprompt;

namespace MicrophoneLevelLogger.Client.View;

public class RecordView : MicrophoneView, IRecordView
{
    public string InputRecordName()
    {
        return Prompt.Input<string>("録音名を入力してください。");
    }

    public void Wait(TimeSpan timeout)
    {
        ConsoleEx.Wait(timeout);
    }

    public void NotifyResult(IEnumerable<RecordResult> results)
    {
        lock (this)
        {
            Build
                .TextTable<RecordResult>(builder =>
                {
                    builder.Borders.InsideHorizontal.AsDisable();
                    builder.Columns.Add(x => x.No);
                    builder.Columns.Add(x => x.Name);
                    builder.Columns.Add(x => x.Min).FormatAs("{0:#.00}");
                    builder.Columns.Add(x => x.Avg).FormatAs("{0:#.00}");
                    builder.Columns.Add(x => x.Max).FormatAs("{0:#.00}");
                })
                .WriteLine(results);
        }
    }

    public void NotifyStarting(TimeSpan timeSpan)
    {
        ConsoleEx.WriteLine($"{timeSpan.Seconds}秒間、録音します。", ConsoleColor.White, ConsoleColor.Red);
    }
}