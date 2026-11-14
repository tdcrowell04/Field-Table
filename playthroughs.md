# Playthroughs: lessons so far

Triston's ask: three playthroughs of random missions (an expert radio operator on Opus, someone who knows the military but talks casually on Sonnet, someone with no military experience on Sonnet), then an AAR and a plan. *"This will be how we iterate and improve the game from here on out."*

Paused at Triston's word: *"Let's just pause here for now and take our lessons and write them down where they can't get lost then we'll pick up after the usage reset."* Also: *"Alright let's re design the test to ensure better results, but also in reality a player would bend to fit the game so we just need to see what's realistic and make it better each time we do this"* and *"we'll hold off on the next play test for now as well."*

Triston's rulings on 2026-11-06 (in the bible's decision log): fix run 1's bugs, then a fresh expert replays the same raid for 8 turns; the scoreboard, the same map, and the player debrief are accepted. Real Haiku through an API key is off: *"Oh well if it's separate from my plan I ain't paying more money for that"*.

## How the test ran

- The real playtest page ran in a headless browser (`Playtest/playthrough/server.js`). `Playtest/playthrough/ft.py` drove it one minute at a time.
- **The player** was an agent that never saw the code or the verb list. Each minute it got what the page shows: the radio net, the squad list, known contacts, and a picture of the map. It answered with its thoughts and its radio traffic, or "wait."
- **The game master** was Haiku. The page's own order-reading prompt went to a fresh Haiku agent each time, and Haiku's answer went back into the page unchanged. (This cloud workspace has no Claude API key, so the page couldn't call Haiku itself.)
- **The director and the Marines' voices** used the page's fallback (the die and the code's own lines), not Haiku. Running them through Haiku would have meant more than one agent at a time.
- After each readback, the player got one chance to correct before the minute played.

## Run 1: the expert, stopped at turn 10 of 40

Marines, *Raid the observation post* ("Jasper Blind"). Win: wreck the post's radio and scopes, then get everyone alive back past the start line.

What happened:
- **Turn 1:** a strong plan. The drone goes over the post, Alpha to the D3 woods, Charlie by a covered route to E2, Bravo trailing Charlie.
- **Turn 2:**
  - Two orders were lost: the drone order and Bravo's follow.
  - Alpha walked straight over the crest the player had said to stay off, and the post saw them at about 300 m.
  - Duarte was killed and Novak went down in the second minute.
- **Turns 3–5:**
  - The player set up a base of fire and pulled Novak back to Doc. Doc stabilized him.
  - Under fire and with the drone overhead, the enemy left the post and ran east to the map edge.
- **Turns 6–9:** Charlie climbed to the post. Sato and Banks topped the ridge and went down within 10 seconds to a machine gun at J10, about 600 m away.
- **Turn 10:** the player was screening with smoke and dragging the wounded off the crest when we paused. The post was never searched.

### Bugs (the game got it wrong; no new design needed)

1. **Brackets in names.** This was the biggest single problem.
   - The order-reading prompt lists places as "the post (F5)" and contacts as "PKM gunner 2 (enemy)". Haiku copies them whole, and the game can't match them.
   - About a dozen say-agains came from this, and the voices read the brackets aloud ("Overwatch on the post (F5)").
   - It blocked the assault on the post once, and kept Marines from firing on enemies the drone could see.
   - The player took "we don't have eyes on that" as realism and switched to area fire.
   - Fix: match names with the brackets taken off, and list them without brackets in the prompt.
2. **A team order wipes a member's own order** when both come in the same message. Duarte's "observe F5" and Mercer's drone were lost this way in turn 1.
3. **A team can't follow a team.** "Bravo, follow Charlie" failed twice.
4. **Re-reading the whole minute dropped an order** that was read right the first time (the player's own move with Bravo).
5. **"Halt and hold at D4" walked Keller back** to the edge of D4 instead of stopping him where he was. A grid square becomes the nearest point in it, so "drag Novak back to B3" left him on the C3 side of the line.
6. **The interpreter left his hold** and tagged along with Lead.
7. **The game master invented an order:** Alpha (two men and a casualty) "assaulting the post." Nobody told them to.
8. **The game master missed verbs it has:**
   - "Halt and launch the drone" became "follow Charlie."
   - "Find me the gun" became Mercer watching J10 by eye instead of flying the drone there.
   - "Be ready to finish the search" became "say again, search what?"
9. **Smoke past 35 m was refused** ("too far to throw") instead of thrown as far as it goes toward the spot.
10. **No "taking fire" call** when the machine gun at J10 hit Sato and Banks, and nobody fired back.

### Asked for and not there yet (times asked in run 1)

| Ask | Times | Round 20 item that covers it |
|---|---|---|
| Cease fire, hold fire, weapons tight | 7 | v3 (fire control that stays on) |
| Routes and waypoints: "via C2 then D2," "stay off the crest" | 5 | v3 (route) |
| Report, call targets, keep eyes on and report, SALUTE | 4 | v1 (report), v2 (trigger: when an enemy is seen) |
| Steps and triggers: "when you're set, Gustaf on F5," "pop smoke, then drag Sato" | 4 | v2 |
| Disregard | 3 | none yet |
| Areas and spans: "the crest from F4 to F6," "G5 through G7" | 2 | v3 (places described from the map), partly |
| Wreck the radio and scopes (the raid's main task; it only works if you say "search") | 2 | none yet |
| Ask a Marine for status | 2 | v1 (ask) |
| Trail a team by a distance | 2 | v3 (keep away from), partly |
| One Marine, two jobs: fly the drone and stay with his team | 2 | none yet |
| Mission intent and ROE said on the net | 2 | none yet |
| Be ready, stand by | 1 | v2 (on your word) |

Most of this is the round 20 verb plan (v1 to v3). Run 1 is more evidence for it.

The missing route and crest orders caused the first two casualties.

### What worked

- The drone: it found all eight enemy within a minute of going up.
- Casualties: dragging a man, setting a collection point, and Doc stabilizing Novak.
- Detaining and talking to the downed enemy, read right the first time.
- Halt and hold, overwatch from a place, a base of fire, and area fire on a grid.
- The enemy broke contact under fire and kept the crest covered from far off, as a real post would.

### Balance notes

- In the second minute, Alpha topped a crest about 300 m from the post: one killed, one down.
- At about 600 m, one machine gun put two Marines down within 10 seconds (one plate saved a hit first).
- Moving up the slope was slow: Charlie spent about two minutes at E4.

### The player

The expert read bugs as realism and learned the parser's limits ("it wants one square, not a span"). Triston: real players bend to fit the game, so the job is to find what's realistic and make it better each time.

### Cost

- Each Haiku read as an agent cost about 70,000 tokens and took 30 to 50 seconds. Run 1 used 13 of them.
- The expert's context passed 115,000 tokens by turn 10.
- Each minute of play took 2 to 3 minutes of real time.

## Changes to the test (ruled 2026-11-06: 2 to 5 accepted, 1 off)

1. **Real Haiku for every part.** With a Claude API key added as a secret in the cloud environment, the harness can send the page's exact prompts to Haiku directly. That covers the director and the voices too, not just the order reader. Each read would take seconds instead of about 40, and cost a small part of the tokens.
2. **Fix the bugs above first,** so the next run finds new problems instead of the same ones.
3. **A scoreboard for every run.** The harness counts orders sent, orders taken, say-agains, asks not covered, and orders the game master invented, so runs can be compared.
4. **The same mission and seed for all three players,** so the runs compare directly, then random missions after that.
5. **A short debrief from each player at the end:** which say-agains felt real and which felt broken.

## Fixed since run 1 (2026-11-06)

All ten bugs above have a fix and a test (`Sim/FieldTable.Sim.Tests/RunOneFixesTests.cs`), except that the game master inventing or missing orders (7 and 8) can only be steered by its prompt, so the replay will show whether that worked.

- **1:** a name with a note in brackets is that place or contact. The prompt also lists names without brackets now.
- **2:** in one radio call, a Marine named on his own keeps his own order over his team's.
- **3:** a team follows a team by following its leader.
- **4:** a re-read keeps an earlier order for anyone it leaves out.
- **5:** told to hold, the interpreter stays put. The carry problem turned out to be different from what I first wrote: telling a carrier the same thing again made him set the casualty down where he stood and report "at B3". Now he keeps carrying.
- **6:** same fix as the interpreter in 5.
- **7, 8:** new prompt rules: orders only to whoever the player talks to; fly drone for anything about the drone; skip "be ready to"; "disregard" with nothing new means hold.
- **9:** smoke past a throw goes as far as it can toward the spot.
- **10:** hit by someone nobody sees, the team calls "Taking fire from the southeast! Can't see the shooter."

## The scoreboard

`python3 Playtest/playthrough/ft.py score <run> [turns]` scores a run from its transcript, the same way for every run. For each turn it counts the first read of the player's traffic and the final one before the minute played:

- **Orders:** how many the game master read.
- **Refused:** orders the game turned down for someone ("Say again…", "Too far…").
- **Asked:** the game master's own say-agains.
- **Missing:** asks not covered.
- **Corrections:** how many follow-up messages the turn took.

Orders the game master made up or lost are counted by hand.

Run 1, its first 8 turns (turns 7 and 8 were waits):

```
turn | first read                                  | final read                                  | corrections
   1 |  5 orders,  3 refused,  0 asked,  7 missing |  5 orders,  1 refused,  0 asked,  4 missing | 1
   2 |  5 orders,  1 refused,  0 asked,  4 missing |  4 orders,  0 refused,  0 asked,  3 missing | 1
   3 |  8 orders,  2 refused,  1 asked,  1 missing |  9 orders,  0 refused,  0 asked,  0 missing | 1
   4 | 10 orders,  1 refused,  2 asked,  2 missing |  9 orders,  1 refused,  0 asked,  5 missing | 1
   5 |  7 orders,  3 refused,  0 asked,  3 missing |  8 orders,  0 refused,  0 asked,  2 missing | 1
   6 |  2 orders,  0 refused,  0 asked,  1 missing |  2 orders,  0 refused,  0 asked,  1 missing | 0
all  | 37 orders, 10 refused,  3 asked, 18 missing | 37 orders,  2 refused,  0 asked, 15 missing | 5
```

By hand for run 1's first 8 turns:
- One order was made up (Alpha's assault, turn 5).
- Two orders were lost to a team order (Duarte's observe and Mercer's drone, turn 1).
- Two Marines were down by the end of turn 2 (Duarte killed, Novak hit).
- Charlie hadn't reached the post.

## The replay (run 1b), 8 minutes played 2026-11-12

Run 1's map (38545), a fresh expert, the run 1 fixes in. Record: `Playtest/playthrough/run-1b-transcript.md` and `run-1b-notes.md`. Scoreboard (`ft.py score 1b 8`):

```
turn | first read                                  | final read                                  | corrections
   1 |  6 orders,  0 refused,  1 asked,  3 missing |  6 orders,  0 refused,  1 asked,  0 missing | 1
   2 |  5 orders,  1 refused,  0 asked,  2 missing |  7 orders,  0 refused,  0 asked,  2 missing | 1
   3 | 14 orders,  0 refused,  0 asked,  6 missing | 16 orders,  0 refused,  0 asked,  2 missing | 1
   4 |  7 orders,  0 refused,  2 asked,  1 missing |  7 orders,  0 refused,  0 asked,  1 missing | 1
   5 |  6 orders,  0 refused,  1 asked,  4 missing |  8 orders,  0 refused,  0 asked,  7 missing | 1
   6 |  4 orders,  0 refused,  0 asked,  1 missing |  4 orders,  0 refused,  0 asked,  1 missing | 0
   7 |  5 orders,  0 refused,  0 asked,  3 missing |  6 orders,  0 refused,  0 asked,  3 missing | 1
   8 |  4 orders,  0 refused,  0 asked,  1 missing |  4 orders,  0 refused,  0 asked,  1 missing | 0
all  | 51 orders,  1 refused,  4 asked, 21 missing | 58 orders,  0 refused,  1 asked, 17 missing | 6
```

Quick read, before the debrief:
- **Turned down on the first read:** 1 of 51 orders (run 1: 10 of 37). Turned down after corrections: 0 (run 1: 2). The bracket names, team orders wiping a man's own order, and lost drone orders are gone.
- **Marines hurt by minute 8:** none (run 1: one killed and one down by minute 2).
- **Not covered:** still about the same (21 asks; run 1, 18). It's the same asks: cease fire and hold fire, "when set, then…", routes, "wreck the radio and scopes", shifting fire on a trigger. All of them are in the round 20 verb plan.
- **New problems found:**
  1. A team's correction in a later message lost to the members' own orders from the first message (fix 2 went too far: the later message should win).
  2. Readbacks promise "then …" steps that never get given.
  3. The squad leader's own moves ("Lead moving to D3", "Ruiz moving to E5") are never read as orders; only "Ruiz, move to E4" works.
  4. A re-read dropped Doc's treat and Keller's detain once.
  5. With hold fire not covered, "Gustaf on the post" opened fire at 500 m and gave the raid away. The player: *"Fire control being unenforceable is the most unrealistic thing so far."*
  6. Boone spent minutes 7 and 8 at E4 on "search the post" without closing the last 115 m.
  7. The harness lost minute 6's radio net to a crash (fixed).
- **The post wasn't searched by minute 8 in either run.**

## Morning report (overnight work, 2026-11-14)

Triston: *"Ahead and do some work at midnight tonight and I'll see what you've done in the morning"*. This was the first night working the studio way: minor calls made and reported, big calls left for you.

### What I fixed (minor calls, all with tests, 347 pass; the phone page is updated)

From the replay:
- **A correction now wins.** A later message's team order replaces its members' earlier orders, unless the newest message also speaks to that person by name.
- **Lead moves himself.** "Lead moving to D3" now moves the squad leader.
- **Honest readbacks.** They say only what was taken, and "moving to the post to search it" until the Marines get there.
- **Corrections keep good orders.** The game master now sees the orders already read and keeps them.
- **Boone didn't stall.** The post sits on a slope that climbs about 70 m in 85 m. The readbacks now show how long a move takes ("404 m, about 9 min"), and players said this helped.

From runs 2 and 3:
- **The drone stays up.** A drone operator flying the drone stays on it when his team gets an order.
- **What you watch isn't where you go.** "Put the gun on the post" no longer sends a gunner walking 400 m to the enemy alone.
- **A gunner told where to aim keeps moving with his team.**
- **Downed enemy can be named.** The game master is told which enemy are down or hurt, and "the wounded Kansians at F5" finds them for treat, talk, and detain.
- **"The hill" means the hill they're on or were sent to,** and "the crest of the hill" is the hill.
- **"Hold fire" is never read as "stop."** In run 3 it had cancelled Alpha's move.
- **"Wreck / smash / destroy the radio and scopes" is a search of that place.** This was the top mission complaint from all three players.
- **Say-agains no longer quote the game's limits** ("I can only suppress one square per order").

### The scoreboards (first 8 minutes, same raid, same map)

Each line: the first read of each turn's traffic | the final read after corrections | corrections.

```
Run 1 (expert, before any fixes)   | 37 orders, 10 refused,  3 asked, 18 missing | 37 orders,  2 refused,  0 asked, 15 missing | 5
Replay (expert, run 1 fixes)       | 51 orders,  1 refused,  4 asked, 21 missing | 58 orders,  0 refused,  1 asked, 17 missing | 6
Run 2 (casual)                     | 43 orders,  1 refused,  5 asked, 13 missing | 43 orders,  1 refused,  1 asked, 12 missing | 5
Run 3 (no military)                | 24 orders,  2 refused,  6 asked, 19 missing | 30 orders,  3 refused,  0 asked, 18 missing | 5
```

- **Orders turned down on the first read** went from 10 of 37 (run 1) to 1 to 2 in each later run.
- **Marines hurt:** run 1 had two down by minute 2. The replay, run 2, and run 3 had none.
- **Not covered** stayed about the same, 13 to 21 asks per run. The same asks every time (below).
- **Nobody searched the post by minute 8 in any run.** Part of that is the climb, part is "wreck the radio" not being taken (fixed tonight).

### What each player hit

- **The expert:** fire control above all (*"Fire control can't be optional"*). Also the mission task never taken, team callsigns, multi-step orders, Lead's own moves. *"The battlefield felt real. The radio is close."*
- **The casual player:** plain words misread: "which ridge, which slope, which crest" (*"There is only one hill"*), Ortiz sent alone toward the post on "overwatch," Doc saying *"Nobody's hit, Lead"* next to enemy wounded. *"Anything after the first verb got dropped."* Liked the travel times in the readbacks.
- **The new player:** "which hill" three times; hold fire read as "stop" (cancelled Alpha's move); a radio failure with no explanation (*"made me feel I'd lost control"*); readbacks claiming a search while still walking (*"looks like they're doing it when they aren't"*). Asked for a short line on what words the game takes, and a clear note on every part not understood.

### The trends

1. **Every player, every run: fire control.** Hold fire, cease fire, weapons tight, "only shoot what you're sure of." The squad fires on its own anyway, gave away the raid twice, and won fights the players didn't choose. This is v3 in round 20.
2. **Every player: "do this, then that."** Routes, "when you're set," "after the road, the post," shifting fire on a trigger. This is v2 in round 20.
3. **Plain words for places.** "The hill," "the ridge," "the slope." The casual and new players hit this most. Fixed tonight for "the hill"; describing ground from the map is v3.
4. **The drone is everyone's favorite part,** and the enemy breaking contact under fire felt real to all three.
5. **Players bend to the parser.** All three learned to name Marines one at a time, give one square, and use grid squares. That's realistic, as you said. The fixes above take away the worst of what they had to learn.

### Your calls (added to round 20 on the ledger, since round 20 isn't answered yet)

- **b1, steep hills make moves slow.** My pick: gentler hills on the map, same climbing pace.
- **b2, the brief undercounts the enemy** ("a few soldiers" against 8 with two machine guns and an RPG). My pick: a rough true count in the brief, with the fog in the details.
- **The rest of round 20 still waits on you,** and v1 to v3 (verbs, steps and triggers, fire control and routes) are exactly what all three players asked for most.

## Pick up here

1. Your answers to round 20 (including b1 and b2). Then build what you accept, starting with v3's fire control and v2's steps, the two things every player hit.
2. After that, the same three players replay the raid on map 38545 to see whether the trends move. Each run's scoreboard comes from `ft.py score <run> 8`.
3. All four runs are recorded in `Playtest/playthrough/` (run-1, run-1b, run-2, run-3: transcript, notes with debriefs, score). The player prompts are in `players/`.

## How to run a replay

Ruled: a fresh expert replays run 1 for 8 turns, then a debrief, then the comparison with the scoreboard above. Run 1 was **map 38545**, which rebuilds it exactly: same brief, places, Marines, and starting spots.

1. Build the page: `Playtest/build.sh`. It was built with the fixes on 2026-11-06.
2. Start the harness: `node Playtest/playthrough/server.js Playtest/out <shots dir> 8802`. Stop any old one first; two at once crash on the same port.
3. Start the mission: `PLAYER_DIR=<player dir> python3 Playtest/playthrough/ft.py start 1b marines 38545`, and save what it prints as `<player dir>/turn-01.txt`.
4. Start the player: a fresh Opus agent with `Playtest/playthrough/players/expert.md`. Put the turn-01 path in place of `{TURN1}`.
5. Each turn:
   - The player's traffic goes in with `ft.py send 1b "<traffic>"`.
   - A fresh Haiku agent reads the prompt file it writes. Use only Read and Write, and save the JSON to `answer.json`.
   - Then run `ft.py reader 1b answer.json`.
   - If anything was turned down or not covered, the player gets one correction.
   - Then `PLAYER_DIR=<player dir> ft.py play 1b`, and the player gets the turn file.
   - Use one agent at a time.
6. After 8 turns, send the player `players/debrief.md`.
7. Run `ft.py score 1b 8` and set it beside run 1's.

Also:
- Runs 2 and 3 (the casual player and the new player) haven't started.
- Ledger round 20 has no answers yet.
- The full record of run 1 is `Playtest/playthrough/run-1-transcript.md`, with the running notes in `Playtest/playthrough/run-1-notes.md`. The scorer reads `transcript-<run>.md` next to `ft.py`, so copy run 1's there to score it.
