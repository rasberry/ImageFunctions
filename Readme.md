# Image Functions #
A collection of various image processing functions

## Pages ##
* [Usage](../../wiki/usage)
* [Examples](../../wiki/examples)
* [TODO](../../wiki/todo)

## Commands ##
* run project
  * ```dotnet run -p src --```
* test project
  * ```dotnet test```
* build wiki
  * Note: Only do this if all tests pass. Buiding the wiki takes a long time.
    ```
    set BUILDWIKI=1
    dotnet test --filter TestBuildWiki
    set BUILDWIKI=0
    ```

## Notes ##
* pull/push from wiki
  * ```git subtree pull --prefix wiki wiki master```
  * ```git subtree push --prefix wiki wiki master```