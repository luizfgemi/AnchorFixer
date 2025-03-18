# ⚠️🚀 WARNING: BACKUP YOUR SAVE! 🚀⚠️

**This mod modifies your save files automatically. Always BACKUP your KSP saves before using AnchorFixer!**

Even though AnchorFixer is designed to fix a specific bug with Ground Anchors, any automated tool that writes to `.sfs` files could potentially cause unexpected behavior. Stay safe and backup first! 🧑‍🚀✨

---

# AnchorFixer

![GitHub Workflow Status](https://github.com/luizfgemi/AnchorFixer/actions/workflows/build.yml/badge.svg)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/luizfgemi/AnchorFixer)

**C# Mod inspired by the original Python script "ksp-anchor-keeper" by OliverWieland**

### 🎯 Purpose:
This mod automatically detects when Ground Anchors are placed in the scene and protects their original positions from a known bug that shifts their coordinates in subsequent saves.

### ⚙ Features:
- Automatically monitors all Ground Anchor instances.
- Restores original positions during any type of save (manual, quicksave, autosave).
- Saves the anchor state to `PluginData/anchors.json` for persistence between game sessions.

### ✅ Automated Build & Release:
- **CI/CD:** This mod is built and packaged automatically via **GitHub Actions**.
- Each release includes a `.zip` ready for manual install or CKAN submission.

### ✨ Credits:
- **OliverWieland** for the original Python script ([ksp-anchor-keeper](https://github.com/OliverWieland/ksp-anchor-keeper))
- **C# Mod Version:** by luizfgemi

### 📦 Installation:
1. Download the latest release from the [releases page](https://github.com/YourNameHere/AnchorFixer/releases).
2. Drop the `GameData/AnchorFixer/` folder into your KSP `GameData` directory.

### 🛰 Compatibility:
- **KSP:** 1.12.x
- **CKAN:** CKAN-ready using the provided `AnchorFixer.version` metadata

### 📝 License:
MIT
