# Usage #

```
Usage ImageFunctions (action) [options]
 -h / --help                  Show full help
 (action) -h                  Action specific help
 -# / --rect ([x,y,]w,h)      Apply function to given rectagular area (defaults to entire image)
 --format (name)              Save any output files as specified format
 --max-threads (number)       Restrict parallel processing to a given number of threads (defaults to # of cores)
 --engine (name)              Select image engine (default SixLabors)
 --actions                    List possible actions
 --colors                     List available colors
 --formats                    List output formats

Available Engines:
 1. ImageMagick               
 2. SixLabors                 

1. PixelateDetails [options] (input image) [output image]
 Creates areas of flat color by recusively splitting high detail chunks
 -p                           Use proportianally sized sections (default is square sized sections)
 -s (number)[%]               Multiple or percent of image dimension used for splitting (default 2.0)
 -r (number)[%]               Count or percent or sections to re-split (default 50%)

2. Derivatives [options] (input image) [output image]
 Computes the color change rate - similar to edge detection
 -g                           Grayscale output
 -a                           Calculate absolute value difference

3. AreaSmoother [options] (input image) [output image]
 Blends adjacent areas of flat color together by sampling the nearest two colors to the area
 -t (number)                  Number of times to run fit function (default 7)
 --sampler (name)             Use given sampler (defaults to nearest pixel)
 --metric (name) [args]       Use alterntive distance function

4. AreaSmoother2 [options] (input image) [output image]
 Blends adjacent areas of flat color together by blending horizontal and vertical gradients
 -H                           Horizontal only
 -V                           Vertical only

5. ZoomBlur [options] (input image) [output image]
 Blends rays of pixels to produce a 'zoom' effect
 -z  (number)[%]              Zoom amount (default 1.1)
 -cc (number) (number)        Coordinates of zoom center in pixels
 -cp (number)[%] (number)[%]  Coordinates of zoom center by proportion (default 50% 50%)
 --sampler (name)             Use given sampler (defaults to nearest pixel)
 --metric (name) [args]       Use alterntive distance function

6. Swirl [options] (input image) [output image]
 Smears pixels in a circle around a point
 -cx (number) (number)        Swirl center X and Y coordinate in pixels
 -cp (number)[%] (number)[%]  Swirl center X and Y coordinate proportionaly (default 50%,50%)
 -rx (number)                 Swirl radius in pixels
 -rp (number)[%]              Swirl radius proportional to smallest image dimension (default 90%)
 -s  (number)[%]              Number of rotations (default 0.9)
 -ccw                         Rotate Counter-clockwise. (default is clockwise)
 --sampler (name)             Use given sampler (defaults to nearest pixel)
 --metric (name) [args]       Use alterntive distance function

7. Deform [options] (input image) [output image]
 Warps an image using a mapping function
 -cc (number) (number)        Coordinates of center in pixels
 -cp (number)[%] (number)[%]  Coordinates of center by proportion (default 50% 50%)
 -e (number)                  (e) Power Exponent (default 2.0)
 -m (mode)                    Choose mode (default Polynomial)
 --sampler (name)             Use given sampler (defaults to nearest pixel)

 Available Modes
 1. Polynomial                x^e/w, y^e/h
 2. Inverted                  n/x, n/y; n = (x^e + y^e)

8. Encrypt [options] (input image) [output image]
 Encrypt or Decrypts all or parts of an image
 Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option
 -d                           Enable decryption (default is to encrypt)
 -p (text)                    Password used to encrypt / decrypt image
 -pi                          Ask for the password on the command prompt (instead of -p)
 -raw                         Treat password text as a raw string (shell escaping still applies)
 -iv (text)                   Initialization Vector - must be exactly 16 bytes
 -salt (text)                 Encryption salt parameter - must be at least 8 bytes long
 -iter (number)               Number of RFC-2898 rounds to use (default 3119)
 -test                        Print out any specified (text) inputs as hex and exit

9. PixelRules [options] (input image) [output image]
 Average a set of pixels by following a minimaztion function
 -m (mode)                    Which mode to use (default StairCaseDescend)
 -n (number)                  Number of times to apply operation (default 1)
 -x (number)                  Maximum number of iterations - in case of infinte loops (default 100)
 --sampler (name)             Use given sampler (defaults to nearest pixel)
 --metric (name) [args]       Use alterntive distance function

 Available Modes
 1. StairCaseDescend          move towards smallest distance
 2. StairCaseAscend           move towards largest distance
 3. StairCaseClosest          move towards closest distance
 4. StairCaseFarthest         move towards farthest distance

10. ImgDiff [options] (image one) (image two) [output image]
 Highlights differences between two images.
 By default differeces are hilighted based on distance ranging from hilight color to white
 -o (number)[%]               Overlay hilight color at given opacity
 -i                           Match identical pixels instead of differences
 -x                           Output original pixels instead of hilighting them
 -c (color)                   Change hilight color (default is magenta)

11. AllColors [options] [output image]
 Creates an image with every possible 24-bit color ordered by chosen pattern.
 -p (pattern)                 Sort by Pattern (default BitOrder)
 -s (space)                   Sort by color space components (instead of pattern)
 -so (n,...)                  Change priority order of components (default 1,2,3,4)
 -np                          Use single threaded sort function instead of parallel sort

 Available Patterns
 1. BitOrder                  Numeric order
 2. AERT                      AERT brightness
 3. HSP                       HSP color model brightness
 4. WCAG2                     WCAG2 relative luminance
 5. SMPTE240M                 Luminance SMPTE 240M (1999)
 6. Luminance709              Luminance BT.709
 7. Luminance601              Luminance BT.601
 8. Luminance2020             Luminance BT.2020

 Available Spaces
 1. RGB                       
 2. HSV                       
 3. HSL                       
 4. HSI                       
 5. YCbCr                     
 6. CieXyz                    
 7. Cmyk                      

12. SpearGraphic [options] [output image]
 Creates a spear graphic
 -g (name)                    Choose which graphic to create
 -bg (color)                  Change Background color (default transparent)
 -rs (number)                 Random Int32 seed value (defaults to system picked)

 Available Graphics
 1. First_Twist1              
 2. First_Twist2              
 3. First_Twist3              
 4. Second_Twist3a            
 5. Second_Twist3b            
 6. Second_Twist3c            
 7. Second_Twist4             
 8. Third                     
 9. Fourth                    

14. UlamSpiral [options] [output image]
 Creates an Ulam spiral graphic 
 -p                           Color pixel if prime (true if -f not specified)
 -f                           Color pixel based on number of divisors; dot size is proportional to divisor count
 -6m                          Color primes depending on if they are 6*m+1 or 6*m-1
 -c (x,y)                     Center x,y coordinate (default 0,0)
 -m (mapping)                 Mapping used to translate x,y into an index number (default spiral)
 -s (number)                  Spacing between points (default 1)
 -ds (number)                 Maximum dot size in pixels; decimals allowed (default 1.0)
 -dt (dot type)               Dot used for drawing (default circle)
 -c(1,2,3,4) (color)          Colors to be used depending on mode. (setting any of the colors is optional)

 Color Mappings:
 default                      c1=background  c2=primes
 -f                           c1=background  c2=primes  c3=composites
 -6m                          c1=background  c2=6m-1    c3=composites  c4=6m+1

 Available Mappings:
 1. Linear                    Linear mapping left to right, top to bottom
 2. Diagonal                  Diagonal winding from top left
 3. Spiral                    Spiral mapping inside to outside

 Available Dot Types:
 1. Blob                      Draws a spherical fading dot
 2. Circle                    Draws a regular circle
 3. Square                    Draws a regular square

15. GraphNet [options] [output image]
 Creates a plot of a boolean-like network with a random starring state.
 -b (number)                  Number of states (default 2)
 -n (number)                  Number of nodes in the network (defaults to width of image)
 -c (number)                  Connections per node (default 3)
 -p (number)                  Chance of inserting a perturbation (default 0)
 -rs (number)[%]              Random Int32 seed value (defaults to system picked)

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

Available Colors:
Note: Colors may be specified as a name or as a hex value
AliceBlue                     F0F8FFFF
AntiqueWhite                  FAEBD7FF
Aqua                          00FFFFFF
Aquamarine                    7FFFD4FF
Azure                         F0FFFFFF
Beige                         F5F5DCFF
Bisque                        FFE4C4FF
Black                         000000FF
BlanchedAlmond                FFEBCDFF
Blue                          0000FFFF
BlueViolet                    8A2BE2FF
Brown                         A52A2AFF
BurlyWood                     DEB887FF
CadetBlue                     5F9EA0FF
Chartreuse                    7FFF00FF
Chocolate                     D2691EFF
Coral                         FF7F50FF
CornflowerBlue                6495EDFF
Cornsilk                      FFF8DCFF
Crimson                       DC143CFF
Cyan                          00FFFFFF
DarkBlue                      00008BFF
DarkCyan                      008B8BFF
DarkGoldenrod                 B8860BFF
DarkGray                      A9A9A9FF
DarkGreen                     006400FF
DarkKhaki                     BDB76BFF
DarkMagenta                   8B008BFF
DarkOliveGreen                556B2FFF
DarkOrange                    FF8C00FF
DarkOrchid                    9932CCFF
DarkRed                       8B0000FF
DarkSalmon                    E9967AFF
DarkSeaGreen                  8FBC8BFF
DarkSlateBlue                 483D8BFF
DarkSlateGray                 2F4F4FFF
DarkTurquoise                 00CED1FF
DarkViolet                    9400D3FF
DeepPink                      FF1493FF
DeepSkyBlue                   00BFFFFF
DimGray                       696969FF
DodgerBlue                    1E90FFFF
Firebrick                     B22222FF
FloralWhite                   FFFAF0FF
ForestGreen                   228B22FF
Fuchsia                       FF00FFFF
Gainsboro                     DCDCDCFF
GhostWhite                    F8F8FFFF
Gold                          FFD700FF
Goldenrod                     DAA520FF
Gray                          808080FF
Green                         008000FF
GreenYellow                   ADFF2FFF
Honeydew                      F0FFF0FF
HotPink                       FF69B4FF
IndianRed                     CD5C5CFF
Indigo                        4B0082FF
Ivory                         FFFFF0FF
Khaki                         F0E68CFF
Lavender                      E6E6FAFF
LavenderBlush                 FFF0F5FF
LawnGreen                     7CFC00FF
LemonChiffon                  FFFACDFF
LightBlue                     ADD8E6FF
LightCoral                    F08080FF
LightCyan                     E0FFFFFF
LightGoldenrodYellow          FAFAD2FF
LightGray                     D3D3D3FF
LightGreen                    90EE90FF
LightPink                     FFB6C1FF
LightSalmon                   FFA07AFF
LightSeaGreen                 20B2AAFF
LightSkyBlue                  87CEFAFF
LightSlateGray                778899FF
LightSteelBlue                B0C4DEFF
LightYellow                   FFFFE0FF
Lime                          00FF00FF
LimeGreen                     32CD32FF
Linen                         FAF0E6FF
Magenta                       FF00FFFF
Maroon                        800000FF
MediumAquamarine              66CDAAFF
MediumBlue                    0000CDFF
MediumOrchid                  BA55D3FF
MediumPurple                  9370DBFF
MediumSeaGreen                3CB371FF
MediumSlateBlue               7B68EEFF
MediumSpringGreen             00FA9AFF
MediumTurquoise               48D1CCFF
MediumVioletRed               C71585FF
MidnightBlue                  191970FF
MintCream                     F5FFFAFF
MistyRose                     FFE4E1FF
Moccasin                      FFE4B5FF
NavajoWhite                   FFDEADFF
Navy                          000080FF
OldLace                       FDF5E6FF
Olive                         808000FF
OliveDrab                     6B8E23FF
Orange                        FFA500FF
OrangeRed                     FF4500FF
Orchid                        DA70D6FF
PaleGoldenrod                 EEE8AAFF
PaleGreen                     98FB98FF
PaleTurquoise                 AFEEEEFF
PaleVioletRed                 DB7093FF
PapayaWhip                    FFEFD5FF
PeachPuff                     FFDAB9FF
Peru                          CD853FFF
Pink                          FFC0CBFF
Plum                          DDA0DDFF
PowderBlue                    B0E0E6FF
Purple                        800080FF
RebeccaPurple                 663399FF
Red                           FF0000FF
RosyBrown                     BC8F8FFF
RoyalBlue                     4169E1FF
SaddleBrown                   8B4513FF
Salmon                        FA8072FF
SandyBrown                    F4A460FF
SeaGreen                      2E8B57FF
SeaShell                      FFF5EEFF
Sienna                        A0522DFF
Silver                        C0C0C0FF
SkyBlue                       87CEEBFF
SlateBlue                     6A5ACDFF
SlateGray                     708090FF
Snow                          FFFAFAFF
SpringGreen                   00FF7FFF
SteelBlue                     4682B4FF
Tan                           D2B48CFF
Teal                          008080FF
Thistle                       D8BFD8FF
Tomato                        FF6347FF
Transparent                   FFFFFF00
Turquoise                     40E0D0FF
Violet                        EE82EEFF
Wheat                         F5DEB3FF
White                         FFFFFFFF
WhiteSmoke                    F5F5F5FF
Yellow                        FFFF00FF
YellowGreen                   9ACD32FF

Available Formats:
Note: Formats are engine specific
PNG                           image/png [png]
JPEG                          image/jpeg [jpg,jpeg,jfif]
GIF                           image/gif [gif]
BMP                           image/bmp [bm,bmp,dip]

```