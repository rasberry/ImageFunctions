# Image Functions #
A collection of various image processing functions

```
Usage ImageFunctions (action) [options]
 -h / --help                 Show full help
 (action) -h                 Action specific help
 --actions                   List possible actions
 -# / --rect (x,y,w,h)       Apply function to given rectagular area (defaults to entire image)

1. PixelateDetails [options] (input image) [output image]
 Creates areas of flat color by recusively splitting high detail chunks
 -p                          Use proportianally sized sections (default is square sized sections)
 -s (number)[%]              Multiple or percent of image dimension used for splitting (default 2.0)
 -r (number)[%]              Count or percent or sections to re-split (default 50%)

2. Derivatives [options] (input image) [output image]
 Computes the color change rate - similar to edge detection
 -g                          Grayscale output
 -a                          Calculate absolute value difference

3. AreaSmoother [options] (input image) [output image]
 Blends adjacent areas of flat color together by sampling the nearest two colors to the area
 -t (number)                 Number of times to run fit function (default 7)

4. AreaSmoother2 [options] (input image) [output image]
 Blends adjacent areas of flat color together by blending horizontal and vertical gradients
 -H                          Horizontal only
 -V                          Vertical only

5. ZoomBlur [options] (input image) [output image]
 Blends rays of pixels to produce a 'zoom' effect
 -z  (number)[%]              Zoom amount (default 1.1)
 -cc (number) (number)        Coordinates of zoom center in pixels
 -cp (number)[%] (number)[%]  Coordinates of zoom center by proportion (default 50% 50%)

```

## TODO ##
### General ###
* maybe update to process everything in rgba64 ?
  * looks like we need to wait for rc1 to do this
  * using SixLabors.ImageSharp.Color
* look at paralellizing the processing functions
* maybe color usage a little ?
* maybe add syntax to chain image functions together
  * possibly using '--' to seperate actions
  * need to figure out input/output file(s)
  * is there a need for multiple-in / multiple-out ?

### AreaSmoother ###
* create an areasmoother that samples surrounding pixels then weighted-averages them based on distance - similar to original but without picking the 'best' vector
* AS2 has an off-by-one error when using --rect option

### PixelateDetails ###

### Derivatives ###
* maybe add other types of derivatives