﻿using Kamishibai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Specter;
using Specter.Repository;
using Specter.View;
using Specter.ViewModel;

var builder = KamishibaiApplication<App, MainWindow>.CreateBuilder();

builder.Services.AddSingleton<IAudioRecordRepository, AudioRecordRepository>();
builder.Services.AddSingleton<IAudioInterfaceProvider, AudioInterfaceProvider>();
builder.Services.AddSingleton<IAudioRecorderProvider, AudioRecorderProvider>();
builder.Services.AddTransient<ISettingsRepository, SettingsRepository>();
builder.Services.AddTransient<IDecibelsReaderProvider, DecibelsReaderProvider>();

builder.Services.AddPresentation<MainWindow, MainWindowViewModel>();
builder.Services.AddPresentation<MonitoringPage, MonitoringPageViewModel>();
builder.Services.AddPresentation<SettingsPage, SettingsPageViewModel>();

await builder
    .Build()
    .RunAsync();