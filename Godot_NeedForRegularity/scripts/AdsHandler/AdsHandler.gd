extends Node2D

onready var instanceId = self.get_instance_id() 
onready var canLoad = true;
#onready var timer = $Timer

func _ready():
	pass

func loadBanner():
	if canLoad:
		#print_debug("LOADING ADS")
		canLoad = false;
		applovin_max.loadBanner("103d719e82a36c48", true, instanceId)
		#timer.start();

func _on_banner_loaded(id):
	#print_debug("LOADED ADS")	
	applovin_max.showBanner(id);

func _on_banner_shown(id):
	#print_debug("SHOWNING ADS")
	yield(get_tree().create_timer(5),"timeout")
	#print_debug("REMOVE ADS")	
	applovin_max.removeBanner(id)
	canLoad = true;


#func _on_Timer_timeout():
	#timer.stop()
	#active = true
