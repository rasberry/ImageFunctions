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
  * Only do this if all tests pass.
  * Building the wiki may take a long time.
  * To regenerate test images include the parameter "--images"
  ```
  dotnet run --project test -- --images
  ```

## Notes ##
* pull/push from wiki
  * ```git subtree pull --prefix wiki wiki master```
  * ```git subtree push --prefix wiki wiki master```
