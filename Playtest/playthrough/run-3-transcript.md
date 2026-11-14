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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-001.png
```

### Player traffic (turn 1)

> Alpha, go east along the north side of that hill toward the road, stay low and don't shoot unless you see enemy for sure. Stop at the road and tell me what you see.
> Bravo and Charlie, follow Alpha about 100 meters back.
> Mercer, get the drone up and look at the post.
> Doc and the translator stay with me behind Alpha.
> Everybody hold fire for now.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "squad", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "squad", "text": "Holding fire, Lead."}, {"who": "Bravo", "text": "Bravo, following Alpha."}, {"who": "Charlie", "text": "Charlie, following Alpha."}, {"who": "Mercer", "text": "Mercer, drone up, flying to the post."}, {"who": "Marsh", "text": "Doc, staying with Lead."}, {"who": "Keller", "text": "Keller, staying with Lead."}], "sayAgain": ["Alpha: Say again, which hill?"], "notCovered": [{"who": "Alpha", "text": "go east along the north side of that hill toward the road, stay low and don't shoot unless you see enemy for sure"}, {"who": "Alpha", "text": "stop at the road and tell me what you see"}, {"who": "Bravo", "text": "stay about 100 meters back from Alpha"}, {"who": "Charlie", "text": "stay about 100 meters back from Alpha"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
squad · hold
Holding fire, Lead.
Novak
Ortiz
Doyle
Duarte
Ruiz
Pike
Tran
Vega
Okafor
Boone
Sato
Banks
Bravo · followAlpha
Bravo, following Alpha.
Pike
Tran
Vega
Okafor
Charlie · followAlpha
Charlie, following Alpha.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, drone up, flying to the post.
Mercer
Marsh · followLead
Doc, staying with Lead.
Marsh
Keller · followLead
Keller, staying with Lead.
Keller
Alpha: Say again, which hill?
Not covered yetAlpha
go east along the north side of that hill toward the road, stay low and don't shoot unless you see enemy for sure. Saved so it can be built.
Not covered yetAlpha
stop at the road and tell me what you see. Saved so it can be built.
Not covered yetBravo
stay about 100 meters back from Alpha. Saved so it can be built.
Not covered yetCharlie
stay about 100 meters back from Alpha. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 1)

> Alpha, the round hill with the trees right in front of us, the one at C3. Go around the top of it, then east to the road near E4. Stay low, don't shoot unless you see enemy for sure, and tell me what you see when you get to the road.
> Bravo and Charlie, stay about 100 meters back from Alpha.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "squad", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "follow", "where": null, "target": "Alpha", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "follow", "where": null, "target": "Lead", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, moving to Hill 77."}, {"who": "squad", "text": "Squad, holding."}, {"who": "Bravo", "text": "Bravo, following Alpha."}, {"who": "Charlie", "text": "Charlie, following Alpha."}, {"who": "Mercer", "text": "Mercer, flying the drone to the post."}, {"who": "Marsh", "text": "Doc, following Lead."}, {"who": "Keller", "text": "Interpreter, following Lead."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "then east to the road at E4"}, {"who": "Alpha", "text": "tell me what you see when you get to the road"}, {"who": "Alpha", "text": "stay low, don't shoot unless you see enemy for sure"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · moveHill 77
Alpha, moving to Hill 77.
Novak, 101 m, about 2 min
Ortiz, 106 m, about 3 min
Doyle, 112 m, about 3 min
Duarte, 110 m, about 3 min
squad · hold
Squad, holding.
Novak
Ortiz
Doyle
Duarte
Ruiz
Pike
Tran
Vega
Okafor
Boone
Sato
Banks
Bravo · followAlpha
Bravo, following Alpha.
Pike
Tran
Vega
Okafor
Charlie · followAlpha
Charlie, following Alpha.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, flying the drone to the post.
Mercer
Marsh · followLead
Doc, following Lead.
Marsh
Keller · followLead
Interpreter, following Lead.
Keller
Not covered yetAlpha
then east to the road at E4. Saved so it can be built.
Not covered yetAlpha
tell me what you see when you get to the road. Saved so it can be built.
Not covered yetAlpha
stay low, don't shoot unless you see enemy for sure. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 2 of 40 (Turn 2 of 40 · 1:00)
RADIO NET SINCE YOUR LAST LOOK:
  [0:00] Lead: Alpha, the round hill with the trees right in front of us, the one at C3. Go around the top of it, then east to the road near E4. Stay low, don't shoot unless you see enemy for sure, and tell me what you see when you get to the road.
Bravo and Charlie, stay about 100 meters back from Alpha.
  [0:00] Alpha: moving to Hill 77.
  [0:00] squad: holding.
  [0:00] Bravo: following Alpha.
  [0:00] Charlie: following Alpha.
  [0:00] Mercer: flying the drone to the post.
  [0:00] Marsh: Doc, following Lead.
  [0:00] Keller: Interpreter, following Lead.
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
  Novak (Team leader, Alpha) at B3: ok; doing: nothing; 210 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: nothing; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at B3: ok; doing: nothing; 420 rds
  Duarte (Rifleman, marksman, Alpha) at B3: ok; doing: nothing; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Lead; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Lead; 0 rds
  Pike (Team leader, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Tran (Automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Boone (Team leader, Charlie) at B3: ok; doing: follow Alpha; 210 rds
  Sato (Automatic rifleman, Charlie) at B3: ok; doing: follow Alpha; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at A3: ok; doing: follow Alpha; 420 rds
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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-002.png
```

### Player traffic (turn 2)

> Alpha, good, go to the hill and stop there. Don't go to the road yet. Get low and watch.
> Everybody hold fire until I say.
> Mercer, keep the drone over the post and keep telling me where the guys with the big machine guns are.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}],
 "readbacks": [{"who": "Mercer", "text": "Mercer, flying the drone over the post."}],
 "sayAgain": ["Alpha: Say again, which hill?"],
 "notCovered": [{"who": "squad", "text": "hold fire until I say"}, {"who": "Mercer", "text": "keep telling Lead where the PKM gunners are"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Mercer · fly dronethe post
Mercer, flying the drone over the post.
Mercer
Alpha: Say again, which hill?
Not covered yetsquad
hold fire until I say. Saved so it can be built.
Not covered yetMercer
keep telling Lead where the PKM gunners are. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 2)

> Alpha, go to Hill 77 at C3. Stop there and get low.
> Everybody hold fire.

Game master (Haiku) read it as:
```json
{"orders":[{"who":"Alpha","verb":"move","where":"Hill 77","target":null,"facing":null,"speed":null,"posture":"prone","swim":false,"kind":null},{"who":"Mercer","verb":"fly drone","where":"the post","target":null,"facing":null,"speed":null,"posture":null,"swim":false,"kind":null}],"readbacks":[{"who":"Alpha","text":"Alpha, moving to Hill 77, get low."},{"who":"Mercer","text":"Mercer, drone over the post."}],"sayAgain":[],"notCovered":[{"who":"squad","text":"hold fire until I say"},{"who":"Mercer","text":"keep telling Lead where the big machine guns are"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · moveHill 77 · prone
Alpha, moving to Hill 77, get low.
Novak, 101 m, about 2 min
Ortiz, 106 m, about 3 min
Doyle, 112 m, about 3 min
Duarte, 110 m, about 3 min
Mercer · fly dronethe post
Mercer, drone over the post.
Mercer
Not covered yetsquad
hold fire until I say. Saved so it can be built.
Not covered yetMercer
keep telling Lead where the big machine guns are. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 3 of 40 (Turn 3 of 40 · 2:00)
RADIO NET SINCE YOUR LAST LOOK:
  [1:00] Lead: Alpha, go to Hill 77 at C3. Stop there and get low.
Everybody hold fire.
  [1:00] Alpha: moving to Hill 77, get low.
  [1:00] Mercer: drone over the post.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at B3: ok; doing: move Hill 77; 210 rds
  Ortiz (Automatic rifleman, Alpha) at B3: ok; doing: move Hill 77; 660 rds
  Doyle (Assistant automatic rifleman, Alpha) at B3: ok; doing: move Hill 77; 420 rds
  Duarte (Rifleman, marksman, Alpha) at B3: ok; doing: move Hill 77; 120 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Lead; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Lead; 0 rds
  Pike (Team leader, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Tran (Automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Boone (Team leader, Charlie) at B3: ok; doing: follow Alpha; 210 rds
  Sato (Automatic rifleman, Charlie) at B3: ok; doing: follow Alpha; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B3: ok; doing: follow Alpha; 420 rds
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
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-003.png
```

### Turn played

```
TIME: turn 4 of 40 (Turn 4 of 40 · 3:00)
RADIO NET SINCE YOUR LAST LOOK:
  [2:00] Lead: No answer from Alpha. Their radio's down.
  [2:47] Team leader 1 is wounded by M320 grenade launcher, hit in the leg.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: nothing; 210 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: move Hill 77; 606 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: move Hill 77; 417 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: move Hill 77; 115 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Lead; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Lead; 0 rds
  Pike (Team leader, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Tran (Automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 660 rds
  Vega (Assistant automatic rifleman, Bravo) at B3: ok; doing: follow Alpha; 420 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at B3: ok; doing: follow Alpha; 210 rds
  Boone (Team leader, Charlie) at B3: ok; doing: follow Alpha; 210 rds
  Sato (Automatic rifleman, Charlie) at B3: ok; doing: follow Alpha; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at B3: ok; doing: follow Alpha; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at F5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now, wounded
  PKM gunner 1 (op) at F4, seen now
  PKM gunner 2 (op) at E5, seen now
  Rifleman 1 (op) at E6, seen now
  Rifleman 2 (op) at E5, seen now
  Rifleman 3 (op) at E5, seen now
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-004.png
```

### Player traffic (turn 4)

> Bravo and Charlie, move up to Hill 77 and get low with Alpha.
> Alpha, if you can hear me, stay on the hill and don't shoot.
> Mercer, keep the drone on the post.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "hold", "where": null, "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to Hill 77."}, {"who": "Charlie", "text": "Charlie, moving to Hill 77."}, {"who": "Alpha", "text": "Alpha, holding on the hill."}, {"who": "Mercer", "text": "Mercer, flying the drone over the post."}], "sayAgain": [], "notCovered": [{"who": "Bravo", "text": "get low with Alpha"}, {"who": "Charlie", "text": "get low with Alpha"}, {"who": "Alpha", "text": "don't shoot"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · moveHill 77
Bravo, moving to Hill 77.
Pike, 42 m
Tran, 48 m
Vega, 56 m
Okafor, 51 m
Charlie · moveHill 77
Charlie, moving to Hill 77.
Boone, 68 m
Sato, 74 m, about 2 min
Banks, 81 m, about 2 min
Alpha · hold
Alpha, holding on the hill.
Novak: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Ortiz: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Doyle: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Duarte: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Mercer · fly dronethe post
Mercer, flying the drone over the post.
Mercer
Not covered yetBravo
get low with Alpha. Saved so it can be built.
Not covered yetCharlie
get low with Alpha. Saved so it can be built.
Not covered yetAlpha
don't shoot. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 5 of 40 (Turn 5 of 40 · 4:00)
RADIO NET SINCE YOUR LAST LOOK:
  [3:00] Lead: Bravo and Charlie, move up to Hill 77 and get low with Alpha.
Alpha, if you can hear me, stay on the hill and don't shoot.
Mercer, keep the drone on the post.
  [3:00] Bravo: moving to Hill 77.
  [3:00] Charlie: moving to Hill 77.
  [3:00] Alpha: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
  [3:00] Mercer: flying the drone over the post.
  [3:14] Charlie: Contact on the drone, Kansian RPG gunner at E5.
  [3:14] Drone spots RPG gunner 1 at E5.
  [3:28] Team leader 1 is down, hit by M320 grenade launcher, hit again in the leg.
  [3:35] Rifleman 2 is wounded by M27, automatic rifleman, hit in the leg.
  [3:35] Rifleman 2 is down, hit by M27, automatic rifleman, hit again in the leg.
  [3:36] Rifleman 3 is down, hit by M27 rifle, hit in the chest.
  [3:47] RPG gunner 1 is down, hit by M27, automatic rifleman, hit in the chest.
  [3:51] PKM gunner 2 is wounded by M27 rifle, hit in the arm.
  [3:51] PKM gunner 2 is saved by the plate.
  [3:51] PKM gunner 2 is down, hit by M27 rifle, hit in the chest.
  [3:53] Bravo: Set at Hill 77.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: nothing; 192 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 456 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 384 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: nothing; 99 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: follow Lead; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: follow Lead; 0 rds
  Pike (Team leader, Bravo) at C3: ok; doing: nothing; 183 rds
  Tran (Automatic rifleman, Bravo) at C3: ok; doing: nothing; 594 rds
  Vega (Assistant automatic rifleman, Bravo) at C3: ok; doing: nothing; 408 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at C3: ok; doing: nothing; 183 rds
  Boone (Team leader, Charlie) at C3: ok; doing: move Hill 77; 210 rds
  Sato (Automatic rifleman, Charlie) at C3: ok; doing: move Hill 77; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C3: ok; doing: move Hill 77; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at G5, seen now
  Observer 1 (op) at F5, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at F5, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F5, seen now, down
  Rifleman 1 (op) at F6, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-005.png
```

### Player traffic (turn 5)

> Alpha, keep shooting the guys you can see, but only the ones in uniform with guns.
> Bravo, move up to the road at E4 and help shoot. Charlie, go with them.
> Don't shoot anybody who's down or has hands up, I want a prisoner.
> Mercer, keep the drone on the post and tell me what's still standing.
> Doc and translator, stay on the hill.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "road at E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "follow", "where": null, "target": "Bravo", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to road at E4."}, {"who": "Charlie", "text": "Charlie, following Bravo."}, {"who": "Mercer", "text": "Mercer, flying the drone to the post."}], "sayAgain": ["Alpha: Say again, which targets?", "Marsh: Say again, which hill?", "Keller: Say again, which hill?"], "notCovered": [{"who": "Alpha", "text": "shoot only at the enemy in uniform with guns"}, {"who": "Bravo", "text": "help shoot"}, {"who": "squad", "text": "don't shoot anybody who's down or has hands up; take a prisoner"}, {"who": "Mercer", "text": "report what's still standing"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · moveroad at E4
Bravo, moving to road at E4.
Pike, 202 m, about 2 min
Tran, 201 m, about 2 min
Vega, 203 m, about 2 min
Okafor, 200 m, about 2 min
Charlie · followBravo
Charlie, following Bravo.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, flying the drone to the post.
Mercer
Alpha: Say again, which targets?
Marsh: Say again, which hill?
Keller: Say again, which hill?
Not covered yetAlpha
shoot only at the enemy in uniform with guns. Saved so it can be built.
Not covered yetBravo
help shoot. Saved so it can be built.
Not covered yetsquad
don't shoot anybody who's down or has hands up; take a prisoner. Saved so it can be built.
Not covered yetMercer
report what's still standing. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 5)

> Alpha, shoot the enemy soldiers at E5 and F5, the ones by the road.
> Doc and translator, stay right here on Hill 77 at C3.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Alpha", "verb": "fire", "where": "E5", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "move", "where": "road at E4", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "follow", "where": null, "target": "Bravo", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Marsh", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Keller", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Alpha", "text": "Alpha, shooting at E5."}, {"who": "Bravo", "text": "Bravo, moving to road at E4."}, {"who": "Charlie", "text": "Charlie, following Bravo."}, {"who": "Mercer", "text": "Mercer, drone to the post."}, {"who": "Marsh", "text": "Doc, moving to Hill 77."}, {"who": "Keller", "text": "Keller, moving to Hill 77."}], "sayAgain": [], "notCovered": [{"who": "Alpha", "text": "shoot the enemy soldiers at F5"}, {"who": "Mercer", "text": "tell me what's still standing"}, {"who": "squad", "text": "don't shoot anybody who's down or has hands up, take a prisoner"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Alpha · fireE5
Alpha, shooting at E5.
Novak: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Ortiz: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Doyle: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Duarte: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
Bravo · moveroad at E4
Bravo, moving to road at E4.
Pike, 202 m, about 2 min
Tran, 201 m, about 2 min
Vega, 203 m, about 2 min
Okafor, 200 m, about 2 min
Charlie · followBravo
Charlie, following Bravo.
Boone
Sato
Banks
Mercer · fly dronethe post
Mercer, drone to the post.
Mercer
Marsh · moveHill 77
Doc, moving to Hill 77.
Marsh, 131 m, about 3 min
Keller · moveHill 77
Keller, moving to Hill 77.
Keller, 129 m, about 3 min
Not covered yetAlpha
shoot the enemy soldiers at F5. Saved so it can be built.
Not covered yetMercer
tell me what's still standing. Saved so it can be built.
Not covered yetsquad
don't shoot anybody who's down or has hands up, take a prisoner. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 6 of 40 (Turn 6 of 40 · 5:00)
RADIO NET SINCE YOUR LAST LOOK:
  [4:00] Lead: Alpha, shoot the enemy soldiers at E5 and F5, the ones by the road.
Doc and translator, stay right here on Hill 77 at C3.
  [4:00] Alpha: Alpha isn't answering, Lead. Their radio's down; get within 50 m and shout it.
  [4:00] Bravo: moving to road at E4.
  [4:00] Charlie: following Bravo.
  [4:00] Mercer: drone to the post.
  [4:00] Marsh: Doc, moving to Hill 77.
  [4:00] Keller: moving to Hill 77.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at C3: ok; doing: nothing; 192 rds
  Ortiz (Automatic rifleman, Alpha) at C3: ok; doing: nothing; 456 rds
  Doyle (Assistant automatic rifleman, Alpha) at C3: ok; doing: nothing; 384 rds
  Duarte (Rifleman, marksman, Alpha) at C3: ok; doing: nothing; 99 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: move Hill 77; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: move Hill 77; 0 rds
  Pike (Team leader, Bravo) at D3: ok; doing: move road at E4; 183 rds
  Tran (Automatic rifleman, Bravo) at D3: ok; doing: move road at E4; 594 rds
  Vega (Assistant automatic rifleman, Bravo) at D3: ok; doing: move road at E4; 408 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at D3: ok; doing: move road at E4; 183 rds
  Boone (Team leader, Charlie) at D3: ok; doing: follow Bravo; 210 rds
  Sato (Automatic rifleman, Charlie) at D3: ok; doing: follow Bravo; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at C3: ok; doing: follow Bravo; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone the post; 210 rds
  drone: up over F5
KNOWN CONTACTS:
  Squad leader 1 (op) at H6, seen now
  Observer 1 (op) at G6, seen now
  Team leader 1 (op) at F5, seen now, down
  PKM gunner 1 (op) at H6, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F5, seen now, down
  Rifleman 1 (op) at G8, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-006.png
```

### Player traffic (turn 6)

> Bravo, keep going to the road at E4, then on to the post at F5. Charlie, go with Bravo.
> Charlie, when you get to the post, search it and smash the radio and scopes.
> Bravo, watch east for the guys who ran off, they went toward G6 and H6.
> Ruiz, go up to Hill 77 and get Alpha to follow Bravo and cover east.
> Mercer, keep the drone up and watch where the runners go.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "road at E4", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "follow", "where": null, "target": "Bravo", "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "follow", "where": null, "target": "Bravo", "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the road at E4, watching east."}, {"who": "Charlie", "text": "Charlie, following Bravo."}, {"who": "Alpha", "text": "Alpha, following Bravo, covering east."}, {"who": "Ruiz", "text": "Lead, moving to Hill 77."}], "sayAgain": ["Mercer: Say again, where do you want the drone?"], "notCovered": [{"who": "Bravo", "text": "then on to the post at F5"}, {"who": "Charlie", "text": "when you get to the post, search it and smash the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · moveroad at E4 · facing east
Bravo, moving to the road at E4, watching east.
Pike, 118 m
Tran, 117 m
Vega, 119 m
Okafor, 116 m
Charlie · followBravo
Charlie, following Bravo.
Boone
Sato
Banks
Mercer
Alpha · followBravo · facing east
Alpha, following Bravo, covering east.
Novak
Ortiz
Doyle
Duarte
Ruiz · moveHill 77
Lead, moving to Hill 77.
Ruiz, 125 m, about 3 min
Mercer: Say again, where do you want the drone?
Not covered yetBravo
then on to the post at F5. Saved so it can be built.
Not covered yetCharlie
when you get to the post, search it and smash the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 6)

> Mercer, put the drone over G6, east of the post.
> Bravo and Charlie, after the road, go to the post at F5.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Bravo", "verb": "move", "where": "road at E4", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Charlie", "verb": "move", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "follow", "where": null, "target": "Bravo", "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Ruiz", "verb": "move", "where": "Hill 77", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Mercer", "verb": "fly drone", "where": "G6", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Bravo", "text": "Bravo, moving to the road at E4, facing east."}, {"who": "Charlie", "text": "Charlie, moving to the post."}, {"who": "Alpha", "text": "Alpha, following Bravo, covering east."}, {"who": "Ruiz", "text": "Lead, moving to Hill 77."}, {"who": "Mercer", "text": "Mercer, flying the drone over G6."}], "sayAgain": [], "notCovered": [{"who": "Charlie", "text": "search the post and smash the radio and scopes"}, {"who": "Bravo", "text": "go on to the post at F5 after the road"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Bravo · moveroad at E4 · facing east
Bravo, moving to the road at E4, facing east.
Pike, 118 m
Tran, 117 m
Vega, 119 m
Okafor, 116 m
Charlie · movethe post
Charlie, moving to the post.
Boone, 237 m, about 6 min
Sato, 245 m, about 6 min
Banks, 256 m, about 6 min
Alpha · followBravo · facing east
Alpha, following Bravo, covering east.
Novak
Ortiz
Doyle
Duarte
Ruiz · moveHill 77
Lead, moving to Hill 77.
Ruiz, 125 m, about 3 min
Mercer · fly droneG6
Mercer, flying the drone over G6.
Mercer
Not covered yetCharlie
search the post and smash the radio and scopes. Saved so it can be built.
Not covered yetBravo
go on to the post at F5 after the road. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 7 of 40 (Turn 7 of 40 · 6:00)
RADIO NET SINCE YOUR LAST LOOK:
  [5:00] Lead: Mercer, put the drone over G6, east of the post.
Bravo and Charlie, after the road, go to the post at F5.
  [5:00] Bravo: moving to the road at E4, facing east.
  [5:00] Charlie: moving to the post.
  [5:00] Alpha: following Bravo, covering east.
  [5:00] Ruiz: Lead, moving to Hill 77.
  [5:00] Mercer: flying the drone over G6.
  [5:00] Alpha: Radio check, Lead. We're back up.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at D3: ok; doing: follow Bravo facing east; 192 rds
  Ortiz (Automatic rifleman, Alpha) at D3: ok; doing: follow Bravo facing east; 456 rds
  Doyle (Assistant automatic rifleman, Alpha) at D3: ok; doing: follow Bravo facing east; 384 rds
  Duarte (Rifleman, marksman, Alpha) at D3: ok; doing: follow Bravo facing east; 99 rds
  Ruiz (Squad leader, HQ) at B3: ok; doing: move Hill 77; 210 rds
  Marsh (Corpsman, HQ) at B3: ok; doing: move Hill 77; 150 rds
  Keller (Interpreter, HQ) at B3: ok; doing: move Hill 77; 0 rds
  Pike (Team leader, Bravo) at E3: ok; doing: move road at E4 facing east; 183 rds
  Tran (Automatic rifleman, Bravo) at E4: ok; doing: move road at E4 facing east; 594 rds
  Vega (Assistant automatic rifleman, Bravo) at E3: ok; doing: move road at E4 facing east; 408 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at E4: ok; doing: move road at E4 facing east; 183 rds
  Boone (Team leader, Charlie) at D4: ok; doing: move the post; 210 rds
  Sato (Automatic rifleman, Charlie) at D4: ok; doing: move the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at D4: ok; doing: move the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A3: ok; doing: fly drone G6; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at J7, seen now
  Observer 1 (op) at I6, seen now
  Team leader 1 (op) at F5, last seen just now
  PKM gunner 1 (op) at J6, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F5, seen now, down
  Rifleman 1 (op) at I9, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-007.png
```

### Player traffic (turn 7)

> Charlie, when you get to the post, search it and wreck the radio and scopes.
> Bravo, go to the post too and cover Charlie, facing east.
> Alpha, go to the road at E4 and watch east. Don't shoot the guys on the ground. Keep one of them alive for me to question.
> Doc, check the wounded at the post once it's safe.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "overwatch", "where": "the post", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "overwatch", "where": "road at E4", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Charlie", "text": "Charlie, searching the post when we get there."}, {"who": "Bravo", "text": "Bravo, moving to the post, overwatch east."}, {"who": "Alpha", "text": "Alpha, moving to the road at E4, overwatch east."}], "sayAgain": ["Alpha: Say again, which man on the ground do you want kept alive?"], "notCovered": [{"who": "Charlie", "text": "wreck the radio and scopes"}, {"who": "Alpha", "text": "don't shoot the guys on the ground"}, {"who": "Marsh", "text": "check the wounded at the post once it's safe"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · searchthe post
Charlie, searching the post when we get there.
Boone, 178 m, about 5 min
Sato, 183 m, about 5 min
Banks, 189 m, about 5 min
Mercer, 528 m, about 12 min
Bravo · overwatchthe post · facing east
Bravo, moving to the post, overwatch east.
Pike, 150 m, about 5 min
Tran, 146 m, about 5 min
Vega, 154 m, about 5 min
Okafor, 142 m, about 5 min
Alpha · overwatchroad at E4 · facing east
Alpha, moving to the road at E4, overwatch east.
Novak, 118 m
Ortiz, 117 m
Doyle, 119 m
Duarte, 116 m
Alpha: Say again, which man on the ground do you want kept alive?
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Not covered yetAlpha
don't shoot the guys on the ground. Saved so it can be built.
Not covered yetMarsh
check the wounded at the post once it's safe. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Player traffic (turn 7)

> Alpha, the wounded guy at F5 by the post, the one with the leg wound. Don't shoot him, just guard him when we get there.

Game master (Haiku) read it as:
```json
{"orders": [{"who": "Charlie", "verb": "search", "where": "the post", "target": null, "facing": null, "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Bravo", "verb": "overwatch", "where": "the post", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}, {"who": "Alpha", "verb": "overwatch", "where": "road at E4", "target": null, "facing": "east", "speed": null, "posture": null, "swim": false, "kind": null}], "readbacks": [{"who": "Charlie", "text": "Charlie, moving to the post, search it."}, {"who": "Bravo", "text": "Bravo, moving to the post, overwatch east."}, {"who": "Alpha", "text": "Alpha, moving to the road at E4, overwatch east."}], "sayAgain": ["Alpha: Say again, I don't have a wounded man at F5 on my list. Which one?"], "notCovered": [{"who": "Charlie", "text": "wreck the radio and scopes"}, {"who": "Alpha", "text": "don't shoot the men on the ground, keep one alive to question"}, {"who": "Alpha", "text": "guard the wounded man at F5 when we get there, don't shoot him"}, {"who": "Marsh", "text": "check the wounded at the post once it's safe"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · searchthe post
Charlie, moving to the post, search it.
Boone, 178 m, about 5 min
Sato, 183 m, about 5 min
Banks, 189 m, about 5 min
Mercer, 528 m, about 12 min
Bravo · overwatchthe post · facing east
Bravo, moving to the post, overwatch east.
Pike, 150 m, about 5 min
Tran, 146 m, about 5 min
Vega, 154 m, about 5 min
Okafor, 142 m, about 5 min
Alpha · overwatchroad at E4 · facing east
Alpha, moving to the road at E4, overwatch east.
Novak, 118 m
Ortiz, 117 m
Doyle, 119 m
Duarte, 116 m
Alpha: Say again, I don't have a wounded man at F5 on my list. Which one?
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Not covered yetAlpha
don't shoot the men on the ground, keep one alive to question. Saved so it can be built.
Not covered yetAlpha
guard the wounded man at F5 when we get there, don't shoot him. Saved so it can be built.
Not covered yetMarsh
check the wounded at the post once it's safe. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 8 of 40 (Turn 8 of 40 · 7:00)
RADIO NET SINCE YOUR LAST LOOK:
  [6:00] Lead: Alpha, the wounded guy at F5 by the post, the one with the leg wound. Don't shoot him, just guard him when we get there.
  [6:00] Charlie: moving to the post, search it.
  [6:00] Bravo: moving to the post, overwatch east.
  [6:00] Alpha: moving to the road at E4, overwatch east.
  [6:00] Alpha: Say again, I don't have a wounded man at F5 on my list. Which one?
  [6:30] Charlie: Contact on the drone, Kansian squad leader at J8.
  [6:30] Drone spots Squad leader 1 at J8.
  [6:36] Charlie: Contact on the drone, Kansian rifleman at J10.
  [6:36] Drone spots Rifleman 1 at J10.
  [6:37] Keller: Set at Hill 77.
  [6:41] Marsh: Set at Hill 77.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at E3: ok; doing: overwatch road at E4 facing east; 192 rds
  Ortiz (Automatic rifleman, Alpha) at E3: ok; doing: overwatch road at E4 facing east; 456 rds
  Doyle (Assistant automatic rifleman, Alpha) at E3: ok; doing: overwatch road at E4 facing east; 384 rds
  Duarte (Rifleman, marksman, Alpha) at E4: ok; doing: overwatch road at E4 facing east; 99 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: move Hill 77; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at E4: ok; doing: overwatch the post facing east; 183 rds
  Tran (Automatic rifleman, Bravo) at E4: ok; doing: overwatch the post facing east; 594 rds
  Vega (Assistant automatic rifleman, Bravo) at E4: ok; doing: overwatch the post facing east; 408 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at E4: ok; doing: overwatch the post facing east; 183 rds
  Boone (Team leader, Charlie) at D4: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at D4: ok; doing: search the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at D4: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at A4: ok; doing: search the post; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at J9, seen now
  Observer 1 (op) at J7, seen now
  Team leader 1 (op) at F5, last seen 1 min ago
  PKM gunner 1 (op) at J7, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F5, seen now, down
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-008.png
```

### Player traffic (turn 8)

> Charlie, you're still at D4, not at the post. Go to the post at F5 now and search it. Wreck the radio and scopes.
> Bravo, go to the post at F5 too and cover Charlie.
> Alpha, hold at the road at E4 and watch east.

Game master (Haiku) read it as:
```json
{"orders":[{"who":"Charlie","verb":"search","where":"the post","target":null,"facing":null,"speed":null,"posture":null,"swim":false,"kind":null},{"who":"Bravo","verb":"overwatch","where":"the post","target":null,"facing":null,"speed":null,"posture":null,"swim":false,"kind":null},{"who":"Alpha","verb":"move","where":"road at E4","target":null,"facing":"east","speed":null,"posture":null,"swim":false,"kind":null}],"readbacks":[{"who":"Charlie","text":"Charlie, searching the post."},{"who":"Bravo","text":"Bravo, moving to the post, overwatch."},{"who":"Alpha","text":"Alpha, moving to road at E4, facing east."}],"sayAgain":[],"notCovered":[{"who":"Charlie","text":"wreck the radio and scopes"}]}
```

Preview:
```
ORDERS READY · READ BY HAIKU
Charlie · searchthe post
Charlie, searching the post.
Boone, 137 m, about 5 min
Sato, 141 m, about 5 min
Banks, 145 m, about 5 min
Mercer, 446 m, about 10 min
Bravo · overwatchthe post
Bravo, moving to the post, overwatch.
Pike, 117 m, about 5 min
Tran, 113 m, about 4 min
Vega, 120 m, about 5 min
Okafor, 110 m, about 4 min
Alpha · moveroad at E4 · facing east
Alpha, moving to road at E4, facing east.
Novak, 34 m
Ortiz, 33 m
Doyle, 35 m
Duarte, 34 m
Not covered yetCharlie
wreck the radio and scopes. Saved so it can be built.
Play turn sends the ✓ orders and plays one minute. Type more to add to these or change them.
```

### Turn played

```
TIME: turn 9 of 40 (Turn 9 of 40 · 8:00)
RADIO NET SINCE YOUR LAST LOOK:
  [7:00] Lead: Charlie, you're still at D4, not at the post. Go to the post at F5 now and search it. Wreck the radio and scopes.
Bravo, go to the post at F5 too and cover Charlie.
Alpha, hold at the road at E4 and watch east.
  [7:00] Charlie: searching the post.
  [7:00] Bravo: moving to the post, overwatch.
  [7:00] Alpha: moving to road at E4, facing east.
  [7:03] Charlie: Contact on the drone, Kansian machine gunner at J8.
  [7:03] Drone spots PKM gunner 1 at J8.
  [7:04] Charlie: Contact on the drone, Kansian rifleman at J8.
  [7:04] Drone spots Observer 1 at J8.
  [7:32] Ruiz: Set at Hill 77.
  [7:34] Alpha: Set at road at E4.
OBJECTIVE STATUS: notyet | extras: A prisoner taken: notyet; Every Marine comes home alive: ok
YOUR PEOPLE:
  Novak (Team leader, Alpha) at E4: ok; doing: nothing; 192 rds
  Ortiz (Automatic rifleman, Alpha) at E4: ok; doing: nothing; 456 rds
  Doyle (Assistant automatic rifleman, Alpha) at E3: ok; doing: nothing; 384 rds
  Duarte (Rifleman, marksman, Alpha) at E4: ok; doing: nothing; 99 rds
  Ruiz (Squad leader, HQ) at C3: ok; doing: nothing; 210 rds
  Marsh (Corpsman, HQ) at C3: ok; doing: nothing; 150 rds
  Keller (Interpreter, HQ) at C3: ok; doing: nothing; 0 rds
  Pike (Team leader, Bravo) at E4: ok; doing: overwatch the post; 183 rds
  Tran (Automatic rifleman, Bravo) at E4: ok; doing: overwatch the post; 594 rds
  Vega (Assistant automatic rifleman, Bravo) at E4: ok; doing: overwatch the post; 408 rds
  Okafor (Rifleman, Carl Gustaf, Bravo) at E4: ok; doing: overwatch the post; 183 rds
  Boone (Team leader, Charlie) at E4: ok; doing: search the post; 210 rds
  Sato (Automatic rifleman, Charlie) at E4: ok; doing: search the post; 660 rds
  Banks (Assistant automatic rifleman, Charlie) at E4: ok; doing: search the post; 420 rds
  Mercer (Rifleman, drone operator, Charlie) at B4: ok; doing: search the post; 210 rds
  drone: up over G6
KNOWN CONTACTS:
  Squad leader 1 (op) at J10, seen now
  Observer 1 (op) at J10, seen now
  Team leader 1 (op) at F5, last seen 2 min ago
  PKM gunner 1 (op) at J9, seen now
  PKM gunner 2 (op) at F6, seen now, down
  RPG gunner 1 (op) at F5, seen now, down
  Rifleman 1 (op) at J10, seen now
  Rifleman 2 (op) at E5, seen now, down
  Rifleman 3 (op) at E5, seen now, down
MAP PICTURE: /tmp/claude-0/-home-user-Field-Table/58c019e3-dc8b-5709-9785-c655fc882daf/scratchpad/shots-3/map-009.png
```

