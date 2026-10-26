# Field Table: game bible

*Started Oct 16, 2026 · @Triston. Updated Oct 26, 2026 to match every ruling so far. Overruled ideas have been taken out.*

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
- Grid references: Click the map to drop the exact grid square into the order. A grid is always a grid.
- Typed orders: Free text.
- Voice orders: Push-to-talk, using local speech-to-text (Whisper family). Primed with callsigns, the phonetic alphabet, building numbers, and the verb list. Spoken grids like "golf four" convert to G4. Added after typed orders work, since voice just produces text.
- Order modes: A difficulty selector of its own, separate from the odds setting.
  - Radio: proper radio format; sloppy orders get "say again." It comes with a guide to the structure it will and will not accept, with examples. Silly orders stay off the guide.
  - Plain English: normal talk.
  - Loose: anything goes and the AI sorts it out.
- Order styles: Radio talk, plain English, and spells (for fantasy). Players can type in other languages too, if it's not too hard.
- Verbs and blanks: Verbs are hard-coded: move, assault, overwatch, suppress, call for fire, treat casualty, carry, hold, report, and so on. Each verb has blanks for who, where, what target, and when or on what condition. The AI picks the verb and fills the blanks, locked by a grammar so it can only output units, grids, and verbs that exist.
- Silly orders: Funny stuff like dancing is a real thing you can do, in every order mode, for the actual D&D feel. Nothing illegal. Radio mode still wants these in proper radio format.
- Unclear orders: Answered in-world, like "Bravo, say again, which building?" The game never guesses which unit, building, or grid an order means.
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
| Enemy AI (traditional) | Enemy decisions, using behavior trees or utility AI: fast, consistent, keeps its own plan |
| Language AI | Turning orders into verbs and blanks, voicing units and higher on the net, writing the debrief from logged facts, and deciding orders the code doesn't cover yet |
| Validation (code) | Checking each command is possible: time, ammo, friendlies in the grid. Failures become in-world pushback |

- Memory: The game holds the mission log; the AI only gets the small slice each call needs.

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

- Design lab: The Field Table prototype is free playtesting of the core idea. Note what felt fair, what didn't, and which rulings to veto.
- Branches: Each played mission's decision points become its branches. Example, Operation Cinder Ford: the PKM, the unarmed pack on the bridge, and the two enemy wounded each lead to different outcomes depending on the player's choices.
- Two content sources: Hand-built branching missions for campaign or story, and missions generated from building blocks (terrain set, objective type, enemy package, complications) for replay.
- Radio lines: Written as templates with blanks, like "Lead, [unit]. Set at [location], eyes on [target]." These can be voice-acted, with optional AI flavor on top.
- First mission candidate: Cinder Ford, the bridge seizure, already played and debriefed.
- Models: Claude writes Blender scripts that build rough base models. Triston finishes them.

## Mods

Modders can make their own experiences and share them with people.

- What modders can change: units, factions, maps, missions, verbs, radio lines, art, rules, map symbols, and order language (like spells).
- Safety: data and art only at first, no custom code, because code mods can carry viruses when people share them.
- Custom verbs: verb recipes. Modders build new verbs from pieces the game already has (move, play an animation, roll a die, apply an effect), so a mod can't touch the player's PC. The most popular recipes on Steam Workshop get baked into the game properly. Locked-down scripting, with files and the internet blocked, may come later.
- Custom rules: modders can make custom rules with the existing verbs.
- Sharing: Steam Workshop.

## AI usage and technical approach

Live AI is allowed in this game. This project is separate from Conduits' no-AI-assets rule.

- Local models: Run on the player's PC through LLM for Unity or llama-cpp-unity (both built on llama.cpp). Candidates: Qwen3 (4B to 14B), Phi-4, gpt-oss-20b, Gemma 4. Check each license before shipping.
- Default and minimum spec: A small model by default, starting with Qwen3 4B (Apache 2.0 license, free to ship in a paid game). Newer models and their licenses get rechecked before release. The minimum is a graphics card with 8 GB of memory.
- Order reader: The local model reads orders from day one, so what gets tested is what ships. Nothing of Triston's personal license goes in the game. The open-source parts (the model, LLM for Unity, llama.cpp) get their license notices in the credits.
- Structured output: A GBNF grammar locks the model to valid verbs, units, and grids.
- GPU budget: Roughly 3 GB for a 4B model, 5 to 6 GB for 8B, 9 to 10 GB for 14B, shared with graphics. The sim pauses or slows while the model thinks, which fits radio delay.
- Speech: Whisper-family speech-to-text, a few hundred megabytes, push-to-talk.
- Shipping: Model files download on first launch. Local AI only at launch.
- Cloud: Added later only if lots of players can't run the local model. If it comes, it runs through a backend we control, never with a key inside the game.
- Guardrails: A hard no on sexual, racist, illegal, or similar content in anything the AI writes. The grammar lock keeps parsed orders to real verbs, units, and grids, and a filter covers anything the AI writes freely (orders the code doesn't cover, and the pocket DM).
- Bug reports: When a player is connected, orders the code doesn't cover get reported quietly. Reports send the order text and the game state, never voice recordings. Players are told in the privacy policy and get an off switch in settings. No popup during play.
- Steam: Disclose the live AI on the store page, with its guardrails (the grammar lock, the content filter, and the bug-report off switch). Add a line for the models: "base models made with AI help, finished by hand." Store page art and trailers are made by hand. Claude's help writing the game's code counts as a developer tool, which Steam doesn't ask about.

## Build order

Each step is a complete, playable game on its own. Don't start a step until the one before it is fun.

1. Squad slice: One map, one squad, one mission, WEGO turns, typed orders, verbs and blanks. If this isn't fun, nothing above it will be.
2. Voice: Push-to-talk on top of the typed path, with readback.
3. Co-op and versus at squad level.
4. Platoon: Three squads plus attachments; the radio net and platoon sergeant problems.
5. Release on Steam Early Access at platoon size, with all three settings (Marines, sci-fi, fantasy).
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

## Open questions

- [ ] The full verb catalog for tactical and tabletop RPG play (`verbs.md`, round 6)

> This is something totally separate and won't get in the way of conduits but I thought could be fun and if it goes well could be something that gets built after conduits same universe getting folded into it if things go well and I can hire a studio because it went well. Otherwise another project for after conduits if it doesn't go well

*Update, Oct 26, 2026: "I mean yeah I changed my mind. I can't work on conduits til I hear back from the play test and learn guitar so what should I do in the meantime lol."*
