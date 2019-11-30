# Image Functions #
A collection of various image processing functions

## Pages ##
* [Usage](usage)
* [Examples](examples)
* [TODO](todo)

## Commands ##
* run project
  * ```dotnet run -p src --```
* test project
  * ```dotnet test -d n```
* build wiki
  * Notes:
    * Only do this if all tests pass.
    * Buiding the wiki may take a long time.
    * To regenerate test images you must remove wiki/img/img-*
    ```
    set BUILDWIKI=1
    dotnet test --filter TestBuildWiki
    set BUILDWIKI=0
    ```

## Notes ##
* pull/push from wiki
  * ```git subtree pull --prefix wiki wiki master```
  * ```git subtree push --prefix wiki wiki master```
