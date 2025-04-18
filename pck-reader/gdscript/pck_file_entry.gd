extends RefCounted
class_name PckFileEntry

# One entry in the PCK file table.

var path: String
var offset: int
var size: int
var md5: PackedByteArray
var flags: int

func _init(path: String = "", offset: int = 0, size: int = 0, md5: PackedByteArray = PackedByteArray(), flags: int = 0) -> void:
	self.path = path
	self.offset = offset
	self.size = size
	self.md5 = md5
	self.flags = flags
