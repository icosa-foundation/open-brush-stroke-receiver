## Open Brush Stroke Receiver

A sample Unity app showing how to receive and render real time stroke data from Open Brush

You currently need to use a build that enables the experimental API

https://docs.openbrush.app/alternate-and-experimental-builds/experimental-builds/open-brush-api

So either use that specific feature build: https://docs.openbrush.app/alternate-and-experimental-builds/experimental-builds/downloads#open-brush-api

or the "All in one" build: https://docs.openbrush.app/alternate-and-experimental-builds/experimental-builds/downloads#all-in-one

Installation
------------

Either download the whole repo and open as a Unity Project or add the package to your own project using UPM. Edit your project's ./Packages/manifest.json file:

"com.icosa.strokereceiver": "https://github.com/IxxyXR/open-brush-stroke-receiver.git#upm"

Usage
-----

See the example scene or the supplied prefabs.

To test it out run an API build of Open Brush and then run the Line Renderer scene from the example Unity project on the same PC.

The Unity project will register with Open Brush as a "stroke listener". Every stroke you draw will update the line renderer including color and pressure.