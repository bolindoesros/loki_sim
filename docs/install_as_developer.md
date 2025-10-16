## Dev guide

Welcome to behind the scene, where magic happens and robots come to life! This guide will walk you through the installation and setup process for developers who want to contribute to the UnitySim project. Thank you so much for being part of this exciting journey!

If you just want to run the simulation, please refer to the [download guide](download.md).

**Table of Contents**
- [Project structure](#project-structure)
- [Installation steps](#installation-steps)
- [Opening the project in Unity](#opening-the-project-in-unity)
- [Recommended development workflow](#recommended-development-workflow)
- [Git notes](#git-notes)

### Project structure

This is a comprehensive Unity simulation platform that integrates multiple specialized components as Git submodules:

```
UnitySim/                           # Overall project structure
├── UnityAuvSim/                   # Core AUV simulation project (this repo)
│   ├── Assets/                    # Unity project assets
│   │   ├── Crest/                # Ocean and water simulation system
│   │   ├── Models/               # 3D models and meshes
│   │   ├── Perception/           # Labeller for perception camera
│   │   ├── Resources/            # Runtime loadable assets
│   │   ├── Scenes/               # Unity scene files
│   │   ├── Scripts/              # C# scripts for simulation logic
│   │   ├── Settings/             # Project configuration files
│   │   └── TextMesh Pro/         # Text rendering system
│   ├── docs/                     # Documentation
│   ├── Library/                  # Unity generated files (auto-generated)
│   ├── Packages/                 # Unity Package Manager dependencies
│   ├── ProjectSettings/          # Unity project configuration
│   └── *.csproj, *.sln          # Visual Studio project files
├── UnityMavros/                   # MAVLink/ROS bridge integration
├── UnitySensors/                  # Sensor simulation framework
├── ROS-TCP-Connector/            # ROS communication package
├── com.unity.perception/          # Unity ML perception tools
└── .gitmodules                   # Git submodule configuration
```

UnitySim integrates several specialized components:

- **UnityAuvSim** - Main AUV simulation framework and Unity project
- **UnityMavros** - Emulator for Mavros
- **UnitySensors** - Comprehensive sensor simulation package
- **com.unity.perception** - Unity's machine learning perception tools
- **ROS-TCP-Connector** - ROS communication package
- **Crest** - Advanced ocean and water graphics simulation

### Installation steps

Please ensure that you have added your SSH key to your GitHub account, as the submodules are cloned via SSH. Please follow online tutorial/GPT to find out how to do so.

Open a new powershell terminal (Windows) and copy-paste the entire block below to clone the repository and its submodules:

```powershell
cd Documents; mkdir UnitySim; cd UnitySim
git clone --recurse-submodules git@github.com:NTU-Mecatron/UnityAuvSim.git
git clone git@github.com:NTU-Mecatron/UnityMavros.git
git clone git@github.com:NTU-Mecatron/com.unity.perception.git
git clone git@github.com:NTU-Mecatron/ROS-TCP-Connector.git
git clone git@github.com:NTU-Mecatron/UnitySensors.git

```

If you use other terminals, please adapt and run the commands separately.

> Note: We did not put all these submodules inside a single repository because of some bugs regarding Git LFS. You may also run into some warnings regarding Git LFS when cloning `UnitySensors` but you can safely ignore them.

### Opening the project in Unity

After cloning all of them, please open the Unity Hub application, click on "Add", and select the folder **`UnityAuvSim`** inside the `UnitySim` folder you just created. This will add the project to your Unity Hub.

![](images/opening_the_project.png)

UnityHub will prompt you to install `Editor version 6000.2.8f1` (the latest Unity6 version at the time of writing). Please accept and install this version as it is a Long-Term Support (LTS) version. If you already have this version installed, you can skip this step.

Upon opening the project for the first time, Unity will take some time to import all the assets and compile the scripts. Please be patient as this may take a few minutes. A lot of warnings may appear in the console, but you can safely press the "Clear" button to clear them all.

Proceed to open any of the scenes available in the `Assets/Scenes`. For example, upon opening the `sauvc` scene, you should see something like this:

![](images/sauvc_scene.png)

### Recommended development workflow

On Windows, many people often use Visual Studio as their IDE for Unity development. If you use this, when you install Visual Studio, please ensure that you have selected the "Game development with Unity" workload. Another option that works on all platforms is Visual Studio Code (VSCode). Please install the C# and Unity extension for VSCode to enable C# support.

To select your preferred IDE, go to `Edit -> Preferences -> External Tools` and select either Visual Studio or VSCode as your external script editor.

To track git changes, since we have many repositories (aka packages) in this project, it is recommended to open the entire `UnitySim` (the parent of the simulation) folder as a workspace in VSCode. This way, you can track changes across all repositories in one place, all in VSCode's Source Control tab.

![](images/vscode_workspace.png)

> Note: When you double click on a script in Unity, it will open the script in the folder of the simulation project (e.g., `UnityAuvSim`). This is different from opening the entire `UnitySim` folder as a workspace. You can still edit the script, but you will not be able to track git changes across all repositories in the Source Control tab. Hence, you can continue with this, and only open the entire `UnitySim` folder as a workspace when you want to track git changes and ready to commit.

### Git notes

A few things to note regarding git workflow:

1. **Make changes to a prefab, not to the scene**. For example, when you add something into the 'auv' game object, you may temporarily add it in the scene, but please remember to apply the changes to the prefab by clicking on the "Overrides" dropdown and selecting "Apply All". This will ensure that your changes are saved to the prefab asset, not the scene. If you make changes to the scene, it will be hard to track changes in git and will definitely lead to merge conflicts. Your PR will simply be rejected.

2. Open a new branch called `feature/xxx` or `fix/xxx` from the `main` branch when you want to add a new feature. Each branch should only contain one feature or bug fix, and should only contain commits and file changes related to that feature or bug fix. Committing irrelevant changes will lead to your PR being rejected, again.

3. If you are working on multiple features, open multiple branches, one for each feature **all from the `main` branch**. There is an important distinction between branching from `main` and branching from another feature branch. Make sure that all branches are created from `main`. This will make merging and code review much easier.

4. Unity sometimes makes changes to a lot of files which are not related to your feature or bug fix. Please do not commit these, unless you are sure that they are related to your feature or bug fix. If you are unsure, please ask the project manager.

5. Be aware which scripts/components/prefabs should be created in the main project (`UnityAuvSim`) and which should be created in the packages (e.g., `UnitySensors`). If you are unsure, please ask the project manager.