extends Node2D

onready var grid = $Grid
onready var cam = $Camera2D
onready var STEP = $BoidController.GridStepSize
onready var size = get_viewport().get_visible_rect().size

func _ready():
	_create_vertical()

func _create_vertical():
	for i in range(0, 500):
		var vert := Line2D.new()
		var hor := Line2D.new()
		var v1 := Vector2(-size.x / 2 + STEP*i, -size.y)
		var v2 := Vector2(-size.x / 2 + STEP*i, size.y)
		var h1 := Vector2(-size.x, -size.y / 2 + STEP*i)
		var h2 := Vector2(size.x, -size.y / 2 + STEP*i)
		vert.add_point(v1)
		vert.add_point(v2)
		hor.add_point(h1)
		hor.add_point(h2)
		vert.width = 1
		hor.width = 1
		vert.default_color = Color8(75, 0, 150)
		hor.default_color = Color8(75, 0, 150)
		grid.add_child(vert)
		grid.add_child(hor)
