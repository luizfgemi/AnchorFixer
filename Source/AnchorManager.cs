using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AnchorManager
{
    private Dictionary<uint, Vector3d> anchorOriginalPositions = new Dictionary<uint, Vector3d>();
    private string savePath;
    private bool isLoaded = false;

    public AnchorManager()
    {
        savePath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData", "AnchorFixer", "PluginData", "anchors.json");
    }

    public void RegisterAnchor(uint flightID, Vector3d position)
    {
        if (!anchorOriginalPositions.ContainsKey(flightID))
        {
            anchorOriginalPositions[flightID] = position;
            Debug.Log($"[AnchorFixer] Anchor registered ID {flightID}: {position}");
        }
    }

    public void FixAnchorsInSave(ConfigNode node)
    {
        var vessels = node.GetNode("FLIGHTSTATE")?.GetNodes("VESSEL");
        if (vessels == null) return;

        int totalAnchors = 0;
        int anchorsFixed = 0;

        foreach (var vessel in vessels)
        {
            foreach (var part in vessel.GetNodes("PART"))
            {
                string partName = part.GetValue("part");
                if (partName.Contains("groundAnchor"))
                {
                    totalAnchors++;
                    uint flightID = uint.Parse(part.GetValue("flightID"));
                    if (anchorOriginalPositions.TryGetValue(flightID, out Vector3d originalPos))
                    {
                        string posStr = part.GetValue("pos");
                        Vector3d currentPos = ParseVector(posStr);

                        if (currentPos != originalPos)
                        {
                            anchorsFixed++;
                            Debug.Log($"[AnchorFixer] Restoring ID {flightID} from {currentPos} to {originalPos}");
                            part.SetValue("pos", $"{originalPos.x} , {originalPos.y} , {originalPos.z}", true);
                        }
                    }
                }
            }
        }

        if (totalAnchors > 0)
        {
            Debug.Log($"[AnchorFixer] Save hook processed {totalAnchors} anchors. Restored {anchorsFixed} anchors.");
        }
        else
        {
            Debug.Log("[AnchorFixer] Save hook found no deployed Ground Anchors in this scene.");
        }
    }

        Debug.Log($"[AnchorFixer] Save hook processed {totalAnchors} anchors. Restored {anchorsFixed} anchors.");
    }

    public void SaveAnchorsToFile()
    {
        var dict = anchorOriginalPositions.ToDictionary(
            kv => kv.Key.ToString(),
            kv => new SerializableVector { x = kv.Value.x, y = kv.Value.y, z = kv.Value.z });
        string json = MiniJSON.Serialize(new Dictionary<string, object> { { "anchors", dict } });
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        File.WriteAllText(savePath, json);
    }

    public void LoadAnchorsFromFile()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.Log("[AnchorFixer] Save path is not set. Cannot load anchors.");
            return;
        }

        if (!File.Exists(savePath))
        {
            Debug.Log("[AnchorFixer] No anchors file found. Starting with empty data.");
            return;
        }

        string json = File.ReadAllText(savePath);
        var data = (Dictionary<string, object>)MiniJSON.Deserialize(json);

        if (data != null && data.ContainsKey("anchors"))
        {
            var anchors = (Dictionary<string, object>)data["anchors"];
            foreach (var kv in anchors)
            {
                var vecDict = (Dictionary<string, object>)kv.Value;
                double x = Convert.ToDouble(vecDict["x"]);
                double y = Convert.ToDouble(vecDict["y"]);
                double z = Convert.ToDouble(vecDict["z"]);
                anchorOriginalPositions[uint.Parse(kv.Key)] = new Vector3d(x, y, z);
            }
            Debug.Log("[AnchorFixer] Anchors loaded from file.");
        }
        else
        {
            Debug.Log("[AnchorFixer] Anchors file is empty or corrupted.");
        }
    }
    
    public void EnsureAnchorsLoaded()
    {
        if (isLoaded) return;
        LoadAnchorsFromFile();
        isLoaded = true;
    }

    private Vector3d ParseVector(string vecStr)
{
    var parts = vecStr.Split(',').Select(s => double.Parse(s.Trim())).ToArray();
    return new Vector3d(parts[0], parts[1], parts[2]);
}

    [System.Serializable]
    private class SerializableVector { public double x, y, z; }
}
