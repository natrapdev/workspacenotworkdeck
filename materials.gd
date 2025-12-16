extends Node

var material_data: Dictionary = {
	# density is measured in grams per cubic centimetre (g/cm^3)
	"skin": {
		"density": 1.1
	},
	"gambeson": {
		"density": 1.3, # Per layer - total should be calculated density * numberOfLayers
	},
	"chainmail": {
		"density": 4,
	},
	"iron": {
		"density": 7.874
	},
	"steel": {
		"density": 7.85
	}
	
}
