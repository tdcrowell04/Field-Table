# Field Table: game bible

*Started Oct 16, 2026 · @Triston. Updated Oct 27, 2026 to match every ruling so far. Overruled ideas have been taken out.*

## Overview

A WEGO tactical game where you command units by talking to them, typed or by voice, and watch the fight play out. Think computer D&D: XCOM-adjacent, but with more people and more options for scale. It also has a pocket DM mode for tabletop RPG sessions with friends.

Pitch: *"A tabletop game you run by talking. Command Marines, a sci-fi squad, or a fantasy party, with every roll in the open."*

- Title: Field Table
- Settings at launch: Marines (today), sci-fi, and fantasy, each usable in whatever modes get released
- Status: Concept. Code work starts now. The Field Table web prototype is the live design lab.
- Engine: Unity 6, same as Conduits
- Relation to Conduits: A separate project and side work. Conduits is the main build, through its Steam Next Fest demo (October 2027). Field Table is what Triston works on in the meantime, while waiting on the Conduits playtest and learning guitar. *"Field table is side work but if it's not too difficult and it gets released first then great."*
- Roots: Kriegsspiel (Prussian officer training, 1820s), Chainmail and early D&D, and Marine Corps Gazette tactical decision games.

## Design pillars

Every feature has to serve at least one of these four. If it fights one, it gets cut or reworked.

1. Realism baked in. Movement rates, fire support timelines, suppression, casualty clocks, sustainment, and ROE work like the real thing. Outcomes follow historical norms, not movies.
2. Fair and visible. The player can always see why something happened. Odds are shown before commitment and rolls are shown after, like dice rolled in the open.
3. Talk like the radio. Orders are given in natural language, typed or spoken, the way a leader talks on the net. Units read back and push back like real Marines.
4. Honest fog of war. The player knows only what their units can see or are told. Bad intel is part of the game; a trap hidden in a mismatch between the map and the truth is not.

In sci-fi and fantasy, each setting follows its own rules just as strictly: consistent, visible, and no movie logic. The Marines follow real-world norms. "Talk like the radio" becomes each setting's own way of giving orders: comms chatter for sci-fi, spoken commands and spells for fantasy. Picking Radio mode comes with a guide to the structure it will and will not accept, with examples. Silly orders stay off the guide, so people figure them out on their own.

## Settings, modes, and group play

- **Settings:** Marines (today), sci-fi, and fantasy, all at launch.
- **Tactical mode:** WEGO turns, as described below.
- **Pocket DM mode:** tabletop RPG sessions. The game uses its own generic tabletop RPG and tabletop strategy rules, written for Field Table. Code covers as much of a typical campaign as possible, and the AI decides anything the code doesn't cover yet (see Simulation).
- **Solo, co-op, and versus.**
  - Co-op: each friend runs a squad, and one can be the platoon commander. The friends' voice chat is the radio net, so they have to actually coordinate.
  - Versus: each player can command either faction, or both can be the same faction. Each side sees only what its own units see. A turn timer keeps anyone from stalling.
  - Pocket DM with a group: either an AI-only DM, or a friend acts as DM with the AI helping and the friend able to overrule it.
- **Who is hostile:** each mission sets who is hostile to whom, so civilians can be a target (a bad-guy faction going after civilians that the player has to stop). If a mission doesn't say, different forces are enemies and civilians are no one's enemy.
- **Connecting:** online with friends through Steam. The host's PC runs the game, so there are no game servers to pay for at the start.
- **Session length:** tactical missions run 20 to 45 minutes. RPG sessions run longer, and the group sets the length.

## Core loop

The game runs in WEGO turns: both sides give orders, then both sides move at the same time for a set stretch while the player watches. Real enemies don't wait their turn, and it matches the rhythm of radio traffic. Turn length is a setting, with a 1-minute default.

1. Brief. A short BLUF frag; deeper details come over the net or from the map.
2. Order. Select units and give orders, typed or spoken.
3. Preview. The game shows each unit's parsed order, path, exposure, odds, and warnings.
4. Readback. Units and supporting elements read back on the net, with pushback where warranted.
5. Commit. Say or press execute, or fix individual orders first.
6. Watch. The turn plays out simultaneously, with radio traffic and visible rolls.
7. Repeat until the mission ends, then debrief: an after action review with the hidden truth revealed.

## Command interface

The player clicks a unit, then talks to it. Selecting first means the game always knows who an order is for, so the AI never has to guess.

- Selecting units: Click a unit on the map or in the unit list. Saying a callsign also works.
- Grid references: Click the map to drop the exact grid square into the order. A grid is always a grid. Clicking the map also gives the spot's six-digit coordinates, so the player never has to type them: *"Also clicking on the map should give the 6 digit coordinates so the player doesn't have to type them."* Three digits for meters east from the map's west edge, three for meters north from the south edge: 452 287 is 452 m east and 287 m north, good to 1 m. Tapping the ground offers the coordinates first; tapping a named place offers its name first. Grid squares still work.
- Map markers: anything important said on the radio puts a marker on the map, and clicking or hovering it shows what was said: *"I want there to be icons that pop up on the map for all spotting not just drone spotting. So anything important that gets said on the radio has a map marker that shows text when you click or hover it."* Enemies don't get one, since their see-through figure already marks where they were last seen; everything else does, fire positions included: *"I think for enemies it's already solved by the semi transparent enemy model, but other stuff should be marked including if you make fire positions and such."* Each kind has its own color and sign. A pin fades over 10 minutes and is gone after 20, and a new call of the same kind in the same spot replaces the old pin.
- Typed orders: Free text.
- Voice orders: Push-to-talk, using local speech-to-text (Whisper family). Primed with callsigns, the phonetic alphabet, building numbers, and the verb list. Spoken grids like "golf four" convert to G4. Added after typed orders work, since voice just produces text.
- Order modes: A difficulty selector of its own, separate from the odds setting.
  - Radio: proper radio format; sloppy orders get "say again." It comes with a guide to the structure it will and will not accept, with examples. Silly orders stay off the guide.
  - Plain English: normal talk.
  - Loose: anything goes and the AI sorts it out.
- Order styles: Radio talk, plain English, and spells (for fantasy). Players can type in other languages too, if it's not too hard.
- Verbs and blanks: The full verb list is in `verbs.md`: 86 verbs for tactical and tabletop RPG play, all built in the squad slice. *"We're adding all of the verbs."* Built so far (28): move, hold, fire, suppress, take cover, change posture, assault, withdraw, follow, observe, report, treat casualty, throw grenade, hide, drop, emote, bound, overwatch, throw smoke, clear, breach, fly drone, talk, search, carry, detain, share ammo, and request (CASEVAC). *"When will they come"* The rest come in batches, each tested before it goes on the page: the rest of the tactical verbs, then the RPG verbs. Verbs are hard-coded: move, assault, overwatch, suppress, call for fire, treat casualty, carry, hold, report, and so on. Each verb has blanks for who, where, what target, and when or on what condition. The AI picks the verb and fills the blanks, locked by a grammar so it can only output units, grids, and verbs that exist.
- How blanks: Speed, quiet or not, posture, distance to keep, which way to face, and when to open fire are blanks on a verb, not separate verbs.
- SOPs: Standing orders the unit follows without being told, set once for the unit and changed for a single mission when needed, as in the prototype.
- Formatted reports: The RTO, Doc, or team leaders build 9-lines, SALUTEs, and other formatted reports from what's already known; the player gives only the decisions that are theirs. *"I feel like I want the reports to be formatted as a choice in radio mode, doing the reports at all a choice in plain english, and a choice in free form but not really necessary."*
- Out-of-game questions: A separate box for asking the game master questions out of character, without spending game time.
- Silly orders: Funny stuff like dancing is a real thing you can do, in every order mode, for the actual D&D feel. Nothing illegal. Radio mode still wants these in proper radio format.
- Unclear orders: Answered in-world, like "Bravo, say again, which building?" The game never guesses which unit, building, or grid an order means.
- Everyday orders count: a Marine or team and a place with no verb, like "Charlie, C3", means move there, and "the woods at C3" means the nearest woods in C3. The playtest doesn't ask for radio talk: *"I'm just saying I'm not trying to do all the radio speak for the play test that would get exhausting."*
- Orders the code doesn't cover: The AI decides what happens, which doesn't always mean the unit does it. It can give a reason it isn't possible right now, or flatly refuse if the order is insulting or illegal. If it's something absolutely ridiculous, the unit answers in a clever way that fits the setting, never cringy. A bug report gets filed quietly (see AI usage), and the most-requested orders get added as real verbs.

Example: "Alpha, once Charlie takes over, move on 4, only once Bravo and Charlie confirm" becomes assault, unit Alpha, target building 4, trigger: Bravo and Charlie report in position.

## Order preview, odds, and dice

No order fires directly. Every order produces a plan, the plan is shown, and nothing moves until the player commits.

- Map overlay: Planned paths, with stretches visible to known enemies shown in red plus exposure time. Fire orders show their odds on the target. Indirect fire shows its impact area and dispersion.
- Order cards: One per unit, showing the parsed verb, target, trigger, ETA, odds, and warnings. This is where a mistranscribed order gets caught before it costs anything.
- Fixing orders: Voice ("Bravo, belay that, building 1") updates one card only. Paths and targets can also be dragged with the mouse.
- Safety checks: Fire into a square holding buildings, civilians, or friendlies is flagged by the FDC or observer and needs confirmation.
- Dice: Every roll is a die (d20, d6, or whatever fits). The player rolls the big moments: an attack, a skill check, a called shot. Everything else rolls itself on screen and shows in the log. Settings let players roll everything or roll nothing.
- Dice skins: Cool dice skins unlock with achievements.
- Combat log: After the turn, every roll is shown with its odds and result.

Odds are calculated only against known contacts and labeled that way. A clean plan can still get hit by a gun nobody spotted.

| Difficulty | Odds shown as |
|---|---|
| Training | Exact percentages, XCOM style |
| Realistic | Plain judgments: good shot, marginal, long odds |
| Hardcore | No odds; map and judgment only |

## Simulation

Code owns every hard fact it covers; the AI handles language. When an order falls outside what the code covers, the AI decides it, and a quiet bug report flags it so the code can cover it later. Keeping the facts in code is what keeps rulings fair, because a model can never misplace a round or a unit the code tracks.

| Layer | Owns |
|---|---|
| Game code (simulation) | Movement, terrain, passability, line of sight, hit chances, suppression, fires and dispersion, ammo, casualties, the clock |
| Enemy AI (coded, steered by the in-game AI) | Enemy decisions: a smart coded enemy (behavior trees or utility AI) that's fast and consistent, steered by the in-game AI. *"Let's write a smart coed enemy that then gets steered by the in game AI."* |
| Language AI | Turning orders into verbs and blanks, voicing units and higher on the net, writing the debrief from logged facts, and deciding orders the code doesn't cover yet |
| Validation (code) | Checking each command is possible: time, ammo, friendlies in the grid. Failures become in-world pushback |

- Memory: The game holds the mission log; the AI only gets the small slice each call needs.

Fire and wounds, as built (ledger rounds 11 and 12):

- A shot: one d100 roll to hit a burst. On a hit, a second die says how many rounds of the burst hit (1 up to the burst size), and each one gets its own wound roll, so a machine gun burst is far deadlier than a rifle's.
- Getting pinned: fire landing near someone pins them down. At 80% pinned they go prone and won't get up and move until it fades, about 8% a second.
- Where a hit lands (a d20) decides how bad it is: 1–2 head, killed unless the helmet stops it (3 in 20, about 1 in 7); 3–9 chest, half stop on the plate (knocked flat and pinned, not hurt), the rest are down and bleed out in 10 minutes instead of 15; 10–12 belly, down, 10 minutes; 13–16 arm, wounded, shoots at half accuracy; 17–20 leg, wounded, can't walk and has to be carried. *"And then casevac depending on where hit and everything? Because every GSW is different"* And the helmet: *"There should be the one offs where the helmet saves"*
- Getting hit again: hit in the arm or leg while wounded, you go down; hit in the chest or belly while down, you're killed. A plate or helmet still stops a round on someone who's down, but they don't call out "I'm good."
- Patching up: treatment patches a wounded Marine up, but an arm hit still shoots at half and a leg hit still can't walk.
- The enemy wears no helmets or plates.
- The clock on a casualty: a Marine who's down dies after 15 minutes without treatment (10 for the chest or belly). Doc stabilizes someone in 60 seconds; any other Marine takes 3 minutes.
- CASEVAC: a Marine who's down is out of the fight until evacuated. Two Marines carry a casualty at half walking speed to a casualty collection point you pick; one alone goes at a quarter. Lead calls in CASEVAC with a 9-line that Doc fills in from what's known; you only pick the landing zone. A helicopter comes, or a ground ambulance if the landing zone is on a road or trail, 15 to 30 minutes after the call (a die picks). It waits 3 minutes and takes everyone down or unable to walk within 25 m, and the end screen counts who got out.
- Blasts: grenades and rockets roll for everyone inside the blast, friends included. A miss lands a few meters off in a random direction. *"There should be a difference with HE and AP"*
- HE and AP: HE gives the full blast (5 to 6 m), but a wall or building between you and the blast protects you. AP (armor-piercing, like the RPG's anti-tank rocket) has a small blast (2 m), but it punches through walls, so cover doesn't help whoever is behind the spot it hits. The M320 carries 5 HE and 3 AP; the RPG gunner 1 HE and 2 AP. Unless you name one, the launcher uses AP on someone in a building or behind a wall, and HE otherwise. Naming HE or AP fires one of those, then the Marine keeps firing his rifle: "Novak, HE on building 4." Armored vehicles get AP too: *"Yep and armored vehicles get AP too."*
- Deep water: carrying over 15 kg into deep water and you go under; 20 seconds later you drown. Dropping gear takes a Marine down to 12 kg, light enough to swim.
- The enemy, until the in-game AI steers it: it fights from where it starts, and falls back when it's hurt and pinned, or when half its side is down. Civilians get down when the shooting starts.
- Friendly fire: a Marine hit by his own side is called on the net ("Check fire! Check fire! Reyes is hit by our own fire.") and drops the mission's rating one grade, as far as F. The end screen lists it.

The verbs from ledger round 13, as built:

- Bound: halves take turns rushing 50 m at a run while the other half kneels and covers. With two or more teams, the teams take turns; inside one team, every other Marine.
- Overwatch: the Marine stays where he is (or goes where you say), faces the area you name, and shoots anything there before anything else.
- Smoke: one smoke grenade each. A cloud 20 m across that hangs 90 seconds. Eyes and NODs can't see through it; thermals can. Told to a team, one Marine throws it. With no spot named, it goes 15 m toward the nearest known enemy.
- Clear: 10 seconds for each 25 square meters of the building, and it goes on while anyone hostile is still inside. Everyone inside is found. Inside the same building, the walls don't count as cover.
- Breach: 20 seconds to open a 3 m gap in a wall, hedge, building, or cliff, which leaves rubble. Doors come later, with the building insides.
- The drone: 15 seconds to launch. It flies 12 m a second, 60 m up, sees with a thermal, and the enemy can't spot or shoot it yet. The battery lasts 20 minutes, then it flies home and lands. The operator kneels at the controller and doesn't fight while it's up. What it spots, his team calls in: "Charlie: Contact on the drone, ..."
- Talk: 20 seconds next to them. A civilian tells what they've seen of the enemy, and the squad now knows it. *"There should be need of interpereter sometimes and sometimes the enemy should talk depending on what's going on. Again, stop making things so narrow, the scenarios in the artifact game were so dynamic and fun because the situation responded to how I handled everything."* So (ledger round 14): some missions put the squad among people who don't speak English, and then talking to them needs the interpreter (a new squad member) close by. The enemy talks when it fits: a wounded or detained fighter can be questioned and may lie, refuse, or give something up; fighters shout to each other; one alone and pinned may surrender. The AI plays each person and writes what they say. Code decides what they know and whether it's true, so no one can invent a fact.
- Search: 20 seconds. A place: finds anyone within 40 m who can be seen at all, hidden or not. A person (down, detained, or a civilian): takes an enemy's weapon and ammo.
- Detain: 30 seconds next to them. Civilians, enemies who are down, or wounded enemies pinned flat. A detained person can't do anything.
- Share ammo: 30 seconds. Everyone in the team within 25 m with the same kind of ammo ends up at the same share of a full load, so the automatic rifleman still carries the most. *"Yes but ammo types matter too."* Marines share only with others carrying the same round: 5.56 with 5.56. M320 rounds go only to another M320, split evenly.

## Look

- Main view: the sand table. The drone feed only shows when you actually have a drone up.
- Both views are tipped at a slight angle so the figures can be seen.
- Simple models of individual units that actually move and shoot, drawn bigger than true scale, like tabletop miniatures.
- Marines look like today's Marines. The Marine rifle squad is today's 13-Marine squad: a squad leader and three teams of four (team leader with the grenade launcher, automatic rifleman, assistant automatic rifleman, rifleman), with one rifleman trained to fly the squad's attack drones, plus the platoon's corpsman (M4 carbine, labeled Navy).
- Marine gear: high-cut helmet in a camo cover with a night-vision mount and goggles flipped up, ear and eye protection, coyote plate carrier with pouches, knee pads, dark gloves, coyote boots, M27 with a suppressor and scope.
- Figures must not blend into the ground: a dark outline on the sand table, a white glow on the drone feed, and side rings under every figure on both views (blue for Marines, red for the enemy, white for civilians).
- All tracers are one warm yellow (#ffd166), the same for both sides.
- Camo matches the map.
- Map symbols: above company level, units show as map symbols. At company level and below, it's just the figures.
- Last known positions: a unit you can't see right now looks different from one you can, so you know it's only where it was last seen. *"There should be something indicating if they're not still seen at a spot if it's just last known."* It shows as a faded figure with how long ago it was seen ("last seen 2 min ago"), fading more the older it gets.
- 3D, from the angled camera: *"Ensure the maps are 3D as well so units can go in and out and they can hide and all that."* *"And viewed from that angled camera from earlier"*
- The figures page's look: *"I really liked how the html looked that you gave me earlier too btw, with some tweaks that could look super cool"* The phone playtest page uses it.
- The game's own look: real 3D ground and buildings (hills, floors, going in and out), with drawn figures standing on them like paper miniatures, the way the figures page works. Modders draw a figure instead of building a 3D model. (Ledger round 10, option c.)
- Buildings: every building has doors and windows. Units go in and out through doors, or through a wall they breach. Inside, a unit is either at a window (sees out, can be seen and shot at, gets the wall's cover) or back from the windows (hidden, can't see out). Village buildings have 1 or 2 floors, town buildings 1 to 3; upstairs windows see farther and over walls and hedges.
- Hiding: tall crops, reeds, trees, gullies, and walls hide you, more so lying down and holding still. A unit told to hide gets low, holds still, and picks the best concealment nearby.

## Maps

- Every map is generated by code for each mission, from building blocks, reusing the same models. No hand-built maps. *"cinder ford doesn't need to be hand built."*
- Hills come in random heights. *"Random heights for random hills, some are up to 150, some are only 15. That's what makes this INTERESTING is the RANDOMNESS. You can always tell the game master you want a specific hill size you know."*
- Buildings are made from a small kit of parts (walls, roofs, doors) snapped together into any size, at real size.
- Creeks are knee deep and fordable anywhere, at 40% of walking speed, unless a map marks one deeper.
- Terrain values (speed, cover, height, how much it blocks sight, how well it hides you) live in `Data/terrain.json`. Game data lives in `Data/`, the same way a mod's files would.
- The 15 map features, any of which can be in a map but don't have to be: hills, ridgelines and cliffs, rivers too deep to wade, creeks, lakes and ponds, marsh and reeds, forest, woodlots and orchards, hedgerows and stone walls, farm fields (plowed, stubble, and tall crops that hide you), roads and trails, bridges, villages and farms, towns, and gullies. Sci-fi and fantasy can add their own later.
- Deep water: crossing a river takes a bridge, a boat, or a swim. *"don't miss the chance for a player to tell a fully kitted up marine to swim across the river and they can't and drown. Everything should be that custom."*
- Each map gets 3 to 11 of the 15 features at random, plus whatever the start state requires (a bridge seizure needs water and a bridge). Maps are 8 by 6 squares of 100 m by default; a start state can set its own. Maps are big enough for the squad to start well back, and always have common-sense cover and avenues of approach, unless the scenario is about having none: *"Bigger maps plus ensuring there's common sense cover and avenues of approach, unless part of the scenario is there being no cover"*
- With bridges in play, every road that crosses water gets a bridge. Without them, roads keep to one bank of a river and cross creeks at fords. Trails never run through deep water.
- Villages have 4 to 9 buildings; towns are a street grid with 2 to 6 buildings per block. Both prefer a road but move off it if they must. Building sizes from the kit: shed 6×5 m, house 10×8, farmhouse 14×10, long house 18×9, barn 20×12, warehouse 30×20 (towns only). Buildings are numbered north to south, then west to east.
- Cliffs run along part of a ridge's face, broken wherever a road, a settlement, or water crosses. Place names come from `Data/names.json`, so each setting can bring its own.
- Before a mission, players can ask the game master for anything specific (a 100 m hill, a town, no river) and the generator builds it in.
- Maps are written down in one map format, like a save file, so generated maps can be saved and replayed, players can write maps by hand for their own missions, and maps can be shared on Steam Workshop.

## Sight and spotting

- Line of sight: eye height 1.7 m standing, 1.1 m kneeling, 0.4 m prone, checked against the ground and everything on it every meter. Hills, buildings, and walls block it; trees, hedges, and reeds thin it and then block it. Someone inside a building sees out its windows and can be seen at them.
- Spotting builds up while someone is in view, with no dice: about half a second for someone standing in the open at 100 m, 6 seconds at 600 m, much slower if they're prone, still, or hidden.
- Day and night are both in from the start: day, twilight, and night, with NODs and thermals. *"No dude it was already in the little artifact game you made, why can't you include it in something you're actually thinking about?"*
- Ranges: the naked eye spots a person at 1,000 m by day, 400 m at twilight, 50 m at night. NODs reach 300 m at twilight or night, spot 40% slower, and don't help by day. A thermal reaches 1,500 m day or night and sees body heat through reeds and grass, not through trees or walls.
- Friction: in about one mission in five, one or two things go wrong on their own (a Marine left his NODs behind, a radio battery dies, a weapon jams, a guide is late, the map is out of date), picked at random from a list. The debrief says what happened. *"let's do more like 20%"*
- Gear isn't guaranteed. Each start state sets who carries what, and sometimes someone is missing something. *"Not necessarily, what if you were out for a short op and it went bad and now someone didn't pack his nods because he didn't want to have to worry aobut it. Shit like that shouldn't happen every game or people will get sick of it but shit should just happen like that, not even just in gear but in other stuff too."*
- Illumination rounds and flares light an area up to twilight for a short time (built with call for fire).
- Firing at night shows the shooter to anyone with a line of sight, even past eye range (built with fire). *"Unless using something to compensate for that if the gear is meant for night"*

## Scale and layers

Every unit is an individual model up to company level. Above that, units become map symbols, and the player can drop into any fight below or let it autoresolve.

| Layer | What a model represents | How it plays |
|---|---|---|
| Fire team to platoon | Each Marine | Full WEGO with individual models |
| Company | Each Marine (about 180 to 200 plus enemy) | Full WEGO; heavy checks only near the enemy, simpler models at distance |
| Battalion and up | Companies and platoons as map symbols | Abstract layer; drill down into any company fight or autoresolve it |

Autoresolve rule: At company level and below, autoresolve is the detailed battle run invisibly at high speed, with AI subordinates deciding as the player would. Watching or skipping gives the same results. Above company, the abstract model is tuned against thousands of those invisible battles so it stays grounded in the same simulation.

## Progression and campaign

- Units have names, pick up small realistic skills, and can die for good.
- The player's record as a leader is kept too.
- Optional ironman setting: saves can't be reloaded.
- Three kinds of play: a linked campaign (missions in a row where results carry over: the dead stay dead and the wounded sit out), standalone one-off missions, and generated missions the game builds fresh from pieces each time.

## Fairness rules

These come straight from problems hit while playing the Field Table prototype. Each one is non-negotiable.

1. A grid is the grid the player sees. Fire lands in the square called, with normal dispersion inside it. It never drifts to another square.
2. Bad intel is the intel's fault. If a target wasn't where friendlies reported, rounds land where called and miss. Nothing in another square gets hit.
3. Danger gets flagged first. Fire near buildings, civilians, or friendlies needs player confirmation, as a real FDC would ask.
4. "I can't see it" is a valid report. Saying you lack observation is accurate reporting, answered in-world ("No one has eyes on the bridge"), never a fault by itself.
5. Judge by what was known then. Debriefs grade decisions on the information the player had at that moment, never on hidden truth revealed afterward.
6. Credit the asks. Requests for help that higher refused are credited, and routes or timelines set by higher can't count against the player.
7. Rulings can be contested. A rewind-and-review option, like the prototype's Contest button: the player argues the rule, not the result.

## Missions and content

Every mission played in the Field Table prototype is a candidate mission for the game, kept or cut as the design matures.

- Mission start states: Triston writes 25. Add-in pieces (a sniper, an IED, civilians wandering in, reinforcements, bad weather, comms dropping) get switched in and out by code depending on what's happening, like Left 4 Dead's AI Director. The in-game AI does the briefings, radio talk, and reactions.
- The director starts now, in the playtest (ledger round 14). It watches the mission and switches in add-ins when they fit: reinforcements if you're slow, civilians running if the shooting gets close, a sniper or IED on the obvious route, the enemy pulling back if you flank them, a radio dropping out. The in-game AI picks which, inside limits the code sets, and voices it.
- When the AI makes a mission, it fills a mission template and code checks the result. *"Yeah it uses the mission start states and fills in pre made adders to make it unique."*
- Player-built missions and campaigns: players can bring them built all the way or only partly. The AI fills in whatever they leave out, and they share them on Steam Workshop.
- Cinder Ford: the bridge seizure is one of the 25 start states, played on a freshly generated map each time.
  - Main objective: seize and hold the bridge.
  - Enemy: a different number and mix each mission, so "unknown strength" in the brief means something. They set up on the far bank. Not always the same kinds: *"Not always PKM and rpg. Random."* The mix should fit the mission: *"I really think that enemy squad composition should make sense for what the scenario is and what the objective is and what the enemy objective is."* So (ledger round 14) the enemy gets its own goal, and the goal picks who's there and where. A die picks one: hold the bridge (riflemen dug in near it, a machine gun on the approach), delay you (a few fighters far back who fall back across), blow it (an engineer with charges plus guards, so "the bridge left standing" can now fail), or ambush (they let you onto the bridge, then open up). The brief hints, but doesn't say which. How many each goal brings, where they set up, and what sets them off are Claude's numbers, up for accept or veto in ledger round 15. The end screen says what the goal was.
  - Where the squad starts: on the near bank, out of the enemy's sight, not lined up: *"the scenarios should make sense and be dynamic. But I would think we wouldn't have the squad starting directly in the enemy's sight, all lined up right next to each other"* As close to 300 m from the bridge as the cover allows, never under 150 m. Fire teams in wedges facing the bridge, one behind another, and anyone the enemy could see moves to the nearest hidden spot within 30 m.
  - A second bridge: some maps have another bridge over the same river. The squad can start where it's a way round, so you can cross there and flank.
  - Civilians: 3 to 5, each in a house in the village by the bridge.
  - How it ends: you lose with 7 Marines down, or after turn 40. You win with a Marine within 15 m of the bridge and no enemy still fighting within 250 m, at the end of two turns in a row.
  - Secondary objectives: no civilians hurt and the bridge left standing (both from the commander's intent), and every Marine comes home alive. An enemy out to blow the bridge can now break the second one.
  - The squad brings an interpreter (ledger round 14), because the villagers and the fighters don't speak English.
  - Secondary objectives don't change winning or losing. The quick brief shows how you're doing on each during the mission, and the end screen grades each one. *"In order to get the best rating you have to complete the other objectives"* The best rating is S+: *"S+ as highest"* Under it (ledger round 13, (b)): S+ with every secondary objective met, then S, A, and B as fewer are met, and C for winning with a Marine killed. A loss is F. Friendly fire lowers it: *"Yeah and friendly fire incidents or things like that also detract."* Each Marine hit by our own fire drops a win one grade, all the way to F: *"Down to F"*
  - Marine names: last names drawn from a list of 24 for each mission, including Reyes, Lund, Ortiz, Hale, and Kim.
- Quick brief: the brief opens on a short version: the main objective, the secondary objectives, how you win and lose, the rules of engagement, and the enemy seen so far, each objective with a check or a cross as the mission goes. A Full brief button shows the whole thing. A new mission opens on the quick brief instead of the net, and the brief doesn't fill the start of the radio traffic; the net starts with the first orders.

- Design lab: The Field Table prototype is free playtesting of the core idea. Note what felt fair, what didn't, and which rulings to veto.
- Branches: Each played mission's decision points become its branches. Example, Operation Cinder Ford: the PKM, the unarmed pack on the bridge, and the two enemy wounded each lead to different outcomes depending on the player's choices.
- Radio lines: Written as templates with blanks, like "Lead, [unit]. Set at [location], eyes on [target]." These can be voice-acted, with optional AI flavor on top.
- Calls on their own: Marines call out without being asked ("Taking fire! Enemy machine gunner at F4.", "I'm hit!", "Man down!", "Alpha: Set at C4.", "I'm black on ammo!"). Each team calls a contact once, and again only if it moves or comes back. A contact in a new grid square counts as moving, and one out of sight for 30 seconds or more counts as coming back.
- Models: Claude writes Blender scripts that build rough base models. Triston finishes them.

## Mods

Modders can make their own experiences and share them with people.

- What modders can change: units, factions, maps, missions, verbs, radio lines, art, rules, map symbols, and order language (like spells).
- Safety: data and art only at first, no custom code, because code mods can carry viruses when people share them.
- Custom verbs: verb recipes. Modders build new verbs from pieces the game already has (move, play an animation, roll a die, apply an effect), so a mod can't touch the player's PC. The most popular recipes on Steam Workshop get baked into the game properly. Locked-down scripting, with files and the internet blocked, may come later.
- Custom rules: modders can make custom rules with the existing verbs.
- Custom blanks: modders can add their own blanks for how a verb is done (like "zigzag"), built from the same pieces as verb recipes.
- Custom models and animations: *"custom models only need to have one moving animation, it never speeds up or slows down the animation or anything like that. Any custom emotes can be programmed in, and attached to a verb. People can get as intricate or simple with animations as they want to. Or they can use our animations."*
- Poses for a custom model with one animation: a model with a standard human skeleton can use our animations for kneeling, prone, firing, and down. One without is shown like a tabletop miniature: lowered, laid flat, or tipped over.
- Sharing: Steam Workshop.

## AI usage and technical approach

Live AI is allowed in this game. This project is separate from Conduits' no-AI-assets rule.

- Local models: Run on the player's PC through LLM for Unity or llama-cpp-unity (both built on llama.cpp). Candidates: Qwen3 (4B to 14B), Phi-4, gpt-oss-20b, Gemma 4. Check each license before shipping.
- Default and minimum spec: A small model by default, starting with Qwen3 4B (Apache 2.0 license, free to ship in a paid game). Newer models and their licenses get rechecked before release. The minimum is a graphics card with 8 GB of memory.
- Order reader: The local model reads orders from day one, so what gets tested is what ships. Nothing of Triston's personal license goes in the game. The open-source parts (the model, LLM for Unity, llama.cpp) get their license notices in the credits.
- Structured output: A GBNF grammar locks the model to valid verbs, units, and grids.
- GPU budget: Roughly 3 GB for a 4B model, 5 to 6 GB for 8B, 9 to 10 GB for 14B, shared with graphics. The sim pauses or slows while the model thinks, which fits radio delay.
- Speech: Whisper-family speech-to-text, a few hundred megabytes, push-to-talk.
- Shipping: Model files download on first launch. Local AI only at launch. The model file stays out of the repo; during development it lives only on Triston's PC.
- Players' own AI models: an advanced setting to load their own local model file. Local files only, no cloud keys. The content filter always stays on. In co-op and versus, everyone uses the host's model. Labeled "use at your own risk," with a short list of tested models.
- Phone playtesting: a playtest page runs the same simulation in a phone's browser, with Haiku reading typed orders as a stand-in for the local model. Nothing from it ships. *"Use Haiku for this please so I don't eat up my usage and it should feel closer to what's in the final thing right"*
  - You open it from a link on your phone, like the ledger. Nothing to install.
  - It runs the same simulation code the game uses, built for the browser with .NET's WebAssembly tools.
  - Its code lives in the repo, in `Playtest/`.
  - What it has: the figures-page look on a generated bridge-seizure map, the 13-Marine squad and its interpreter, typed orders read by Haiku, and turns that play out.
  - Figures are drawn 3 to 4 times life size so you can see them, and never smaller than a thumbnail. Buildings, trees, and the ground are true size.
  - Figures always draw on top of trees, so you never lose a Marine under the canopy. A building with someone inside turns see-through.
  - Daylight for now; night views next, once the page works on Triston's phone.
  - Order preview: Send has Haiku read the orders, then shows each Marine's route and any problem before anything happens. Play turn commits them and plays one minute. Orders that can't be done are dropped with a "say again."
  - Playback at 4 times speed (15 seconds a turn), with 2×, 8×, Pause, and Skip. Speeding up a turn speeds up only the movement across the map, never the animations: *"I don't want the time speed up to make the animations faster or slower just the actual movement across the map"*
  - Every roll that touches one of your Marines goes in the Dice tab. The big ones (blasts and how bad a hit is) pop up on the table as they happen.
  - If Haiku isn't available, a simple built-in reader takes orders: who, the verb words, place names, and grid squares.
  - The page saves to its own database what was typed, how it was read, anything not covered yet, and how each turn ended. Nothing else.
  - On a phone turned sideways (any screen wider than it is tall), the map goes on the left, and the net, squad, brief, dice, and order box stack on the right.
  - The map can go full screen, with everything else below it: *"I want the map to be able to be fullscreen with the other stuff below it"* (how it works is in ledger round 15)
  - Radio calls about a place leave a pin there; tap one to read the call. Tapping the ground shows its six-digit coordinates with a button that puts them in the order box. The end screen shows the rating.
  - Haiku also plays the director, picking an add-in or none before each turn from the ones the code says fit, and gives villagers, prisoners, and shouting fighters their own words. The code decides what they know; a line that changes or drops a square, a number, or a name is thrown out and the code's own words stand.
- Cloud: Added later only if lots of players can't run the local model. If it comes, it runs through a backend we control, never with a key inside the game.
- Guardrails: A hard no on sexual, racist, illegal, or similar content in anything the AI writes. The grammar lock keeps parsed orders to real verbs, units, and grids, and a filter covers anything the AI writes freely (orders the code doesn't cover, and the pocket DM).
- Bug reports: When a player is connected, orders the code doesn't cover get reported quietly. Reports send the order text and the game state, never voice recordings. Players are told in the privacy policy and get an off switch in settings. No popup during play.
- Steam: Disclose the live AI on the store page, with its guardrails (the grammar lock, the content filter, and the bug-report off switch). Add a line for the models: "base models made with AI help, finished by hand." Store page art and trailers are made by hand. Claude's help writing the game's code counts as a developer tool, which Steam doesn't ask about.

## Build order

Each step is a complete, playable game on its own. Don't start a step until the one before it is fun.

1. Squad slice: One generated map, one squad, one mission (the bridge seizure), WEGO turns, typed orders, verbs and blanks. If this isn't fun, nothing above it will be.
2. Voice: Push-to-talk on top of the typed path, with readback.
3. Co-op and versus at squad level.
4. Platoon: Three squads plus attachments; the radio net and platoon sergeant problems.
5. Release on Steam Early Access at platoon size, with all three settings (Marines, sci-fi, fantasy).
   - Later, after the PC Early Access release: a mobile version for newer phones only (8 GB of memory or more), with mod sharing and online play through other services, since Steam isn't on phones.
6. Mods and pocket DM during Early Access.
7. Company: Full individual models and the first real autoresolve.
8. Battalion and up: The abstract layer with drill-down into any company fight.

Price: decided closer to release.

## Decision log

Every call gets ruled before anything is built. Rulings only; overruled ideas are taken out.

### 2026-10-16

| Decision | Ruling |
|---|---|
| Separate project from Conduits; live AI allowed in this game | Accepted |
| WEGO turns: simultaneous movement after orders | Accepted |
| Click a unit to select it, then type or speak to it | Accepted |
| Click the map to insert exact grid references | Accepted |
| Verbs are hard-coded; the AI picks verbs and fills blanks (on 10/26 the AI also got to decide orders the code doesn't cover yet) | Accepted |
| Code owns movement, terrain, passability, and hit chances | Accepted |
| Simple models that move and shoot, not symbols | Accepted |
| Individual models up to company; abstract counters above | Accepted |
| Drill down into any lower fight or autoresolve it | Accepted |
| Field Table missions feed the game's content | Accepted |
| Order preview with overlay, order cards, readback, and execute | Accepted |
| Push-to-talk voice orders with readback | Accepted |
| Odds display set by difficulty: exact, plain judgments, or none | Accepted |

### 2026-10-26: the figures page

| Ruling | How it was made |
|---|---|
| Both views are tipped at a slight angle so the figures can be seen | Triston: *"Can you do it slightly at an angle so you can actually see the models just a little bit"*; then picked "Both" |
| The Marines look like today's Marines | Triston: *"make them look more 'modern day marine'"* |
| Today's 13-Marine rifle squad, plus the platoon's corpsman | Triston picked "Today's squad" |
| The figures must not blend into the ground | Triston: *"Just ensure the models don't blend in TOO well"* |
| Side rings under every figure: blue for Marines, red for the enemy, white for civilians | Accepted |
| The side rings show on the drone feed too | Triston's ruling |
| All tracers are one color, the same for both sides | Triston picked "All one color" |
| That one color is a warm yellow (#ffd166) | Accepted |
| The camo matches the map, with a switch on the page between green and dry ground | Triston picked "Match the map", then "A switch for both" |
| The page opens on dry ground, then remembers the last pick | Accepted |
| The turn at Cinder Ford is made up to show the figures | Accepted |
| The page opens with both views side by side | Accepted |
| The team leader carries the grenade launcher; no separate grenadier | Accepted |
| Each team of four: team leader, automatic rifleman, assistant automatic rifleman, rifleman. Charlie's rifleman is the drone operator | Accepted |
| The corpsman carries an M4 carbine and is labeled Navy | Accepted |
| The Marine gear as listed under Look | Accepted |
| Charlie holds at building 5 with its drone over building 6 | Accepted |
| Figures are drawn bigger than true scale, like tabletop miniatures | Accepted |

### 2026-10-26: direction

| Decision | Ruling |
|---|---|
| Release timing | *"Field table is side work but if it's not too difficult and it gets released first then great. I'm waiting until I finish my guitar learning for Conduits anyways."* |
| Group play | *"I want group play both co op and vs"* |
| Order modes | *"Yeah but as a separate difficulty selector from the other one"* |
| Pocket DM | A separate mode where the AI decides outcomes for free-form play. *"this can be looped into the code as well, we can come up with all of the code for what would be in your typical D&D campaign and ensure we cover all of what the game needs with code"* |
| Orders the code doesn't cover | *"The AI should decide it but an automatic bug report gets filed quietly if they're connected to the server and we can add in that instance after."* Round 2: accepted for every setting. *"AI decides and if it's something absolutely ridiculous it just responds in a setting appropriate clever but not cringy way."* |
| Versus | *"Either or, or both can be the same faction."* |
| Code work starts now | *"Yah why not"* |
| Mods | *"mod support and custom units and factions and symbols and language should be something that modders can do to make their own experiences and share them with people."* |
| The repo holds the handoff and the figures page, at the top level | *"Also yes to this repo"*; ledger: accepted |
| The bible lives in `bible.md`, updated each time something is ruled | Accepted. *"Also remove any mention of things that are overruled."* |

### 2026-10-26: the ledger

| Decision | Ruling | Triston's words |
|---|---|---|
| Settings at launch | Marines (today), sci-fi, and fantasy, all released at once | *"No let's release it all at once. We can do 1 Marines, 1 sci-fi, and 1 fantasy all usable in whatever modes get released."* Round 2: confirmed these are the three eras. |
| Title | "Field Table" | |
| Look | Sand table is the main view; drone feed only when a drone is up | |
| Turn length | A setting, 1-minute default | *"Make that a setting you can change, but I like a minute."* |
| Session length | Tactical missions 20 to 45 minutes; RPG sessions longer, set by the group | *"Unless it's a tabletop RPG session then that should take much longer but of course be custom like normal D&D"* |
| Campaign | Linked campaign, standalone missions, and generated missions | |
| Progression | Named units, small realistic skills, permanent death, leader record, optional ironman | |
| Order modes | Radio, Plain English, Loose; silly orders in every mode; most-requested orders get added | *"I want people to be able to do funny stuff too, that way you get the actual D&D feel. So like dancing should be a thing, silly other shit should be a thing, just nothing illegal or anything if that makes sense. We should keep track of orders that people most request and add in the most popular ones as things you can do too."* |
| Dice | The player rolls the big moments; the rest roll themselves on screen; settings to roll everything or nothing; dice skins for achievements | *"I also want all of the rolls to be a dice roll that the player does. Whether it's D20 or D6 or whatever. There should be cool dice skins for different achievements and stuff too."* Round 2: *"A setting also lets players roll nothing."* |
| Order language | Spells and plain English as order styles; typing in other languages if it's not too hard | *"Yeah I meant spells and plain english instead of radio talk but other languages should be something you can type in I guess if it's not too hard."* |
| Map symbols | Above company: map symbols. Company and below: just the figures | *"So for any operation larger than company level operations, insignia, and vise versa."* Round 2: *"Company and below it's just the figures. Everything else yes."* |
| Co-op | Each friend runs a squad; one can be platoon commander; voice chat is the radio net | |
| Versus | Each side sees only what its units see; turn timer | |
| Connecting | Online through Steam; the host's PC runs the game | *"Is it possible to have the bug report thing be free or no"* |
| Pocket DM with a group | AI-only DM, or a friend as DM with the AI helping and able to be overruled | |
| Pocket DM rules | Our own generic tabletop RPG and tabletop strategy rules; modders can make custom rules with the existing verbs | *"I say that we basically just do generic tabletop RPG and tabletop strategy rules with the ability for modders to make custom rules with the existing verbs and such."* |
| Bug reports | Order text and game state, never voice; privacy policy line and an off switch; no popup | |
| Mods | Data and art only at first, no custom code | *"Yeah that was my big question on how we could make this custom but safe."* |
| Custom verbs | Verb recipes; popular Workshop recipes get baked in; locked-down scripting may come later | *"Let's roll with verb recipes and then we can see which recipes are most popular on workshop and bake them into the game properly."* |
| Mod sharing | Steam Workshop | |
| Local AI | A small model by default; minimum spec is a graphics card with 8 GB of memory | |
| Cloud AI | Local only at launch; cloud later only if lots of players can't run it | Asked first: *"Would we run into usage issues or no? I din't know if I want to do cloud AI that sounds too finnicky. Do other games do this?"* Then accepted. |
| Build order | Squad slice, voice, co-op and versus at squad level, platoon, Steam Early Access, mods and pocket DM during Early Access, company, battalion | |
| Early Access | Opens at platoon size with all three settings | Picked (a). *"I don't see how this will take more than a few week's worth of work or so. I feel like you can hopefully help me make the models for the game (i will do the finished product but you can help speed it up)"* |
| Price | Decided closer to release | |
| The ledger page | Olive and coyote look; code in Claude's scratch space; one link for every round | |
| Pitch | "A tabletop game you run by talking. Command Marines, a sci-fi squad, or a fantasy party, with every roll in the open." | *"Yes"* |
| Early Access size | Platoon, as the build order says | *"Platoon"* |
| Pillars in sci-fi and fantasy | Each setting follows its own rules just as strictly; the Marines follow real-world norms; each setting has its own way of giving orders | *"Yeah and if you select talk like the radio, it should come with the structure of what it will and will not accept."* |

### 2026-10-26: ledger round 3 and Steam

| Decision | Ruling | Triston's words |
|---|---|---|
| Radio mode guide | Shows the structure Radio mode will and won't accept, with examples; silly orders stay off it | *"Just leave the silly orders off of that, see if people can figure it out on their own."* |
| New bible sections | Settings and modes, look, progression and campaign, mods | |
| What "the AI decides" means | The AI decides the outcome, which may be a reason it isn't possible right now or a flat refusal | *"AI decides what it will do in those instances, it doesn't necessarily mean that it will execute them. That could mean that it comes up with a reason that's not possible at that time or just give a flat out refusal if it's something insulting or illegal."* |
| Closing note about Conduits | Kept word for word, with Triston's update below it | *"I mean yeah I changed my mind. I can't work on conduits til I hear back from the play test and learn guitar so what should I do in the meantime lol."* |
| Conduits is the main build | Accepted | *"Yes it's the main build this is just something I want to do holy shit."* |
| The handoff file | Left as it is; the bible is the current word | |
| Steam's AI rule | Disclose the live AI with its guardrails; a store page line for AI-helped models; hand-made store art and trailers | *"I agree what you're saying in the response hard no on sexual, racist, illegal, etc... and I will disclose the core of the game and sure I'll have the disclaimer."* |
| Help with models | Claude writes Blender scripts for rough base models; Triston finishes them | |
| First build work | Claude writes a build plan for the squad slice and puts it in the repo | |

### 2026-10-26: ledger round 4 (the squad slice plan)

| Decision | Ruling | Triston's words |
|---|---|---|
| The slice uses the Marines and the Cinder Ford bridge seizure | Accepted | |
| The simulation is plain C# kept apart from Unity | Accepted | |
| Units, verbs, and maps are data files from day one | Accepted | |
| Verbs in the slice | 25 verbs (the list goes to round 5) | *"Let's get 25 verbs made."* |
| Plain English mode only in the slice | Accepted | *"Sure."* |
| The order reader | The AI reads orders from the start, with nothing of Triston's personal license in the released game (how it's done goes to round 5) | *"Let's get the local model or you running in it first as long as it'll release with no traces of my personal license or anything in the game later."* |
| Orders the code doesn't cover go to a local log until the bug-report server exists | Accepted | |
| The 14 pieces of the slice, in order | Accepted | |
| Every decision goes in an updated ledger, saved as a rule in `CLAUDE.md` | Accepted | *"I always want an updated ledger ok"* |

### 2026-10-26: ledger round 5

| Decision | Ruling | Triston's words |
|---|---|---|
| How the AI reads orders from the start | The local model from day one | Picked (a) |
| Starting model | Qwen3 4B; recheck newer models and licenses before release | |
| The 25 slice verbs | Moving: move, bound, follow, withdraw, hold, take cover, change posture. Fighting: fire, suppress, overwatch, throw grenade, throw smoke, assault, clear, breach. Looking and talking: observe, report, fly drone, talk. Care and gear: treat casualty, carry, search, detain, share ammo. Fun: emote | *"Let's make sure that we have the requisite amount of verbs to cover almost any action that would be required in a tactical scenario and in a tabletop rpg scenario. You can also look at the actual Field Table artifact and see what verbs have been used in the games I've played."* |
| Start building pieces 1 and 2 | Accepted | |

### 2026-10-26: ledger round 6

| Decision | Ruling | Triston's words |
|---|---|---|
| The full verb catalog in `verbs.md` | Accepted | |
| Verbs in the slice | All of them | *"We're adding all of the verbs."* |
| Speed, posture, and the like are blanks on a verb | Accepted | *"Idk what do you think? Which allows for the most customization and recipe crafting to get the most custom experience possible without losing the pre determined code aspect."* |
| SOPs from the prototype | Accepted | |
| Formatted reports built by the RTO, Doc, or team leaders | Accepted | *"I feel like I want the reports to be formatted as a choice in radio mode, doing the reports at all a choice in plain english, and a choice in free form but not really necessary."* |
| Out-of-game questions | Accepted | |
| The simulation in `Sim/` as a package Unity loads; the Unity project in `Unity/` | Accepted | |
| Unity template: Universal 3D | Accepted | |
| The simulation moves in tenths of a second | Accepted | |
| Meters, 1-meter cells, grid square size set per map | Accepted | |
| Claude's own dice roller (PCG32) | Accepted | |
| Who is hostile to whom | Civilians can be someone's enemy | *"Civilians can be someone's enemy. If you want a situation where there's a bad guy faction targeting civilians and you have to stop them ."* |
| Data files in JSON, read with Newtonsoft | Accepted | |
| Git LFS for big art files | Accepted | |
| Build pieces 3 and 4 next | Accepted | |

### 2026-10-26: in chat

| Decision | Ruling |
|---|---|
| The enemy | *"Let's write a smart coed enemy that then gets steered by the in game AI."* |

### 2026-10-26: ledger round 7

| Decision | Ruling | Triston's words |
|---|---|---|
| 25 mission start states with add-in pieces switched by code | Accepted | |
| Player-built missions and campaigns, whole or partial | Accepted | |
| AI-made missions fill a template that code checks | Accepted | *"Yeah it uses the mission start states and fills in pre made adders to make it unique."* |
| Generated maps | Accepted | *"Except cinder ford doesn't need to be hand built."* |
| Building models | A small kit of parts snapped together | Picked (b) |
| A phone playtest page | Accepted | |
| How the phone page reads orders | Haiku reads typed orders as a stand-in | Picked (a). *"Use Haiku for this please so I don't eat up my usage and it should feel closer to what's in the final thing right"* |
| Every verb gets built in the slice | Accepted | *"Yes because we're releasing everything not just cinder ford"* |
| Modders can add their own blanks | Accepted | |
| Reports by order mode | Accepted | |
| The hand-built Cinder Ford map | Deleted; test on generated maps only | Picked (b) |
| One map format for everything | Vetoed (re-asked in round 8, since the wording was unclear) | *"I want them to all be different and have varying terrain and interesting things. To accomplish that let's figure out 15 different things that can be (but don't have to be) in any given map."* |
| Cinder Ford becomes a start state | Accepted | |
| The map generator joins the build order | Accepted | |
| Hills | Random heights | *"Random heights for random hills, some are up to 150, some are only 15. That's what makes this INTERESTING is the RANDOMNESS. You can always tell the game master you want a specific hill size you know."* |
| Terrain numbers | Accepted | *"Great"* |
| Creeks fordable at 40% speed | Accepted | |
| Line of sight | Accepted | |
| Spotting builds up over time | Accepted | *"There should be something indicating if they're not still seen at a spot if it's just last known."* |
| Daylight only for now | Vetoed: night, NODs, and thermals are in now | *"No dude it was already in the little artifact game you made, why can't you include it in something you're actually thinking about?"* |
| Who is hostile by default | Accepted | |
| Game data in `Data/` | Accepted | |
| The AI model file stays out of the repo | Accepted | |
| Finish piece 4, then the generator, the phone page, movement, and fire | Accepted | |

### 2026-10-26: in chat

| Decision | Ruling |
|---|---|
| No static first map | *"I really don't see the need of having a static first map, it's kind've going away from what I wanted this to be in the first place"* |
| Custom models and animations | *"custom models only need to have one moving animation, it never speeds up or slows down the animation or anything like that. Any custom emotes can be programmed in, and attached to a verb. People can get as intricate or simple with animations as they want to. Or they can use our animations."* |

### 2026-10-26: ledger round 8

| Decision | Ruling | Triston's words |
|---|---|---|
| The 15 map features | Accepted | *"Great, also don't miss the chance for a player to tell a fully kitted up marine to swim across the river and they can't and drown. Everything should be that custom."* |
| How the mix is picked | Accepted | |
| Asking the game master for specifics | Accepted | |
| Map format, asked again | Accepted | |
| Night ranges | Accepted | |
| Who carries what | Vetoed: gear isn't guaranteed, and things go wrong sometimes | *"Not necessarily, what if you were out for a short op and it went bad and now someone didn't pack his nods because he didn't want to have to worry aobut it. Shit like that shouldn't happen every game or people will get sick of it but shit should just happen like that, not even just in gear but in other stuff too."* |
| Illumination and flares | Accepted | |
| Muzzle flashes | Accepted | *"Unless using something to compensate for that if the gear is meant for night"* |
| How a last-known marker looks | Accepted | |
| Poses for a model with one animation | Accepted | |
| Players' own AI models | Accepted | |
| Build the map generator | Accepted | |
| Then the phone playtest page | Accepted | |

### 2026-10-26: in chat, after round 8

| Decision | Ruling |
|---|---|
| 3D maps | *"Ensure the maps are 3D as well so units can go in and out and they can hide and all that."* |
| The camera | *"And viewed from that angled camera from earlier"* |
| The look | *"I really liked how the html looked that you gave me earlier too btw, with some tweaks that could look super cool"* |

### 2026-10-26: ledger round 9

| Decision | Ruling | Triston's words |
|---|---|---|
| The phone page in the figures-page look | Accepted | |
| The game's own look | Undecided | Picked (a), with *"I actually like the drawn figures a look. I don't know which one of these to pick"* |
| Going in and out of buildings | Accepted | |
| Upper floors | Accepted | |
| Hiding outside | Accepted | |
| Friction | Accepted, about one mission in five | *"let's do more like 20%"* |
| Map size | Accepted | |
| How many features | Accepted | |
| Roads and water | Accepted | |
| Villages and towns | Accepted | |
| Building sizes | Accepted | |
| Place names | Accepted | |
| Cliffs | Accepted | |
| Building numbers | Accepted | |
| A mobile version, after the PC Early Access release, newer phones only | Accepted | |
| The phone playtest page next | Accepted | *"Where do I do that at?"* |

### 2026-10-26: ledger round 10

| Decision | Ruling | Triston's words |
|---|---|---|
| The game's own look | (c): real 3D ground and buildings, with drawn figures standing on them like paper miniatures | |
| Where you play the playtest page: a link on your phone | Accepted | |
| How it runs: the same simulation, built for the browser with .NET's WebAssembly tools | Accepted | |
| Where its code lives: `Playtest/` in the repo | Accepted | |
| Build the playtest page | Accepted | |

### 2026-10-27: ledger round 11

| Decision | Ruling | Triston's words |
|---|---|---|
| How a shot is decided (one d100 per burst) | Asked again in round 12 | *"A d100 for the whole burst? How does that work"* |
| Getting pinned | Not answered; asked again in round 12 | |
| Wounded or down | Accepted | |
| The clock on a casualty | Accepted | *"And then casevac depending on where hit and everything? Because every GSW is different"* |
| Blasts | Accepted | *"There should be a difference with HE and AP"* |
| Deep water | Accepted | |
| The enemy, until the in-game AI steers it | Accepted | |
| How many enemy | (c): random each mission, 5 to 9 | |
| Where the enemy sets up | Accepted | |
| Civilians | Accepted | |
| How it ends | Accepted | |
| Marine names | Accepted | |
| Radio calls on their own | (b): each team calls a contact once, and again only if it moves or comes back | |
| The first 16 verbs | Accepted | *"When will they come"* |
| Figure and building size | Accepted | |
| Figures stay visible | Accepted | |
| Daylight for now | (a): daylight for now; night views next | |
| Order preview | Accepted | |
| Playback speed | Accepted | |
| Which dice you see | Accepted | |
| A backup reader | Accepted | |
| What the page saves | Accepted | |
| Landscape on your phone | Accepted | |
| Quick brief | Accepted | |
| Secondary objectives for the bridge seizure | Accepted | |
| What secondary objectives count for | Accepted | *"In order to get the best rating you have to complete the other objectives"* |
| Starting a mission | Accepted | |

### 2026-10-27: ledger round 12

| Decision | Ruling | Triston's words |
|---|---|---|
| How a shot is decided | (c): one roll to hit; on a hit, a die for how many rounds hit, each with a wound roll | |
| Getting pinned | Accepted | |
| The enemy mix: always a PKM and an RPG, the rest riflemen | Vetoed: each fighter a random kind | *"Not always PKM and rpg. Random."* |
| The rating | Accepted, with S+ as the highest | *"S+ as highest"* |
| Where a Marine is hit | Accepted, plus helmet saves now and then | *"There should be the one offs where the helmet saves"* |
| CASEVAC | Accepted | |
| HE and AP | Accepted | |
| What "moves or comes back" means | Accepted | *"I don't want the time speed up to make the animations faster or slower just the actual movement across the map"* |
| The rest of the verbs, and when | Accepted | |

### 2026-10-27: ledger round 13

| Decision | Ruling | Triston's words |
|---|---|---|
| The rating scale under S+ | (b): S+ every one met, then S, A, B as fewer are met, C for winning with Marines killed; a loss is F | *"Yeah and friendly fire incidents or things like that also detract."* |
| How often the helmet saves | Accepted | |
| Getting hit again | Accepted | |
| Patching up a bad arm or leg | Accepted | |
| The random enemy | Accepted | *"I really think that enemy squad composition should make sense for what the scenario is and what the objective is and what the enemy objective is."* |
| HE and AP rounds | Accepted | *"Yep and armored vehicles get AP too."* |
| CASEVAC details | Accepted | |
| Bound | Accepted | |
| Overwatch | Accepted | |
| Smoke | Accepted | |
| Clear | Accepted | |
| Breach | Accepted | |
| The drone | Accepted | *"I want there to be icons that pop up on the map for all spotting not just drone spotting. So anything important that gets said on the radio has a map marker that shows text when you click or hover it."* |
| Talk | Accepted | *"There should be need of interpereter sometimes and sometimes the enemy should talk depending on what's going on. Again, stop making things so narrow, the scenarios in the artifact game were so dynamic and fun because the situation responded to how I handled everything."* |
| Search | Accepted | |
| Detain | Accepted | |
| Share ammo | Accepted | *"Yes but ammo types matter too."* |
| A place with no verb | Accepted | *"I'm just saying I'm not trying to do all the radio speak for the play test that would get exhausting."* |
| Ground in a square | Accepted | *"Also clicking on the map should give the 6 digit coordinates so the player doesn't have to type them."* |
| Where the squad starts | No pick; out of the enemy's sight and not lined up | *"the scenarios should make sense and be dynamic. But I would think we wouldn't have the squad starting directly in the enemy's sight, all lined up right next to each other"* |

In chat, after round 13: *"How is it that in the artifact game all of the spotting and starts and stuff was figured out but we can't get it here"*

### 2026-10-27: ledger round 14

| Decision | Ruling | Triston's words |
|---|---|---|
| Talk: interpreters, and the enemy talking | Accepted | |
| An enemy that fits the mission | Accepted | |
| Missions that respond to how you play | Accepted | |
| Maps where nowhere is hidden | (c): bigger maps, plus common-sense cover and avenues of approach | *"Bigger maps plus ensuring there's common sense cover and avenues of approach, unless part of the scenario is there being no cover"* |
| Friendly fire and the rating | Accepted, down to F | *"Down to F"* |
| Map pins | Accepted, but not for enemies; fire positions too | *"I think for enemies it's already solved by the semi transparent enemy model, but other stuff should be marked including if you make fire positions and such."* |
| Six-digit coordinates | Accepted | |
| The start | Accepted | |
| Ammo types | Accepted | |
| A second bridge | Accepted | |

In chat, after round 14: *"I want the map to be able to be fullscreen with the other stuff below it"*

In chat: *"It says from the well back but I have no idea where that is"* Claude's delay hint ("from well back") didn't say where; it now gives the distance past the bridge and the grid square (ledger round 15).

## Open questions

- [ ] Claude's calls from the round 14 build: the map size and cover, enemy goals, talk, and the director (ledger round 15)

> This is something totally separate and won't get in the way of conduits but I thought could be fun and if it goes well could be something that gets built after conduits same universe getting folded into it if things go well and I can hire a studio because it went well. Otherwise another project for after conduits if it doesn't go well

*Update, Oct 26, 2026: "I mean yeah I changed my mind. I can't work on conduits til I hear back from the play test and learn guitar so what should I do in the meantime lol."*
