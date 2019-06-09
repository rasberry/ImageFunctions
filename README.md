# Image Functions #
A collection of various image processing functions

```
Usage ImageFunctions (action) [options]
 -h / --help                 Show full help
 (action) -h                 Action specific help
 --actions                   List possible actions

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
```

## TODO ##
### General ###
* maybe pre-process input / output file since every function is going to need that
* maybe update to process everything in rgba64 ?
* look at paralellizing the processing functions

### AreaSmoother ###
* create an areasmoother that samples surrounding pixels then weighted-averages them based on distance - similar to original but without picking the 'best' vector

### PixelateDetails ###

### Derivatives ###
* maybe add other types of derivatives