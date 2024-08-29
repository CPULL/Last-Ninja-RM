@tool
extends EditorPlugin

const editorAddon = preload("RoomManagerUI.tscn")

func _enter_tree():
	add_tool_menu_item("Generate Room", Callable(self, "Generate"))
	add_tool_menu_item("Load Room", Callable(self, "FromCode"))

func _exit_tree():
	remove_tool_menu_item("Generate Room")
	remove_tool_menu_item("Load Room")


func Generate():
	var room = EditorInterface.get_edited_scene_root().get_node("Room")
	var res : String = ""
	for item : Node3D in room.get_children():
		var n = "";
		if (item.editor_description.length() > 1):
			n += item.editor_description + ", ";
		else:
			n += item.name +", "
		while n.length() < 15:
			n += " "
		res += "      new(PartType."
		
		n += ("%7.3f" % item.position.x) + "f, "
		n += ("%7.3f" % item.position.y) + "f, "
		n += ("%7.3f" % item.position.z) + "f,    "
		while n.length() < 56:
			n += " "

		n += ("%4.2f" % item.scale.x) + "f, "
		n += ("%4.2f" % item.scale.y) + "f, "
		n += ("%4.2f" % item.scale.z) + "f,    "
		while n.length() < 84:
			n += " "
		
		n += ("%d" % item.rotation_degrees.x) + ", "
		n += ("%d" % item.rotation_degrees.y) + ", "
		n += ("%d" % item.rotation_degrees.z) + "    "
		while n.length() < 106:
			n += " "
		
		# res += n + "1, 1, 1),\n" # color, it is not used
		res += n + "),\n" # color, it is not used
	print(res)

func FromCode():
	var room = EditorInterface.get_edited_scene_root().get_node("Room")
	room.ReBuildRoom()
