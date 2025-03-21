# ⚠️ WARNING: BACKUP YOUR SAVE! ⚠️

**This mod modifies your save files automatically. Always BACKUP your KSP saves before using AnchorFixer!**


---

# AnchorFixer

[![Build AnchorFixer](https://github.com/luizfgemi/AnchorFixer/actions/workflows/build.yml/badge.svg)](https://github.com/luizfgemi/AnchorFixer/actions)
[![Latest Release](https://img.shields.io/github/v/release/luizfgemi/AnchorFixer?label=release)](https://github.com/luizfgemi/AnchorFixer/releases/latest)

### Purpose:
This mod automatically detects when Ground Anchors are placed in the scene and protects their original positions from a known bug that shifts their coordinates in subsequent saves.

### Features:
- Automatically monitors all Ground Anchor instances.
- Restores original positions during any type of save (manual, quicksave, autosave).
- Saves the anchor state to `PluginData/anchors.json` for persistence between game sessions.

### Installation:
1. Download the latest release from the [releases page](https://github.com/YourNameHere/AnchorFixer/releases).
2. Drop the `GameData/AnchorFixer/` folder into your KSP `GameData` directory.

### Compatibility:
- **KSP:** 1.12.x
- **CKAN:** CKAN-ready using the provided `AnchorFixer.version` metadata

### Credits:
- **OliverWieland** for the original idea ([ksp-anchor-keeper](https://github.com/OliverWieland/ksp-anchor-keeper))

### 📝 License:
- MIT
