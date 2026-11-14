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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-001.png
```

### Player traffic (turn 1)

> Alpha, move up onto that hill at C3 and set up overwatch on the post, Ortiz put the SAW where he can see the ridge. Bravo and Charlie, hold at A3 for now, stay low. Mercer, get the drone up and take a look at the post and the slope in front of it. Ruiz, Marsh and Keller stay with Alpha. Nobody shoots unless you've got a positive ID.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "overwatch", "where": "Hill 77", "target": "the post", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, moving to Hill 77, overwatch the post."}, {"who": "Bravo", "text": "Bravo, holding at A3."}, {"who": "Charlie", "text": "Charlie, holding at A3."}, {"who": "Mercer", "text": "Mercer, drone up, flying to the post."}, {"who": "Ruiz", "text": "Ruiz, following Alpha."}, {"who": "Marsh", "text": "Marsh, following Alpha."}, {"who": "Keller", "text": "Keller, following Alpha."}], "sayAgain": ["Ortiz: Say again, which ridge?", "Mercer: Say again, which slope?"], "notCovered": [{"who": "squad", "text": "hold fire unless they have a positive ID"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · overwatchHill 77 · the post
Alpha, moving to Hill 77, overwatch the post.
Novak, 101 m, about 2 min
Ortiz, 106 m, about 3 min
Doyle, 112 m, about 3 min
Duarte, 110 m, about 3 min
Bravo · holdA3
Bravo, holding at A3.
Pike
Tran
Vega
Okafor
Charlie · holdA3
Charlie, holding at A3.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, drone up, flying to the post.
Mercer
Ruiz · followAlpha
Ruiz, following Alpha.
Ruiz
Marsh · followAlpha
Marsh, following Alpha.
Marsh
Keller · followAlpha
Keller, following Alpha.
Keller
Ortiz: Say again, which ridge?
Mercer: Say again, which slope?
Not covered yetsquad
hold fire unless they have a positive ID. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 1)

> Ortiz, the ridge east of the road, the long dark one with the post on it, F5. Mercer, the west slope of that ridge, the side facing us. Look at the post and anything dug in on that side. And again: hold fire unless you've got a positive ID.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "overwatch", "where": "Hill 77", "target": "the post", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "observe", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "hold", "where": "A3", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, moving to Hill 77, overwatch on the post."}, {"who": "Ortiz", "text": "Ortiz, watching the ridge at F5."}, {"who": "Bravo", "text": "Bravo, holding at A3."}, {"who": "Charlie", "text": "Charlie, holding at A3."}, {"who": "Mercer", "text": "Mercer, drone flying to the post."}, {"who": "Ruiz", "text": "Lead, following Alpha."}, {"who": "Marsh", "text": "Marsh, following Alpha."}, {"who": "Keller", "text": "Keller, following Alpha."}], "sayAgain": [], "notCovered": [{"who": "squad", "text": "hold fire unless you've got a positive ID"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · overwatchHill 77 · the post
Alpha, moving to Hill 77, overwatch on the post.
Novak, 101 m, about 2 min
Doyle, 112 m, about 3 min
Duarte, 110 m, about 3 min
Ortiz · observeF5
Ortiz, watching the ridge at F5.
Ortiz
Bravo · holdA3
Bravo, holding at A3.
Pike
Tran
Vega
Okafor
Charlie · holdA3
Charlie, holding at A3.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, drone flying to the post.
Mercer
Ruiz · followAlpha
Lead, following Alpha.
Ruiz
Marsh · followAlpha
Marsh, following Alpha.
Marsh
Keller · followAlpha
Keller, following Alpha.
Keller
Not covered yetsquad
hold fire unless you've got a positive ID. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 2 of 40 (Turn 2 of 40 · 1:00)
RADIO NET SINCE YOUR LAST LOOK:
  [0:00] Lead: Ortiz, the ridge east of the road, the long dark one with the post on it, F5. Mercer, the west slope of that ridge, the side facing us. Look at the post and anything dug in on that side. And again: hold fire unless you've got a positive ID.
  [0:00] Alpha: moving to Hill 77, overwatch on the post.
  [0:00] Ortiz: watching the ridge at F5.
  [0:00] Bravo: holding at A3.
  [0:00] Charlie: holding at A3.
  [0:00] Mercer: drone flying to the post.
  [0:00] Ruiz: Lead, following Alpha.
  [0:00] Marsh: following Alpha.
  [0:00] Keller: following Alpha.
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
  Novak (Team leader, Alpha) at B3: ok; doing: overwatch Hill 77 the post; 210 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: observe F5; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at B3: ok; doing: overwatch Hill 77 the post; 420 rds
  Duarte (Rifleman, marksman, Alpha) at B3: ok; doing: overwatch Hill 77 the post; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: follow Alpha; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Alpha; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Alpha; 0 rds
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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-002.png
```

### Turn played

```
TIME: turn 3 of 40 (Turn 3 of 40 · 2:00)
RADIO NET SINCE YOUR LAST LOOK:
  (quiet)
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: overwatch Hill 77 the post; 210 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: observe F5; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: overwatch Hill 77 the post; 411 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: overwatch Hill 77 the post; 118 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: follow Alpha; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Alpha; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Alpha; 0 rds
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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-003.png
```

### Player traffic (turn 3)

> Alright, here's the plan. Alpha stays on the hill and is our base of fire, Ortiz get up there with the rest of Alpha. Ruiz, Marsh and Keller, go up with them and stay behind the crest. Bravo and Charlie, move together down to the creek at B7 and then east along it, keep in the creek bed and stay low so they can't see you from the ridge. Stop at D7 and wait for me, don't cross the road. Mercer, keep the drone over the post and tell me if anybody moves.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Ortiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "creek at B7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "creek at B7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Ortiz", "text": "Ortiz, moving to Hill 77."}, {"who": "Ruiz", "text": "Ruiz, moving to Hill 77."}, {"who": "Marsh", "text": "Marsh, moving to Hill 77."}, {"who": "Keller", "text": "Keller, moving to Hill 77."}, {"who": "Bravo", "text": "Bravo, moving to the creek at B7."}, {"who": "Charlie", "text": "Charlie, moving to the creek at B7."}, {"who": "Mercer", "text": "Mercer, flying the drone to the post."}], "sayAgain": ["Ruiz: Say again, which crest?"], "notCovered": [{"who": "Bravo", "text": "then east along the creek to D7, keep in the creek bed and stay low, stop at D7 and wait for Lead, don't cross the road"}, {"who": "Charlie", "text": "then east along the creek to D7, keep in the creek bed and stay low, stop at D7 and wait for Lead, don't cross the road"}, {"who": "Mercer", "text": "tell me if anybody moves"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Ortiz · moveHill 77
Ortiz, moving to Hill 77.
Ortiz, 106 m, about 3 min
Ruiz · moveHill 77
Ruiz, moving to Hill 77.
Ruiz, 55 m
Marsh · moveHill 77
Marsh, moving to Hill 77.
Marsh, 62 m
Keller · moveHill 77
Keller, moving to Hill 77.
Keller, 59 m
Bravo · movecreek at B7
Bravo, moving to the creek at B7.
Pike, 408 m, about 5 min
Tran, 421 m, about 5 min
Vega, 424 m, about 5 min
Okafor, 407 m, about 5 min
Charlie · movecreek at B7
Charlie, moving to the creek at B7.
Boone, 416 m, about 5 min
Sato, 425 m, about 5 min
Banks, 429 m, about 5 min
Mercer · fly dronethe post
Mercer, flying the drone to the post.
Mercer
Ruiz: Say again, which crest?
Not covered yetBravo
then east along the creek to D7, keep in the creek bed and stay low, stop at D7 and wait for Lead, don't cross the road. Saved so it can be built.
Not covered yetCharlie
then east along the creek to D7, keep in the creek bed and stay low, stop at D7 and wait for Lead, don't cross the road. Saved so it can be built.
Not covered yetMercer
tell me if anybody moves. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 3)

> Ruiz, the crest of the hill at C3, the back side, away from the post. Bravo, Charlie, once you're in the creek at B7, go east along it to D7 and wait there. Stay in the creek bed, stay low, don't cross the road. Mercer, call it out if anybody over there moves.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "hold", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "creek at D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "creek at D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, holding on Hill 77, base of fire."}, {"who": "Ortiz", "text": "Ortiz, moving to Hill 77."}, {"who": "Ruiz", "text": "Ruiz, moving to Hill 77."}, {"who": "Marsh", "text": "Marsh, moving to Hill 77."}, {"who": "Keller", "text": "Keller, moving to Hill 77."}, {"who": "Bravo", "text": "Bravo, moving along the creek to D7, staying low in the creek bed."}, {"who": "Charlie", "text": "Charlie, moving along the creek to D7, staying low in the creek bed."}, {"who": "Mercer", "text": "Mercer, flying the drone over the post."}], "sayAgain": [], "notCovered": [{"who": "Mercer", "text": "call out if anybody moves over there"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · holdHill 77
Alpha, holding on Hill 77, base of fire.
Novak
Ortiz
Doyle
Duarte
Ruiz · moveHill 77
Ruiz, moving to Hill 77.
Ruiz, 55 m
Marsh · moveHill 77
Marsh, moving to Hill 77.
Marsh, 62 m
Keller · moveHill 77
Keller, moving to Hill 77.
Keller, 59 m
Bravo · movecreek at D7
Bravo, moving along the creek to D7, staying low in the creek bed.
Pike, 444 m, about 5 min
Tran, 451 m, about 6 min
Vega, 477 m, about 6 min
Okafor, 441 m, about 5 min
Charlie · movecreek at D7
Charlie, moving along the creek to D7, staying low in the creek bed.
Boone, 475 m, about 6 min
Sato, 484 m, about 6 min
Banks, 488 m, about 6 min
Mercer, 470 m, about 6 min
Not covered yetMercer
call out if anybody moves over there. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 4 of 40 (Turn 4 of 40 · 3:00)
RADIO NET SINCE YOUR LAST LOOK:
  [2:00] Lead: Ruiz, the crest of the hill at C3, the back side, away from the post. Bravo, Charlie, once you're in the creek at B7, go east along it to D7 and wait there. Stay in the creek bed, stay low, don't cross the road. Mercer, call it out if anybody over there moves.
  [2:00] Alpha: holding on Hill 77, base of fire.
  [2:00] Ruiz: moving to Hill 77.
  [2:00] Marsh: moving to Hill 77.
  [2:00] Keller: moving to Hill 77.
  [2:00] Bravo: moving along the creek to D7, staying low in the creek bed.
  [2:00] Charlie: moving along the creek to D7, staying low in the creek bed.
  [2:12] Squad leader 1 is saved by the helmet.
  [2:51] Team leader 1 is down, hit by M27 rifle, hit in the chest.
  [2:52] Charlie: Contact on the drone, Kansian RPG gunner at E5.
  [2:52] Drone spots RPG gunner 1 at E5.
  [2:52] Ruiz: Set at Hill 77.
  [2:58] Keller: Set at Hill 77.
  [2:59] Rifleman 2 is down, hit by M27 rifle, hit in the belly.
  [2:59] Rifleman 2 killed by M27 rifle.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: nothing; 192 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: nothing; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 342 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: nothing; 100 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 183 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: move Hill 77; 132 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at B4: ok; doing: move creek at D7; 210 rds
  Tran (Automatic rifleman, Bravo) at B4: ok; doing: move creek at D7; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at A4: ok; doing: move creek at D7; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B4: ok; doing: move creek at D7; 210 rds
  Boone (Team leader, Charlie) at A4: ok; doing: move creek at D7; 210 rds
  Sato (Automatic rifleman, Charlie) at A4: ok; doing: move creek at D7; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at A4: ok; doing: move creek at D7; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: move creek at D7; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at F5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at F5, seen now
  PKM gunner 2 (op) at E5, seen now
  RPG gunner 1 (op) at F5, seen now
  Rifleman 1 (op) at F6, seen now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at E5, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-004.png
```

### Player traffic (turn 4)

> Ortiz, you're still at the bottom, get up on the hill with Novak and put that SAW on the post. Alpha, keep working them, shoot the PKM guys first, then the RPG guy. Ruiz, stay behind the crest and keep your head down. Bravo, Charlie, keep moving down the creek to D7.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Ortiz", "verb": "overwatch", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "fire", "where": null, "target": "PKM gunner 1", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "hide", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "creek at D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "creek at D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Ortiz", "text": "Ortiz, moving to the post, overwatch from there."}, {"who": "Alpha", "text": "Alpha, shooting PKM gunner 1."}, {"who": "Ruiz", "text": "Lead, hiding."}, {"who": "Bravo", "text": "Bravo, moving to the creek at D7."}, {"who": "Charlie", "text": "Charlie, moving to the creek at D7."}], "sayAgain": ["Lead: Say again, which crest?"], "notCovered": [{"who": "Alpha", "text": "then shoot PKM gunner 2 as well, and after the PKM guys, shoot RPG gunner 1"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Ortiz · overwatchthe post
Ortiz, moving to the post, overwatch from there.
Ortiz, 404 m, about 9 min
Alpha · firePKM gunner 1
Alpha, shooting PKM gunner 1.
Novak
Doyle
Duarte
Ruiz · hide
Lead, hiding.
Ruiz, 0 m
Bravo · movecreek at D7
Bravo, moving to the creek at D7.
Pike, 359 m, about 4 min
Tran, 369 m, about 4 min
Vega, 391 m, about 5 min
Okafor, 358 m, about 4 min
Charlie · movecreek at D7
Charlie, moving to the creek at D7.
Boone, 386 m, about 5 min
Sato, 397 m, about 5 min
Banks, 403 m, about 5 min
Mercer, 383 m, about 5 min
Lead: Say again, which crest?
Not covered yetAlpha
then shoot PKM gunner 2 as well, and after the PKM guys, shoot RPG gunner 1. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 4)

> Whoa, Ortiz, not the post! Stop. Ortiz, go to the hill at C3, the one Novak's on, and stay there. Do not go toward the post. Ruiz, get up on the hill and stay low behind the top, don't hide, I need you to see. Alpha, after the PKM guy, hit the other PKM, then the RPG guy.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Ortiz", "verb": "move", "where": "Hill 77"}, {"who": "Ruiz", "verb": "move", "where": "Hill 77"}, {"who": "Alpha", "verb": "fire", "target": "PKM gunner 1"}, {"who": "Bravo", "verb": "move", "where": "creek at D7"}, {"who": "Charlie", "verb": "move", "where": "creek at D7"}], "readbacks": [{"who": "Ortiz", "text": "Ortiz, moving to Hill 77."}, {"who": "Ruiz", "text": "Ruiz, moving to Hill 77."}, {"who": "Alpha", "text": "Alpha, firing on PKM gunner 1."}, {"who": "Bravo", "text": "Bravo, moving to creek at D7."}, {"who": "Charlie", "text": "Charlie, moving to creek at D7."}], "sayAgain": [], "notCovered": [{"who": "Ruiz", "text": "stay low behind the top"}, {"who": "Alpha", "text": "after PKM gunner 1, fire at PKM gunner 2, then RPG gunner 1"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Ortiz · moveHill 77
Ortiz, moving to Hill 77.
Ortiz, 106 m, about 3 min
Ruiz · moveHill 77
Ruiz, moving to Hill 77.
Ruiz, 0 m
Alpha · firePKM gunner 1
Alpha, firing on PKM gunner 1.
Novak
Doyle
Duarte
Bravo · movecreek at D7
Bravo, moving to creek at D7.
Pike, 359 m, about 4 min
Tran, 369 m, about 4 min
Vega, 391 m, about 5 min
Okafor, 358 m, about 4 min
Charlie · movecreek at D7
Charlie, moving to creek at D7.
Boone, 386 m, about 5 min
Sato, 397 m, about 5 min
Banks, 403 m, about 5 min
Mercer, 383 m, about 5 min
Not covered yetRuiz
stay low behind the top. Saved so it can be built.
Not covered yetAlpha
after PKM gunner 1, fire at PKM gunner 2, then RPG gunner 1. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 5 of 40 (Turn 5 of 40 · 4:00)
RADIO NET SINCE YOUR LAST LOOK:
  [3:00] Lead: Whoa, Ortiz, not the post! Stop. Ortiz, go to the hill at C3, the one Novak's on, and stay there. Do not go toward the post. Ruiz, get up on the hill and stay low behind the top, don't hide, I need you to see. Alpha, after the PKM guy, hit the other PKM, then the RPG guy.
  [3:00] Ortiz: moving to Hill 77.
  [3:00] Ruiz: moving to Hill 77.
  [3:00] Alpha: firing on PKM gunner 1.
  [3:00] Bravo: moving to creek at D7.
  [3:00] Charlie: moving to creek at D7.
  [3:00] Ruiz: Set at Hill 77.
  [3:00] Alpha: Engaging Rifleman 3.
  [3:00] RPG gunner 1 is wounded by M38 marksman rifle, hit in the arm.
  [3:01] Marsh: Set at Hill 77.
  [3:07] PKM gunner 2 is down, hit by M38 marksman rifle, hit in the belly.
  [3:12] Rifleman 3 is wounded by M4A1 carbine, hit in the arm.
  [3:12] Rifleman 3 is saved by the plate.
  [3:12] Rifleman 3 is down, hit by M4A1 carbine, hit in the chest.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: fire PKM gunner 1; 174 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: move Hill 77; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: fire PKM gunner 1; 327 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: fire PKM gunner 1; 95 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 168 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 117 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at B5: ok; doing: move creek at D7; 210 rds
  Tran (Automatic rifleman, Bravo) at B4: ok; doing: move creek at D7; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B4: ok; doing: move creek at D7; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B5: ok; doing: move creek at D7; 207 rds
  Boone (Team leader, Charlie) at B4: ok; doing: move creek at D7; 210 rds
  Sato (Automatic rifleman, Charlie) at B4: ok; doing: move creek at D7; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B4: ok; doing: move creek at D7; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B4: ok; doing: move creek at D7; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at H6, seen now
  Observer 1 (op) at G6, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at G5, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F6, seen now, wounded
  Rifleman 1 (op) at G7, seen now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at F5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-005.png
```

### Player traffic (turn 5)

> Alpha, keep firing on anything you can ID, don't let them set up again. Bravo, Charlie, keep going to D7 and hold there.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "D7", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, holding. Firing on anything we can ID."}, {"who": "Bravo", "text": "Bravo, moving to D7."}, {"who": "Charlie", "text": "Charlie, moving to D7."}], "sayAgain": [], "notCovered": []}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · hold
Alpha, holding. Firing on anything we can ID.
Novak
Ortiz
Doyle
Duarte
Bravo · moveD7
Bravo, moving to D7.
Pike, 325 m, about 4 min
Tran, 334 m, about 4 min
Vega, 366 m, about 5 min
Okafor, 324 m, about 4 min
Charlie · moveD7
Charlie, moving to D7.
Boone, 351 m, about 4 min
Sato, 364 m, about 5 min
Banks, 369 m, about 5 min
Mercer, 349 m, about 4 min
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 6 of 40 (Turn 6 of 40 · 5:00)
RADIO NET SINCE YOUR LAST LOOK:
  [4:00] Lead: Alpha, keep firing on anything you can ID, don't let them set up again. Bravo, Charlie, keep going to D7 and hold there.
  [4:00] Alpha: holding. Firing on anything we can ID.
  [4:00] Bravo: moving to D7.
  [4:00] Charlie: moving to D7.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: nothing; 174 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: nothing; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 327 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: nothing; 95 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 168 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 117 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at B5: ok; doing: move D7; 210 rds
  Tran (Automatic rifleman, Bravo) at B5: ok; doing: move D7; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B5: ok; doing: move D7; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B5: ok; doing: move D7; 207 rds
  Boone (Team leader, Charlie) at B5: ok; doing: move D7; 210 rds
  Sato (Automatic rifleman, Charlie) at B5: ok; doing: move D7; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B5: ok; doing: move D7; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B5: ok; doing: move D7; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at I7, seen now
  Observer 1 (op) at I7, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at I6, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at G7, seen now, wounded
  Rifleman 1 (op) at H9, seen now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at F5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-006.png
```

### Player traffic (turn 6)

> They broke and ran, so we go now. Bravo, Charlie, forget D7, go straight at the post at F5 across the open ground, spread out, Bravo on the left, Charlie on the right. Get in there and search it, smash the radio and the scopes. Tran and Sato keep your guns pointed at the east side. Mercer, keep the drone on them so we see what comes back. Alpha, keep covering, shift your fire east if they show. Ortiz, quit standing there, move up with Novak. Ruiz, Marsh, Keller, stay put on the hill for now.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Tran", "verb": "overwatch", "where": null, "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Sato", "verb": "overwatch", "where": null, "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "overwatch", "where": null, "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ortiz", "verb": "move", "where": "Novak", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the post, search it."}, {"who": "Charlie", "text": "Charlie, moving to the post, search it."}, {"who": "Tran", "text": "Tran, overwatch east."}, {"who": "Sato", "text": "Sato, overwatch east."}, {"who": "Mercer", "text": "Mercer, flying drone to the post."}, {"who": "Alpha", "text": "Alpha, overwatch east."}, {"who": "Ortiz", "text": "Ortiz, moving to Novak."}, {"who": "Ruiz", "text": "Ruiz, holding."}, {"who": "Marsh", "text": "Marsh, holding."}, {"who": "Keller", "text": "Keller, holding."}], "sayAgain": [], "notCovered": [{"who": "Bravo", "text": "smash the radio and the scopes"}, {"who": "Charlie", "text": "smash the radio and the scopes"}, {"who": "Bravo", "text": "spread out, on the left"}, {"who": "Charlie", "text": "spread out, on the right"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · searchthe post
Bravo, moving to the post, search it.
Pike, 332 m, about 7 min
Vega, 352 m, about 7 min
Okafor, 332 m, about 7 min
Charlie · searchthe post
Charlie, moving to the post, search it.
Boone, 344 m, about 7 min
Banks, 350 m, about 7 min
Tran · overwatchfacing east
Tran, overwatch east.
Tran
Sato · overwatchfacing east
Sato, overwatch east.
Sato
Mercer · fly dronethe post
Mercer, flying drone to the post.
Mercer
Alpha · overwatchfacing east
Alpha, overwatch east.
Novak
Doyle
Duarte
Ortiz · moveNovak
Ortiz, moving to Novak.
Ortiz, 63 m
Ruiz · hold
Ruiz, holding.
Ruiz
Marsh · hold
Marsh, holding.
Marsh
Keller · hold
Keller, holding.
Keller
Not covered yetBravo
smash the radio and the scopes. Saved so it can be built.
Not covered yetCharlie
smash the radio and the scopes. Saved so it can be built.
Not covered yetBravo
spread out, on the left. Saved so it can be built.
Not covered yetCharlie
spread out, on the right. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 6)

> Tran, Sato, no, you go with your teams, move to the post with them and cover the east side when you get there. Whole of Bravo and Charlie goes. Pike, Boone, spread your guys out as you cross, don't bunch up.

Game master (Haiku) read it as:
```json
{"orders": [
{"who": "Bravo", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Tran", "verb": "move", "where": "the post", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Sato", "verb": "move", "where": "the post", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Alpha", "verb": "overwatch", "where": null, "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Ortiz", "verb": "move", "where": "Novak", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Ruiz", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Marsh", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null},
{"who": "Keller", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}
],
"readbacks": [
{"who": "Bravo", "text": "Bravo, moving on the post, search it."},
{"who": "Charlie", "text": "Charlie, moving on the post, search it."},
{"who": "Tran", "text": "Tran, moving to the post, facing east."},
{"who": "Sato", "text": "Sato, moving to the post, facing east."},
{"who": "Mercer", "text": "Mercer, flying the drone over the post."},
{"who": "Alpha", "text": "Alpha, overwatch facing east."},
{"who": "Ortiz", "text": "Ortiz, moving to Novak."},
{"who": "Ruiz", "text": "Ruiz, holding."},
{"who": "Marsh", "text": "Marsh, holding."},
{"who": "Keller", "text": "Keller, holding."}
],
"sayAgain": [],
"notCovered": [
{"who": "Bravo", "text": "smash the radio and the scopes; spread out on the left"},
{"who": "Charlie", "text": "smash the radio and the scopes; spread out on the right"},
{"who": "Pike", "text": "spread your guys out as you cross, don't bunch up"},
{"who": "Boone", "text": "spread your guys out as you cross, don't bunch up"}
]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · searchthe post
Bravo, moving on the post, search it.
Pike, 332 m, about 7 min
Vega, 352 m, about 7 min
Okafor, 332 m, about 7 min
Charlie · searchthe post
Charlie, moving on the post, search it.
Boone, 344 m, about 7 min
Banks, 350 m, about 7 min
Tran · movethe post · facing east
Tran, moving to the post, facing east.
Tran, 335 m, about 7 min
Sato · movethe post · facing east
Sato, moving to the post, facing east.
Sato, 347 m, about 7 min
Mercer · fly dronethe post
Mercer, flying the drone over the post.
Mercer
Alpha · overwatchfacing east
Alpha, overwatch facing east.
Novak
Doyle
Duarte
Ortiz · moveNovak
Ortiz, moving to Novak.
Ortiz, 63 m
Ruiz · hold
Ruiz, holding.
Ruiz
Marsh · hold
Marsh, holding.
Marsh
Keller · hold
Keller, holding.
Keller
Not covered yetBravo
smash the radio and the scopes; spread out on the left. Saved so it can be built.
Not covered yetCharlie
smash the radio and the scopes; spread out on the right. Saved so it can be built.
Not covered yetPike
spread your guys out as you cross, don't bunch up. Saved so it can be built.
Not covered yetBoone
spread your guys out as you cross, don't bunch up. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 7 of 40 (Turn 7 of 40 · 6:00)
RADIO NET SINCE YOUR LAST LOOK:
  [5:00] Lead: Tran, Sato, no, you go with your teams, move to the post with them and cover the east side when you get there. Whole of Bravo and Charlie goes. Pike, Boone, spread your guys out as you cross, don't bunch up.
  [5:00] Bravo: moving on the post, search it.
  [5:00] Charlie: moving on the post, search it.
  [5:00] Tran: moving to the post, facing east.
  [5:00] Sato: moving to the post, facing east.
  [5:00] Mercer: flying the drone over the post.
  [5:00] Alpha: overwatch facing east.
  [5:00] Ortiz: moving to Novak.
  [5:00] Ruiz: holding.
  [5:00] Marsh: holding.
  [5:00] Keller: holding.
  [5:00] Alpha: Set. Overwatch east.
  [5:55] Charlie: Contact on the drone, Kansian squad leader at J8.
  [5:55] Drone spots Squad leader 1 at J8.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: overwatch facing east; 174 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: move Novak; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: overwatch facing east; 327 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: overwatch facing east; 95 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 168 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 117 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at C5: ok; doing: search the post; 210 rds
  Tran (Automatic rifleman, Bravo) at C5: ok; doing: move the post facing east; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at C5: ok; doing: search the post; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C5: ok; doing: search the post; 207 rds
  Boone (Team leader, Charlie) at C5: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at C5: ok; doing: move the post facing east; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C5: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B5: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at J8, seen now
  Observer 1 (op) at J8, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at J7, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at H8, seen now, wounded
  Rifleman 1 (op) at I10, last seen just now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at F5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-007.png
```

### Player traffic (turn 7)

> Bravo, Charlie, keep moving, get on the post and wreck the radio and the scopes when you're there. Medic Marsh, go with them and check the two down guys at F5, if either's still alive, tie him up and Keller goes with you to talk to him. Alpha, stay on the hill and watch east. Mercer, tell me the second you see a bunch of them coming back.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "talk", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "overwatch", "where": "Hill 77", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "report", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the post."}, {"who": "Charlie", "text": "Charlie, moving to the post."}, {"who": "Marsh", "text": "Doc, going to F5 to check the two down men."}, {"who": "Keller", "text": "Keller, going to F5 to talk to the down man."}, {"who": "Alpha", "text": "Alpha, overwatch east from Hill 77."}, {"who": "Mercer", "text": "Lead, I'll report the second I see them coming back."}], "sayAgain": ["Doc: Say again, which two down men at F5?"], "notCovered": [{"who": "Bravo", "text": "wreck the radio and the scopes when you're there"}, {"who": "Charlie", "text": "wreck the radio and the scopes when you're there"}, {"who": "Marsh", "text": "go with Bravo and Charlie, and tie up either down man if still alive"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · movethe post
Bravo, moving to the post.
Pike, 291 m, about 6 min
Tran, 293 m, about 6 min
Vega, 310 m, about 7 min
Okafor, 291 m, about 6 min
Charlie · movethe post
Charlie, moving to the post.
Boone, 303 m, about 6 min
Sato, 305 m, about 6 min
Banks, 308 m, about 7 min
Marsh · treat casualtyF5
Doc, going to F5 to check the two down men.
Marsh: Nobody's hit, Lead.
Keller · talkF5
Keller, going to F5 to talk to the down man.
Keller: Say again, talk to who?
Alpha · overwatchHill 77 · facing east
Alpha, overwatch east from Hill 77.
Novak, 0 m
Ortiz, 3 m
Doyle, 15 m
Duarte, 12 m
Mercer · report
Lead, I'll report the second I see them coming back.
Mercer
Doc: Say again, which two down men at F5?
Not covered yetBravo
wreck the radio and the scopes when you're there. Saved so it can be built.
Not covered yetCharlie
wreck the radio and the scopes when you're there. Saved so it can be built.
Not covered yetMarsh
go with Bravo and Charlie, and tie up either down man if still alive. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 7)

> Doc, the two wounded Kansians lying at F5 at the post, the enemy ones. Keller, go with Doc and talk to whichever one is alive. Both of you move with Bravo, stay behind them, don't get out in front.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "treat casualty", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "talk", "where": "F5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "overwatch", "where": "Hill 77", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "report", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the post."}, {"who": "Charlie", "text": "Charlie, moving to the post."}, {"who": "Marsh", "text": "Doc, moving to F5 to treat the casualties."}, {"who": "Keller", "text": "Interpreter, moving to F5 to talk to the one alive."}, {"who": "Alpha", "text": "Alpha, overwatch east from Hill 77."}, {"who": "Mercer", "text": "Mercer, reporting when a bunch of them come back."}], "sayAgain": ["Marsh: Say again, which one do you want tied up?"], "notCovered": [{"who": "Bravo", "text": "wreck the radio and the scopes at the post"}, {"who": "Charlie", "text": "wreck the radio and the scopes at the post"}, {"who": "Marsh", "text": "stay behind Bravo, don't get out in front"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · movethe post
Bravo, moving to the post.
Pike, 291 m, about 6 min
Tran, 293 m, about 6 min
Vega, 310 m, about 7 min
Okafor, 291 m, about 6 min
Charlie · movethe post
Charlie, moving to the post.
Boone, 303 m, about 6 min
Sato, 305 m, about 6 min
Banks, 308 m, about 7 min
Marsh · treat casualtyF5
Doc, moving to F5 to treat the casualties.
Marsh: Nobody's hit, Lead.
Keller · talkF5
Interpreter, moving to F5 to talk to the one alive.
Keller: Say again, talk to who?
Alpha · overwatchHill 77 · facing east
Alpha, overwatch east from Hill 77.
Novak, 0 m
Ortiz, 3 m
Doyle, 15 m
Duarte, 12 m
Mercer · report
Mercer, reporting when a bunch of them come back.
Mercer
Marsh: Say again, which one do you want tied up?
Not covered yetBravo
wreck the radio and the scopes at the post. Saved so it can be built.
Not covered yetCharlie
wreck the radio and the scopes at the post. Saved so it can be built.
Not covered yetMarsh
stay behind Bravo, don't get out in front. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 8 of 40 (Turn 8 of 40 · 7:00)
RADIO NET SINCE YOUR LAST LOOK:
  [6:00] Lead: Doc, the two wounded Kansians lying at F5 at the post, the enemy ones. Keller, go with Doc and talk to whichever one is alive. Both of you move with Bravo, stay behind them, don't get out in front.
  [6:00] Bravo: moving to the post.
  [6:00] Charlie: moving to the post.
  [6:00] Marsh: Nobody's hit, Lead.
  [6:00] Keller: Say again, talk to who?
  [6:00] Alpha: overwatch east from Hill 77.
  [6:00] Mercer: reporting when a bunch of them come back.
  [6:00] Marsh: Say again, which one do you want tied up?
  [6:02] Charlie: Contact on the drone, Kansian rifleman at J10.
  [6:02] Drone spots Rifleman 1 at J10.
  [6:14] Alpha: Set. Overwatch east.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 174 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 327 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 95 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 168 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 117 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at C5: ok; doing: move the post; 210 rds
  Tran (Automatic rifleman, Bravo) at C5: ok; doing: move the post; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at C5: ok; doing: move the post; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C5: ok; doing: move the post; 207 rds
  Boone (Team leader, Charlie) at C5: ok; doing: move the post; 210 rds
  Sato (Automatic rifleman, Charlie) at C5: ok; doing: move the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C5: ok; doing: move the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B5: ok; doing: nothing; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at J9, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at I8, seen now, wounded
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at F5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-008.png
```

### Player traffic (turn 8)

> Marsh and Keller, stay on the hill for now, I'll call you up once the post is ours. Bravo, Charlie, keep going, get to the post and search it for the radio and scopes. Mercer, you're still back at B5, catch up with Charlie and bring the drone with you.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Marsh", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "follow", "where": null, "target": "Charlie", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}],
 "readbacks": [{"who": "Marsh", "text": "Doc, holding on the hill."}, {"who": "Keller", "text": "Keller, holding on the hill for now."}, {"who": "Bravo", "text": "Bravo, searching the post for the radio and scopes."}, {"who": "Charlie", "text": "Charlie, searching the post for the radio and scopes."}, {"who": "Mercer", "text": "Mercer, following Charlie."}],
 "sayAgain": [],
 "notCovered": [{"who": "Mercer", "text": "bring the drone with you"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Marsh · hold
Doc, holding on the hill.
Marsh
Keller · hold
Keller, holding on the hill for now.
Keller
Bravo · searchthe post
Bravo, searching the post for the radio and scopes.
Pike, 244 m, about 6 min
Tran, 246 m, about 6 min
Vega, 262 m, about 6 min
Okafor, 243 m, about 6 min
Charlie · searchthe post
Charlie, searching the post for the radio and scopes.
Boone, 255 m, about 6 min
Sato, 257 m, about 6 min
Banks, 259 m, about 6 min
Mercer · followCharlie
Mercer, following Charlie.
Mercer
Not covered yetMercer
bring the drone with you. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 9 of 40 (Turn 9 of 40 · 8:00)
RADIO NET SINCE YOUR LAST LOOK:
  [7:00] Lead: Marsh and Keller, stay on the hill for now, I'll call you up once the post is ours. Bravo, Charlie, keep going, get to the post and search it for the radio and scopes. Mercer, you're still back at B5, catch up with Charlie and bring the drone with you.
  [7:00] Marsh: Doc, holding on the hill.
  [7:00] Keller: holding on the hill for now.
  [7:00] Bravo: searching the post for the radio and scopes.
  [7:00] Charlie: searching the post for the radio and scopes.
  [7:00] Mercer: following Charlie.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 174 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 327 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: overwatch Hill 77 facing east; 95 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 168 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 117 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at D5: ok; doing: search the post; 210 rds
  Tran (Automatic rifleman, Bravo) at D5: ok; doing: search the post; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at C5: ok; doing: search the post; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at D5: ok; doing: search the post; 207 rds
  Boone (Team leader, Charlie) at D5: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at C5: ok; doing: search the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C5: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at C5: ok; doing: follow Charlie; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at J10, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at J9, seen now, wounded
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, dead
  Rifleman 3 (op) at F5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-2/map-009.png
```

