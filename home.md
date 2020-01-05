# Image Functions #
A collection of various image processing functions

## Commands ##
* run project
  * ```dotnet run -p src --```
* test project
  * ```dotnet test -d n```
* build wiki
  * Only do this if all tests pass.
  * Buiding the wiki may take a long time.
  * To regenerate test images you must remove wiki/img/img-*
  * **Windows**
  ```
  set BUILDWIKI=1
  erase "wiki\img\img-*.png"
  dotnet test --filter TestBuildWiki
  set BUILDWIKI=0
  ```
  * **Linux**
  ```
  rm wiki/img/img-*.png
  BUILDWIKI=1 dotnet test --filter TestBuildWiki
  ```

## Notes ##
* pull/push from wiki
  * ```git subtree pull --prefix wiki wiki master```
  * ```git subtree push --prefix wiki wiki master```
