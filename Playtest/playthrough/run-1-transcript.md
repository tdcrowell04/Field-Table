## Start

```
MISSION: Operation Jasper Blind (Raid the observation post), playing as Marines
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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-001.png
```

### Player traffic (turn 1)

> All stations, this is Lead. Frag order follows, break.
> Enemy: a few pax at the OP on Hill 17A, grid F5. Shepherd says they've been working the west slope facing us, so expect wire, mines or fighting holes in E4 through E6. Their QRF comes once they know, figure about 8 minutes after that. Break.
> Mission: raid the OP, wreck the radio and the optics, grab a prisoner if we can, then get back across the start line. Break.
> Mercer, get your drone up now. Recon the post at F5 and the west slope E4 to E6, plus Route Jasper north and south of the post. Keep standoff and altitude, don't spook them. Send me a SALUTE on anything you find, over. Break.
> Alpha, you're support by fire. Move along the north side of Hill 77, stay off the crest, and set in the tree line at D3 facing the post. Weapons tight, no firing until I call it or you're engaged. Duarte, glass the post and report what you see. Break.
> Bravo and Charlie, you're the assault, moving with Lead, Doc and Keller. Charlie leads, Bravo trails. Route is north of Hill 77 through C2 and D2, cross Route Jasper at E2, and hold in the brush at E2. We go in from the north along the ridge, not up the west slope. Break.
> ROE: positive ID before engaging. All elements report when set. Lead, over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "woods at D3", "target": null, "facing": "southeast", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "observe", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Mercer, drone up, flying to the post at F5. Standoff and altitude, SALUTE on anything I find."}, {"who": "Alpha", "text": "Alpha, moving to the woods at D3, facing southeast."}, {"who": "Duarte", "text": "Duarte, glassing the post, will report what I see."}, {"who": "Charlie", "text": "Charlie, moving to the scrub at E2."}, {"who": "Bravo", "text": "Bravo, following Charlie."}], "sayAgain": [], "notCovered": [{"who": "squad", "text": "raid the OP, wreck the radio and the optics, grab a prisoner if we can, then get back across the start line"}, {"who": "squad", "text": "positive ID before engaging"}, {"who": "squad", "text": "all elements report when set"}, {"who": "Mercer", "text": "send a SALUTE report on anything you find"}, {"who": "Alpha", "text": "move along the north side of Hill 77 and stay off the crest"}, {"who": "Alpha", "text": "weapons tight, no firing until called or engaged"}, {"who": "Charlie", "text": "route through C2 and D2, cross Route Jasper at E2, approach from the north along the ridge, not up the west slope"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post (F5)
Mercer, drone up, flying to the post at F5. Standoff and altitude, SALUTE on anything I find.
Mercer: Say again, where is the post (F5)?
Alpha · movewoods at D3 · facing southeast
Alpha, moving to the woods at D3, facing southeast.
Novak, 161 m
Ortiz, 165 m
Doyle, 171 m
Duarte, 170 m
Duarte · observethe post (F5)
Duarte, glassing the post, will report what I see.
Duarte: Say again, where is the post (F5)?
Charlie · movescrub at E2
Charlie, moving to the scrub at E2.
Boone, 457 m
Sato, 452 m
Banks, 454 m
Mercer, 467 m
Bravo · followCharlie
Bravo, following Charlie.
Pike: Say again, follow who?
Tran: Say again, follow who?
Vega: Say again, follow who?
Okafor: Say again, follow who?
Not covered yetsquad
raid the OP, wreck the radio and the optics, grab a prisoner if we can, then get back across the start line. Saved so it can be built.
Not covered yetsquad
positive ID before engaging. Saved so it can be built.
Not covered yetsquad
all elements report when set. Saved so it can be built.
Not covered yetMercer
send a SALUTE report on anything you find. Saved so it can be built.
Not covered yetAlpha
move along the north side of Hill 77 and stay off the crest. Saved so it can be built.
Not covered yetAlpha
weapons tight, no firing until called or engaged. Saved so it can be built.
Not covered yetCharlie
route through C2 and D2, cross Route Jasper at E2, approach from the north along the ridge, not up the west slope. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 1)

> Mercer, Duarte, this is Lead. The post is the enemy OP on top of Hill 17A, grid F5, on the ridgeline just east of Route Jasper, about 250 m east-southeast of the D3 tree line. Mercer, fly to grid F5. Duarte, observe grid F5. Over.
> Bravo, this is Lead. Follow Charlie team, Boone's team, stay about 30 m behind them. Over.
> Alpha, route is via grid C2, then to the woods at D3. Over.
> Charlie, route is via grid C2, then D2, then the scrub at E2. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Duarte", "verb": "observe", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "move", "where": "woods at D3", "target": null, "facing": "east and southeast", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Mercer", "text": "Mercer, drone flying to F5."}, {"who": "Duarte", "text": "Duarte, observing F5."}, {"who": "Alpha", "text": "Alpha, moving to the woods at D3, facing east and southeast."}, {"who": "Bravo", "text": "Bravo, following Charlie."}, {"who": "Charlie", "text": "Charlie, moving to the scrub at E2."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "route via grid C2 and D2"}, {"who": "Alpha", "text": "stay off the crest of Hill 77"}, {"who": "Alpha", "text": "weapons tight, no firing until called or engaged"}, {"who": "Mercer", "text": "keep standoff and altitude, send SALUTE reports"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly droneF5
Mercer, drone flying to F5.
Mercer
Duarte · observeF5
Duarte, observing F5.
Duarte
Alpha · movewoods at D3 · facing east and southeast
Alpha, moving to the woods at D3, facing east and southeast.
Novak, 161 m
Ortiz, 165 m
Doyle, 171 m
Duarte, 170 m
Bravo · followCharlie
Bravo, following Charlie.
Pike: Say again, follow who?
Tran: Say again, follow who?
Vega: Say again, follow who?
Okafor: Say again, follow who?
Charlie · movescrub at E2
Charlie, moving to the scrub at E2.
Boone, 457 m
Sato, 452 m
Banks, 454 m
Mercer, 467 m
Not covered yetCharlie
route via grid C2 and D2. Saved so it can be built.
Not covered yetAlpha
stay off the crest of Hill 77. Saved so it can be built.
Not covered yetAlpha
weapons tight, no firing until called or engaged. Saved so it can be built.
Not covered yetMercer
keep standoff and altitude, send SALUTE reports. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 2 of 40 (Turn 2 of 40 · 1:00)
RADIO NET SINCE YOUR LAST LOOK:
  [0:00] Lead: Mercer, Duarte, this is Lead. The post is the enemy OP on top of Hill 17A, grid F5, on the ridgeline just east of Route Jasper, about 250 m east-southeast of the D3 tree line. Mercer, fly to grid F5. Duarte, observe grid F5. Over.
Bravo, this is Lead. Follow Charlie team, Boone's team, stay about 30 m behind them. Over.
Alpha, route is via grid C2, then to the woods at D3. Over.
Charlie, route is via grid C2, then D2, then the scrub at E2. Over.
  [0:00] Mercer: drone flying to F5.
  [0:00] Duarte: observing F5.
  [0:00] Alpha: moving to the woods at D3, facing east and southeast.
  [0:00] Bravo: Say again, follow who?
  [0:00] Charlie: moving to the scrub at E2.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 210 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 420 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at A3: ok; doing: nothing; 210 rds
  Tran (Automatic rifleman, Bravo) at A3: ok; doing: nothing; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at A3: ok; doing: nothing; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at A3: ok; doing: nothing; 210 rds
  Boone (Team leader, Charlie) at B2: ok; doing: move scrub at E2; 210 rds
  Sato (Automatic rifleman, Charlie) at B2: ok; doing: move scrub at E2; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B2: ok; doing: move scrub at E2; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: move scrub at E2; 210 rds
KNOWN CONTACTS:
  none
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-002.png
```

### Player traffic (turn 2)

> Bravo, this is Lead. Disregard follow. Move to the scrub at E2, route via grid C2 then D2, trail Charlie by about 50 m. Over.
> Doc, Keller, this is Lead. On me. We move with Bravo to the scrub at E2. Over.
> Lead moving to the scrub at E2 with Bravo. Break.
> Mercer, this is Lead. I don't see your bird up. Say status of the drone. If you need to halt to launch, halt and launch now, then catch up with Charlie. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the scrub at E2."}, {"who": "Marsh", "text": "Doc, moving to the scrub at E2 with Bravo."}, {"who": "Keller", "text": "Interpreter, moving to the scrub at E2 with Bravo."}, {"who": "Ruiz", "text": "Lead, moving to the scrub at E2 with Bravo."}, {"who": "Mercer", "text": "Mercer, following Charlie."}], "sayAgain": [], "notCovered": [{"who": "Bravo", "text": "disregard the earlier follow order"}, {"who": "Bravo", "text": "trail Charlie about 50 m back, and route via C2 then D2"}, {"who": "Mercer", "text": "say status of the drone"}, {"who": "Mercer", "text": "halt and launch the drone"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · movescrub at E2
Bravo, moving to the scrub at E2.
Pike, 458 m
Tran, 449 m
Vega, 446 m
Okafor, 463 m
Marsh · movescrub at E2
Doc, moving to the scrub at E2 with Bravo.
Marsh, 345 m
Keller · movescrub at E2
Interpreter, moving to the scrub at E2 with Bravo.
Keller, 340 m
Ruiz · movescrub at E2
Lead, moving to the scrub at E2 with Bravo.
Ruiz, 338 m
Mercer · followCharlie
Mercer, following Charlie.
Mercer: Say again, follow who?
Not covered yetBravo
disregard the earlier follow order. Saved so it can be built.
Not covered yetBravo
trail Charlie about 50 m back, and route via C2 then D2. Saved so it can be built.
Not covered yetMercer
say status of the drone. Saved so it can be built.
Not covered yetMercer
halt and launch the drone. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 2)

> Mercer, this is Lead. Disregard follow. You stay with Charlie, moving to the scrub at E2. Separate task: launch your drone and fly it to grid F5. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "scrub at E2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the scrub at E2."}, {"who": "Marsh", "text": "Doc, moving to the scrub at E2 with Bravo."}, {"who": "Keller", "text": "Interpreter, moving to the scrub at E2 with Bravo."}, {"who": "Mercer", "text": "Mercer, flying the drone to F5."}], "sayAgain": [], "notCovered": [{"who": "Bravo", "text": "route via grid C2 then D2, trail Charlie by about 50 m"}, {"who": "Mercer", "text": "say status of the drone"}, {"who": "Mercer", "text": "stay with Charlie, moving to the scrub at E2"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · movescrub at E2
Bravo, moving to the scrub at E2.
Pike, 458 m
Tran, 449 m
Vega, 446 m
Okafor, 463 m
Marsh · movescrub at E2
Doc, moving to the scrub at E2 with Bravo.
Marsh, 345 m
Keller · movescrub at E2
Interpreter, moving to the scrub at E2 with Bravo.
Keller, 340 m
Mercer · fly droneF5
Mercer, flying the drone to F5.
Mercer
Not covered yetBravo
route via grid C2 then D2, trail Charlie by about 50 m. Saved so it can be built.
Not covered yetMercer
say status of the drone. Saved so it can be built.
Not covered yetMercer
stay with Charlie, moving to the scrub at E2. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 3 of 40 (Turn 3 of 40 · 2:00)
RADIO NET SINCE YOUR LAST LOOK:
  [1:00] Lead: Mercer, this is Lead. Disregard follow. You stay with Charlie, moving to the scrub at E2. Separate task: launch your drone and fly it to grid F5. Over.
  [1:00] Bravo: moving to the scrub at E2.
  [1:00] Marsh: Doc, moving to the scrub at E2 with Bravo.
  [1:00] Keller: Interpreter, moving to the scrub at E2 with Bravo.
  [1:00] Mercer: flying the drone to F5.
  [1:14] Mercer: Drone's up.
  [1:17] Alpha: Taking fire! Kansian team leader at F5.
  [1:17] Novak spots Team leader 1 at F5.
  [1:29] Charlie: Contact on the drone, Kansian rifleman at E6.
  [1:29] Charlie: Contact on the drone, Kansian rifleman at E5.
  [1:29] Drone spots Rifleman 1 at E6.
  [1:29] Drone spots Rifleman 2 at E5.
  [1:30] Charlie: Contact on the drone, Kansian rifleman at E5.
  [1:30] Drone spots Rifleman 3 at E5.
  [1:31] Charlie: Contact on the drone, Kansian machine gunner at E5.
  [1:31] Drone spots PKM gunner 2 at E5.
  [1:36] Alpha: Taking fire! Kansian RPG gunner at E5.
  [1:36] Novak spots RPG gunner 1 at E5.
  [1:44] Duarte killed by PKM machine gun, hit in the head.
  [1:49] Charlie: Contact on the drone, Kansian squad leader at F5.
  [1:49] Drone spots Squad leader 1 at F5.
  [1:50] Charlie: Contact on the drone, Kansian rifleman at F5.
  [1:50] Charlie: Contact on the drone, Kansian team leader at F5.
  [1:50] Charlie: Contact on the drone, Kansian machine gunner at F4.
  [1:50] Drone spots Observer 1 at F5.
  [1:50] Drone spots PKM gunner 1 at F4.
  [1:54] Novak: I'm hit! My leg, I can't walk.
  [1:54] Alpha: Man down! Novak is down.
  [1:54] Novak is wounded by AK rifle, hit in the leg.
  [1:54] Novak is down, hit by AK rifle, hit again in the leg.
  [1:55] Rifleman 2 is down, hit by M27 rifle, hit in the chest.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 558 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: move woods at D3 facing east and southeast; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: move scrub at E2; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: move scrub at E2; 0 rds
  Pike (Team leader, Bravo) at B2: ok; doing: move scrub at E2; 210 rds
  Tran (Automatic rifleman, Bravo) at B2: ok; doing: move scrub at E2; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B2: ok; doing: move scrub at E2; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B2: ok; doing: move scrub at E2; 210 rds
  Boone (Team leader, Charlie) at B2: ok; doing: move scrub at E2; 210 rds
  Sato (Automatic rifleman, Charlie) at B2: ok; doing: move scrub at E2; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B2: ok; doing: move scrub at E2; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone F5; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at F5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now
  PKM gunner 1 (op) at F5, seen now
  PKM gunner 2 (op) at E5, seen now
  RPG gunner 1 (op) at F5, seen now
  Rifleman 1 (op) at E6, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-003.png
```

### Player traffic (turn 3)

> Alpha, this is Lead. Halt your move. Ortiz, suppress PKM gunner 2 at E5, rapid fire. Doyle, drag Novak back west over the crest to Lead's position at B3. Over.
> Doc, this is Lead. Hold at B3, this is the casualty collection point. Novak is coming to you, leg wound. Over.
> Bravo, this is Lead. Change of mission. Move to the woods at D3 and set a base of fire on the post. Okafor, Gustaf on PKM gunner 1 at F5. Tran, suppress the trench at E5. Over.
> Charlie, this is Lead. Halt at C2 behind the north shoulder of the hill and hold there. Mercer, keep the bird over F5 and call targets. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "suppress", "where": null, "target": "PKM gunner 2 (enemy)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doyle", "verb": "carry", "where": "B3", "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "hold", "where": "B3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "overwatch", "where": "woods at D3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "fire", "where": null, "target": "PKM gunner 1 (enemy)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "C2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, halting, holding position."}, {"who": "Ortiz", "text": "Ortiz, suppressing PKM gunner 2 at E5, rapid fire."}, {"who": "Doyle", "text": "Doyle, dragging Novak back west to B3."}, {"who": "Marsh", "text": "Doc, holding at B3, casualty collection point."}, {"who": "Bravo", "text": "Bravo, moving to the woods at D3, setting a base of fire on the post."}, {"who": "Okafor", "text": "Okafor, Gustaf on PKM gunner 1 at F5."}, {"who": "Charlie", "text": "Charlie, halting at C2, holding there."}, {"who": "Mercer", "text": "Mercer, keeping the bird over F5."}], "sayAgain": ["Tran: Say again, the trench at E5. I don't have it marked."], "notCovered": [{"who": "Mercer", "text": "call targets"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · hold
Alpha, halting, holding position.
Novak: Novak is down and can't act.
Ortiz
Doyle
Duarte: Duarte is dead and can't act.
Ortiz · suppressPKM gunner 2 (enemy)
Ortiz, suppressing PKM gunner 2 at E5, rapid fire.
Ortiz: Say again, which PKM gunner 2 (enemy)? We don't have eyes on that.
Doyle · carryB3 · Novak
Doyle, dragging Novak back west to B3.
Doyle, 23 m
Marsh · holdB3
Doc, holding at B3, casualty collection point.
Marsh
Bravo · overwatchwoods at D3 · the post (F5)
Bravo, moving to the woods at D3, setting a base of fire on the post.
Pike, 177 m
Tran, 172 m
Vega, 171 m
Okafor, 180 m
Okafor · firePKM gunner 1 (enemy)
Okafor, Gustaf on PKM gunner 1 at F5.
Okafor: Say again, which PKM gunner 1 (enemy)? We don't have eyes on that.
Charlie · moveC2
Charlie, halting at C2, holding there.
Boone, 73 m
Sato, 74 m
Banks, 76 m
Mercer, 132 m
Mercer · fly droneF5
Mercer, keeping the bird over F5.
Mercer
Tran: Say again, the trench at E5. I don't have it marked.
Not covered yetMercer
call targets. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 3)

> Ortiz, this is Lead. Area fire. Suppress grid E5, the slope below the post where the gun is shooting from. Over.
> Okafor, this is Lead. When you're set in the D3 woods, Gustaf HE at grid F5, the post itself. Over.
> Tran, this is Lead. When set, suppress grid E5, the west slope below the post. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "suppress", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doyle", "verb": "carry", "where": "B3", "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doc", "verb": "hold", "where": "B3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "woods at D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "fire", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": "HE"}, {"who": "Tran", "verb": "suppress", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "C2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, halting, holding position."}, {"who": "Ortiz", "text": "Ortiz, area fire on grid E5."}, {"who": "Doyle", "text": "Doyle, dragging Novak back west to B3."}, {"who": "Doc", "text": "Doc, holding at B3, casualty collection point."}, {"who": "Bravo", "text": "Bravo, moving to the woods at D3, setting a base of fire on the post."}, {"who": "Okafor", "text": "Okafor, Gustaf HE on the post at F5, once set in the D3 woods."}, {"who": "Tran", "text": "Tran, suppressing grid E5 once set."}, {"who": "Charlie", "text": "Charlie, moving to C2, holding there."}, {"who": "Mercer", "text": "Mercer, bird over F5, calling targets."}], "sayAgain": [], "notCovered": []}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · hold
Alpha, halting, holding position.
Novak: Novak is down and can't act.
Ortiz
Doyle
Duarte: Duarte is dead and can't act.
Ortiz · suppressE5
Ortiz, area fire on grid E5.
Ortiz
Doyle · carryB3 · Novak
Doyle, dragging Novak back west to B3.
Doyle, 23 m
Doc · holdB3
Doc, holding at B3, casualty collection point.
Marsh
Bravo · movewoods at D3
Bravo, moving to the woods at D3, setting a base of fire on the post.
Pike, 177 m
Tran, 172 m
Vega, 171 m
Okafor, 180 m
Okafor · fireF5 · HE
Okafor, Gustaf HE on the post at F5, once set in the D3 woods.
Okafor
Tran · suppressE5
Tran, suppressing grid E5 once set.
Tran
Charlie · moveC2
Charlie, moving to C2, holding there.
Boone, 73 m
Sato, 74 m
Banks, 76 m
Mercer, 132 m
Mercer · fly droneF5
Mercer, bird over F5, calling targets.
Mercer
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 4 of 40 (Turn 4 of 40 · 3:00)
RADIO NET SINCE YOUR LAST LOOK:
  [2:00] Lead: Ortiz, this is Lead. Area fire. Suppress grid E5, the slope below the post where the gun is shooting from. Over.
Okafor, this is Lead. When you're set in the D3 woods, Gustaf HE at grid F5, the post itself. Over.
Tran, this is Lead. When set, suppress grid E5, the west slope below the post. Over.
  [2:00] Alpha: halting, holding position.
  [2:00] Ortiz: area fire on grid E5.
  [2:00] Doyle: dragging Novak back west to B3.
  [2:00] Doc: holding at B3, casualty collection point.
  [2:00] Bravo: moving to the woods at D3, setting a base of fire on the post.
  [2:00] Okafor: Gustaf HE on the post at F5, once set in the D3 woods.
  [2:00] Tran: suppressing grid E5 once set.
  [2:00] Charlie: moving to C2, holding there.
  [2:00] Mercer: bird over F5, calling targets.
  [2:00] Bravo: Suppressing E5.
  [2:00] Bravo: Engaging F5.
  [2:01] Alpha: Suppressing E5.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: suppress E5; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: carry B3 Novak; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at C3: ok; doing: move scrub at E2; 0 rds
  Pike (Team leader, Bravo) at B2: ok; doing: move woods at D3; 210 rds
  Tran (Automatic rifleman, Bravo) at B2: ok; doing: suppress E5; 480 rds
  Vega (Assistant automatic rifleman, Bravo) at B2: ok; doing: move woods at D3; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B2: ok; doing: fire F5; 138 rds
  Boone (Team leader, Charlie) at C2: ok; doing: move C2; 210 rds
  Sato (Automatic rifleman, Charlie) at C2: ok; doing: move C2; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C2: ok; doing: move C2; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone F5; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at G6, seen now
  Observer 1 (op) at G6, seen now
  Team leader 1 (op) at G6, seen now
  PKM gunner 1 (op) at G5, seen now
  PKM gunner 2 (op) at F7, seen now
  RPG gunner 1 (op) at G7, seen now
  Rifleman 1 (op) at G7, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at G7, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-004.png
```

### Player traffic (turn 4)

> All stations, this is Lead. Drone shows the enemy has pulled off the post to the reverse slope, G5 through G7, and down by the creek at F7. The post is empty. We go now, before their QRF shows. Break.
> Alpha, Bravo, cease fire on E5, nothing there but the one they left. Break.
> Bravo, continue to the woods at D3 and set a base of fire covering the crest of the ridge from F4 to F6. Engage anything that shows on the crest. Okafor, Gustaf ready, same sector. Over.
> Charlie, this is Lead. You are the assault. Move to grid E4, the base of the west slope below the post, and hold there for my call. Over.
> Ruiz moving with Charlie to E4. Keller, disregard E2, move with Charlie to E4. Over.
> Mercer, hold at C2 and keep the bird on the enemy at G5 through G7. Report any movement toward the post. Over.
> Ortiz, hold at C3 and cover the post. Doyle, get Novak to Doc at B3. Doc, treat Novak on arrival. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "overwatch", "where": "woods at D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "overwatch", "where": "woods at D3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "move", "where": "C2", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "report", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "overwatch", "where": "C3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doyle", "verb": "carry", "where": "B3", "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": null, "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the woods at D3, overwatch the crest."}, {"who": "Okafor", "text": "Okafor, Gustaf ready, overwatch same sector from the woods at D3."}, {"who": "Charlie", "text": "Charlie, moving to E4, holding for Lead's call."}, {"who": "Ruiz", "text": "Ruiz, moving with Charlie to E4."}, {"who": "Keller", "text": "Keller, disregard E2, moving with Charlie to E4."}, {"who": "Mercer", "text": "Mercer, moving to C2."}, {"who": "Mercer", "text": "Mercer, reporting any movement toward the post."}, {"who": "Ortiz", "text": "Ortiz, holding at C3, covering the post."}, {"who": "Doyle", "text": "Doyle, carrying Novak to B3."}, {"who": "Marsh", "text": "Marsh, treating Novak on arrival."}], "sayAgain": ["Bravo: Say again, which crest? F4 to F6 is not a place I have.", "Mercer: Say again, which square is the bird on? G5 through G7 is three squares."], "notCovered": [{"who": "Alpha", "text": "cease fire on E5"}, {"who": "Bravo", "text": "cease fire on E5"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · overwatchwoods at D3
Bravo, moving to the woods at D3, overwatch the crest.
Pike, 120 m
Tran, 172 m
Vega, 112 m
Okafor, 180 m
Okafor · overwatchwoods at D3
Okafor, Gustaf ready, overwatch same sector from the woods at D3.
Okafor, 180 m
Charlie · moveE4
Charlie, moving to E4, holding for Lead's call.
Boone, 344 m
Sato, 339 m
Banks, 341 m
Mercer, 373 m
Ruiz · moveE4
Ruiz, moving with Charlie to E4.
Ruiz, 428 m
Keller · moveE4
Keller, disregard E2, moving with Charlie to E4.
Keller, 222 m
Mercer · moveC2
Mercer, moving to C2.
Mercer, 132 m
Mercer · reportthe post (F5)
Mercer, moving to C2.
Mercer: Say again, where is the post (F5)?
Ortiz · overwatchC3 · the post (F5)
Ortiz, holding at C3, covering the post.
Ortiz, 26 m
Doyle · carryB3 · Novak
Doyle, carrying Novak to B3.
Doyle, 1 m
Marsh · treat casualtyNovak
Marsh, treating Novak on arrival.
Marsh, 70 m
Bravo: Say again, which crest? F4 to F6 is not a place I have.
Mercer: Say again, which square is the bird on? G5 through G7 is three squares.
Not covered yetAlpha
cease fire on E5. Saved so it can be built.
Not covered yetBravo
cease fire on E5. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 4)

> Bravo, this is Lead. Your sector is grid F5, the post. Engage any enemy that appears there. Over.
> Mercer, this is Lead. Fly the drone to grid G6. Over.
> Tran, Okafor, Ortiz, this is Lead. Cease fire, cease fire. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "overwatch", "where": "woods at D3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Okafor", "verb": "overwatch", "where": null, "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "overwatch", "where": "C3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doyle", "verb": "carry", "where": "B3", "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": null, "target": "Novak", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "G6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to woods at D3, overwatch the post."}, {"who": "Okafor", "text": "Okafor, Gustaf ready, overwatch the post."}, {"who": "Charlie", "text": "Charlie, moving to E4."}, {"who": "Ruiz", "text": "Ruiz, moving to E4 with Charlie."}, {"who": "Keller", "text": "Keller, moving to E4 with Charlie."}, {"who": "Ortiz", "text": "Ortiz, holding at C3, overwatch the post."}, {"who": "Doyle", "text": "Doyle, carrying Novak to B3."}, {"who": "Marsh", "text": "Marsh, treating Novak on arrival."}, {"who": "Mercer", "text": "Mercer, flying drone to G6."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "cease fire on E5"}, {"who": "Bravo", "text": "cease fire on E5"}, {"who": "Tran", "text": "cease fire"}, {"who": "Okafor", "text": "cease fire"}, {"who": "Ortiz", "text": "cease fire"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · overwatchwoods at D3 · the post (F5)
Bravo, moving to woods at D3, overwatch the post.
Pike, 120 m
Tran, 172 m
Vega, 112 m
Okafor, 180 m
Okafor · overwatchthe post (F5)
Okafor, Gustaf ready, overwatch the post.
Okafor: Say again, which the post (F5)? We don't have eyes on that.
Charlie · moveE4
Charlie, moving to E4.
Boone, 344 m
Sato, 339 m
Banks, 341 m
Mercer, 373 m
Ruiz · moveE4
Ruiz, moving to E4 with Charlie.
Ruiz, 428 m
Keller · moveE4
Keller, moving to E4 with Charlie.
Keller, 222 m
Ortiz · overwatchC3 · the post (F5)
Ortiz, holding at C3, overwatch the post.
Ortiz, 26 m
Doyle · carryB3 · Novak
Doyle, carrying Novak to B3.
Doyle, 1 m
Marsh · treat casualtyNovak
Marsh, treating Novak on arrival.
Marsh, 70 m
Mercer · fly droneG6
Mercer, flying drone to G6.
Mercer
Not covered yetAlpha
cease fire on E5. Saved so it can be built.
Not covered yetBravo
cease fire on E5. Saved so it can be built.
Not covered yetTran
cease fire. Saved so it can be built.
Not covered yetOkafor
cease fire. Saved so it can be built.
Not covered yetOrtiz
cease fire. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 5 of 40 (Turn 5 of 40 · 4:00)
RADIO NET SINCE YOUR LAST LOOK:
  [3:00] Lead: Bravo, this is Lead. Your sector is grid F5, the post. Engage any enemy that appears there. Over.
Mercer, this is Lead. Fly the drone to grid G6. Over.
Tran, Okafor, Ortiz, this is Lead. Cease fire, cease fire. Over.
  [3:00] Bravo: moving to woods at D3, overwatch the post.
  [3:00] Okafor: Say again, which the post (F5)? We don't have eyes on that.
  [3:00] Charlie: moving to E4.
  [3:00] Ruiz: moving to E4 with Charlie.
  [3:00] Keller: moving to E4 with Charlie.
  [3:00] Ortiz: holding at C3, overwatch the post.
  [3:00] Doyle: carrying Novak to B3.
  [3:00] Marsh: treating Novak on arrival.
  [3:00] Mercer: flying drone to G6.
  [3:02] Alpha: Novak is at B3.
  [3:24] Alpha: Set. Overwatch on the post (F5).
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: overwatch C3 the post (F5); 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at B4: ok; doing: move E4; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: treat casualty Novak; 150 rds
  Keller (Interpreter, HQ) at D3: ok; doing: move E4; 0 rds
  Pike (Team leader, Bravo) at C2: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at B2: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at C2: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B2: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at C2: ok; doing: move E4; 210 rds
  Sato (Automatic rifleman, Charlie) at C2: ok; doing: move E4; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C2: ok; doing: move E4; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone G6; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at I7, seen now
  Observer 1 (op) at I7, seen now
  Team leader 1 (op) at I7, seen now
  PKM gunner 1 (op) at I6, seen now
  PKM gunner 2 (op) at H8, seen now
  RPG gunner 1 (op) at H8, seen now
  Rifleman 1 (op) at H9, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at H8, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-005.png
```

### Player traffic (turn 5)

> All stations, this is Lead. Drone has the enemy pulling out east, now at I6, I7 and H8, 400 m past the post. Speed is security, we take the post now. Break.
> Charlie, this is Lead. Change of mission. Move directly to the post at F5 and search it. Wreck the radio and the scopes. Lead is with you. Over.
> Ruiz moving with Charlie to the post at F5. Break.
> Keller, move to the downed enemy at E5 with Charlie. Boone, check him. If he's alive, he's our prisoner, zip-tie and search him, Keller talks to him. Over.
> Mercer, fly the drone to grid H7 and keep eyes on the enemy. Report anything coming back toward the post or anything new from the east, that'll be their QRF. Over.
> Bravo, continue to the woods at D3 and overwatch the post. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "assault", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "the post (F5)", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "talk", "where": "E5", "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Boone", "verb": "detain", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "H7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "overwatch", "where": "woods at D3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, assaulting the post at F5."}, {"who": "Charlie", "text": "Charlie, moving to the post at F5 to search it."}, {"who": "Ruiz", "text": "Ruiz, moving with Charlie to the post at F5."}, {"who": "Keller", "text": "Keller, moving to E5 to talk to Rifleman 2."}, {"who": "Boone", "text": "Boone, detaining Rifleman 2."}, {"who": "Mercer", "text": "Mercer, flying the drone to H7."}, {"who": "Bravo", "text": "Bravo, overwatching the post at F5 from the woods at D3."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "wreck the radio and the scopes"}, {"who": "Boone", "text": "check him; if he's alive, search him after the zip-tie"}, {"who": "Mercer", "text": "keep eyes on the enemy, and report anything coming back toward the post or new from the east"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · assaultthe post (F5)
Alpha, assaulting the post at F5.
Novak: Novak is down and can't act.
Ortiz: Say again, where is the post (F5)?
Doyle: Say again, where is the post (F5)?
Duarte: Duarte is dead and can't act.
Charlie · searchthe post (F5)
Charlie, moving to the post at F5 to search it.
Boone: Say again, where is the post (F5)?
Sato: Say again, where is the post (F5)?
Banks: Say again, where is the post (F5)?
Mercer: Say again, where is the post (F5)?
Ruiz · movethe post (F5)
Ruiz, moving with Charlie to the post at F5.
Ruiz: Say again, where is the post (F5)?
Keller · talkE5 · Rifleman 2
Keller, moving to E5 to talk to Rifleman 2.
Keller, 232 m
Boone · detainRifleman 2
Boone, detaining Rifleman 2.
Boone, 341 m
Mercer · fly droneH7
Mercer, flying the drone to H7.
Mercer
Bravo · overwatchwoods at D3 · the post (F5)
Bravo, overwatching the post at F5 from the woods at D3.
Pike, 54 m
Tran, 118 m
Vega, 44 m
Okafor, 127 m
Not covered yetCharlie
wreck the radio and the scopes. Saved so it can be built.
Not covered yetBoone
check him; if he's alive, search him after the zip-tie. Saved so it can be built.
Not covered yetMercer
keep eyes on the enemy, and report anything coming back toward the post or new from the east. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 5)

> Alpha, this is Lead. Disregard, that order was not for you. Ortiz and Doyle hold at C3 with Doc and Novak. Over.
> Charlie, this is Lead. Move to grid F5 and search grid F5. Over.
> Ruiz moving to grid F5 with Charlie. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Charlie", "verb": "search", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "talk", "where": "E5", "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Boone", "verb": "search", "where": null, "target": "Rifleman 2", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "H7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "overwatch", "where": "woods at D3", "target": "the post (F5)", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "hold", "where": "C3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Doyle", "verb": "hold", "where": "C3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Charlie", "text": "Charlie, moving to grid F5, search it."}, {"who": "Ruiz", "text": "Ruiz, moving to grid F5 with Charlie."}, {"who": "Keller", "text": "Keller, moving to E5 to talk to Rifleman 2."}, {"who": "Boone", "text": "Boone, checking Rifleman 2."}, {"who": "Mercer", "text": "Mercer, flying the drone to H7."}, {"who": "Bravo", "text": "Bravo, moving to the woods at D3, overwatch the post."}, {"who": "Ortiz", "text": "Ortiz, holding at C3."}, {"who": "Doyle", "text": "Doyle, holding at C3."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "wreck the radio and the scopes"}, {"who": "Mercer", "text": "report anything coming back toward the post or anything new from the east"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · searchF5
Charlie, moving to grid F5, search it.
Boone, 385 m
Sato, 379 m
Banks, 381 m
Mercer, 510 m
Ruiz · moveF5
Ruiz, moving to grid F5 with Charlie.
Ruiz, 412 m
Keller · talkE5 · Rifleman 2
Keller, moving to E5 to talk to Rifleman 2.
Keller, 232 m
Boone · searchRifleman 2
Boone, checking Rifleman 2.
Boone, 341 m
Mercer · fly droneH7
Mercer, flying the drone to H7.
Mercer
Bravo · overwatchwoods at D3 · the post (F5)
Bravo, moving to the woods at D3, overwatch the post.
Pike, 54 m
Tran, 118 m
Vega, 44 m
Okafor, 127 m
Ortiz · holdC3
Ortiz, holding at C3.
Ortiz
Doyle · holdC3
Doyle, holding at C3.
Doyle
Not covered yetCharlie
wreck the radio and the scopes. Saved so it can be built.
Not covered yetMercer
report anything coming back toward the post or anything new from the east. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 6 of 40 (Turn 6 of 40 · 5:00)
RADIO NET SINCE YOUR LAST LOOK:
  [4:00] Lead: Alpha, this is Lead. Disregard, that order was not for you. Ortiz and Doyle hold at C3 with Doc and Novak. Over.
Charlie, this is Lead. Move to grid F5 and search grid F5. Over.
Ruiz moving to grid F5 with Charlie. Over.
  [4:00] Charlie: moving to grid F5, search it.
  [4:00] Ruiz: moving to grid F5 with Charlie.
  [4:00] Keller: moving to E5 to talk to Rifleman 2.
  [4:00] Boone: checking Rifleman 2.
  [4:00] Mercer: flying the drone to H7.
  [4:00] Bravo: moving to the woods at D3, overwatch the post.
  [4:00] Ortiz: holding at C3.
  [4:00] Doyle: holding at C3.
  [4:52] Charlie: Contact on the drone, Kansian squad leader at J8.
  [4:52] Drone spots Squad leader 1 at J8.
  [4:53] Doc: Novak is stable.
  [4:53] Marsh treats Novak.
  [4:59] Charlie: Contact on the drone, Kansian team leader at J8.
  [4:59] Charlie: Contact on the drone, Kansian RPG gunner at J9.
  [4:59] Drone spots Team leader 1 at J8.
  [4:59] Drone spots RPG gunner 1 at J9.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit, stable; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at C4: ok; doing: move F5; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at D4: ok; doing: talk E5 Rifleman 2; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at C2: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at D3: ok; doing: search Rifleman 2; 210 rds
  Sato (Automatic rifleman, Charlie) at D3: ok; doing: search F5; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at D3: ok; doing: search F5; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone H7; 210 rds
  drone: up over H7
KNOWN CONTACTS:
  Squad leader 1 (op) at J8, seen now
  Observer 1 (op) at J8, seen now
  Team leader 1 (op) at J8, seen now
  PKM gunner 1 (op) at J7, seen now
  PKM gunner 2 (op) at I9, seen now
  RPG gunner 1 (op) at J9, seen now
  Rifleman 1 (op) at I10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at I9, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-006.png
```

### Player traffic (turn 6)

> Keller, this is Lead. Halt and hold at D4. You don't go up to that enemy before a rifleman does. I'll call you forward once Boone has him secured. Over.
> Mercer, this is Lead. Good work. Keep the bird on the enemy at J8 and watch the east and south edges for their QRF. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Keller", "verb": "hold", "where": "D4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "J8", "target": null, "facing": "east and south", "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Keller", "text": "Keller, holding at D4."}, {"who": "Mercer", "text": "Mercer, drone over J8, watching east and south."}], "sayAgain": [], "notCovered": [{"who": "Keller", "text": "don't go up to that enemy before a rifleman does; Lead calls him forward once Boone has him secured"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Keller · holdD4
Keller, holding at D4.
Keller
Mercer · fly droneJ8 · facing east and south
Mercer, drone over J8, watching east and south.
Mercer
Not covered yetKeller
don't go up to that enemy before a rifleman does; Lead calls him forward once Boone has him secured. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 7 of 40 (Turn 7 of 40 · 6:00)
RADIO NET SINCE YOUR LAST LOOK:
  [5:00] Lead: Keller, this is Lead. Halt and hold at D4. You don't go up to that enemy before a rifleman does. I'll call you forward once Boone has him secured. Over.
Mercer, this is Lead. Good work. Keep the bird on the enemy at J8 and watch the east and south edges for their QRF. Over.
  [5:00] Keller: holding at D4.
  [5:00] Mercer: drone over J8, watching east and south.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit, stable; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at D4: ok; doing: move F5; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at C4: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at D3: ok; doing: search Rifleman 2; 210 rds
  Sato (Automatic rifleman, Charlie) at E3: ok; doing: search F5; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E3: ok; doing: search F5; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone J8 facing east and south; 210 rds
  drone: up over J8
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at J10, seen now
  PKM gunner 1 (op) at J9, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-007.png
```

### Turn played

```
TIME: turn 8 of 40 (Turn 8 of 40 · 7:00)
RADIO NET SINCE YOUR LAST LOOK:
  [6:21] Bravo: Set. Overwatch on the post (F5).
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit, stable; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at D4: ok; doing: move F5; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at D4: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at E4: ok; doing: search Rifleman 2; 210 rds
  Sato (Automatic rifleman, Charlie) at E4: ok; doing: search F5; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E4: ok; doing: search F5; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone J8 facing east and south; 210 rds
  drone: up over J8
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at J10, seen now
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-008.png
```

### Turn played

```
TIME: turn 9 of 40 (Turn 9 of 40 · 8:00)
RADIO NET SINCE YOUR LAST LOOK:
  (quiet)
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit, stable; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at E5: ok; doing: move F5; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at E5: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at E4: ok; doing: search Rifleman 2; 210 rds
  Sato (Automatic rifleman, Charlie) at E4: ok; doing: search F5; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at F4: ok; doing: search F5; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone J8 facing east and south; 210 rds
  drone: up over J8
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at J10, seen now
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-009.png
```

### Turn played

```
TIME: turn 10 of 40 (Turn 10 of 40 · 9:00)
RADIO NET SINCE YOUR LAST LOOK:
  [8:36] Charlie: Man down! Sato is down.
  [8:36] Sato is down, hit by PKM machine gun, hit in the chest.
  [8:46] Banks: I'm hit! My arm.
  [8:46] Banks: Plate took it! I'm good.
  [8:46] Charlie: Man down! Banks is down.
  [8:46] Banks is wounded by PKM machine gun, hit in the arm.
  [8:46] Banks is saved by the plate.
  [8:46] Banks is down, hit by PKM machine gun, hit in the chest.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: failed
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: down, leg hit, stable; doing: nothing; 204 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 378 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 387 rds
  Duarte (Rifleman, marksman, Alpha) at C3: dead; doing: nothing; 113 rds
  Ruiz (Squad leader, HQ) at E5: ok; doing: move F5; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at E5: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 210 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 480 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: overwatch woods at D3 the post (F5); 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: overwatch woods at D3 the post (F5); 138 rds
  Boone (Team leader, Charlie) at E5: ok; doing: search Rifleman 2; 210 rds
  Sato (Automatic rifleman, Charlie) at F5: down; doing: nothing; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at F4: down, arm hit; doing: nothing; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B2: ok; doing: fly drone J8 facing east and south; 210 rds
  drone: up over J8
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at J10, seen now
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at J10, seen now
  RPG gunner 1 (op) at J10, seen now
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at J10, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/harness/shots-1/map-010.png
```

### Player traffic (turn 10)

> All stations, this is Lead. Two down on the crest, Sato at F5 and Banks at F4, PKM fire from the southeast, likely their element at J10. The crest is covered by fire. Casualties come off the crest first. Break.
> Boone, this is Lead. Pop smoke on F5 to screen the crest from the southeast, then drag Sato back west to me at E5. Over.
> Ruiz dragging Banks back west to E5. Break.
> Keller, hold at E5 with me. Over.
> Doc, this is Lead. Novak is stable, leave him with Doyle. Move to E5, that is the new casualty collection point. Two casualties, Sato chest wound, Banks chest and arm. Over.
> Bravo, this is Lead. Move to E4, the west slope below the crest, and stay off the skyline. Be ready to carry casualties and finish the search. Over.
> Mercer, this is Lead. Find me the gun that hit us. Confirm whether it fired from J10 or somewhere closer. Over.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Boone", "verb": "throw smoke", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "carry", "where": "E5", "target": "Banks", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "hold", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "observe", "where": "J10", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Boone", "text": "Boone, popping smoke on F5."}, {"who": "Ruiz", "text": "Ruiz, dragging Banks back to E5."}, {"who": "Keller", "text": "Keller, holding at E5 with Lead."}, {"who": "Marsh", "text": "Doc, moving to E5."}, {"who": "Bravo", "text": "Bravo, moving to E4."}, {"who": "Mercer", "text": "Mercer, observing J10."}], "sayAgain": ["Boone: Smoke on F5 now. Say again the Sato drag as its own order.", "Bravo: Say again, search what?"], "notCovered": []}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Boone · throw smokeF5
Boone, popping smoke on F5.
Boone: Too far to throw, that's past 35 meters.
Ruiz · carryE5 · Banks
Ruiz, dragging Banks back to E5.
Ruiz, 53 m
Keller · holdE5
Keller, holding at E5 with Lead.
Keller
Marsh · moveE5
Doc, moving to E5.
Marsh, 271 m
Bravo · moveE4
Bravo, moving to E4.
Pike, 207 m
Tran, 203 m
Vega, 225 m
Okafor, 200 m
Mercer · observeJ10
Mercer, observing J10.
Mercer
Boone: Smoke on F5 now. Say again the Sato drag as its own order.
Bravo: Say again, search what?
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 10)

> Boone, this is Lead. Drag Sato back west to E5. Over.
> Boone, separate order: when you reach Sato, throw smoke on the crest beside him to screen from the southeast. Over.
> Bravo, this is Lead. Disregard search for now, just move to E4. Over.

