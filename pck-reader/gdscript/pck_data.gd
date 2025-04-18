extends RefCounted
class_name PckData

# Holds all parsed PCK metadata. If is_valid is false, status explains why.
enum ReadStatus {
	SUCCESS,
	BAD_MAGIC,
	BAD_FORMAT,
	ENCRYPTED,
	CORRUPT_ENTRY,
	LOAD_FAILED,
}

var is_valid: bool = false
var status: int = ReadStatus.LOAD_FAILED
var format: int = 0
var engine_version_major: int = 0
var engine_version_minor: int = 0
var engine_version_patch: int = 0
var encrypted: bool = false
var relative_file_base: bool = false
var file_base: int = 0
var directories: Array = []
var files: Array = []

static func new_failed(read_status: int) -> PckData:
	var d = PckData.new()
	d.is_valid = false
	d.status = read_status
	return d

static func new_success(
		p_format: int,
		p_ver_maj: int,
		p_ver_min: int,
		p_ver_pat: int,
		p_encrypted: bool,
		p_relative_fb: bool,
		p_file_base: int,
		p_dirs: Array,
		p_files: Array
	) -> PckData:
	var d = PckData.new()
	d.is_valid = true
	d.status = ReadStatus.SUCCESS
	d.format = p_format
	d.engine_version_major = p_ver_maj
	d.engine_version_minor = p_ver_min
	d.engine_version_patch = p_ver_pat
	d.encrypted = p_encrypted
	d.relative_file_base = p_relative_fb
	d.file_base = p_file_base
	d.directories = p_dirs
	d.files = p_files
	return d
