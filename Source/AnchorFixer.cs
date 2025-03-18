using System.Collections.Generic;
using UnityEngine;
using KSP;

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
        Debug.Log($"[AnchorFixer] Scanning part: {part.partName}, ID: {part.flightID}");

        if (part.partName.Contains("groundAnchor"))
        {
            Debug.Log($"[AnchorFixer] CheckAnchor detected: {part.partName}, ID: {part.flightID}, pos: {part.transform.position}");
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
}
