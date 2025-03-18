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

    private void EnsureSavePath()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData", "AnchorFixer", "PluginData", "anchors.json");
            Debug.Log($"[AnchorFixer] Save path initialized: {savePath}");
        }
    }

    public void EnsureAnchorsLoaded()
    {
        if (isLoaded) return;
        LoadAnchorsFromFile();
        isLoaded = true;
    }

    public void LoadAnchorsFromFile()
    {
        EnsureSavePath();

        if (!File.Exists(savePath))
        {
            Debug.Log("[AnchorFixer] No anchors file found. Starting with empty data.");
            return;
        }

        string json = File.ReadAllText(savePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.Log("[AnchorFixer] Anchors file is empty.");
            return;
        }

        var data = MiniJSON.Deserialize(json) as Dictionary<string, object>;

        if (data != null && data.ContainsKey("anchors"))
        {
            var anchors = data["anchors"] as Dictionary<string, object>;
            if (anchors != null)
            {
                foreach (var kv in anchors)
                {
                    var vecDict = kv.Value as Dictionary<string, object>;
                    if (vecDict != null)
                    {
                        double x = Convert.ToDouble(vecDict["x"]);
                        double y = Convert.ToDouble(vecDict["y"]);
                        double z = Convert.ToDouble(vecDict["z"]);
                        anchorOriginalPositions[uint.Parse(kv.Key)] = new Vector3d(x, y, z);
                    }
                }
                Debug.Log("[AnchorFixer] Anchors loaded from file.");
            }
        }
        else
        {
            Debug.Log("[AnchorFixer] Anchors file is empty or corrupted.");
        }
    }

    public void RegisterAnchor(uint flightID, Vector3d position)
    {
        Debug.Log($"[AnchorFixer] Trying to register ID {flightID} at position {position}");

        if (!anchorOriginalPositions.ContainsKey(flightID))
        {
            anchorOriginalPositions[flightID] = position;
            Debug.Log($"[AnchorFixer] Anchor registered ID {flightID}: {position}");
        }
        else
        {
            Debug.Log($"[AnchorFixer] Anchor ID {flightID} already registered.");
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

    public void SaveAnchorsToFile()
    {
        EnsureSavePath();

        var dict = anchorOriginalPositions.ToDictionary(
            kv => kv.Key.ToString(),
            kv => new SerializableVector { x = kv.Value.x, y = kv.Value.y, z = kv.Value.z });
        string json = MiniJSON.Serialize(new Dictionary<string, object> { { "anchors", dict } });
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        File.WriteAllText(savePath, json);
    }

    private Vector3d ParseVector(string vecStr)
    {
        var parts = vecStr.Split(',').Select(s => double.Parse(s.Trim())).ToArray();
        return new Vector3d(parts[0], parts[1], parts[2]);
    }

    [System.Serializable]
    private class SerializableVector { public double x, y, z; }
}
