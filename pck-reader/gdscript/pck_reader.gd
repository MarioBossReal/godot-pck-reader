extends RefCounted
class_name PckReader

# Static entry point for reading a .pck and producing a PckData
const PACK_HEADER_MAGIC    = 0x43504447 #"GDPC"
const PACK_FORMAT_VERSION  = 2
const PACK_DIR_ENCRYPTED   = 1 << 0
const PACK_REL_FILEBASE    = 1 << 1

# Parses the pck at pck_path
static func read(pck_path: String) -> PckData:
	var file = FileAccess.open(pck_path, FileAccess.ModeFlags.READ)
	if file == null:
		return PckData.new_failed(PckData.ReadStatus.LOAD_FAILED)

	var dirs_dict := {}
	var files_arr := []

	# --- Header ---
	if file.get_32() != PACK_HEADER_MAGIC:
		return PckData.new_failed(PckData.ReadStatus.BAD_MAGIC)

	var format = file.get_32()
	if format != PACK_FORMAT_VERSION:
		return PckData.new_failed(PckData.ReadStatus.BAD_FORMAT)

	var ver_maj = file.get_32()
	var ver_min = file.get_32()
	var ver_pat = file.get_32()

	var pack_flags = file.get_32()
	var enc_dirs = (pack_flags & PACK_DIR_ENCRYPTED) != 0
	var rel_fb   = (pack_flags & PACK_REL_FILEBASE) != 0

	var file_base = file.get_64()

	# skip reserved (16 × 4 bytes)
	for i in range(16):
		file.get_32()

	var file_count = file.get_32()
	if enc_dirs:
		return PckData.new_failed(PckData.ReadStatus.ENCRYPTED)

	# --- Entries ---
	for i in range(file_count):
		var name_len = file.get_32()
		if name_len == 0:
			return PckData.new_failed(PckData.ReadStatus.CORRUPT_ENTRY)

		var name_bytes = file.get_buffer(name_len)
		var path = name_bytes.get_string_from_utf8()

		# collect directory
		var slash_idx = path.rfind("/")
		if slash_idx >= 0:
			dirs_dict[path.substr(0, slash_idx)] = true

		var offset = file.get_64()
		var size = file.get_64()
		var md5 = file.get_buffer(16)
		var flags_entry = file.get_32()

		var entry = PckFileEntry.new(path, offset, size, md5, flags_entry)
		files_arr.append(entry)

	# Build final lists
	var dirs_array = dirs_dict.keys()
	return PckData.new_success(format, ver_maj, ver_min, ver_pat,
		enc_dirs, rel_fb, file_base,
		dirs_array, files_arr)
