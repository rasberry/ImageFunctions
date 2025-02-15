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
= create a gimp plugin
= add more tests
= add coverage report
= change canvas to another color space (edit in another color space)
= string art - https://www.youtube.com/watch?v=WGccIFf6MF8
</pre>

# Issues #
## GUI ##
<pre>
= preview image cannot be zoomed or scrolled
  = https://github.com/sn4k3/UVtools/blob/master/UVtools.AvaloniaControls/AdvancedImageBox.cs
  = https://github.com/cyotek/Cyotek.Windows.Forms.ImageBox/tree/master/Cyotek.Windows.Forms.ImageBox
= loaded image layer is named 'item' instead of being derived from file name
= save image does not work
= when removing the last layer, the down button does not gray out on the (now) last item
= fix unusual inputs
  = allcolors option -l / --legacy should pick just '-l'
  = ulamspiral -c(1,2,3,4) - might need to re-work this
= make usage description stand out a little - maybe a border under the text ?
= finish adding controls
  = color picker
  = point selector
= selecting dropdown from speargraphic causes -g paramter to mysteriously appear for other functions
= some function job are not finishing
= consider breaking up MainWindowViewModel .. it's getting a bit big
= maybe use https://docs.avaloniaui.net/docs/concepts/reactiveui/data-persistence instead of custom options persistance
= look at replacing TextBlockSelectedReg with a binding

X maybe capture log messages - probably need to create a log provider
  = maybe this ? https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line
  = tried https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator
  = but was too inflexible. not sure how to share messages between logs and exceptions
X maybe remove the Debug method from Log and switch to using Trace.Writeline instead
  = sounds like trace is soft-deprecated - https://learn.microsoft.com/en-us/dotnet/core/diagnostics/logging-tracing
</pre>