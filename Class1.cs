using System;
using System.Collections.Generic;
using BaseX;
using CodeX;
using FrooxEngine.UIX;
using POpusCodec.Enums;
using FrooxEngine;
using NeosModLoader;
using HarmonyLib;
using FrooxEngine.LogiX.Math;
namespace NovrX
{
    [GloballyRegistered]
    [HarmonyPatch(typeof(FrooxEngine.AudioStreamController), nameof(FrooxEngine.AudioStreamController.BuildUI))]
    public class AudioStreamSpawner_patch :  Component, IComponentBase, IDestroyable, IWorker, IWorldElement, IUpdatable, IChangeable, IAudioUpdatable, IInitializable, ILinkable
    {
        // i hate making patches for things, this doesn't even work as of 27/05/2024
       
        public static SyncRef<OpusStream<StereoSample>> _stream;

        public static SyncRef<AudioOutput> _audioOutput;
        public static Slot rootz;
        public static bool IsOwnedByLocalUser => _stream.Target?.User == rootz.LocalUser;

        public static IAudioStream Stream => _stream.Target;

        public const string BITRATE_SETTING = "AudioStream.Bitrate";

        public const string DEVICE_NAME = "AudioStream.DeviceName";

        public static Sync<float> BitrateKbps;

        public static Sync<string> DeviceName;

        public static Sync<string> _bitrateString;

        [HarmonyPrefix]
        
        public static bool Prefix(ref int bitrate, ref int deviceIndex, Slot root)
        {
            if (bitrate == 0) {
                NovrX.NovrXinitClass.log("bitrate is 0");
            }
            rootz = root;

            AudioOutput audioOutput = root.AttachComponent<AudioOutput>();
            UserAudioStream<StereoSample> userAudioStream = root.AttachComponent<UserAudioStream<StereoSample>>();
            OpusStream<StereoSample> opusStream = root.LocalUser.AddStream<OpusStream<StereoSample>>();
            opusStream.BitRate.Value = bitrate;
            opusStream.ApplicationType.Value = OpusApplicationType.Audio;
            opusStream.MinimumVolume.Value = 0f;
            root.DestroyWhenDestroyed(opusStream);
            VolumeMeter volumeMeter = root.AttachComponent<VolumeMeter>();
            volumeMeter.Power.Value = 0.5f;
            volumeMeter.Smoothing.Value = 0.5f;
            volumeMeter.Source.Target = opusStream;
            userAudioStream.TargetDeviceIndex = deviceIndex;
            userAudioStream.Stream.Target = opusStream;
            userAudioStream.UseFilteredData.Value = false;
            audioOutput.DopplerLevel.Value = 0f;
            audioOutput.Source.Target = opusStream;
            audioOutput.ExludeLocalUser();
            UIBuilder uIBuilder = new UIBuilder(root);
            RadiantUI_Constants.SetupDefaultStyle(uIBuilder);
            uIBuilder.HorizontalHeader(48f, out var header, out var content);
            uIBuilder.ForceNext = header;
            header.AddFixedPadding(8f);
            LocaleString text = "Tools.StreamAudio.Title".AsLocaleKey("username", root.LocalUser.UserNameField);
            uIBuilder.Text(in text).Color.Value = RadiantUI_Constants.HEADING_COLOR;
            audioOutput.AudioTypeGroup.Value = AudioTypeGroup.Multimedia;
            uIBuilder.NestInto(content);
            List<RectTransform> list = uIBuilder.SplitVertically(0.3f, 0.25f, 0.5f);
            uIBuilder.NestInto(list[0]);
            color tint = color.Black;
            uIBuilder.Panel(in tint);
            tint = RadiantUI_Constants.LABEL_COLOR;
            Image image = uIBuilder.Image(in tint);
            ProgressBar progressBar = image.Slot.AttachComponent<ProgressBar>();
            progressBar.SetTarget(image.RectTransform);
            progressBar.Progress.DriveFrom(volumeMeter.Volume);
            uIBuilder.NestOut();
            uIBuilder.NestOut();
            uIBuilder.NestInto(list[1]);
            uIBuilder.Slider(32f).Value.DriveFrom(audioOutput.Volume, writeBack: true);
            uIBuilder.NestOut();
            uIBuilder.NestInto(list[2]);
            uIBuilder.SplitHorizontally(0.5f, out var left, out var right);
            left.AddFixedPadding(8f);
            right.AddFixedPadding(8f);
            uIBuilder.ForceNext = left;
            text = "";
            Button button = uIBuilder.Button(in text, OnToggleBroadcast);
            button.Label.RectTransform.AddFixedPadding(4f);
            uIBuilder.ForceNext = right;
            text = "";
            Button button2 = uIBuilder.Button(in text, OnTogglePlayForOwner);
            button2.Label.RectTransform.AddFixedPadding(4f);
            _stream.Target = opusStream;
            _audioOutput.Target = audioOutput;
            UpdateBroadcast(button);
            UpdatePlayForOwner(button2);
            NovrX.NovrXinitClass.log("spawned out the thing");
            return false;
        
        }
        public static void UpdateBroadcast(IButton button)
        {

            button.LabelTextField.Value = "string content";
        }

        public static void UpdatePlayForOwner(IButton button)
        {
            button.LabelTextField.Value = "string content";
        }

       
        public static void OnToggleBroadcast(IButton button, ButtonEventData eventData)
        {
            AudioOutput target = _audioOutput.Target;
            target.Spatialize.Value = !target.Spatialize.Value;
            target.SpatialBlend.Value = (target.Spatialize.Value ? 1 : 0);
            UpdateBroadcast(button);
        }

    
        public static void OnTogglePlayForOwner(IButton button, ButtonEventData eventData)
        {
            if (IsOwnedByLocalUser)
            {
                AudioOutput target = _audioOutput.Target;
                if (target.IsLocalUserExluded)
                {
                    target.RemoveExludedUser(rootz.LocalUser);
                }
                else
                {
                    target.ExludeLocalUser();
                }

                UpdatePlayForOwner(button);
            }
        }



        public override ISyncMember GetSyncMember(int index)
        {
            return index switch
            {
                0 => persistent,
                1 => updateOrder,
                2 => EnabledField,
                3 => _stream,
                4 => _audioOutput,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static AudioStreamController __New()
        {
            return new AudioStreamController();
        }
    }
}
