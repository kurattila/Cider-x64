<?php

function logToFile($msg)
{ 
	// open file
	$filename = "Cider-x64-Log.txt";
	$fd = fopen($filename, "a");
	// append date/time to message
	$str = "[" . date("Y/m/d H:i:s", mktime()) . "] " . $msg; 
	// write string
	fwrite($fd, $str . "\n");
	// close file
	fclose($fd);
}

	$computer = $_REQUEST['computer'];
	$version = $_REQUEST['version'];
	logToFile("Version check: [$version] $computer");
    echo "Newest Cider-x64 version: [1.0.0], published Jan 16, 2016";
?>