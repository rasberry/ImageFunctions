# TODO #
## General ##
* maybe update to process everything in rgba64 ?
  * looks like we need to wait for rc1 to do this
  * using SixLabors.ImageSharp.Color
  * for now change uses of rgba32 to Vector4 - Vector4 is float so
    it will preserve accuracy better than rgba32
* maybe color the 'usage' text a little ?
* maybe add syntax to chain image functions together
  * possibly using '--' to seperate actions
  * need to figure out input/output file(s)
  * is there a need for multiple-in / multiple-out ?
* samplers seem to have a n off-by-one issue (see dotnet run -- areasmoother salieri-bx.png -t 2 --max-threads 1 --sampler 2)
* add noise functions
  * perlin
  * opensimplex - https://gist.github.com/digitalshadow/134a3a02b67cecd72181
  * simplex ?
  * maybe add random points and then draw gradients between closest ones (use trie?)
    * might be able to do this little by little
      1. random points in an area
      1. add gradients
      1. save border points for next area
      1. repeat
* maybe do a tree ring expanding from random cener points
  * for each pixel
  * find nearest starting point (trie?) - dotnet add package KdTree
  * calculate distance to point
  * color pixel based on distance

## AreaSmoother ##
* create an areasmoother that samples surrounding pixels then weighted-averages them based on distance - similar to original but without picking the 'best' vector
* AS2 has an off-by-one error when using --rect option

## PixelateDetails ##
* Paralellize and add progress bar

## Derivatives ##
* Paralellize and add progress bar
* maybe add other types of derivatives

## ZoomBlur ##
* add a vertial / horizontal only zoom blur (curtain blur ?)

## Swirl ##
* is it possible to do a outer swirl (outside of radius towards edge instead of inwards?)

## Encryption ##
* add option to change key size? supposedly options are (128,192,256)

## Deform ##
* maybe implement [fisheye](https://stackoverflow.com/questions/2477774/correcting-fisheye-distortion-programmatically)
