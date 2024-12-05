using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AetherMasterScript : MonoBehaviour
{
    //edits the aether materials to fix positioning
    
    public void Update()
    {
        if (WorldPlayer.Instance != null)
        {
            Vector3 pos = WorldPlayer.Instance.transform.position;
            Shader.SetGlobalVector("_PlayerPos", pos);
            Shader.SetGlobalFloat("_PlayerAether", WorldPlayer.Instance.GetAetherShaderTime());
            Shader.SetGlobalFloat("_PlayerLight", WorldPlayer.Instance.GetLightShaderTime());
            Shader.SetGlobalFloat("_PlayerBubble", WorldPlayer.Instance.GetBubbleShaderTime());
            Shader.SetGlobalFloat("_PlayerLeaf", WorldPlayer.Instance.GetLeafShaderTime());
            //Shader.SetGlobalMatrix("UNITY_MATRIX_IV", MainManager.Instance.Camera.transform.localToWorldMatrix);
        }
        else
        {
            Shader.SetGlobalVector("_PlayerPos", new Vector4(float.NaN, float.NaN, float.NaN, float.NaN));
            Shader.SetGlobalFloat("_PlayerAether", 0);
            Shader.SetGlobalFloat("_PlayerLight", 0);
            Shader.SetGlobalFloat("_PlayerBubble", 0);
            Shader.SetGlobalFloat("_PlayerLeaf", 0);
            //Shader.SetGlobalMatrix("UNITY_MATRIX_IV", MainManager.Instance.Camera.transform.localToWorldMatrix);
        }
    }
}
