#!/usr/bin/env python3
"""Drives the playthrough harness. Prints what the player sees, and keeps a transcript for the AAR.

  ft.py start <run> <side>        new random mission for a side; prints the brief and the first view
  ft.py send <run> <text>         the player's traffic; writes the reader prompt to a file for the Haiku agent
  ft.py reader <run> <file>       hands the Haiku agent's answer to the page; prints the preview
  ft.py play <run>                plays the turn; prints the net, the squad, contacts, and the map picture
  ft.py view <run>                prints the current view without playing
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
        L += ['MISSION: %s (%s), playing as %s' % (st['title'], st['mission'], st['side']), 'BRIEF:'] + ['  ' + b for b in st['brief']]
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


def main():
    cmd, run = sys.argv[1], sys.argv[2]
    if cmd == 'start':
        r = call('start', {'side': sys.argv[3]})
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
    elif cmd == 'view':
        r = call('state')
        s = call('shot')
        print(view(r['state'], net=r['net'], mapfile=s['map']))


if __name__ == '__main__':
    main()
