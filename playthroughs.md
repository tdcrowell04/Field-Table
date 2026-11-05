# Playthroughs: lessons so far

Triston's ask: three playthroughs of random missions (an expert radio operator on Opus, someone who knows the military but talks casually on Sonnet, someone with no military experience on Sonnet), then an AAR and a plan. *"This will be how we iterate and improve the game from here on out."*

Paused at Triston's word: *"Let's just pause here for now and take our lessons and write them down where they can't get lost then we'll pick up after the usage reset."* Also: *"Alright let's re design the test to ensure better results, but also in reality a player would bend to fit the game so we just need to see what's realistic and make it better each time we do this"* and *"we'll hold off on the next play test for now as well."*

Nothing below is ruled yet. The fixes and the test changes go to Triston for accept or veto before any of them are built.

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

## Changes to the test, for Triston to accept or veto

1. **Real Haiku for every part.** With a Claude API key added as a secret in the cloud environment, the harness can send the page's exact prompts to Haiku directly. That covers the director and the voices too, not just the order reader. Each read would take seconds instead of about 40, and cost a small part of the tokens.
2. **Fix the bugs above first,** so the next run finds new problems instead of the same ones.
3. **A scoreboard for every run.** The harness counts orders sent, orders taken, say-agains, asks not covered, and orders the game master invented, so runs can be compared.
4. **The same mission and seed for all three players,** so the runs compare directly, then random missions after that.
5. **A short debrief from each player at the end:** which say-agains felt real and which felt broken.

## Pick up here

- Run 1 stopped at turn 10. Its game lives only in this container and won't survive, so start fresh.
- Runs 2 and 3 haven't started.
- Ledger round 20 has no answers yet.
- The full record of run 1 is `Playtest/playthrough/run-1-transcript.md`, with the running notes in `Playtest/playthrough/run-1-notes.md`.
- To run the harness:
  1. Build the page (`Playtest/build.sh`).
  2. Start the harness: `node Playtest/playthrough/server.js Playtest/out <shots dir> 8802`.
  3. Drive it: `python3 Playtest/playthrough/ft.py start|send|reader|play <run> ...`.
  4. Each `send` writes the order-reader prompt to a file for Haiku. `reader` takes Haiku's answer back.
  5. `PLAYER_DIR` saves each minute's view for the player.
