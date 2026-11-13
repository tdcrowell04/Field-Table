# Field Table: notes for Claude

Field Table is Triston's game. Triston is the designer; Claude is the coder.

## How Triston works

- **We work like a game studio (2026-11-13; replaces the old "every decision needs accept or veto" rule).** *"Actually what we're going to do is I'm going to treat us like an actual game studio. I don't need to micromanage and rule on every tiny little puny thing. I'm going to have you tell me if you understand what I'm looking for and have you make minor decisions and then check with me to see what I think"*
  - Claude makes the minor calls and builds, then shows Triston what it did and why, so he can keep it or change it.
  - Big calls still go to Triston before they're built: what the game is, how it feels to play, new missions, sides, or systems, anything that changes a ruling in the bible, and anything that costs money.
  - Minor calls Claude just makes: bug fixes, tuning inside a ruling, wording, how something is coded, and test setup.
- **Every set of decisions goes in an updated ledger.** *"I always want an updated ledger ok"*
  - The ledger is one artifact: https://claude.ai/artifact/Bx6onmgSEqG7G1cXZr3qE6
  - Each set of decisions is a new round on that same link. Earlier rounds stay as read-only tabs.
  - Answers save to the page's database, one collection per round (`answers` for round 1, then `round2`, `round3`, and so on). Read them with the ArtifactData tool when Triston says he's done.
  - The page's source isn't in the repo (Triston's ruling). In a new session, read it back with the Artifact tool's `read` action before republishing.
- **Keep the asking short.** No re-asking what's already ruled. Conduits is the main build; Field Table is what Triston works on in the meantime. Don't ask about that again.
- **Plain English.** Short, clear, no jargon. American spelling. Never use the phrase "barney style" or the word "grown".
- **Keep Triston's words verbatim** when recording rulings.
- **Correct yourself out loud** if you find you told Triston something wrong.
- **Record every ruling in `bible.md`,** and take out ideas that were overruled. *"Also remove any mention of things that are overruled."*
- **Live AI is allowed in Field Table.** Conduits' no-AI-assets rule doesn't apply here.

## Where things are

- `bible.md`: the game bible. The current word on every ruling.
- `squad-slice-plan.md`: the build plan for step 1, the squad slice.
- `field-table-handoff.md`: the handoff from the session before this repo existed. Kept as a record; don't edit it.
- `field-table-figures.html`: the figures page. Published at https://claude.ai/artifact/Hbqx4vtEoRbHamyprSCqEW
- `playthroughs.md`: what the agent playthroughs taught us, the proposed test changes, and where to pick up. The harness and run records are in `Playtest/playthrough/`.
- `Playtest/`: the phone playtest page. `build.sh` builds it into `Playtest/out/`; publish `out/playtest.html` with everything in `out/` as its files, to https://claude.ai/artifact/A9AH1SGi5o5Rt2Uoe4bu9y

## The workspace

Claude's cloud workspace can't run Unity or Blender. Claude writes the code and Blender scripts; Triston runs them on his PC.
