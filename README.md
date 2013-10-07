# Terrible EVE IGB Gate/JB Router Finder

![terrible screenshot](http://i.imgur.com/sJ30v09.png)

Enter system name, press button.

Tiny C# program linked against GARPA Topographical Survey DLLs plots a route
from your current system (needs trust enabled) to the destination.

Terrible JavaScript program squeezes route into a list with neat brackets for
gates or jump bridges.

Autopilot route is set to the next jump bridge on the route, if any, or the
destination otherwise.

Only works if you have GTS installed and the DLLs around and taught GTS about
your jump bridge network etc. GTS doesn't have to be running, though. Listens
on port 23455.
