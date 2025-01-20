# Image Functions #
A collection of various image processing functions

## Pages ##
* [Usage](../../wiki/usage)
* [Examples](../../wiki/examples)
* [TODO](../../wiki/todo)

## Commands ##
* run project
  * ```dotnet run --project src --```
* test project
  * ```dotnet test```
  * ```dotnet test -l "console;verbosity=detailed" --filter "TestZoomBlur"```
* build wiki
  * ```dotnet run --project Writer```

## Notes ##
* find out which images tests are using
  ```bash
  #!/bin/bash
  function allNames {
  	grep -iIrh --include \*.cs -A 2 'GetImageNames' | \
  	grep -iPo 'new string\[\].*' | \
  	awk -F '[, ]'  '{for (i = 4; i <= NF - 1; i++) {printf "%s\n", $i};}' | \
  	tr -d '"'

  	ls ./Resources/images | \
  	awk -F '.' '{print $1}'
  }
  allNames | sort | uniq -ic | sort
  ```


# TODO #
<pre>
= move imagemagick to it's own plugin (to expose more imagemagick stuff)
= opencv (emgucv) plugin ?
= create a UI plugin
= create a gimp plugin
= add more tests
= create nuget packages
= add coverage report
= change canvas to another color space (edit in another color space)
= string art - https://www.youtube.com/watch?v=WGccIFf6MF8
</pre>

# Issues #
## GUI ##
<pre>
= when changing functions previously selected parameters stay and cannot be unselected
= when applying a function, loaded images don't seem to be affected ?
= cannot remove a layer
= no busy screen when function is running
= no progress bar while function is running
= no stop button to cancen a running function
= preview image cannot be zoomed or scrolled
= loaded image layer is named 'item' instead of being derived from file name
= save does not work
= no feedback for errors
= be able to specify an initial size for functions that create a layer
  = ? expose the global -# option
  = ? allow user to add empty layer and specify size
</pre>