# Hadal

A 2D underwater exploration and survival game about discovering marine life. Swim through the Sunlight Zone, survive encounters with predators, build a bestiary, and use what you learn to challenge a quiz boss.

## Features

- Discover marine species and read descriptions, trivia, and real-world photos in the bestiary.
- Fight with a tail whip, earn experience, and choose skill upgrades when you level up.
- Use bioluminescence, camouflage, echolocation, venom, and chemosynthesis abilities.
- Explore with a minimap, depth display, and water that darkens as you descend.
- Complete the Sunlight Zone bestiary to unlock its knowledge-based boss encounter.

## Screenshots

Click a screenshot to view it at full size.

| Underwater exploration | Bestiary and skill journal |
| --- | --- |
| [![Player exploring the Sunlight Zone with health, experience, depth, and minimap UI](docs/screenshots/exploration.png)](docs/screenshots/exploration.png) | [![Bioluminescent Flare journal entry with a real-world photo, biology notes, and trivia](docs/screenshots/skills-journal.png)](docs/screenshots/skills-journal.png) |
| Track health, experience, depth, and nearby surroundings while swimming. | Browse biology notes, photos, trivia, and ability references. |

| Skill selection | Quiz boss encounter |
| --- | --- |
| [![Level-up screen offering three skill cards with descriptions and Select buttons](docs/screenshots/skill-selection.png)](docs/screenshots/skill-selection.png) | [![Quiz boss encounter showing the player, Goliath Grouper, health bars, and a multiple-choice question](docs/screenshots/quiz.png)](docs/screenshots/quiz.png) |
| Choose a new ability or upgrade when you level up. | Put your marine-life knowledge to the test with multiple-choice questions. |

| Main menu | End-of-dive summary |
| --- | --- |
| [![Hadal Descent title screen with Start and Exit options](docs/screenshots/main-menu.png)](docs/screenshots/main-menu.png) | [![Game-over screen showing survival time, level reached, deepest dive, and retry options](docs/screenshots/run-summary.png)](docs/screenshots/run-summary.png) |
| Start a dive from the ocean-themed title screen. | Review survival time, level reached, and deepest depth before retrying. |

## Development status

Early prototype, version **0.1.0**. The current content centers on the Sunlight Zone and its boss. Deeper zone names appear in the depth display, but they do not represent finished biomes. Progress currently lasts for the session; there is no save system.

## Run from source

You need **Unity Hub**, **Unity Editor 6000.3.14f1**, and **Git with Git LFS**. Install Windows Build Support if you want to create a Windows executable. A keyboard and mouse are the current control setup.

```sh
git lfs install
git clone https://github.com/kousagii/Hadal.git
cd Hadal
git lfs pull
```

1. In Unity Hub, choose **Add project from disk** and select the `Hadal` folder.
2. Open it with **6000.3.14f1** and let Unity finish importing assets and restoring packages.
3. Open `Assets/Scenes/StartScene.unity`, press **Play**, and start the game from the menu.

This checkout contains Unity source. Build outputs are not committed. To make an executable, open **File > Build Profiles**, select the Windows profile, and build into `Builds/Windows`. Keep `StartScene` before `MainScene` in the scene list. Run the resulting `.exe` with its accompanying data folder and runtime files beside it.

## Controls

| Action | Input |
| --- | --- |
| Swim | WASD or arrow keys |
| Tail whip | Left mouse button or J |
| Open/close bestiary | Tab |
| Pause/resume | Escape |
| Activate acquired skills | K, L, and ; in acquisition order, or their on-screen buttons |
| Choose upgrades / answer quiz | Mouse |

