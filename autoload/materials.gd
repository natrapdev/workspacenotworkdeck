extends Node

var material_data: Dictionary = {
	# density is measured in grams per cubic centimetre (g/cm^3)
	"flesh": {
		"density": 0.985,
		"absorption": 0
	},
	"gambeson": {
		"density": 1.3, # Per layer - total should be calculated density * numberOfLayers
		"absorption": 0.7
	},
	"chainmail": {
		"density": 4,
		"absorption": 0.15
	},
	"iron": {
		"density": 7.874,
		"absorption": 0
	},
	"steel": {
		"density": 7.85,
		"absorption": 0
	}
}
