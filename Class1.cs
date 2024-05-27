using System;
using System.Collections.Generic;
using BaseX;
using CodeX;
using FrooxEngine.UIX;
using POpusCodec.Enums;
using FrooxEngine;

using HarmonyLib;
using FrooxEngine.LogiX.Math;
namespace NovrX
{
    [GloballyRegistered]
    public class AudioStreamSpawner : Component, IComponentBase, IDestroyable, IWorker, IWorldElement, IUpdatable, IChangeable, IAudioUpdatable, IInitializable, ILinkable
    {

        protected readonly SyncRef<OpusStream<StereoSample>> _stream;

        protected readonly SyncRef<AudioOutput> _audioOutput;

        public bool IsOwnedByLocalUser => _stream.Target?.User == base.LocalUser;

        public IAudioStream Stream => _stream.Target;

        public const string BITRATE_SETTING = "AudioStream.Bitrate";

        public const string DEVICE_NAME = "AudioStream.DeviceName";

        public readonly Sync<float> BitrateKbps;

        public readonly Sync<string> DeviceName;

        protected readonly Sync<string> _bitrateString;
        [HarmonyPatch(typeof(FrooxEngine.AudioStreamSpawner))]
    public class AudioStreamSpawner_Patch : Component,  IComponentBase, IDestroyable, IWorker, IWorldElement, IUpdatable, IChangeable, IAudioUpdatable, IInitializable, ILinkable
        {
            protected readonly SyncRef<OpusStream<StereoSample>> _stream;

            protected readonly SyncRef<AudioOutput> _audioOutput;

            public bool IsOwnedByLocalUser => _stream.Target?.User == base.LocalUser;

            public IAudioStream Stream => _stream.Target;

            public const string BITRATE_SETTING = "AudioStream.Bitrate";

            public const string DEVICE_NAME = "AudioStream.DeviceName";

            public readonly Sync<float> BitrateKbps;

            public readonly Sync<string> DeviceName;

            protected readonly Sync<string> _bitrateString;

         

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FrooxEngine.AudioStreamController), nameof(FrooxEngine.AudioStreamController.BuildUI), new Type[] { typeof(int), typeof(int),typeof(Slot), })]
        public static bool Prefix(ref int bitrate, ref int deviceIndex, Slot root)
            {
                
                AudioOutput audioOutput = root.AttachComponent<AudioOutput>();
                UserAudioStream<StereoSample> userAudioStream = root.AttachComponent<UserAudioStream<StereoSample>>();
                OpusStream<StereoSample> stream = root.LocalUser.AddStream<OpusStream<StereoSample>>();
                stream.BitRate.Value = bitrate;
                stream.ApplicationType.Value = OpusApplicationType.Audio;
                stream.MinimumVolume.Value = 0f;
                stream.MinimumBufferDelay.Value = 0.2f;
                stream.BufferSize.Value = 24000;
                VolumeMeter deviceVolume = root.AttachComponent<VolumeMeter>();
                deviceVolume.Power.Value = 0.5f;
                deviceVolume.Smoothing.Value = 0.5f;
                deviceVolume.Source.Target = stream;
                userAudioStream.TargetDeviceIndex = deviceIndex;
                userAudioStream.Stream.Target = stream;
                userAudioStream.UseFilteredData.Value = false;
                audioOutput.DopplerLevel.Value = 0f;
                audioOutput.AudioTypeGroup.Value = AudioTypeGroup.Multimedia;
                audioOutput.Source.Target = stream;
                audioOutput.ExludeLocalUser();
                color tint = color.Green;
                NovrX.UIX.UIBuilder ui = new NovrX.UIX.UIBuilder(root);
                ui.Style.TextColor = tint;
                ui.Style.ButtonColor = tint;
                ui.Style.ButtonSpriteColor = color.White;
                ui.Style.DisabledColor = new color(0.1f, 0.08f, 0.08f);
                ui.Style.DisabledAlpha = 0.25f;
                ui.HorizontalHeader(48f, out var header, out var content);
                ui.ForceNext = header;
                header.AddFixedPadding(8f);
                LocaleString text = "maow";/*"Tools.StreamAudio.Title".AsLocaleKey("username","GOT FUCKING DAMMIT PLEASE JUST WORK DAMIT ");*/
                ui.Text(ref text).Color.Value = color.Red;
                ui.NestInto(content);
                List<RectTransform> split = ui.SplitVertically(0.3f, 0.25f, 0.5f);
                ui.NestInto(split[0]);
                ui.Panel(ref  tint);
                tint = color.Green;
                Image voiceBar = ui.Image(ref tint);
                ProgressBar progressBar = voiceBar.Slot.AttachComponent<ProgressBar>();
                progressBar.SetTarget(voiceBar.RectTransform);
                progressBar.Progress.DriveFrom(deviceVolume.Volume);
                ui.NestOut();
                ui.NestOut();
                ui.NestInto(split[1]);
                ui.Slider(32f).Value.DriveFrom(audioOutput.Volume, true);
                ui.NestOut();
                ui.NestInto(split[2]);
                ui.SplitHorizontally(0.5f, out var left, out var right);
                left.AddFixedPadding(8f);
                right.AddFixedPadding(8f);
                ui.ForceNext = left;
                text = "";
                ui.ForceNext = right;
                text = "";
                return false;
            }
        }
    }

}
