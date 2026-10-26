# Setting up Field Table on your PC

## What's in the repo

- `Sim/com.fieldtable.sim/`: the simulation (map, units, dice, clock, turns). Plain C# with no Unity code, packaged so Unity can load it.
- `Sim/FieldTable.Sim/` and `Sim/FieldTable.Sim.Tests/`: build and test the simulation without Unity.
- `Unity/`: the Unity 6 project. You create it once, with the steps below.

## Make the Unity project (one time)

1. Pull this branch so the `Field-Table` folder is up to date on your PC.
2. Open Unity Hub and click **New project**.
3. Pick a **Unity 6** editor and the **Universal 3D** template.
4. Set **Project name** to `Unity` and **Location** to your `Field-Table` folder, so the project lands in `Field-Table/Unity`. Click **Create project**.
5. When the editor opens, go to **Window > Package Manager**. Click the **+** button, then **Install package from disk**. Pick `Field-Table/Sim/com.fieldtable.sim/package.json`.
6. Check it worked: Package Manager lists **Field Table Simulation**, and the Console shows no red errors. Unity also installs Newtonsoft JSON on its own, since the simulation needs it.
7. Commit and push. Unity will have made `.meta` files inside `Sim/com.fieldtable.sim/`; commit those too.

If something goes wrong, copy the red Console errors to Claude.

## Run the simulation tests without Unity

Needs the .NET 8 SDK. From the `Sim` folder:

```
dotnet test
```
