# Godot PCK Reader

A simple PCK file parser for **Godot Engine 4.4**, available in both **GDScript** and **C#**.
It reads the `.pck` header and file table, then returns metadata including directories and individual file entries with offsets and sizes.

---

## Use Case

The primary use case for this tool is for validating the directory structure of a PCK before mounting it to your running game.
Even if you never manually load external Godot Resource files, mounting PCKs to your game carries the inherent risk that a
core resource file could be overridden with a malicious resource.

## Example

Suppose you have a PCK "mymod.pck" that you wish to load into your game. 
Here are the entries of the mod PCK:

```text
mods/
└── mymod/
    └── mod_data.json
game/
└── prefabs/
    └── crate.tscn   ← core game resource that will be overridden
```

Your mod mounting code may only intentionally load the safe .json file, but unknown to you a core game resource has been overridden by an external resource, which could be holding a malicious payload.
At some point, your game will load the crate.tscn prefab and possibly execute arbitrary code injected by a malicious mod maker.

## Solution

The solution here is to structure your game so that all mods are required to have their files within a unique and defined base directory.
For example, a mod named "mymod.pck" must not have any directory that does not begin with "mods/mymod/". This way, even if there are malicious resource files
embedded within the PCK, they will never override any core game files and thus your application will never unintentionally load a resource that comes from an unexpected directory.

---

## GDScript Usage Example

```gdscript

var pck_path = "user://mods/mymod.pck"
var data = PckReader.read(pck_path)
if not data.is_valid:
    print("Failed to read mod PCK: %s" % data.status)
    return

var mod_name = pck_path.get_file().get_basename() # "mymod"
var base_dir = "mods/%s/" % mod_name

for dir in data.directories:
    if not dir.begins_with(base_dir):
        print("Invalid directory in mod PCK: %s" % dir)
        return

print("Mod PCK is valid.")
# Code to mount PCK
# ...
```

## CSharp Usage

```csharp
var pckPath = "user://mods/mymod.pck";
var data = PckReader.Read(pckPath);

if (!data.IsValid)
{
    GD.Print($"Failed to read mod PCK: {data.Status}");
    return;
}

var modName = Path.GetFileNameWithoutExtension(pckPath);
var baseDir = $"mods/{modName}/"

foreach (var dir in data.Directories)
{
    if (!dir.StartsWith(baseDir)
    {
        GD.Print($"Invalid directory in mod PCK: {dir}";
        return;     
    }
}

GD.Print("Mod PCK is valid.");
// Code to mount PCK
// ...
```
