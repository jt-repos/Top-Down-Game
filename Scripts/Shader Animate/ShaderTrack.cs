using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(ShaderControlAsset))]
[TrackBindingType(typeof(Renderer))]
public class ShaderTrack : TrackAsset
{
    public string ShaderVarName;
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<ShaderMixer>.Create(graph, inputCount);
        mixer.GetBehaviour().ShaderVarName = ShaderVarName;
        return mixer;
    }
}
