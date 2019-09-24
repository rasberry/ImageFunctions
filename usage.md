# Usage #

```
Usage ImageFunctions (action) [options]
 -h / --help                 Show full help
 (action) -h                 Action specific help
 --actions                   List possible actions
 -# / --rect (x,y,w,h)       Apply function to given rectagular area (defaults to entire image)
 --max-threads (number)      Restrict parallel processing to a given number of threads (defaults to # of cores)

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
 --sampler (name)            Use given sampler (defaults to nearest pixel)
 --metric (name) [args]      Use alterntive distance function

4. AreaSmoother2 [options] (input image) [output image]
 Blends adjacent areas of flat color together by blending horizontal and vertical gradients
 -H                          Horizontal only
 -V                          Vertical only

5. ZoomBlur [options] (input image) [output image]
 Blends rays of pixels to produce a 'zoom' effect
 -z  (number)[%]             Zoom amount (default 1.1)
 -cc (number) (number)       Coordinates of zoom center in pixels
 -cp (number)[%] (number)[%] Coordinates of zoom center by proportion (default 50% 50%)
 -oh                         Only zoom horizontally
 -ov                         Only zoom vertically
 --sampler (name)            Use given sampler (defaults to nearest pixel)
 --metric (name) [args]      Use alterntive distance function

6. Swirl [options] (input image) [output image]
 Smears pixels in a circle around a point
 -cx (number) (number)       Swirl center X and Y coordinate in pixels
 -cp (number)[%] (number)[%] Swirl center X and Y coordinate proportionaly (default 50%,50%)
 -rx (number)                Swirl radius in pixels
 -rp (number)[%]             Swirl radius proportional to smallest image dimension (default 90%)
 -s  (number)[%]             Number of rotations (default 0.9)
 -ccw                        Rotate Counter-clockwise. (default is clockwise)
 --sampler (name)            Use given sampler (defaults to nearest pixel)
 --metric (name) [args]      Use alterntive distance function

7. Deform [options] (input image) [output image]
 Warps an image using a mapping function
 -cc (number) (number)       Coordinates of center in pixels
 -cp (number)[%] (number)[%] Coordinates of center by proportion (default 50% 50%)
 -e (number)                 (e) Power Exponent (default 2.0)
 -m (mode)                   Choose mode (default Polynomial)
 --sampler (name)            Use given sampler (defaults to nearest pixel)

 Available Modes
 1. Polynomial - x^e/w, y^e/h
 2. Inverted   - n/x, n/y; n = (x^e + y^e)

8. Encrypt [options] (input image) [output image]
 Encrypt or Decrypts all or parts of an image
 Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option
 -d                          Enable decryption (default is to encrypt)
 -p (text)                   Password used to encrypt / decrypt image
 -pi                         Ask for the password on the command prompt (instead of -p)
 -raw                        Treat password text as a raw string (shell escaping still applies)
 -iv (text)                  Initialization Vector - must be exactly 16 bytes
 -salt (text)                Encryption salt parameter - must be at least 8 bytes long
 -iter (number)              Number of RFC-2898 rounds to use (default 3119)
 -test                       Print out any specified (text) inputs as hex and exit

Available Samplers:
1. NearestNeighbor
2. Bicubic
3. Box
4. CatmullRom
5. Hermite
6. Lanczos2
7. Lanczos3
8. Lanczos5
9. Lanczos8
10. MitchellNetravali
11. Robidoux
12. RobidouxSharp
13. Spline
14. Triangle
15. Welch

Available Metrics:
1. Manhattan
2. Euclidean
3. Chebyshev
4. ChebyshevInv
5. Minkowski (p-factor)
6. Canberra
```