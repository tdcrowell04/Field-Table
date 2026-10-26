# Field Table: game bible

*The bible from 2026-10-16 is kept word for word below. New rulings go in the decision log at the bottom, in Triston's words. The sections above the log haven't been rewritten to match the new rulings yet; where they disagree, the log wins.*

*Oct 16, 2026 · @Triston*

## Overview

A realistic, WEGO tactical game where you command Marines by talking to them, typed or by voice, and watch the fight play out. Think computer D&D with modern troops instead of RPG characters: XCOM-adjacent, but with more people and more options for scale.

- Working title: Field Table (placeholder, taken from the prototype)
- Status: Concept. The Field Table web prototype is the live design lab.
- Engine: Unity 6, same as Conduits
- Relation to Conduits: A separate project. Conduits stays the main build through its Steam Next Fest demo (October 2027). This one develops in the meantime as design work, with code later.
- Roots: Kriegsspiel (Prussian officer training, 1820s), Chainmail and early D&D, and Marine Corps Gazette tactical decision games.

## Design pillars

Every feature has to serve at least one of these four. If it fights one, it gets cut or reworked.

1. Realism baked in. Movement rates, fire support timelines, suppression, casualty clocks, sustainment, and ROE work like the real thing. Outcomes follow historical norms, not movies.
2. Fair and visible. The player can always see why something happened. Odds are shown before commitment and rolls are shown after, like dice rolled in the open.
3. Talk like the radio. Orders are given in natural language, typed or spoken, the way a leader talks on the net. Units read back and push back like real Marines.
4. Honest fog of war. The player knows only what their Marines can see or are told. Bad intel is part of the game; a trap hidden in a mismatch between the map and the truth is not.

## Core loop

The game runs in WEGO turns: both sides give orders, then both sides move at the same time for a set stretch while the player watches. Real enemies don't wait their turn, and it matches the rhythm of radio traffic.

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
- Grid references: Click the map to drop the exact grid square into the order. A grid is always a grid.
- Typed orders: Free text, written like radio traffic.
- Voice orders: Push-to-talk, using local speech-to-text (Whisper family). Primed with callsigns, the phonetic alphabet, building numbers, and the verb list. Spoken grids like "golf four" convert to G4. Added after typed orders work, since voice just produces text.
- Verbs and blanks: Verbs are hard-coded: move, assault, overwatch, suppress, call for fire, treat casualty, carry, hold, report, and so on. Each verb has blanks for who, where, what target, and when or on what condition. The AI picks the verb and fills the blanks, locked by a grammar so it can only output units, grids, and verbs that exist.
- Unclear orders: Answered in-world, like "Bravo, say again, which building?" The game never guesses.

Example: "Alpha, once Charlie takes over, move on 4, only once Bravo and Charlie confirm" becomes assault, unit Alpha, target building 4, trigger: Bravo and Charlie report in position.

## Order preview and odds

No order fires directly. Every order produces a plan, the plan is shown, and nothing moves until the player commits.

- Map overlay: Planned paths, with stretches visible to known enemies shown in red plus exposure time. Fire orders show their odds on the target. Indirect fire shows its impact area and dispersion.
- Order cards: One per unit, showing the parsed verb, target, trigger, ETA, odds, and warnings. This is where a mistranscribed order gets caught before it costs anything.
- Fixing orders: Voice ("Bravo, belay that, building 1") updates one card only. Paths and targets can also be dragged with the mouse.
- Safety checks: Fire into a square holding buildings, civilians, or friendlies is flagged by the FDC or observer and needs confirmation.
- Combat log: After the turn, every roll is shown with its odds and result.

Odds are calculated only against known contacts and labeled that way. A clean plan can still get hit by a gun nobody spotted.

| Difficulty | Odds shown as |
|---|---|
| Training | Exact percentages, XCOM style |
| Realistic | Plain judgments: good shot, marginal, long odds |
| Hardcore | No odds; map and judgment only |

## Simulation

Code owns every hard fact; the AI only handles language. This split is what keeps rulings fair, because a model can never misplace a round or a unit.

| Layer | Owns |
|---|---|
| Game code (simulation) | Movement, terrain, passability, line of sight, hit chances, suppression, fires and dispersion, ammo, casualties, the clock |
| Enemy AI (traditional) | Enemy decisions, using behavior trees or utility AI: fast, consistent, keeps its own plan |
| Language AI | Turning orders into verbs and blanks, voicing units and higher on the net, writing the debrief from logged facts |
| Validation (code) | Checking each command is possible: time, ammo, friendlies in the grid. Failures become in-world pushback |

- Visuals: Simple models of individual Marines and enemies that actually move and shoot, not symbols.
- Memory: The game holds the mission log; the AI only gets the small slice each call needs.

## Scale and layers

Every Marine is an individual model up to company level. Above that, units become abstract counters, and the player can drop into any fight below or let it autoresolve.

| Layer | What a model represents | How it plays |
|---|---|---|
| Fire team to platoon | Each Marine | Full WEGO with individual models |
| Company | Each Marine (about 180 to 200 plus enemy) | Full WEGO; heavy checks only near the enemy, simpler models at distance |
| Battalion and up | Companies and platoons as counters | Abstract layer; drill down into any company fight or autoresolve it |

Autoresolve rule: At company level and below, autoresolve is the detailed battle run invisibly at high speed, with AI subordinates deciding as the player would. Watching or skipping gives the same results. Above company, the abstract model is tuned against thousands of those invisible battles so it stays grounded in the same simulation.

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

- Design lab: Field Table is free playtesting of the core idea while Conduits stays the main build. Note what felt fair, what didn't, and which rulings to veto.
- Branches: Each played mission's decision points become its branches. Example, Operation Cinder Ford: the PKM, the unarmed pack on the bridge, and the two enemy wounded each lead to different outcomes depending on the player's choices.
- Two content sources: Hand-built branching missions for campaign or story, and missions generated from building blocks (terrain set, objective type, enemy package, complications) for replay.
- Radio lines: Written as templates with blanks, like "Lead, [unit]. Set at [location], eyes on [target]." These can be voice-acted, with optional AI flavor on top.
- First mission candidate: Cinder Ford, the bridge seizure, already played and debriefed.

## AI usage and technical approach

Live AI is allowed in this game. This project is separate from Conduits' no-AI-assets rule.

- Local models: Run on the player's PC through LLM for Unity or llama-cpp-unity (both built on llama.cpp). Candidates: Qwen3 (4B to 14B), Phi-4, gpt-oss-20b, Gemma 4. Check each license before shipping.
- Structured output: A GBNF grammar locks the model to valid verbs, units, and grids.
- GPU budget: Roughly 3 GB for a 4B model, 5 to 6 GB for 8B, 9 to 10 GB for 14B, shared with graphics. The sim pauses or slows while the model thinks, which fits radio delay.
- Speech: Whisper-family speech-to-text, a few hundred megabytes, push-to-talk.
- Shipping: Model files download on first launch. Possible tiers: small model, bigger model, optional cloud mode.
- Cloud option: Claude API through a backend you control, never a key inside the game. Fast models for chatter, stronger ones for adjudication and debriefs.
- Steam: Live AI-generated content must be disclosed.

## Build order

Each step is a complete, playable game on its own. Don't start a step until the one before it is fun.

1. Squad slice: One map, one squad, one mission, WEGO turns, typed orders, verbs and blanks. If this isn't fun, nothing above it will be.
2. Voice: Push-to-talk on top of the typed path, with readback.
3. Platoon: Three squads plus attachments; the radio net and platoon sergeant problems.
4. Company: Full individual models and the first real autoresolve.
5. Battalion and up: The abstract layer with drill-down into any company fight.

## Decision log

Every call gets ruled before anything is built. All entries below were raised on 2026-10-16.

| Decision | Ruling |
|---|---|
| Separate project from Conduits; live AI allowed in this game | Accepted |
| WEGO turns: simultaneous movement after orders | Accepted |
| Click a unit to select it, then type or speak to it | Accepted |
| Click the map to insert exact grid references | Accepted |
| Verbs are hard-coded; the AI only picks verbs and fills blanks | Accepted |
| Code owns movement, terrain, passability, and hit chances | Accepted |
| Simple models that move and shoot, not symbols | Accepted |
| Individual models up to company; abstract counters above | Accepted |
| Drill down into any lower fight or autoresolve it | Accepted |
| Field Table missions feed the game's content | Accepted |
| Order preview with overlay, order cards, readback, and execute | Accepted |
| Push-to-talk voice orders with readback | Accepted |
| Odds display set by difficulty: exact, plain judgments, or none | Accepted |

## Open questions

- [ ] Final title for the game
- [ ] WEGO turn length: fixed (30 seconds, a few minutes) or set by the scenario?
- [ ] Visual style: top-down sand table, drone feed, or a mix?
- [ ] Campaign structure: standalone missions, a linked campaign, or both?
- [ ] Progression: Marines gaining experience, the player's record as a leader, or neither?
- [ ] Default model tier and minimum spec for local AI
- [ ] Pricing for any cloud mode: built into the price, subscription, or bring your own key?
- [ ] Branch of service and era: Marine Corps only, or Army and others later?
- [ ] When does code work start relative to the Conduits demo?

> This is something totally separate and won't get in the way of conduits but I thought could be fun and if it goes well could be something that gets built after conduits same universe getting folded into it if things go well and I can hire a studio because it went well. Otherwise another project for after conduits if it doesn't go well

---

## Decision log, continued

### Figures page (2026-10-26)

| Decision | Ruling |
|---|---|
| Both views (sand table and drone feed) are tipped at a slight angle so the figures can be seen | Accepted. Triston: *"Can you do it slightly at an angle so you can actually see the models just a little bit"*; then picked "Both" |
| The Marines look like today's Marines | Accepted. Triston: *"make them look more 'modern day marine'"* |
| Today's 13-Marine rifle squad: a squad leader and three teams of four, one rifleman trained to fly the squad's attack drones, plus the platoon's corpsman | Accepted. Triston picked "Today's squad" |
| The figures must not blend into the ground | Accepted. Triston: *"Just ensure the models don't blend in TOO well"* |
| Side rings under every figure: blue for Marines, red for the enemy, white for civilians | Accepted |
| No rings on the drone feed | Vetoed; rings added |
| Orange tracers for Marines, green for the enemy | Vetoed; Triston picked "All one color" |
| That one color is a warm yellow (#ffd166) | Accepted |
| The camo matches the map, with a switch between green and dry ground | Accepted. Triston picked "Match the map", then "A switch for both" |
| The page opens on dry ground, then remembers the last pick | Accepted |
| The turn at Cinder Ford is made up to show the figures | Accepted |
| The page opens with both views side by side | Accepted |
| The team leader carries the grenade launcher; no separate grenadier | Accepted |
| Each team of four: team leader, automatic rifleman, assistant automatic rifleman, rifleman. Charlie's rifleman is the drone operator | Accepted |
| The corpsman carries an M4 carbine and is labeled Navy | Accepted |
| Gear: high-cut helmet in a camo cover with night-vision mount and goggles flipped up, ear and eye protection, coyote plate carrier with pouches, knee pads, dark gloves, coyote boots, M27 with a suppressor and scope | Accepted |
| Charlie holds at building 5 with its drone over building 6 | Accepted |
| Figures are drawn bigger than true scale, like tabletop miniatures | Accepted |

### Direction (2026-10-26, in chat)

| Decision | Ruling |
|---|---|
| Release timing | *"Field table is side work but if it's not too difficult and it gets released first then great. I'm waiting until I finish my guitar learning for Conduits anyways."* |
| Group play | *"I want group play both co op and vs"* |
| Modes for how the AI takes orders | *"Yeah but as a separate difficulty selector from the other one"* |
| Pocket DM | *"b, but this can be looped into the code as well, we can come up with all of the code for what would be in your typical D&D campaign and ensure we cover all of what the game needs with code"* (b: a separate mode where the AI decides outcomes, for free-form play) |
| When a player tries something the code doesn't cover | *"The AI should decide it but an automatic bug report gets filed quietly if they're connected to the server and we can add in that instance after."* |
| Versus: who the other player commands | *"Either or, or both can be the same faction."* |
| Code work starts now | *"Yah why not"* |
| Mods | *"mod support and custom units and factions and symbols and language should be something that modders can do to make their own experiences and share them with people."* |
| Commit the handoff and figures page to the repo | *"Also yes to this repo"* |

### The ledger (2026-10-26)

Claude's suggestions, with Triston's verdict and words.

| # | Decision (Claude's suggestion) | Ruling | Triston's words |
|---|---|---|---|
| A | Both files sit at the top level of the repo | Accepted | |
| B | Pull the bible into its own file, `bible.md`, and update it each time something is ruled | Accepted | |
| Q1 | What "symbols" means for mods | Answered | *"So for any operation larger than company level operations, insignia, and vise versa."* |
| Q2 | What "language" means for mods | Answered | *"Yeah I meant spells and plain english instead of radio talk but other languages should be something you can type in I guess if it's not too hard."* |
| 1 | Pitch: Marines are the flagship and released first; fantasy and mods after | Not accepted (follow-up open) | *"No let's release it all at once. We can do 1 Marines, 1 sci-fi, and 1 fantasy all usable in whatever modes get released."* |
| 2 | Keep the title "Field Table" | Accepted | |
| 3 | Sand table is the main view; drone feed only when a drone is up | Accepted | |
| 4 | Each mission sets turn length; squad missions default to 30 seconds | Answered | *"Make that a setting you can change, but I like a minute."* |
| 5 | Missions run 20 to 45 minutes | Answered | *"Unless it's a tabletop RPG session then that should take much longer but of course be custom like normal D&D"* |
| 6 | Campaign: linked campaign plus standalone and generated missions | Question back | *"Wdym?"* |
| 7 | Named Marines, small realistic skills, permanent death, leader record, optional ironman | Accepted | |
| 8 | Order modes: Radio, Plain English, Loose | Answered | *"Yeah sounds good. I want people to be able to do funny stuff too, that way you get the actual D&D feel. So like dancing should be a thing, silly other shit should be a thing, just nothing illegal or anything if that makes sense. We should keep track of orders that people most request and add in the most popular ones as things you can do too. I also want all of the rolls to be a dice roll that the player does. Whether it's D20 or D6 or whatever. There should be cool dice skins for different achievements and stuff too."* |
| 9 | Co-op: each friend runs a squad, one can be platoon commander, voice chat is the radio net | Accepted | |
| 10 | Versus: each side sees only what its units see; turn timer | Accepted | |
| 11 | Online through Steam, host's PC runs the game; small separate bug-report server | Accepted | *"Is it possible to have the bug report thing be free or no"* |
| 12 | Pocket DM with a group: AI-only DM, or a friend as DM with the AI helping | Accepted | |
| 13 | Build the pocket DM on the free fifth-edition rules (SRD 5.2) | Vetoed | *"I say that we basically just do generic tabletop RPG and tabletop strategy rules with the ability for modders to make custom rules with the existing verbs and such. Could we do custom modded verbs too? Or would that be too much access? How could we do custom verbs for people? I know Arma 3 has crazy modding capability."* |
| 14 | Bug reports send order text and game state, never voice; privacy policy line and an off switch; no popup | Accepted | |
| 15 | Mods: units, factions, maps, missions, verbs, radio lines, art; data and art only at first | Accepted | *"Yeah that was my big question on how we could make this custom but safe."* |
| 16 | Share mods through Steam Workshop | Accepted | |
| 17 | Small local model by default; minimum 8 GB graphics card; weaker PCs use cloud | Accepted | |
| 18 | Local AI free with the game; cloud an optional monthly fee | Accepted (question open) | *"Would we run into usage issues or no? I din't know if I want to do cloud AI that sounds too finnicky. Do other games do this?"* |
| 19 | Today's Marines at launch; other eras later | Vetoed | *"Nah let's do the 3 eras I had said before"* |
| 20 | Build order: squad slice, voice, co-op and versus at squad level, platoon, Steam Early Access, mods and pocket DM during Early Access, company, battalion | Accepted (see 1) | |
| 21 | Decide the price closer to release | Accepted | |
| — | Anything else | Question | *"Are there any games on the market that use AI that are doing well or do people just hate AI too much?"* |

### Open questions from 2026-10-16, where they stand

- [x] Final title: "Field Table" (ledger 2)
- [ ] WEGO turn length: a setting; Triston likes a minute (ledger 4, follow-up open)
- [x] Visual style: sand table main view, drone feed when a drone is up (ledger 3)
- [ ] Campaign structure (ledger 6, follow-up open)
- [x] Progression: named Marines, small skills, permanent death, leader record, optional ironman (ledger 7)
- [x] Default model tier and minimum spec: small model, 8 GB graphics card (ledger 17)
- [ ] Pricing for any cloud mode (ledger 18, follow-up open)
- [ ] Branch of service and era (ledger 19 vetoed, follow-up open)
- [x] When code work starts: now ("Yah why not")
