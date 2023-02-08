﻿using FluentTextTable;
using MicrophoneLevelLogger.Command;
using MicrophoneLevelLogger.Domain;
using NAudio.Utils;
using Sharprompt;

namespace MicrophoneLevelLogger.View;

public class ShowInputLevelView : IShowInputLevelView
{
    public void NotifyResult(AudioInterfaceInputLevels audioInterfaceInputLevels)
    {
        lock (this)
        {
            Build
                .TextTable<MicrophoneInputLevel>(builder =>
                {
                    builder.Borders.InsideHorizontal.AsDisable();
                    builder.Columns.Add(x => x.Name);
                    builder.Columns.Add(x => x.Min).FormatAs("{0:#.00}");
                    builder.Columns.Add(x => x.Avg).FormatAs("{0:#.00}");
                    builder.Columns.Add(x => x.Median).FormatAs("{0:#.00}");
                    builder.Columns.Add(x => x.Max).FormatAs("{0:#.00}");
                })
                .WriteLine(audioInterfaceInputLevels.Microphones);
        }
    }
}