# TODO #
## General ##
* maybe color the 'usage' text a little ?
* maybe add syntax to chain image functions together
  * possibly using '--' to seperate actions
  * need to figure out input/output file(s)
  * is there a need for multiple-in / multiple-out ?
* samplers seem to have a n off-by-one issue (see dotnet run -- areasmoother salieri-bx.png -t 2 --max-threads 1 --sampler 2)
* use ```private static readonly ImageComparer ValidatorComparer = ImageComparer.TolerantPercentage(0.05f);``` instead of custom comparer for image tests

## Ideas ##
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
* take a look at https://en.wikipedia.org/wiki/Rose_(mathematics)
  * https://en.wikipedia.org/wiki/Spirograph
  * https://en.wikipedia.org/wiki/Maurer_rose
* spiral graphs
  * remove/keep prime number spots
  * make rotation distance configurable (best one is golden ratio)
* maze generator
  * http://www.neocomputer.org/projects/eller.html
  * prims
  * others.. there's lots
* fermats last theorem graph
  * plot result of x^2 + y^2 distance from whole number
  * add option to change exponent (2 is the only one that has answers to x^2+y^2=z^2)
  * add option to change starting point

## AreaSmoother ##
* create an areasmoother that samples surrounding pixels then weighted-averages them based on distance - similar to original but without picking the 'best' vector
* AS2 has an off-by-one error when using --rect option

## PixelateDetails ##
* Paralellize and add progress bar
* add a mode where borders inside of boxes are added
  * border color options [average, original, user specified color]
  * fill options [average, original, user specified color]
  * need to come up with color input parser
    * rgb 0-255    : "127,63,255"
    * rgb decimals : "0.5,0.25,1.0"
    * rgb percent  : "50%,25%,100%"
    * rgb Hex      : "#7F3FFF"
    * other color spaces ?
  * maybe include option to specify which boders to render ? (nesw) or (trbl)

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

## AllColors ##
* add arrangements
  * left to right / right to left
  * spiral in to out
* add option to invert sort order
