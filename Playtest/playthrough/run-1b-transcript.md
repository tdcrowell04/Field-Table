## Start

```
MISSION: Operation Jasper Blind (Raid the observation post), playing as Marines, map 38545
BRIEF:
  Task: raid their observation post on Hill 17A (F5), wreck its radio and scopes (search the post), and get back past the start line.
  Intent: their artillery blind, and a prisoner if you can get one.
  Enemy: a few soldiers at the post. Their reaction force will come once they know, sometime between turn 8 and turn 14. A shepherd says they've been busy on the slope facing us.
  Civilians: none known nearby. Standing ROE: positive ID before engaging.
  You: 13 Marines in three fire teams plus Doc and an interpreter, at B3, about 400 m from the post, just behind the start line. The villagers and the Kansian soldiers don't speak English.
MAIN OBJECTIVE: Wreck the post's radio and scopes, then get back past the start line.
WIN: The post's radio and scopes wrecked (a Marine searches within 10 m of them), and every Marine still alive back past the start line at the end of a turn.
LOSE: 7 Marines down, or past turn 40.
ROE: Positive ID before engaging.
EXTRAS: A prisoner taken: notyet; Every Marine comes home alive: ok
MAP: columns A to J, rows 1 to 10, each square 100 m. Six-digit coordinates are meters east then meters north.
PLACES: Hill 77 (C3), Hill 16 (C10), Hill 17 (E9), Hill 17A (F5), Broken Ridge (F6), Bitter Creek (E7), Route Jasper (E5), Bitter Creek bridge (E7), the post (F5)
LINES: the start line
BUILDINGS: 
TIME: turn 1 of 40 (Turn 1 of 40 · 0:00)
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at B3: ok; doing: nothing; 210 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: nothing; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at B3: ok; doing: nothing; 420 rds
  Duarte (Rifleman, marksman, Alpha) at B3: ok; doing: nothing; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at A3: ok; doing: nothing; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: nothing; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at A3: ok; doing: nothing; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: nothing; 210 rds
  Boone (Team leader, Charlie) at A3: ok; doing: nothing; 210 rds
  Sato (Automatic rifleman, Charlie) at A3: ok; doing: nothing; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at A3: ok; doing: nothing; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: nothing; 210 rds
KNOWN CONTACTS:
  none
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-001.png
```

### Player traffic (turn 1)

> All stations, this is Lead. FRAGO follows. Break.
> Charlie, this is Lead. Mercer, put the drone up now. Recon the post at F5 and the whole west slope of Hill 17A from E4 to E6. Look for wire, mines, trenches, and how many personnel. Report SALUTE when you have eyes on. Charlie, hold security at A3 facing east and south. Over.
> Alpha, this is Lead. Move up the north side of Hill 77, stay below the crest, set in at D3 on the military crest by the treeline, facing east toward the post. Weapons tight, positive ID only. Do not skyline. Report set. Over.
> Bravo, this is Lead. Hold at A3 and prep to follow Alpha. Okafor, keep the Gustaf ready. Over.
> HQ, Doc and Keller move with me behind Alpha. Casualty collection point is the treeline at C3. Break.
> Nobody crosses the start line until I say so. Lead, out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "A3", "target": null, "facing": "east and south", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "D3", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Mercer, drone going up now. Recon on the post."}, {"who": "Charlie", "text": "Charlie, moving to A3, security facing east and south."}, {"who": "Alpha", "text": "Alpha, moving to D3, facing east."}, {"who": "Bravo", "text": "Bravo, holding at A3."}, {"who": "Marsh", "text": "Doc, following Lead."}, {"who": "Keller", "text": "Keller, following Lead."}], "sayAgain": ["Mercer: Say again, which spot on the west slope?"], "notCovered": [{"who": "Okafor", "text": "keep the Gustaf ready"}, {"who": "Alpha", "text": "weapons tight, positive ID only, do not skyline"}, {"who": "squad", "text": "nobody crosses the start line until I say so"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post
Mercer, drone going up now. Recon on the post.
Mercer
Charlie · moveA3 · facing east and south
Charlie, moving to A3, security facing east and south.
Boone, 29 m
Sato, 32 m
Banks, 32 m
Alpha · moveD3 · facing east
Alpha, moving to D3, facing east.
Novak, 223 m
Ortiz, 228 m
Doyle, 234 m
Duarte, 232 m
Bravo · holdA3
Bravo, holding at A3.
Pike
Tran
Vega
Okafor
Marsh · followLead
Doc, following Lead.
Marsh
Keller · followLead
Keller, following Lead.
Keller
Mercer: Say again, which spot on the west slope?
Not covered yetOkafor
keep the Gustaf ready. Saved so it can be built.
Not covered yetAlpha
weapons tight, positive ID only, do not skyline. Saved so it can be built.
Not covered yetsquad
nobody crosses the start line until I say so. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 1)

> Mercer, this is Lead. West slope means the face of Hill 17A looking at us, grid squares E4, E5, and E6, between Route Jasper and the post. Fly the post at F5 first, then run that slope north to south. Over.
> Alpha, this is Lead. Weapons tight, positive ID before you engage. Stay below the crest, do not skyline. Over.
> All stations, Lead. Nobody crosses the start line until I give the word. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "hold", "where": "A3", "target": null, "facing": "east and south", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "D3", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "Lead", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "Lead", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Mercer, drone up, flying to the post."}, {"who": "Charlie", "text": "Charlie, holding security at A3, facing east and south."}, {"who": "Alpha", "text": "Alpha, moving to D3, facing east."}, {"who": "Bravo", "text": "Bravo, holding at A3."}, {"who": "Marsh", "text": "Doc, moving with Lead."}, {"who": "Keller", "text": "Interpreter, moving with Lead."}], "sayAgain": ["Mercer: Roger the post first. Say again, which square for the slope run?"], "notCovered": []}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post
Mercer, drone up, flying to the post.
Mercer
Charlie · holdA3 · facing east and south
Charlie, holding security at A3, facing east and south.
Boone
Sato
Banks
Alpha · moveD3 · facing east
Alpha, moving to D3, facing east.
Novak, 223 m
Ortiz, 228 m
Doyle, 234 m
Duarte, 232 m
Bravo · holdA3
Bravo, holding at A3.
Pike
Tran
Vega
Okafor
Marsh · moveLead
Doc, moving with Lead.
Marsh, 7 m
Keller · moveLead
Interpreter, moving with Lead.
Keller, 7 m
Mercer: Roger the post first. Say again, which square for the slope run?
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 2 of 40 (Turn 2 of 40 · 1:00)
RADIO NET SINCE YOUR LAST LOOK:
  [0:00] Lead: Mercer, this is Lead. West slope means the face of Hill 17A looking at us, grid squares E4, E5, and E6, between Route Jasper and the post. Fly the post at F5 first, then run that slope north to south. Over.
Alpha, this is Lead. Weapons tight, positive ID before you engage. Stay below the crest, do not skyline. Over.
All stations, Lead. Nobody crosses the start line until I give the word. Out.
  [0:00] Mercer: drone up, flying to the post.
  [0:00] Charlie: holding security at A3, facing east and south.
  [0:00] Alpha: moving to D3, facing east.
  [0:00] Bravo: holding at A3.
  [0:00] Marsh: Doc, moving with Lead.
  [0:00] Keller: Interpreter, moving with Lead.
  [0:00] Mercer: Roger the post first. Say again, which square for the slope run?
  [0:08] Keller: Set at Lead.
  [0:11] Marsh: Set at Lead.
  [0:14] Mercer: Drone's up.
  [0:30] Charlie: Contact on the drone, Kansian rifleman at E6.
  [0:30] Charlie: Contact on the drone, Kansian rifleman at E5.
  [0:30] Charlie: Contact on the drone, Kansian rifleman at E5.
  [0:30] Drone spots Rifleman 1 at E6.
  [0:30] Drone spots Rifleman 2 at E5.
  [0:30] Drone spots Rifleman 3 at E5.
  [0:32] Charlie: Contact on the drone, Kansian machine gunner at E5.
  [0:32] Drone spots PKM gunner 2 at E5.
  [0:51] Charlie: Contact on the drone, Kansian squad leader at F5.
  [0:51] Charlie: Contact on the drone, Kansian rifleman at F5.
  [0:51] Charlie: Contact on the drone, Kansian team leader at F5.
  [0:51] Charlie: Contact on the drone, Kansian machine gunner at F4.
  [0:51] Drone spots Squad leader 1 at F5.
  [0:51] Drone spots Observer 1 at F5.
  [0:51] Drone spots Team leader 1 at F5.
  [0:51] Drone spots PKM gunner 1 at F4.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: move D3 facing east; 210 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: move D3 facing east; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: move D3 facing east; 420 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: move D3 facing east; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at A3: ok; doing: nothing; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: nothing; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at A3: ok; doing: nothing; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: nothing; 210 rds
  Boone (Team leader, Charlie) at A3: ok; doing: nothing; 210 rds
  Sato (Automatic rifleman, Charlie) at A3: ok; doing: nothing; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at A3: ok; doing: nothing; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at F5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now
  PKM gunner 1 (op) at F4, seen now
  PKM gunner 2 (op) at E5, seen now
  Rifleman 1 (op) at E6, seen now
  Rifleman 2 (op) at E5, seen now
  Rifleman 3 (op) at E5, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-002.png
```

### Player traffic (turn 2)

> Mercer, this is Lead. Square E5. Then hold the drone over the post and keep eyes on, report any movement. Over.
> Alpha, this is Lead. Continue to D3. On set, Ortiz lay the gun on PKM at E5, Duarte take the PKM at F4. Hold fire until my command. Over.
> Bravo, this is Lead. Move now to D3, set in on Alpha's right, Tran's gun on E5 and E6. Okafor, Gustaf on the bunker at F5. Hold fire until my command. Over.
> Charlie, this is Lead. Warning order: you are the assault element with HQ. Stand by to move north around Hill 77 by C2, D1, E2, and hit the post from the north. Over.
> All stations, Lead. Enemy is eight pax, two PKMs, dug in on the west slope E5 and E6 and the post at F5. Weapons tight until I call it. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "overwatch", "where": null, "target": "PKM gunner 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "overwatch", "where": null, "target": "PKM gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Lead, Mercer. Drone to the post, eyes on, reporting any movement."}, {"who": "Alpha", "text": "Alpha, moving to D3."}, {"who": "Ortiz", "text": "Ortiz, overwatch PKM gunner 2 at E5."}, {"who": "Duarte", "text": "Duarte, overwatch PKM gunner 1 at F4."}, {"who": "Bravo", "text": "Bravo, moving to D3."}], "sayAgain": ["Tran: Say again, E5 or E6?", "Okafor: Say again, which target is the bunker at F5?"], "notCovered": [{"who": "Alpha", "text": "hold fire until my command"}, {"who": "Bravo", "text": "hold fire until my command"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post
Lead, Mercer. Drone to the post, eyes on, reporting any movement.
Mercer
Alpha · moveD3
Alpha, moving to D3.
Novak, 139 m
Doyle, 151 m
Ortiz · overwatchPKM gunner 2
Ortiz, overwatch PKM gunner 2 at E5.
Ortiz
Duarte · overwatchPKM gunner 1
Duarte, overwatch PKM gunner 1 at F4.
Duarte
Bravo · moveD3
Bravo, moving to D3.
Pike, 267 m
Tran, 272 m
Vega, 263 m
Okafor, 275 m
Tran: Say again, E5 or E6?
Okafor: Say again, which target is the bunker at F5?
Not covered yetAlpha
hold fire until my command. Saved so it can be built.
Not covered yetBravo
hold fire until my command. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 2)

> Tran, this is Lead. E5, the PKM. Over.
> Okafor, this is Lead. Target is the post itself at F5, where the squad leader and observer are. Over.
> Ortiz, Duarte, this is Lead. Move with Alpha to D3 first, then overwatch from there. Over.
> Alpha, Bravo, this is Lead. Hold fire. Do not engage until I say open fire. How copy, over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "overwatch", "where": "D3", "target": "PKM gunner 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "overwatch", "where": "D3", "target": "PKM gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Tran", "verb": "overwatch", "where": null, "target": "PKM gunner 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "fire", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Mercer, drone to the post, eyes on, will report movement."}, {"who": "Alpha", "text": "Alpha, moving to D3."}, {"who": "Ortiz", "text": "Ortiz, moving to D3 with Alpha, then overwatch on the PKM at E5."}, {"who": "Duarte", "text": "Duarte, moving to D3 with Alpha, then overwatch on the PKM at F4."}, {"who": "Bravo", "text": "Bravo, moving to D3."}, {"who": "Tran", "text": "Tran, overwatch on the PKM at E5."}, {"who": "Okafor", "text": "Okafor, Gustaf on the post."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "hold fire until I say open fire"}, {"who": "Bravo", "text": "hold fire until I say open fire"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post
Mercer, drone to the post, eyes on, will report movement.
Mercer
Alpha · moveD3
Alpha, moving to D3.
Novak, 139 m
Doyle, 151 m
Ortiz · overwatchD3 · PKM gunner 2
Ortiz, moving to D3 with Alpha, then overwatch on the PKM at E5.
Ortiz, 144 m
Duarte · overwatchD3 · PKM gunner 1
Duarte, moving to D3 with Alpha, then overwatch on the PKM at F4.
Duarte, 148 m
Bravo · moveD3
Bravo, moving to D3.
Pike, 267 m
Vega, 263 m
Tran · overwatchPKM gunner 2
Tran, overwatch on the PKM at E5.
Tran
Okafor · firethe post
Okafor, Gustaf on the post.
Okafor
Not covered yetAlpha
hold fire until I say open fire. Saved so it can be built.
Not covered yetBravo
hold fire until I say open fire. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 3 of 40 (Turn 3 of 40 · 2:00)
RADIO NET SINCE YOUR LAST LOOK:
  [1:00] Lead: Tran, this is Lead. E5, the PKM. Over.
Okafor, this is Lead. Target is the post itself at F5, where the squad leader and observer are. Over.
Ortiz, Duarte, this is Lead. Move with Alpha to D3 first, then overwatch from there. Over.
Alpha, Bravo, this is Lead. Hold fire. Do not engage until I say open fire. How copy, over.
  [1:00] Mercer: drone to the post, eyes on, will report movement.
  [1:00] Alpha: moving to D3.
  [1:00] Ortiz: moving to D3 with Alpha, then overwatch on the PKM at E5.
  [1:00] Duarte: moving to D3 with Alpha, then overwatch on the PKM at F4.
  [1:00] Bravo: moving to D3.
  [1:00] Tran: overwatch on the PKM at E5.
  [1:00] Okafor: Gustaf on the post.
  [1:00] Bravo: Set. Overwatch on PKM gunner 2.
  [1:00] Bravo: Engaging F5.
  [1:21] Team leader 1 killed by M320 grenade launcher, hit in the head.
  [1:22] Charlie: Contact on the drone, Kansian RPG gunner at E5.
  [1:22] Drone spots RPG gunner 1 at E5.
  [1:34] Rifleman 2 is wounded by M38 marksman rifle, hit in the arm.
  [1:52] Rifleman 2 is down, hit by M27 rifle, hit in the chest.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: move D3; 180 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 PKM gunner 2; 504 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: move D3; 372 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: overwatch D3 PKM gunner 1; 103 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at B3: ok; doing: move D3; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: overwatch PKM gunner 2; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B3: ok; doing: move D3; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: fire the post; 174 rds
  Boone (Team leader, Charlie) at A3: ok; doing: nothing; 210 rds
  Sato (Automatic rifleman, Charlie) at A3: ok; doing: nothing; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at A3: ok; doing: nothing; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at G5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now, dead
  PKM gunner 1 (op) at G5, seen now
  PKM gunner 2 (op) at F6, seen now
  RPG gunner 1 (op) at F6, seen now
  Rifleman 1 (op) at F6, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at F6, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-003.png
```

### Player traffic (turn 3)

> All stations, this is Lead. We are compromised. Break. Base of fire, open fire.
> Alpha, this is Lead. Get everybody set at D3 now. Ortiz suppress F6, Duarte take PKM at G5. Over.
> Bravo, this is Lead. Tran, Vega, Okafor, Pike, move to D3 now and set on Alpha's right. Tran suppress F6. Okafor, Gustaf on the RPG at F6 once you are set at D3. Over.
> Charlie, this is Lead. Execute. Move by C2, D1, E2 to E3, then assault south onto the post at F5 from the north. Mercer, keep the drone on the post and call me any movement. Over.
> HQ, Doc and Keller with me, we move with Charlie. Break.
> Alpha, Bravo, shift fire south off F5 when Charlie reaches E3. That is your shift trigger. Lead, out.

Game master (Haiku) read it as:
```json
{"orders": [
{"who": "Novak", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Doyle", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Ortiz", "verb": "suppress", "where": "F6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Duarte", "verb": "fire", "where": null, "target": "PKM gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Vega", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Pike", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Tran", "verb": "suppress", "where": "F6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Okafor", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Boone", "verb": "move", "where": "E3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Sato", "verb": "move", "where": "E3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Banks", "verb": "move", "where": "E3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Marsh", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Keller", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}
],
"readbacks": [
{"who": "Novak", "text": "Novak, moving to D3."},
{"who": "Doyle", "text": "Doyle, moving to D3."},
{"who": "Ortiz", "text": "Ortiz, suppressing F6."},
{"who": "Duarte", "text": "Duarte, taking PKM gunner 1."},
{"who": "Vega", "text": "Vega, moving to D3."},
{"who": "Pike", "text": "Pike, moving to D3."},
{"who": "Tran", "text": "Tran, suppressing F6."},
{"who": "Okafor", "text": "Okafor, moving to D3."},
{"who": "Boone", "text": "Boone, moving to E3."},
{"who": "Sato", "text": "Sato, moving to E3."},
{"who": "Banks", "text": "Banks, moving to E3."},
{"who": "Mercer", "text": "Mercer, drone to the post."},
{"who": "Marsh", "text": "Doc, following Charlie."},
{"who": "Keller", "text": "Interpreter, following Charlie."}
],
"sayAgain": [],
"notCovered": [
{"who": "squad", "text": "base of fire, open fire (no target given)"},
{"who": "Okafor", "text": "fire on the RPG at F6 once set at D3"},
{"who": "Charlie", "text": "then assault south onto the post at F5 from the north, by way of C2, D1, E2"},
{"who": "Mercer", "text": "call Lead on any movement"},
{"who": "Alpha", "text": "shift fire south off F5 when Charlie reaches E3"},
{"who": "Bravo", "text": "shift fire south off F5 when Charlie reaches E3"}
]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Novak · moveD3
Novak, moving to D3.
Novak, 55 m
Doyle · moveD3
Doyle, moving to D3.
Doyle, 67 m
Ortiz · suppressF6
Ortiz, suppressing F6.
Ortiz
Duarte · firePKM gunner 1
Duarte, taking PKM gunner 1.
Duarte
Vega · moveD3
Vega, moving to D3.
Vega, 211 m
Pike · moveD3
Pike, moving to D3.
Pike, 194 m
Tran · suppressF6
Tran, suppressing F6.
Tran
Okafor · moveD3
Okafor, moving to D3.
Okafor, 275 m
Boone · moveE3
Boone, moving to E3.
Boone, 400 m
Sato · moveE3
Sato, moving to E3.
Sato, 380 m
Banks · moveE3
Banks, moving to E3.
Banks, 386 m
Mercer · fly dronethe post
Mercer, drone to the post.
Mercer
Marsh · followCharlie
Doc, following Charlie.
Marsh
Keller · followCharlie
Interpreter, following Charlie.
Keller
Not covered yetsquad
base of fire, open fire (no target given). Saved so it can be built.
Not covered yetOkafor
fire on the RPG at F6 once set at D3. Saved so it can be built.
Not covered yetCharlie
then assault south onto the post at F5 from the north, by way of C2, D1, E2. Saved so it can be built.
Not covered yetMercer
call Lead on any movement. Saved so it can be built.
Not covered yetAlpha
shift fire south off F5 when Charlie reaches E3. Saved so it can be built.
Not covered yetBravo
shift fire south off F5 when Charlie reaches E3. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 3)

> Charlie, this is Lead. Correction. Move to D1 first, stay north of Hill 77 crest and out of sight of the post. Report at D1. Over.
> Okafor, this is Lead. When you are set at D3, fire the Gustaf at the RPG gunner at F6. Over.
> Tran, this is Lead. Move to D3 with Bravo, then suppress F6. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "suppress", "where": "F6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "fire", "where": null, "target": "PKM gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Tran", "verb": "suppress", "where": "F6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "fire", "where": null, "target": "RPG gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Pike", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Vega", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "hide", "where": "D1", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, moving to D3."}, {"who": "Ortiz", "text": "Ortiz, suppressing F6."}, {"who": "Duarte", "text": "Duarte, firing on PKM gunner 1."}, {"who": "Tran", "text": "Tran, suppressing F6 once set at D3."}, {"who": "Okafor", "text": "Okafor, Gustaf on RPG gunner 1 once set at D3."}, {"who": "Pike", "text": "Pike, moving to D3."}, {"who": "Vega", "text": "Vega, moving to D3."}, {"who": "Charlie", "text": "Charlie, moving to D1, staying out of sight. Will report at D1."}, {"who": "Mercer", "text": "Mercer, flying drone over the post."}, {"who": "Marsh", "text": "Marsh, following Charlie."}, {"who": "Keller", "text": "Keller, following Charlie."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "shift fire south off F5 when Charlie reaches E3"}, {"who": "Bravo", "text": "shift fire south off F5 when Charlie reaches E3"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Novak · moveD3
Novak, moving to D3.
Novak, 55 m
Doyle · moveD3
Doyle, moving to D3.
Doyle, 67 m
Boone · moveE3
Boone, moving to E3.
Boone, 400 m
Sato · moveE3
Sato, moving to E3.
Sato, 380 m
Banks · moveE3
Banks, moving to E3.
Banks, 386 m
Alpha · moveD3
Alpha, moving to D3.
Ortiz · suppressF6
Ortiz, suppressing F6.
Ortiz
Duarte · firePKM gunner 1
Duarte, firing on PKM gunner 1.
Duarte
Tran · suppressF6
Tran, suppressing F6 once set at D3.
Tran
Okafor · fireRPG gunner 1
Okafor, Gustaf on RPG gunner 1 once set at D3.
Okafor
Pike · moveD3
Pike, moving to D3.
Pike, 194 m
Vega · moveD3
Vega, moving to D3.
Vega, 211 m
Charlie · hideD1
Charlie, moving to D1, staying out of sight. Will report at D1.
Mercer · fly dronethe post
Mercer, flying drone over the post.
Mercer
Marsh · followCharlie
Marsh, following Charlie.
Marsh
Keller · followCharlie
Keller, following Charlie.
Keller
Not covered yetAlpha
shift fire south off F5 when Charlie reaches E3. Saved so it can be built.
Not covered yetBravo
shift fire south off F5 when Charlie reaches E3. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 4 of 40 (Turn 4 of 40 · 3:00)
RADIO NET SINCE YOUR LAST LOOK:
  [2:00] Lead: Charlie, this is Lead. Correction. Move to D1 first, stay north of Hill 77 crest and out of sight of the post. Report at D1. Over.
Okafor, this is Lead. When you are set at D3, fire the Gustaf at the RPG gunner at F6. Over.
Tran, this is Lead. Move to D3 with Bravo, then suppress F6. Over.
  [2:00] Novak: moving to D3.
  [2:00] Doyle: moving to D3.
  [2:00] Boone: moving to E3.
  [2:00] Sato: moving to E3.
  [2:00] Banks: moving to E3.
  [2:00] Ortiz: suppressing F6.
  [2:00] Duarte: firing on PKM gunner 1.
  [2:00] Tran: suppressing F6 once set at D3.
  [2:00] Okafor: Gustaf on RPG gunner 1 once set at D3.
  [2:00] Pike: moving to D3.
  [2:00] Vega: moving to D3.
  [2:00] Mercer: flying drone over the post.
  [2:00] Marsh: following Charlie.
  [2:00] Keller: following Charlie.
  [2:00] Bravo: Suppressing F6.
  [2:00] Alpha: Suppressing F6.
  [2:40] Alpha: Set at D3.
  [2:48] Alpha: Set at D3.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: nothing; 180 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: suppress F6; 324 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: nothing; 372 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: fire PKM gunner 1; 103 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B2: ok; doing: follow Charlie; 150 rds
  Keller (Interpreter, HQ) at B2: ok; doing: follow Charlie; 0 rds
  Pike (Team leader, Bravo) at C3: ok; doing: move D3; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: suppress F6; 480 rds
  Vega (Assistant automatic rifleman, Bravo) at C3: ok; doing: move D3; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: fire RPG gunner 1; 174 rds
  Boone (Team leader, Charlie) at B2: ok; doing: move E3; 210 rds
  Sato (Automatic rifleman, Charlie) at B3: ok; doing: move E3; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B3: ok; doing: move E3; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at H6, seen now
  Observer 1 (op) at H6, seen now
  Team leader 1 (op) at F5, seen now, dead
  PKM gunner 1 (op) at H6, seen now
  PKM gunner 2 (op) at G7, seen now
  RPG gunner 1 (op) at H7, seen now
  Rifleman 1 (op) at G8, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at G7, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-004.png
```

### Player traffic (turn 4)

> All stations, this is Lead. Enemy has pulled off the post to H6 and G7. Break.
> Alpha, this is Lead. Suppress H6 and G7. Duarte, move up to D3 and take PKM gunner 1 at H6. Over.
> Bravo, this is Lead. Continue to D3, then suppress G7. Over.
> Charlie, this is Lead. Continue to E3, then move to the post at F5 and search it. Wreck the radio and scopes. Over.
> Mercer, this is Lead. Keep the drone over G7 and H6 and report if they move back toward the post. Over.
> Doc, Keller, stay with Charlie. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "suppress", "where": "H6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "E3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "G7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, suppressing H6."}, {"who": "Duarte", "text": "Duarte, moving up to D3, then on PKM gunner 1 at H6."}, {"who": "Bravo", "text": "Bravo, continuing to D3, then suppressing G7."}, {"who": "Charlie", "text": "Charlie, continuing to E3, then to the post at F5."}, {"who": "Mercer", "text": "Mercer, drone over G7, reporting if they move back toward the post."}, {"who": "Marsh", "text": "Doc, staying with Charlie."}, {"who": "Keller", "text": "Interpreter, staying with Charlie."}], "sayAgain": ["Alpha: Say again, G7 too? I can only suppress one square per order.", "Mercer: Say again, drone over G7 only, or H6 too?"], "notCovered": [{"who": "Charlie", "text": "wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · suppressH6
Alpha, suppressing H6.
Novak
Ortiz
Doyle
Duarte · moveD3
Duarte, moving up to D3, then on PKM gunner 1 at H6.
Duarte, 64 m
Bravo · moveD3
Bravo, continuing to D3, then suppressing G7.
Pike, 109 m
Tran, 272 m
Vega, 128 m
Okafor, 275 m
Charlie · moveE3
Charlie, continuing to E3, then to the post at F5.
Boone, 331 m
Sato, 322 m
Banks, 327 m
Mercer · fly droneG7
Mercer, drone over G7, reporting if they move back toward the post.
Mercer
Marsh · followCharlie
Doc, staying with Charlie.
Marsh
Keller · followCharlie
Interpreter, staying with Charlie.
Keller
Alpha: Say again, G7 too? I can only suppress one square per order.
Mercer: Say again, drone over G7 only, or H6 too?
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 4)

> Alpha, this is Lead. H6 only. Bravo has G7. Over.
> Mercer, this is Lead. Hover between them over G6 so you see both. Over.
> Charlie, this is Lead. When you reach the post, search it and wreck the radio and scopes. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "suppress", "where": "H6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "suppress", "where": "G7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "G6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, suppressing H6."}, {"who": "Duarte", "text": "Duarte, moving to D3."}, {"who": "Bravo", "text": "Bravo, suppressing G7."}, {"who": "Charlie", "text": "Charlie, searching the post."}, {"who": "Mercer", "text": "Mercer, flying the drone over G6."}, {"who": "Marsh", "text": "Doc, following Charlie."}, {"who": "Keller", "text": "Interpreter, following Charlie."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · suppressH6
Alpha, suppressing H6.
Novak
Ortiz
Doyle
Duarte · moveD3
Duarte, moving to D3.
Duarte, 64 m
Bravo · suppressG7
Bravo, suppressing G7.
Pike
Tran
Vega
Okafor
Charlie · searchthe post
Charlie, searching the post.
Boone, 438 m
Sato, 429 m
Banks, 404 m
Mercer · fly droneG6
Mercer, flying the drone over G6.
Mercer
Marsh · followCharlie
Doc, following Charlie.
Marsh
Keller · followCharlie
Interpreter, following Charlie.
Keller
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 5 of 40 (Turn 5 of 40 · 4:00)
RADIO NET SINCE YOUR LAST LOOK:
  [3:00] Lead: Alpha, this is Lead. H6 only. Bravo has G7. Over.
Mercer, this is Lead. Hover between them over G6 so you see both. Over.
Charlie, this is Lead. When you reach the post, search it and wreck the radio and scopes. Over.
  [3:00] Alpha: suppressing H6.
  [3:00] Duarte: moving to D3.
  [3:00] Bravo: suppressing G7.
  [3:00] Charlie: searching the post.
  [3:00] Mercer: flying the drone over G6.
  [3:00] Marsh: Doc, following Charlie.
  [3:00] Keller: Interpreter, following Charlie.
  [3:00] Alpha: Suppressing H6.
  [3:26] Charlie: Contact on the drone, Kansian rifleman at H8.
  [3:26] Drone spots Rifleman 3 at H8.
  [3:46] Alpha: Set at D3.
  [3:53] Charlie: Contact on the drone, Kansian rifleman at I9.
  [3:53] Drone spots Rifleman 1 at I9.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: suppress H6; 111 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: suppress H6; 144 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: suppress H6; 303 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: nothing; 103 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Charlie; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Charlie; 0 rds
  Pike (Team leader, Bravo) at C3: ok; doing: suppress G7; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: suppress G7; 480 rds
  Vega (Assistant automatic rifleman, Bravo) at C3: ok; doing: suppress G7; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: suppress G7; 174 rds
  Boone (Team leader, Charlie) at B3: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at C3: ok; doing: search the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C3: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone G6; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at J7, seen now
  Observer 1 (op) at J7, seen now
  Team leader 1 (op) at F5, seen now, dead
  PKM gunner 1 (op) at J7, seen now
  PKM gunner 2 (op) at H8, seen now
  RPG gunner 1 (op) at I8, seen now
  Rifleman 1 (op) at I9, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at I8, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-005.png
```

### Player traffic (turn 5)

> All stations, this is Lead. Cease fire, cease fire. Enemy is out past 400 m at H8 to J7. Conserve ammo. Break.
> Alpha, this is Lead. Hold at D3, overwatch the post and the creek line to the south. Fire only on enemy moving toward the post. Over.
> Bravo, this is Lead. Tran, Okafor, move to D3 now and join Pike. Over.
> Charlie, this is Lead. Move direct to the post at F5 now, search it, wreck the radio and scopes. Over.
> Doc, Keller, go with Charlie to E5. Check the downed Kansian rifleman there. If he's alive, treat him and we take him as a prisoner. Over.
> Lead moving to D3. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "overwatch", "where": "D3", "target": "the post", "facing": "south", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Tran", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "detain", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, holding at D3, overwatch the post, facing south."}, {"who": "Tran", "text": "Tran, moving to D3 now."}, {"who": "Okafor", "text": "Okafor, moving to D3 now."}, {"who": "Charlie", "text": "Charlie, moving direct to the post, searching it."}, {"who": "Marsh", "text": "Marsh, treating Rifleman 2 if he's alive."}, {"who": "Keller", "text": "Keller, detaining Rifleman 2 if he's alive."}], "sayAgain": ["Alpha: Say again, which creek line?"], "notCovered": [{"who": "squad", "text": "cease fire"}, {"who": "squad", "text": "conserve ammo"}, {"who": "Alpha", "text": "fire only on enemy moving toward the post"}, {"who": "Charlie", "text": "wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · overwatchD3 · the post · facing south
Alpha, holding at D3, overwatch the post, facing south.
Novak, 0 m
Ortiz, 60 m
Doyle, 0 m
Duarte, 0 m
Tran · moveD3
Tran, moving to D3 now.
Tran, 272 m
Okafor · moveD3
Okafor, moving to D3 now.
Okafor, 275 m
Charlie · searchthe post
Charlie, moving direct to the post, searching it.
Boone, 430 m
Sato, 345 m
Banks, 327 m
Mercer, 528 m
Marsh · treat casualtyRifleman 2
Marsh, treating Rifleman 2 if he's alive.
Marsh, 443 m
Keller · detainRifleman 2
Keller, detaining Rifleman 2 if he's alive.
Keller, 443 m
Alpha: Say again, which creek line?
Not covered yetsquad
cease fire. Saved so it can be built.
Not covered yetsquad
conserve ammo. Saved so it can be built.
Not covered yetAlpha
fire only on enemy moving toward the post. Saved so it can be built.
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 5)

> Alpha, this is Lead. Bitter Creek, row 7, from the bridge at E7 east to H7. Over.
> Pike, Vega, this is Lead. Stop suppressing G7. Hold fire, move to D3 and set with Alpha. Over.
> Charlie, this is Lead. At the post, wreck the radio and scopes. Over.
> Ruiz moving to D3 now. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "overwatch", "where": "D3", "target": "the post", "facing": "south", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Tran", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Pike", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Vega", "verb": "move", "where": "D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, overwatch the post from D3, facing south."}, {"who": "Tran", "text": "Tran, moving to D3, joining Pike."}, {"who": "Okafor", "text": "Okafor, moving to D3, joining Pike."}, {"who": "Pike", "text": "Pike, moving to D3, setting with Alpha."}, {"who": "Vega", "text": "Vega, moving to D3, setting with Alpha."}, {"who": "Charlie", "text": "Charlie, moving to the post, searching it."}, {"who": "Marsh", "text": "Doc moving to E5 with Charlie."}, {"who": "Keller", "text": "Keller, moving to E5."}], "sayAgain": [], "notCovered": [{"who": "squad", "text": "cease fire"}, {"who": "squad", "text": "conserve ammo"}, {"who": "Alpha", "text": "overwatch Bitter Creek, row 7, from the bridge at E7 east to H7"}, {"who": "Alpha", "text": "fire only on enemy moving toward the post"}, {"who": "Charlie", "text": "wreck the radio and scopes at the post"}, {"who": "Marsh", "text": "check the downed Kansian rifleman at E5 and treat him if he's alive"}, {"who": "Keller", "text": "take the rifleman prisoner if he's alive"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · overwatchD3 · the post · facing south
Alpha, overwatch the post from D3, facing south.
Novak, 0 m
Ortiz, 60 m
Doyle, 0 m
Duarte, 0 m
Tran · moveD3
Tran, moving to D3, joining Pike.
Tran, 272 m
Okafor · moveD3
Okafor, moving to D3, joining Pike.
Okafor, 275 m
Pike · moveD3
Pike, moving to D3, setting with Alpha.
Pike, 109 m
Vega · moveD3
Vega, moving to D3, setting with Alpha.
Vega, 128 m
Charlie · searchthe post
Charlie, moving to the post, searching it.
Boone, 430 m
Sato, 345 m
Banks, 327 m
Mercer, 528 m
Marsh · moveE5
Doc moving to E5 with Charlie.
Marsh, 411 m
Keller · moveE5
Keller, moving to E5.
Keller, 411 m
Not covered yetsquad
cease fire. Saved so it can be built.
Not covered yetsquad
conserve ammo. Saved so it can be built.
Not covered yetAlpha
overwatch Bitter Creek, row 7, from the bridge at E7 east to H7. Saved so it can be built.
Not covered yetAlpha
fire only on enemy moving toward the post. Saved so it can be built.
Not covered yetCharlie
wreck the radio and scopes at the post. Saved so it can be built.
Not covered yetMarsh
check the downed Kansian rifleman at E5 and treat him if he's alive. Saved so it can be built.
Not covered yetKeller
take the rifleman prisoner if he's alive. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 6 of 40 (Turn 6 of 40 · 5:00)
RADIO NET SINCE YOUR LAST LOOK:
  [4:00] Lead: Alpha, this is Lead. Bitter Creek, row 7, from the bridge at E7 east to H7. Over.
Pike, Vega, this is Lead. Stop suppressing G7. Hold fire, move to D3 and set with Alpha. Over.
Charlie, this is Lead. At the post, wreck the radio and scopes. Over.
Ruiz moving to D3 now. Out.
  [4:00] Alpha: overwatch the post from D3, facing south.
  [4:00] Tran: moving to D3, joining Pike.
  [4:00] Okafor: moving to D3, joining Pike.
  [4:00] Pike: moving to D3, setting with Alpha.
  [4:00] Vega: moving to D3, setting with Alpha.
  [4:00] Charlie: moving to the post, searching it.
  [4:00] Marsh: Doc moving to E5 with Charlie.
  [4:00] Keller: moving to E5.
  [4:07] Charlie: Contact on the drone, Kansian RPG gunner at I9.
  [4:07] Drone spots RPG gunner 1 at I9.
  [4:25] Charlie: Contact on the drone, Kansian squad leader at J8.
  [4:25] Drone spots Squad leader 1 at J8.
  [4:26] Charlie: Contact on the drone, Kansian rifleman at J8.
  [4:26] Drone spots Observer 1 at J8.
  [4:28] Charlie: Contact on the drone, Kansian RPG gunner at J9.
  [4:40] Alpha: Set. Overwatch on the post.
  [4:40] Charlie: Contact on the drone, Kansian rifleman at J9.
  [4:40] Drone spots Rifleman 3 at J9.
  [4:41] Charlie: Contact on the drone, Kansian rifleman at J10.
  [4:41] Drone spots Rifleman 1 at J10.
  [4:53] Charlie: Contact on the drone, Kansian machine gunner at J8.
  [4:53] Drone spots PKM gunner 1 at J8.
  [4:54] Charlie: Contact on the drone, Kansian RPG gunner at J10.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 111 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 144 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 303 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 103 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: move E5; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: move E5; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: move D3; 210 rds
  Tran (Automatic rifleman, Bravo) at B3: ok; doing: move D3; 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: move D3; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B3: ok; doing: move D3; 174 rds
  Boone (Team leader, Charlie) at C3: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at C4: ok; doing: search the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C3: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: search the post; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at J9, seen now
  Observer 1 (op) at J9, seen now
  Team leader 1 (op) at F5, seen now, dead
  PKM gunner 1 (op) at J8, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-006.png
```

### Player traffic (turn 6)

> Charlie, this is Lead. Enemy has broken contact to the southeast edge, J9 and J10. Move fast, double time, straight to the post at F5. Over.
> Mercer, this is Lead. Stay at A4, keep the drone over the enemy at J9 and report if they turn back. Over.
> Doc, Keller, this is Lead. Move with me to E5 now, double time. Over.
> Ruiz moving to E5. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Charlie", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": "run", "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "J9", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "E5", "target": null, "facing": null, "speed": "run", "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "E5", "target": null, "facing": null, "speed": "run", "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Charlie", "text": "Charlie, running to the post."}, {"who": "Mercer", "text": "Mercer, drone over J9, flying it now."}, {"who": "Marsh", "text": "Doc, running to E5."}, {"who": "Keller", "text": "Keller, running to E5 with Lead."}], "sayAgain": [], "notCovered": [{"who": "Mercer", "text": "stay at A4 and report if they turn back"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · movethe post · run
Charlie, running to the post.
Boone, 319 m
Sato, 261 m
Banks, 243 m
Mercer · fly droneJ9
Mercer, drone over J9, flying it now.
Mercer
Marsh · moveE5 · run
Doc, running to E5.
Marsh, 356 m
Keller · moveE5 · run
Keller, running to E5 with Lead.
Keller, 356 m
Not covered yetMercer
stay at A4 and report if they turn back. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 7 of 40 (Turn 7 of 40 · 6:00)
RADIO NET SINCE YOUR LAST LOOK:
  (quiet)
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 111 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 144 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 303 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 103 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at D4: ok; doing: move E5; 150 rds
  Keller (Interpreter, HQ) at D4: ok; doing: move E5; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: nothing; 210 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: move D3; 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: nothing; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: move D3; 174 rds
  Boone (Team leader, Charlie) at D4: ok; doing: move the post; 210 rds
  Sato (Automatic rifleman, Charlie) at E4: ok; doing: move the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E4: ok; doing: move the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: fly drone J9; 210 rds
  drone: up over J9
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, last seen just now
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-008.png
```
(harness crashed printing this minute; its radio net was lost)

### Player traffic (turn 7)

> Charlie, this is Lead. Good speed, continue to the post. On arrival, search it and wreck the radio and scopes. Sato, Banks, set security facing south and east while Boone searches. Over.
> Ruiz, move to E4. Over.
> Alpha, Bravo, this is Lead. Prepare to withdraw. Once Charlie reports the post wrecked, you cover their pull-back from D3. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Charlie", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Boone", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Sato", "verb": "observe", "where": null, "target": null, "facing": "south and east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Banks", "verb": "observe", "where": null, "target": null, "facing": "south and east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Charlie", "text": "Charlie, moving to the post."}, {"who": "Boone", "text": "Boone, searching the post."}, {"who": "Sato", "text": "Sato, observing south and east."}, {"who": "Banks", "text": "Banks, observing south and east."}, {"who": "Ruiz", "text": "Ruiz, moving to E4."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "wreck the radio and scopes"}, {"who": "Alpha", "text": "once Charlie reports the post wrecked, cover their pull-back from D3"}, {"who": "Bravo", "text": "once Charlie reports the post wrecked, cover their pull-back from D3"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · movethe post
Charlie, moving to the post.
Mercer, 446 m
Boone · searchthe post
Boone, searching the post.
Boone, 148 m
Sato · observefacing south and east
Sato, observing south and east.
Sato
Banks · observefacing south and east
Banks, observing south and east.
Banks
Ruiz · moveE4
Ruiz, moving to E4.
Ruiz, 428 m
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Not covered yetAlpha
once Charlie reports the post wrecked, cover their pull-back from D3. Saved so it can be built.
Not covered yetBravo
once Charlie reports the post wrecked, cover their pull-back from D3. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 7)

> Mercer, this is Lead. Correction, stay at A4 and keep flying the drone. Do not move. Over.
> Sato, Banks, this is Lead. Move with Boone to the post at F5 first, then set security facing south and east. Over.
> Boone, this is Lead. At the post, wreck the radio and scopes. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Boone", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Sato", "verb": "move", "where": "the post", "target": null, "facing": "south and east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Banks", "verb": "move", "where": "the post", "target": null, "facing": "south and east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "J9", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Boone", "text": "Boone, moving to the post, searching it."}, {"who": "Sato", "text": "Sato, moving to the post, facing south and east."}, {"who": "Banks", "text": "Banks, moving to the post, facing south and east."}, {"who": "Ruiz", "text": "Moving to E4."}, {"who": "Mercer", "text": "Mercer, staying at A4, drone stays over J9."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "cover the pull-back from D3 once Charlie reports the post wrecked"}, {"who": "Bravo", "text": "cover the pull-back from D3 once Charlie reports the post wrecked"}, {"who": "Boone", "text": "wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · movethe post
Charlie, moving to the post.
Boone · searchthe post
Boone, moving to the post, searching it.
Boone, 148 m
Sato · movethe post · facing south and east
Sato, moving to the post, facing south and east.
Sato, 65 m
Banks · movethe post · facing south and east
Banks, moving to the post, facing south and east.
Banks, 123 m
Ruiz · moveE4
Moving to E4.
Ruiz, 428 m
Mercer · fly droneJ9
Mercer, staying at A4, drone stays over J9.
Mercer
Not covered yetAlpha
cover the pull-back from D3 once Charlie reports the post wrecked. Saved so it can be built.
Not covered yetBravo
cover the pull-back from D3 once Charlie reports the post wrecked. Saved so it can be built.
Not covered yetBoone
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 8 of 40 (Turn 8 of 40 · 7:00)
RADIO NET SINCE YOUR LAST LOOK:
  [6:00] Lead: Mercer, this is Lead. Correction, stay at A4 and keep flying the drone. Do not move. Over.
Sato, Banks, this is Lead. Move with Boone to the post at F5 first, then set security facing south and east. Over.
Boone, this is Lead. At the post, wreck the radio and scopes. Over.
  [6:00] Boone: moving to the post, searching it.
  [6:00] Sato: moving to the post, facing south and east.
  [6:00] Banks: moving to the post, facing south and east.
  [6:00] Ruiz: Moving to E4.
  [6:00] Mercer: staying at A4, drone stays over J9.
  [6:00] Alpha: Contact, Kansian sniper at E2.
  [6:00] Novak spots Sniper 1 at E2.
  [6:05] Sniper 1 is down, hit by M27 rifle, hit in the chest.
  [6:44] Marsh: Set at E5.
  [6:44] Keller: Set at E5.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 102 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 126 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 294 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 101 rds
  Ruiz (Squad leader, HQ) at B4: ok; doing: move E4; 210 rds
  Marsh (Corpsman, HQ) at E5: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at E5: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: nothing; 201 rds
  Tran (Automatic rifleman, Bravo) at D3: ok; doing: move D3; 462 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: nothing; 411 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at D3: ok; doing: move D3; 168 rds
  Boone (Team leader, Charlie) at E4: ok; doing: search the post; 204 rds
  Sato (Automatic rifleman, Charlie) at E5: ok; doing: move the post facing south and east; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E4: ok; doing: move the post facing south and east; 414 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: fly drone J9; 210 rds
  drone: up over J9
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, last seen 1 min ago
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
  Sniper 1 (op) at E2, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-009.png
```

### Player traffic (turn 8)

> Boone, this is Lead. Continue to the post, search it, wreck the radio and scopes. Over.
> Doc, this is Lead. Treat the downed Kansian rifleman at E5. Over.
> Keller, this is Lead. Detain him, search him, he is our prisoner. Over.
> Alpha, this is Lead. Good kill on the sniper at E2. Novak, keep an eye north, there may be more up there. Out.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Boone", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "detain", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Novak", "verb": "observe", "where": null, "target": null, "facing": "north", "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Boone", "text": "Boone, searching the post."}, {"who": "Marsh", "text": "Marsh, treating Rifleman 2."}, {"who": "Keller", "text": "Keller, detaining Rifleman 2."}, {"who": "Novak", "text": "Novak, observing north."}], "sayAgain": [], "notCovered": [{"who": "Boone", "text": "wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Boone · searchthe post
Boone, searching the post.
Boone, 115 m
Marsh · treat casualtyRifleman 2
Marsh, treating Rifleman 2.
Marsh, 35 m
Keller · detainRifleman 2
Keller, detaining Rifleman 2.
Keller, 84 m
Novak · observefacing north
Novak, observing north.
Novak
Not covered yetBoone
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 9 of 40 (Turn 9 of 40 · 8:00)
RADIO NET SINCE YOUR LAST LOOK:
  [7:00] Lead: Boone, this is Lead. Continue to the post, search it, wreck the radio and scopes. Over.
Doc, this is Lead. Treat the downed Kansian rifleman at E5. Over.
Keller, this is Lead. Detain him, search him, he is our prisoner. Over.
Alpha, this is Lead. Good kill on the sniper at E2. Novak, keep an eye north, there may be more up there. Out.
  [7:00] Boone: searching the post.
  [7:00] Marsh: treating Rifleman 2.
  [7:00] Keller: detaining Rifleman 2.
  [7:00] Novak: observing north.
  [7:25] Bravo: Set at D3.
  [7:27] Bravo: Set at D3.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: observe facing north; 102 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 126 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 294 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: overwatch D3 the post facing south; 101 rds
  Ruiz (Squad leader, HQ) at C4: ok; doing: move E4; 210 rds
  Marsh (Corpsman, HQ) at E5: ok; doing: treat casualty Rifleman 2; 150 rds
  Keller (Interpreter, HQ) at E5: ok; doing: detain Rifleman 2; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: nothing; 201 rds
  Tran (Automatic rifleman, Bravo) at D3: ok; doing: nothing; 462 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: nothing; 411 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at D3: ok; doing: nothing; 168 rds
  Boone (Team leader, Charlie) at E4: ok; doing: search the post; 204 rds
  Sato (Automatic rifleman, Charlie) at E5: ok; doing: move the post facing south and east; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E4: ok; doing: move the post facing south and east; 414 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: fly drone J9; 210 rds
  drone: up over J9
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, last seen 2 min ago
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
  Sniper 1 (op) at E2, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-1b/map-010.png
```

