using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using CodeX;
using FrooxEngine.LogiX;
using FrooxEngine.Undo;
using NetX;
using FrooxEngine;
using HarmonyLib;
using FrooxEngine.UIX;
using NovrX;
namespace NovrX
{
    [HarmonyPatch(typeof(FrooxEngine.AudioStreamSpawner), nameof(FrooxEngine.AudioStreamSpawner.Pressed))]
    public class AudioStreamSpawerthing
    {
        public static void Prefix(IButton button, ButtonEventData eventData)
        {
            UIBuilder uIBuilder = new UIBuilder(NovrX.AudioStreamSpawner_patch.rootz.OpenModalOverlay(new float2(0.4f, 0.5f)));
            RadiantUI_Constants.SetupDefaultStyle(uIBuilder);
            uIBuilder.SplitVertically(0.875f, out var top, out var bottom);
            AudioInputDeviceSelection audioInputDeviceSelection = top.Slot.AttachComponent<AudioInputDeviceSelection>();
            audioInputDeviceSelection.UseFilteredData.Value = false;
            audioInputDeviceSelection.SelectedDeviceIndex.Value = AudioStreamSpawner_patch.rootz.InputInterface.FindAudioInputIndex(NovrX.AudioStreamSpawner_patch.DeviceName.Value, caseSensitive: false, allowPartialMatch: false);
            NovrX.AudioStreamSpawner_patch.DeviceName.DriveFrom(audioInputDeviceSelection.SelectedDeviceName);
            uIBuilder.NestInto(bottom);
            uIBuilder.SplitHorizontally(0.7f, out var left, out var right);
            uIBuilder.Style.TextAutoSizeMax *= 1.5f;
            uIBuilder.Style.ButtonTextPadding *= 2f;
            uIBuilder.NestInto(left);
            uIBuilder.SplitHorizontally(0.333f, out var left2, out var right2);
            uIBuilder.ForceNext = left2;
            LocaleString text = "Tools.StreamAudio.Bitrate".AsLocaleKey("bitrate", NovrX.AudioStreamSpawner_patch._bitrateString);
            uIBuilder.Text(in text).RectTransform.AddFixedPadding(5f);
            uIBuilder.NestInto(right2);
            Slider<float> slider = uIBuilder.Slider(64f, NovrX.AudioStreamSpawner_patch.BitrateKbps, 2.5f, 500f);
            slider.RectTransform.AddFixedPadding(8f);
            slider.Value.DriveFrom(NovrX.AudioStreamSpawner_patch.BitrateKbps, writeBack: true);
            uIBuilder.NestOut();
            uIBuilder.ForceNext = right;
            uIBuilder.Button(NeosAssets.Graphics.Icons.Voice.Broadcast,"THIS SHITS A CUNT", NovrX.AudioStreamSpawerthing.OnStartStreaming).RectTransform.AddFixedPadding(8f);
        }


        public void Pressing(IButton button, ButtonEventData eventData)
        {
        }

        public void Released(IButton button, ButtonEventData eventData)
        {
        }

        [SyncMethod]

        public static void OnStartStreaming(IButton button, ButtonEventData eventData)
        {
            if (NovrX.AudioStreamSpawner_patch.rootz.World != Userspace.UserspaceWorld)
            {
                return;
            }

            int deviceIndex = NovrX.AudioStreamSpawner_patch.rootz.InputInterface.FindAudioInputIndex(NovrX.AudioStreamSpawner_patch.DeviceName.Value, caseSensitive: false, allowPartialMatch: false);
            FrooxEngine.World _targetWorld = NovrX.AudioStreamSpawner_patch.rootz.Engine.WorldManager.FocusedWorld;
            _targetWorld.RunSynchronously(delegate
            {
                if (!_targetWorld.CanSpawnObjects())
                {
                    NotificationMessage.SpawnTextMessage("Permissions.NotAllowedToSpawn".AsLocaleKey(), color.Red);
                }
                else
                {
                    _targetWorld.GetGloballyRegisteredComponents((AudioStreamController c) => c.IsOwnedByLocalUser).ForEach(delegate (AudioStreamController c)
                    {
                        c.Slot.GetObjectRoot().Destroy();
                    });
                    Slot slot = _targetWorld.RootSlot.LocalUserSpace.AddSlot("Audio Stream");
                    slot.AttachComponent<ObjectRoot>();
                    slot.AttachComponent<DuplicateBlock>();
                    slot.PersistentSelf = false;
                    slot.AttachComponent<DestroyOnUserLeave>().TargetUser.Target = _targetWorld.LocalUser;
                    NeosCanvasPanel neosCanvasPanel = slot.AttachComponent<NeosCanvasPanel>();
                    neosCanvasPanel.Panel.AddCloseButton();
                    neosCanvasPanel.Panel.AddParentButton();
                    neosCanvasPanel.CanvasSize = new float2(400f, 200f);
                    neosCanvasPanel.PhysicalHeight = 0.3f;
                    neosCanvasPanel.Panel.Color = color.Black;
                    AudioStreamController audioStreamController = neosCanvasPanel.Canvas.Slot.AddSlot("UI").AttachComponent<AudioStreamController>();
                    audioStreamController.BuildUI(MathX.RoundToInt(NovrX.AudioStreamSpawner_patch.BitrateKbps.Value * 1000f), deviceIndex, slot);
                    slot.AttachComponent<ReferenceProxy>().Reference.Target = audioStreamController.Stream;
                    slot.PositionInFrontOfUser(float3.Backward);
                }
            });
            button.Slot.CloseModalOverlay();
        }
    }
}
