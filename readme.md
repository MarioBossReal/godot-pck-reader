# PCK Reader

A simple PCK file parser for **Godot Engine 4.4**, available in both **GDScript** and **C#**. It reads the `.pck` header and file table, then returns metadata including directories and individual file entries with offsets and sizes.

---

## Features

- Parses PCK header (magic, version, flags)  
- Detects encrypted packs or corrupt entries via `is_valid` / `status`  
- Collects unique directories and full file entries  
- Lightweight, zero external dependencies  

---

## GDScript Usage

```gdscript
# Load and parse a PCK file
var data = PckReader.read("res://my_game.pck")
if data.is_valid:
    print("Format version:", data.format)
    print("Directories:", data.directories)
    for entry in data.files:
        print(entry.path, entry.offset, entry.size)
else:
    push_error("Failed to read PCK: %s" % data.status)
```

## CSharp Usage

```csharp

```