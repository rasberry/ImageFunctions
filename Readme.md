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
* build wiki
  * ```dotnet run --project Writer```

## Notes ##
* find out which images tests are using
  * ```grep -iIr -A 2 "public static IEnumerable<string> GetImageNames" . | grep -iPo "new string\[\].*" > a.txt```

# TODO #
https://www.youtube.com/watch?v=WGccIFf6MF8
