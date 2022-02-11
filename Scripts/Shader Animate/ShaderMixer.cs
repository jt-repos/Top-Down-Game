using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ShaderMixer : PlayableBehaviour
{
    public string ShaderVarName;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Renderer rend = playerData as Renderer;
        if (rend == null)
            return;

        int inputCount = playable.GetInputCount();
        float finalFloat = 0;
        for (int index = 0; index < inputCount; index++)
        {
            float weight = playable.GetInputWeight(index);
            var inputPlayable = (ScriptPlayable<ShaderPlayable>)playable.GetInput(index);
            ShaderPlayable behavior = inputPlayable.GetBehaviour();
            finalFloat += behavior.FloatVal * weight;
        }

        Material mat;
        if (Application.isPlaying)
            mat = rend.material;
        else
            mat = rend.sharedMaterial;
        mat.SetFloat(ShaderVarName, finalFloat);
    }
}
