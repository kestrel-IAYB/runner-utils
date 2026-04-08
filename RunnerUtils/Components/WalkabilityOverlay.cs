using System.Collections.Generic;
using UnityEngine;

namespace RunnerUtils.Components;

public static class WalkabilityOverlay
{
    private static Dictionary<TerrainData, float[,,]> cachedMaps = new Dictionary<TerrainData, float[,,]>();

    public static void MakeWalkabilityTex(ref TerrainData data, float maxWalkableAngle) {

        if (cachedMaps.TryGetValue(data, out var cachedMap)) {
            data.SetAlphamaps(0, 0, cachedMap);
            return;
        }

        data.terrainLayers[1].diffuseTexture = Texture2D.blackTexture;
        data.terrainLayers[1].diffuseTexture.Apply(true);
            
        float[,,] map = new float[data.alphamapWidth, data.alphamapHeight, data.alphamapLayers];
        for (int y = 0; y < data.alphamapHeight; ++y) {
            for (int x = 0; x < data.alphamapWidth; ++x) {
                float normX = x * 1f / (data.alphamapWidth - 1);
                float normY = y * 1f / (data.alphamapHeight - 1);

                var angle = data.GetSteepness(normY, normX); // why the FUCK is it inverted. why does this work

                if (angle < maxWalkableAngle) {
                    map[x, y, 0] = 1;
                } else {
                    var a2 = angle - 45;
                    map[x, y, 0] = (1 - (a2 / 45)) * 0.25f;
                    map[x, y, 1] = (a2 / 45)*4f;
                }
            }
        }
        cachedMaps.Add(data, map);
        data.SetAlphamaps(0, 0, map);
    }
}