[in russian](README.ru.md)

# Downfall of Perception v0.7
This is a game, adventure in a strange collapsing world.
To some extent, this is an experiment with some mechanics, atmosphere, generation and linear story.

Although the game is completely passable to the end, I give it version 0.7, leaving space for content edits, logic and balance improvements.

You can download the assembled game [here](https://drive.google.com/file/d/1j7zAy8fXLOUsgPZcLH2C7GOkNd1oN9KV/view?usp=sharing)

### Things that I first touched while working on the project:
* Objecitves system. Location profiles.
* Flexible items.
* Cutscenes.
* Voice acting of characters.
* Localization.
* Generation of world geometry.

## Voxels
When I was unaware of the existence of VoxelTools, I wrote my own single threaded GDNative module. It worked well and I could have continued to use it, but I learned about VoxelTools and wanted to use terraforming. In the end, I did not use terraforming, but I liked VoxelTools for its multithreading. The only thing I had to edit the module was, I added a virtual function to VoxelInstancer to get the transformation of an object on the surface so that I could spawn my scenes, not multimesh instances. I will not publish the compiled editor.

## Errors
The mistakes that I made while working on the project.
They are easy to fix, I just have to take the time to do it. In order not to forget, I will describe them here.

### File system
* Inconsistent file naming. Due to the fact that I have not described the naming rules anywhere, I did not name them strictly.
* Inconsistent directory structure. The scenes of object scenes in the file system are randomly enough to get confused. Searching by file name solves this problem, but for cleanliness and organization it is better to arrange files so that it is intuitively clear where everything is.
### Code
* Hardcode. In location profiles and in some game objects, you can find literals that serve as an identifier for an event or achievement. This makes the code fragile.
* At the beginning of work on the game, I decided to write the logic of game objects in GDScript, and the base classes for them in C# (see ScriptedUsable and ScriptedWeapon classes). I just had a dream to create my own scripting language and code game objects with it like in doom or garrys mod, but in practice it turned out that this only complicates the work.
* There are some problems in architecture and object-orientation.
### Resources
* Weak cutscenes, it's just animation and sound. Some events, like screen fading, camera shake and loading another scene, I implemented in the location profiles code. It is necessary to come up with the design of events in the blender project and export them to scenes that inherit the Cutscene class, where all events and their timings will be assigned, which will be converted into frames on the animation tracks.
* When exporting obj models, materials are exported to SubResource, and when imported glb, godot creates separate SpatialMaterial resources. The exporter also should create separate files for the materials.
* Difficult pipeline for audio resources, constantly exporting parts of a project from fl studio is not convenient. Need to come up with something, I know there is software for sound designers, but first I'll try to write my own tool.

### Some video
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/FNDD5uT6_58/0.jpg)](https://www.youtube.com/watch?v=FNDD5uT6_58)