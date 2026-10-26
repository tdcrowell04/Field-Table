# Squad slice: build plan

*Written Oct 26, 2026. This is step 1 of the build order in `bible.md`. Every call in this plan is listed at the bottom for Triston's accept or veto. Nothing gets built until those are ruled.*

## The goal

Prove the core is fun. You select Marines, type orders, see the plan, commit, and watch a 1-minute turn play out with the dice in the open. Then you do it again until the mission ends and you get a debrief.

The bible says it best: *"If this isn't fun, nothing above it will be."* Triston decides when it's fun.

## What's in it

- **One map:** Cinder Ford. A river, the bridge, buildings 1 to 6, a low wall, and trees, laid out like the figures page.
- **One squad:** today's 13-Marine rifle squad (Alpha, Bravo, Charlie, and the squad leader) plus the platoon's corpsman.
- **One mission:** seize the bridge. Enemy: a rifleman, a machine gunner with a PKM, and an RPG gunner. Some civilians.
- **WEGO turns,** with a 1-minute default you can change in settings.
- **Typed orders,** in Plain English mode first.
- **Verbs:** move, hold, overwatch, suppress, assault, report, and treat casualty.
- **Order preview:** a path on the map, an order card per unit, odds, and warnings. Nothing moves until you commit.
- **Readback** on the net, with pushback when an order can't work.
- **Dice:** you roll the big moments, everything else rolls itself on screen, and every roll goes in the combat log.
- **Fog of war:** you only see what your Marines can see.
- **Simple enemy AI** that fights back and keeps its own plan.
- **A short debrief** at the end that shows what you couldn't see during the fight.

## What waits for later steps

Voice, multiplayer, the platoon, sci-fi and fantasy, the pocket DM, mods, Radio and Loose modes, call for fire, carrying casualties, progression and the campaign. The slice is built so these can slot in without starting over (see "How it's built").

## How it's built

- **The simulation is plain C# with no Unity code in it.** Unity only draws the scene and takes your clicks and typing. This makes the rules easy to test, lets the game run a battle invisibly at high speed later (autoresolve), and lets the host's PC run the game for friends later (multiplayer).
- **Units, verbs, and maps are data files from day one,** not hard-wired into the code. That's what lets sci-fi, fantasy, and mods slot in later. A verb is already a recipe of pieces (move, play an animation, roll a die, apply an effect), which is the same thing modders will use.
- **Every roll comes from one seeded dice roller,** so any turn can be replayed exactly. That's what makes the Contest button and the debrief possible.
- **The order reader comes in two stages.** First a simple word-matching reader, so the game is playable and testable right away. Then the local AI model takes over, locked by a grammar to real verbs, units, and grids. Unclear orders get "say again." Orders the code doesn't cover go to the AI to decide, with the guardrails from the bible, and get written to a local log for now. The server for bug reports comes later.
- **Figures start as rough 3D models from my Blender scripts,** in the look the figures page set: tilted sand table view, outlines, side rings, and yellow tracers.

## The pieces, in order

Each piece ends with something you can run or test.

1. **Project setup:** a Unity 6 project in the repo, a folder layout, and git settings for art files.
2. **Simulation core:** the map grid, terrain, units, the turn clock, the dice roller, and an event log, with tests.
3. **Cinder Ford as data:** the river, the bridge, the buildings, the wall, and the trees.
4. **Line of sight and fog of war:** who can see whom, and what you're allowed to know.
5. **Movement:** paths around obstacles, with walking and running speeds by terrain.
6. **Fire:** hit chances, suppression, wounds, and ammo, all through the dice.
7. **Verbs:** the seven verbs as data, plus triggers like "once Bravo reports in position."
8. **The WEGO turn:** orders, preview, commit, then the minute plays out for both sides at once.
9. **Enemy AI:** a simple plan for the three enemies.
10. **The order reader:** word matching first, then the local AI model.
11. **The sand table view:** the tilted camera, the figures, rings, tracers, and the drone feed when a drone is up.
12. **The screen:** selecting units, the order box, order cards, the path overlay, the net log, the dice, and the combat log.
13. **The mission:** the brief, the objective, how it ends, and the debrief.
14. **Playtest and tune** until Triston says it's fun.

## Who does what

- **Claude:** writes the code and the Blender scripts, and pushes them to this repo.
- **Triston:** opens the project in Unity 6 on his PC, plays each piece, and finishes the models.

My workspace can't run Unity or Blender, so Triston is the one who sees each piece run. I'll keep the simulation testable without Unity, so most of it can still be checked here.

## Calls in this plan, for accept or veto

1. The slice uses the Marines and the Cinder Ford bridge seizure.
2. The simulation is plain C# kept apart from Unity.
3. Units, verbs, and maps are data files from day one.
4. Seven verbs for the slice: move, hold, overwatch, suppress, assault, report, and treat casualty. Call for fire and carry come later.
5. Plain English mode only in the slice; Radio and Loose come later.
6. A simple word-matching reader first, then the local AI model.
7. Orders the code doesn't cover go to a local log until the bug-report server exists.
8. The 14 pieces, in that order.
