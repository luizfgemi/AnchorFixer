using System.Collections.Generic;
using UnityEngine;
using KSP;
using System.Linq;
using System.IO;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class AnchorFixer : MonoBehaviour
{
    private AnchorManager anchorManager;

    void Start()
    {
        anchorManager = new AnchorManager();

        GameEvents.onPartCouple.Add(OnPartCouple);
        GameEvents.onVesselWasModified.Add(OnVesselModified);
        GameEvents.onGameStateSave.Add(OnGameSave);

        anchorManager.LoadAnchorsFromFile();
        Debug.Log("[AnchorFixer] Mod is active and monitoring anchors.");
    }

    private void OnPartCouple(GameEvents.FromToAction<Part, Part> data) => CheckAnchor(data.to);

    private void OnVesselModified(Vessel vessel)
    {
        foreach (var part in vessel.parts)
            CheckAnchor(part);
    }

    private void CheckAnchor(Part part)
    {
        if (part.partName.Contains("groundAnchor"))
        {
            anchorManager.RegisterAnchor(part.flightID, part.transform.position);
        }
    }

    private void OnGameSave(ConfigNode node)
    {
        Debug.Log("[AnchorFixer] Intercepting save event, validating anchors...");
        anchorManager.FixAnchorsInSave(node);
        anchorManager.SaveAnchorsToFile();
    }

    void OnDestroy()
    {
        GameEvents.onPartCouple.Remove(OnPartCouple);
        GameEvents.onVesselWasModified.Remove(OnVesselModified);
        GameEvents.onGameStateSave.Remove(OnGameSave);
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

        Debug.Log($"[AnchorFixer] Save hook processed {totalAnchors} anchors. Restored {anchorsFixed} anchors.");
    }

}
