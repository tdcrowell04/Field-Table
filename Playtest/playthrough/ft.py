#!/usr/bin/env python3
"""Drives the playthrough harness. Prints what the player sees, and keeps a transcript for the AAR.

  ft.py start <run> <side> [seed] new random mission for a side (on a set map if a seed is given); prints the brief and the first view
  ft.py send <run> <text>         the player's traffic; writes the reader prompt to a file for the Haiku agent
  ft.py reader <run> <file>       hands the Haiku agent's answer to the page; prints the preview
  ft.py play <run>                plays the turn; prints the net, the squad, contacts, and the map picture
  ft.py view <run>                prints the current view without playing
  ft.py score <run> [turns]       the scoreboard from a run's transcript: per turn, the first read and the final one
"""
import json, sys, os, urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
PORT = int(os.environ.get('FT_PORT', '8802'))


def call(route, body=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request('http://localhost:%d/%s' % (PORT, route), data=data, method='POST' if data else 'GET')
    with urllib.request.urlopen(req, timeout=600) as r:
        return json.loads(r.read().decode())


def log(run, text):
    with open(os.path.join(HERE, 'transcript-%s.md' % run), 'a') as f:
        f.write(text + '\n')


def squad_lines(st):
    out = []
    for u in st['ours']:
        if u.get('name') == 'drone':
            out.append('  drone: up over ' + u['grid'])
            continue
        gone = (' [' + u['gone'] + ']') if u.get('gone') else ''
        out.append('  %s (%s, %s) at %s: %s%s; doing: %s; %s rds' % (u['name'], u['role'], u['team'], u['grid'], u['condition'], gone, u['doing'], u['ammo']))
    return out


def contact_lines(st):
    if not st['contacts']:
        return ['  none']
    return ['  %s (%s) at %s, %s%s' % (c['name'], c['side'], c['grid'], c['seen'], '' if c['condition'] in ('ok', None) else ', ' + c['condition']) for c in st['contacts']]


def view(st, net=None, banner='', mapfile=None, first=False):
    L = []
    if first:
        L += ['MISSION: %s (%s), playing as %s, map %s' % (st['title'], st['mission'], st['side'], st.get('seed')), 'BRIEF:'] + ['  ' + b for b in st['brief']]
        o = st['objectives']
        L += ['MAIN OBJECTIVE: ' + o['main'], 'WIN: ' + o['win'], 'LOSE: ' + o['lose'], 'ROE: ' + (o['roe'] or ''), 'EXTRAS: ' + '; '.join(o['secondary'])]
        m = st['map']
        L += ['MAP: columns %s, rows %s, each square %d m. Six-digit coordinates are meters east then meters north.' % (m['columns'], m['rows'], m['squareMeters']),
              'PLACES: ' + ', '.join(m['places'])]
        if m['controlLines']:
            L.append('LINES: ' + ', '.join(m['controlLines']))
        L.append('BUILDINGS: ' + ', '.join(m['buildings']))
    L.append('TIME: turn %s of %s (%s)' % (st['turn'], st['turnLimit'], st['clock']))
    if net is not None:
        L.append('RADIO NET SINCE YOUR LAST LOOK:')
        L += ['  [%s] %s%s' % (e['t'], (e['who'] + ': ') if e['who'] else '', e['text']) for e in net if e['kind'] != 'turn'] or ['  (quiet)']
    o = st['objectives']
    L.append('OBJECTIVE STATUS: %s | extras: %s' % (o['mainStatus'], '; '.join(o['secondary'])))
    L.append('YOUR PEOPLE:')
    L += squad_lines(st)
    L.append('KNOWN CONTACTS:')
    L += contact_lines(st)
    if st.get('casevac'):
        L.append('CASEVAC: ' + json.dumps(st['casevac']))
    if banner:
        L.append('MISSION OVER: ' + banner.replace('\n', ' | '))
    if mapfile:
        L.append('MAP PICTURE: ' + mapfile)
    return '\n'.join(L)


# ---------- the scoreboard ----------
# Read from the transcript's previews, so every run is scored the same way, run 1 included. For each turn it scores the
# first read of the player's traffic and the last one before the minute played (after any correction):
#   orders   orders the game master read
#   refused  orders the game turned down for someone ("Say again...", "Too far...", "No way...")
#   asked    the game master's own say agains
#   missing  asks it couldn't cover ("Not covered yet")
# and how many corrections the turn took.
import re
REFUSE = re.compile(r"Say again|Too far|No way|doesn't have|isn't answering|Can't walk|can't do", re.I)


def roster_from(text):
    teams = {}
    for m in re.finditer(r"^  (\w+) \((?:[^()]*), (\w+)\) at ", text, re.M):
        teams.setdefault(m.group(2), set()).add(m.group(1))
    return teams


def score_preview(p, teams):
    names = {n for t in teams.values() for n in t}
    out = {'orders': 0, 'refused': 0, 'asked': 0, 'missing': 0}
    who, members, bad, notcov = None, set(), False, False
    lines = p.split('\n')
    for i, line in enumerate(lines):
        line = line.strip()
        if not line or line.startswith('ORDERS READY') or line.startswith('Play turn'):
            continue
        if line.startswith('Not covered yet'):
            out['missing'] += 1
            notcov = True
            continue
        if notcov:
            notcov = False
            continue
        if ' · ' in line:
            if who is not None and bad:
                out['refused'] += 1
            who = line.split(' · ')[0].strip()
            members = teams.get(who, {who})
            if who in ('Doc',):
                members = {who}
            bad = False
            out['orders'] += 1
            continue
        m = re.match(r"^(\w[\w ]*?): (.*)$", line)
        if m and REFUSE.search(m.group(2)):
            speaker = m.group(1)
            if who is not None and (speaker in members or speaker == who) and not (speaker in teams and who != speaker):
                bad = True
            else:
                out['asked'] += 1
    if who is not None and bad:
        out['refused'] += 1
    return out


def score(run, upto):
    text = open(os.path.join(HERE, 'transcript-%s.md' % run)).read()
    teams = roster_from(text.split('## Start')[1].split('```')[1]) if '## Start' in text else {}
    turns = re.split(r'^### Turn played', text, flags=re.M)
    rows, tot = [], {'first': {}, 'final': {}, 'corrections': 0}
    for k, chunk in enumerate(turns[:-1] if len(turns) > 1 else turns):
        previews = re.findall(r'^Preview[^\n]*:\n```\n(.*?)\n```', chunk, re.M | re.S)
        sends = len(re.findall(r'^### Player traffic', chunk, re.M))
        if not previews:
            continue
        t = k + 1
        if t > upto:
            break
        first, final = score_preview(previews[0], teams), score_preview(previews[-1], teams)
        rows.append((t, first, final, sends - 1))
        for key in first:
            tot['first'][key] = tot['first'].get(key, 0) + first[key]
            tot['final'][key] = tot['final'].get(key, 0) + final[key]
        tot['corrections'] += sends - 1
    f = lambda d: '%2d orders, %2d refused, %2d asked, %2d missing' % (d['orders'], d['refused'], d['asked'], d['missing'])
    print('turn | first read                                  | final read                                  | corrections')
    for t, a, b, c in rows:
        print('%4d | %s | %s | %d' % (t, f(a), f(b), c))
    if rows:
        print('all  | %s | %s | %d' % (f(tot['first']), f(tot['final']), tot['corrections']))


def main():
    cmd, run = sys.argv[1], sys.argv[2]
    if cmd == 'start':
        r = call('start', {'side': sys.argv[3], 'seed': int(sys.argv[4]) if len(sys.argv) > 4 else None})
        text = view(r['state'], first=True, mapfile=r['map'])
        log(run, '## Start\n\n```\n' + text + '\n```\n')
        print(text)
    elif cmd == 'send':
        text = sys.argv[3]
        log(run, '### Player traffic (turn %s)\n\n> ' % call('state')['state']['turn'] + text.replace('\n', '\n> ') + '\n')
        r = call('send', {'text': text})
        if r.get('prompt'):
            n = len([f for f in os.listdir(HERE) if f.startswith('prompt-%s-' % run)]) + 1
            f = os.path.join(HERE, 'prompt-%s-%03d.txt' % (run, n))
            open(f, 'w').write(r['prompt'])
            print('PROMPT: ' + f)
        else:
            log(run, 'Preview (simple reader):\n```\n' + r.get('preview', '') + '\n```\n')
            print('PREVIEW (simple reader):\n' + r.get('preview', ''))
    elif cmd == 'reader':
        answer = open(sys.argv[3]).read()
        log(run, 'Game master (Haiku) read it as:\n```json\n' + answer.strip() + '\n```\n')
        r = call('reader', {'text': answer})
        log(run, 'Preview:\n```\n' + r.get('preview', '') + '\n```\n')
        print('PREVIEW:\n' + r.get('preview', ''))
        if r.get('errors'):
            print('PAGE ERRORS: ' + json.dumps(r['errors']))
    elif cmd == 'play':
        r = call('play')
        text = view(r['state'], net=r['net'], banner=r['banner'], mapfile=r['map'])
        log(run, '### Turn played\n\n```\n' + text + '\n```\n')
        pd = os.environ.get('PLAYER_DIR')
        if pd:
            f = os.path.join(pd, 'turn-%02d.txt' % int(r['state']['turn']))
            open(f, 'w').write(text + '\n')
            print('PLAYER FILE: ' + f)
        print(text)
        if r.get('errors'):
            print('PAGE ERRORS: ' + json.dumps(r['errors']))
    elif cmd == 'score':
        score(run, int(sys.argv[3]) if len(sys.argv) > 3 else 99)
    elif cmd == 'view':
        r = call('state')
        s = call('shot')
        print(view(r['state'], net=r['net'], mapfile=s['map']))


if __name__ == '__main__':
    main()
