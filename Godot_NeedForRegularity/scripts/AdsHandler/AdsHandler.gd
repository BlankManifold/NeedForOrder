extends Node2D

onready var instanceId = self.get_instance_id() 
onready var running = false;

func _ready():
	pass

func loadBanner():
	if not running:
		running = true;
		applovin_max.loadBanner("8117f892ef766524", true, instanceId)

func _on_banner_loaded(id):
	applovin_max.showBanner(id);

func _on_banner_shown(id):
	yield(get_tree().create_timer(5),"timeout")
	applovin_max.removeBanner(id)
	running = false;
