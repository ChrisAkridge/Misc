function calc() {
	var input = document.getElementsByName("hours")[0];
	var hours = input.value;
	var cost = 10;
	var result = 0;
	
	while (hours > 0) {
		if (hours - cost < 0) { break; }
		hours -= cost;
		result++;
		cost *= 1.01;
	}
	
	document.getElementById("result").innerHTML = "Heavenly Points: " + result;
}