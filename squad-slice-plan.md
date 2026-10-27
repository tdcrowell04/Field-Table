# Squad slice: build plan

*Written Oct 26, 2026. This is step 1 of the build order in `bible.md`. Every call in this plan is listed at the bottom for Triston's accept or veto. Nothing gets built until those are ruled.*

## The goal

Prove the core is fun. You select Marines, type orders, see the plan, commit, and watch a 1-minute turn play out with the dice in the open. Then you do it again until the mission ends and you get a debrief.

The bible says it best: *"If this isn't fun, nothing above it will be."* Triston decides when it's fun.

## What's in it

- **A generated map:** a fresh one for every mission, built by code from building blocks. No hand-built maps. *"cinder ford doesn't need to be hand built."*
- **One squad:** today's 13-Marine rifle squad (Alpha, Bravo, Charlie, and the squad leader) plus the platoon's corpsman.
- **One mission:** the bridge seizure (Cinder Ford), one of Triston's 25 mission start states, with add-ins switched in by code.
- **WEGO turns,** with a 1-minute default you can change in settings.
- **Typed orders,** in Plain English mode first.
- **Verbs:** all of them, from `verbs.md`. *"We're adding all of the verbs."*
- **Order preview:** a path on the map, an order card per unit, odds, and warnings. Nothing moves until you commit.
- **Readback** on the net, with pushback when an order can't work.
- **Dice:** you roll the big moments, everything else rolls itself on screen, and every roll goes in the combat log.
- **Fog of war:** you only see what your Marines can see, day or night, with NODs and thermals.
- **A smart coded enemy,** steered by the in-game AI.
- **A short debrief** at the end that shows what you couldn't see during the fight.

## What waits for later steps

Voice, multiplayer, the platoon, sci-fi and fantasy content, the pocket DM, mods, Radio and Loose modes, progression and the campaign. The slice is built so these can slot in without starting over (see "How it's built").

## How it's built

- **The simulation is plain C# with no Unity code in it.** Unity only draws the scene and takes your clicks and typing. This makes the rules easy to test, lets the game run a battle invisibly at high speed later (autoresolve), and lets the host's PC run the game for friends later (multiplayer).
- **Units, verbs, and maps are data files from day one,** not hard-wired into the code. That's what lets sci-fi, fantasy, and mods slot in later. A verb is already a recipe of pieces (move, play an animation, roll a die, apply an effect), which is the same thing modders will use.
- **Every roll comes from one seeded dice roller,** so any turn can be replayed exactly. That's what makes the Contest button and the debrief possible.
- **The local model reads orders from day one** (Qwen3 4B to start), locked by a grammar to real verbs, units, and grids, with nothing of Triston's personal license in the released game. Unclear orders get "say again." Orders the code doesn't cover go to the AI to decide, with the guardrails from the bible, and get written to a local log for now. The server for bug reports comes later.
- **A phone playtest page** runs the same simulation in a phone's browser, drawn as a simple top-down table, with Haiku reading typed orders as a stand-in for the local model. Nothing from it ships.
- **Figures start as rough 3D models from my Blender scripts,** in the look the figures page set: tilted sand table view, outlines, side rings, and yellow tracers.

## The pieces, in order

Each piece ends with something you can run or test.

1. **Project setup:** a Unity 6 project in the repo, a folder layout, and git settings for art files.
2. **Simulation core:** the map grid, terrain, units, the turn clock, the dice roller, and an event log, with tests.
3. **Map generator:** a fresh map for each mission from building blocks, using the map features ruled in round 8.
4. **Line of sight and fog of war:** who can see whom, day and night, and what you're allowed to know.
- **Phone playtest page:** after the generator, before movement.
5. **Movement:** paths around obstacles, with walking and running speeds by terrain.
6. **Fire:** hit chances, suppression, wounds, and ammo, all through the dice.
7. **Verbs:** every verb in `verbs.md` as data, plus triggers like "once Bravo reports in position."
8. **The WEGO turn:** orders, preview, commit, then the minute plays out for both sides at once.
9. **Enemy AI:** a smart coded enemy, steered by the in-game AI.
10. **The order reader:** the AI, locked by a grammar.
11. **The sand table view:** the tilted camera, the figures, rings, tracers, and the drone feed when a drone is up.
12. **The screen:** selecting units, the order box, order cards, the path overlay, the net log, the dice, and the combat log.
13. **The mission:** the bridge seizure start state, its add-ins, the brief, how it ends, and the debrief.
14. **Playtest and tune** until Triston says it's fun.

## Who does what

- **Claude:** writes the code and the Blender scripts, and pushes them to this repo.
- **Triston:** opens the project in Unity 6 on his PC, plays each piece, and finishes the models.

My workspace can't run Unity or Blender, so Triston is the one who sees each piece run. I'll keep the simulation testable without Unity, so most of it can still be checked here.

## Rulings on this plan

Ruled in ledger rounds 4 to 7 and logged in `bible.md`.

## Progress

- **Piece 1, project setup:** the repo side is done (folders, git settings, and the simulation packaged for Unity). Triston makes the Unity project with the steps in `SETUP.md`.
- **Piece 2, simulation core:** done and tested here. The map grid and grid references, terrain and units loaded from data, forces and who is hostile to whom, the 1-minute turn clock, the seeded dice roller, and the event log.
- **Piece 3, map generator:** done and tested here. Builds a fresh map from the 15 features for each mission; the same seed gives the same map. A 1,000-map stress run had no failures, about 30 ms per map. (A hand-built Cinder Ford map was made first and then removed in round 7.)
- **Piece 4, line of sight and fog of war:** done and tested here. Hills, buildings, walls, hedges, trees, and reeds block or thin sight; people in buildings see out the windows; spotting builds up over time; day, twilight, and night, with NODs and thermals. 86 tests passed at the time; 106 with the generator.
- **Piece 5, movement:** done and tested here. Routes on a 2-meter grid around deep water and walls; walk, run, and crawl at terrain speeds; teams spread out on arrival; swimming, drowning in full kit, and dropping gear to swim.
- **Piece 6, fire:** done and tested here. One d100 roll per burst, suppression and pinning, wounds and casualties, treating the wounded, and grenades and rockets that hit everyone in the blast. The numbers I picked go to Triston in ledger round 11.
- **Pieces 7 and 9, so far:** 16 of the verbs in `verbs.md` are built, and a simple coded enemy fights from where it starts and falls back when it's hurt or losing. The smart enemy steered by the in-game AI comes later.
- **Piece 13, so far:** the bridge seizure start state, its brief, and how it ends. 139 tests pass.
- **Phone playtest page:** built in `Playtest/` and published at https://claude.ai/artifact/A9AH1SGi5o5Rt2Uoe4bu9y. The same simulation runs in the phone's browser, Haiku reads typed orders, and each turn plays back on the sand table.
