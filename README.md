# NeedForRegularity

Second attempt at game development using the [Godot engine](https://godotengine.org/) with C#. Another abstract/clean/simple 2D-game without great expectations. 
Another much needed learnirng project, building on the (little) knowlegde acquired in the making of [StickyBlokcs](https://github.com/NeurodivergentGames/StickyBlocks) (give it a try: [StickyBlocks in itchio](https://neurodivergentgames.itch.io/stickyblocks)).

## Game objective

Reorganize some chaotically placed geometric objects at will following your inner NeedForRegularity. You can 
choose to reorganize randomally placed: lines, squares, dots and disk. There is no specific goal: create your pattern/order, lower the entropy
and (maybe?) save the configurations created. You can also choose to use a background (faded) pattern to inspire and facilitate your need for regularity.
## My goals:

* Build and complete another little abstract game (completely on my own)
* Learning new Godot specific stuff, for example: better use of collision layer, multi-resolution support, touchscreen inputs support, Android export, place annoying ads in game...
* Learing new C# stuff, I will try to use more C# specific features even if not necessary or more complex: interfaces, delegates, events(when to use events instead of Godot's signals?), "when to use inheritance?", namespaces, setters...
* Try to write more SOLID code, more reusable, more generic (...not high hopes about these...)
* Try a different (from StickyBlocks) game structure planned in UML: using more state machine pattern, some globals (when makes sense), some specific managers classes (inputs manager, UI manager, main manager,...)     
* Put more faith in Goodt and less in my maths skills (I studied theoretical physics): Before going into the rabbit hole of mathematics, try to solve problems using Godot's features! 
* Create a base game mechanics (select/drag/drop/rotate/scale) to be reused in the future in different games (for example in a abstract or also not abstract puzzle game).
## Progress updates

### 23/02/2022

* Created and setup the Godot project: 2d/expand/portrait mode
* Created and connected the repository to VSCoce
* Planned the base structure of different classes/nodes in UML
* Created folders and files structure for the base objects
* Created the `Main` node structure it will contain: list of objects, UI stuff, some manager nodes  
* Created some base interfaces for different kind of geometrical object: `ISelectable`, `IMovable`, `IRotatable`, `IScalable`.
* Created `BaseObject` node: a `KinematicBody2D` with a to-be-defined `CollisionShape2D`
* Started to implemented `BaseObject` class
* Started to implemented `Main`: game machine logic and selection input handler
* Created a `Globals` namespace: `GAMESTATE` and `OBJECTSTATE` enums, `SCREENINFO` struct, `RandomManager` struct

### 26/02/2022
* Created `LevelBarriers` node and class: limits the game area, responsive to `Viewport.size_changed` signal 
* All inputs are handled in `Main` in a main `_UnhandleInput` function that first handles in a generic way the selection/unselection of object and then calls 
  `InputControlFlow` function of the selected object 
* Create `RotatableObject` node: inherited scene of `BaseObject` with a second `KinematicBody2D` to be pressed to control the rotation   
* Created `RotatableObject` as child of `BaseObject` and as `IRotatable`: implemented the `InputControlFlow` function to handle motion/rotation → basically 
  I recreated the selection/unselection/move/rotate functionalities of Godot's editor (or any 2d design software) but with the challening interactions beetwen objects's 
  motion/rotation and `LevelBarriers` constraints
* Implemented a `_overlapple` variable for the objects (modifies with code the collision layer/mask)
* Created nodes and implemented classes of: `SquareObject`, `DotObject`, `DiskObject`  