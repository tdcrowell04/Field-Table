# Field Table: notes for Claude

Field Table is Triston's game. Triston is the designer; Claude is the coder.

## How Triston works

- **Every decision needs Triston's accept or veto before anything else happens.** *"Any decisions you made MUST be answered by me with accept or veto BEFORE anything else happens No matter what I say."* And: *"Only do things that I made decisions on... I'll be the game designer you be the coder no decisions made autonomously by you."*
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

## The workspace

Claude's cloud workspace can't run Unity or Blender. Claude writes the code and Blender scripts; Triston runs them on his PC.
