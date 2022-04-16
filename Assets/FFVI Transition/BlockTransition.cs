using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BlockTransition : MonoBehaviour
{
    public VisualEffect vfxGraph;
    [Min(0)] public int columnFactor = 1;
    private RenderTexture screenTexture;
    public GameObject background;

    public void PlayTansition()
    {
        if (vfxGraph == null)
            return;

        var resolution = Screen.currentResolution;

        var columns = columnFactor * 8; //let's use multiples of 8 columns
        var blockPixelSize = Mathf.RoundToInt(Screen.currentResolution.width / (float) columns);
        var rows = Mathf.CeilToInt(resolution.height / (float) blockPixelSize);
        var cam = Camera.main;

        if (screenTexture != null)
            RenderTexture.ReleaseTemporary(screenTexture);

        screenTexture =
            RenderTexture.GetTemporary(new RenderTextureDescriptor(resolution.width, resolution.height,
                RenderTextureFormat.Default));

        background.SetActive(true);
        cam.targetTexture = screenTexture;
        cam.Render();
        cam.targetTexture = null;
        background.SetActive(false);

        var blocksCount = columns * rows;

        var inverseViewProjection = (cam.projectionMatrix * cam.transform.worldToLocalMatrix).inverse;

        vfxGraph.SetInt("count", blocksCount);
        vfxGraph.SetInt("columns", columns);
        vfxGraph.SetFloat("ortographicSize", cam.orthographicSize);
        vfxGraph.SetVector2("blocksDimension", new float2(columns, rows));
        vfxGraph.SetTexture("screenTexture", screenTexture);
        vfxGraph.SetVector2("screenResolution", new float2(resolution.width, resolution.height));
        vfxGraph.SetFloat("blockSize", blockPixelSize);
        vfxGraph.SetMatrix4x4("inverseViewProjection", inverseViewProjection);
        
        vfxGraph.Reinit();
        vfxGraph.Play();
    }

    private void OnDisable()
    {
        if (screenTexture != null)
            RenderTexture.ReleaseTemporary(screenTexture);
    }

    private void Start()
    {
        PlayTansition();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(BlockTransition))]
public class BlockTransitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        if (GUILayout.Button("Play"))
            (target as BlockTransition).PlayTansition();
    }
}
#endif