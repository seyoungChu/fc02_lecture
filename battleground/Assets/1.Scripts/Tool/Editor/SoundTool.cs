using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using UnityObject = UnityEngine.Object;
using System.Media;

public class SoundTool : EditorWindow
{
    public int uiWidthLarge = 450;
    public int uiWidthMiddle = 300;
    public int uiWidthSmall = 200;
    private int selection = 0;
    private Vector2 SP1 = Vector2.zero;
    private Vector2 SP2 = Vector2.zero;
    private AudioClip soundSource;
    private static SoundData soundData;

    [MenuItem("Tools/Sound Tool")]
    static void Int()
    {
        soundData = CreateInstance<SoundData>();
        soundData.LoadData();

        SoundTool window = GetWindow<SoundTool>(false, "Sound Tool");
        window.Show();
    }
    private void OnGUI()
    {
        if(soundData == null)
        {
            return;
        }
        EditorGUILayout.BeginVertical();
        {
            UnityObject source = soundSource;
            SoundClip sound = soundData.soundClips[selection];
            EditorHelper.EditorToolTopLayer(soundData, ref selection, ref source, uiWidthMiddle);
            soundSource = (AudioClip)source;

            EditorGUILayout.BeginHorizontal();
            {
                EditorHelper.EditorToolListLayer(ref SP1, soundData, ref selection,
                    ref source, uiWidthMiddle);
                soundSource = (AudioClip)source;

                EditorGUILayout.BeginVertical();
                {
                    this.SP2 = EditorGUILayout.BeginScrollView(this.SP2);
                    {
                        if(soundData.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                
                                EditorGUILayout.LabelField("ID", selection.ToString(),
                                    GUILayout.Width(uiWidthLarge));
                                soundData.names[selection] = EditorGUILayout.TextField("Name",
                                    soundData.names[selection], GUILayout.Width(uiWidthLarge));
                                sound.playType = (SoundPlayType)EditorGUILayout.EnumPopup("PlayType",
                                    sound.playType, GUILayout.Width(uiWidthLarge));
                                sound.maxVolume = EditorGUILayout.FloatField("Max Volume",
                                    sound.maxVolume, GUILayout.Width(uiWidthLarge));
                                sound.isLoop = EditorGUILayout.Toggle("LoopClip",
                                    sound.isLoop, GUILayout.Width(uiWidthLarge));
                                EditorGUILayout.Separator();
                                if(this.soundSource == null && sound.clipName != string.Empty)
                                {
                                    this.soundSource = Resources.Load(sound.clipPath + sound.clipName) as AudioClip;
                                }
                                this.soundSource = (AudioClip)EditorGUILayout.ObjectField("Audio Clip",
                                    this.soundSource, typeof(AudioClip), false, GUILayout.Width(uiWidthLarge));
                                if(soundSource != null)
                                {
                                    sound.clipPath = EditorHelper.GetPath(soundSource);
                                    sound.clipName = soundSource.name;
                                    sound.pitch = EditorGUILayout.Slider("Pitch",
                                        sound.pitch, -3.0f, 3.0f, GUILayout.Width(uiWidthLarge));
                                    sound.dopplerLevel = EditorGUILayout.Slider("Doppler",
                                        sound.dopplerLevel, 0.0f, 5.0f, GUILayout.Width(uiWidthLarge));
                                    sound.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup(
                                        "volume Rolloff", sound.rolloffMode, GUILayout.Width(uiWidthLarge));
                                    sound.minDistance = EditorGUILayout.FloatField("min Distance",
                                        sound.minDistance, GUILayout.Width(uiWidthLarge));
                                    sound.maxDistance = EditorGUILayout.FloatField("Max Distance",
                                        sound.maxDistance, GUILayout.Width(uiWidthLarge));
                                    sound.spartialBlend = EditorGUILayout.Slider("PanLevel",
                                        sound.spartialBlend, 0.0f, 1.0f, GUILayout.Width(uiWidthLarge));

                                }
                                else
                                {
                                    sound.clipName = string.Empty;
                                    sound.clipPath = string.Empty;
                                }
                                EditorGUILayout.Separator();
                                if(GUILayout.Button("Add Loop", GUILayout.Width(uiWidthMiddle)))
                                {
                                    soundData.soundClips[selection].AddLoop();
                                }
                                for(int i = 0; i < soundData.soundClips[selection].checkTime.Length;i++)
                                {
                                    EditorGUILayout.BeginVertical("box");
                                    {
                                        GUILayout.Label("Loop Step " + i, EditorStyles.boldLabel);
                                        if(GUILayout.Button("Remove", GUILayout.Width(uiWidthMiddle)))
                                        {
                                            soundData.soundClips[selection].RemoveLoop(i);
                                            return;
                                        }
                                        
                                        sound.checkTime[i] = EditorGUILayout.FloatField("check Time",
                                            sound.checkTime[i], GUILayout.Width(uiWidthMiddle));
                                        sound.setTime[i] = EditorGUILayout.FloatField("Set Time",
                                            sound.setTime[i], GUILayout.Width(uiWidthMiddle));

                                    }
                                    EditorGUILayout.EndVertical();
                                }

                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();


        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        //
        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Reload"))
            {
                soundData = CreateInstance<SoundData>();
                soundData.LoadData();
                selection = 0;
                this.soundSource = null;
            }
            if(GUILayout.Button("Save"))
            {
                soundData.SaveData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

        }
        EditorGUILayout.EndHorizontal();
    }
    public void CreateEnumStructure()
    {
        string enumName = "SoundList";
        StringBuilder builder = new StringBuilder();
        for(int i = 0; i < soundData.names.Length;i++)
        {
            if(!soundData.names[i].ToLower().Contains("none"))
            {
                builder.AppendLine("    "+soundData.names[i] + " = " +i.ToString()+"," );
            }
        }
        EditorHelper.CreateEnumStructure(enumName, builder);
    }

}
