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
= review https://github.com/DarthAffe/HPPH

https://www.youtube.com/watch?v=WGccIFf6MF8
= move imagemagick to it's own plugin (to expose more imagemagick stuff)
= opencv (emgucv) plugin ?
= create a UI plugin
= create a gimp plugin
= add more tests
= create nuget packages
= add coverage report
= change canvas to another color space (edit in another color space)
=
</pre>