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
  * ```grep -iIr -A 2 "public static IEnumerable<string> GetImageNames" . | grep -iPo "new string\[\].*" > a.txt```

# TODO #
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