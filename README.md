# Image Functions #
A collection of various image processing functions

```
Usage ImageFunctions (action) [options]
 -h / --help                 Show full help
 (action) -h                 Action specific help
 --actions                   List possible actions

PixelateDetails [options] (input image) [output image]
 -p                          Use proportianally sized sections
 -s (number)[%]              Multiple or percent of image dimension used for splitting (default 2.0)
 -r (number)[%]              Count or percent or sections to re-split (default 50%)

Derivatives [options] (input image) [output image]
 -g                          Grayscale output
 -a                          Calculate absolute value difference
```

## TODO ##
### General ###
maybe pre-process input / output file since every function is going to need that
maybe update to process everything in rgba64 ?
look at paralellizing the processing functions

### PixelateDetails ###

### Derivatives ###
maybe add other types of derivatives