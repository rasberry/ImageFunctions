# Usage #

```
Usage ImageFunctions (action) [options]
 -h / --help                 Show full help
 (action) -h                 Action specific help
 --actions                   List possible actions
 -# / --rect ([x,y,]w,h)     Apply function to given rectagular area (defaults to entire image)
 --max-threads (number)      Restrict parallel processing to a given number of threads (defaults to # of cores)
 --colors                    List available colors

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

9. PixelRules [options] (input image) [output image]
 Average a set of pixels by following a minimaztion function
 -m (mode)                   Which mode to use (default StairCaseDescend)
 -n (number)                 Number of times to apply operation (default 1)
 -x (number)                 Maximum number of iterations - in case of infinte loops (default 100)
 --sampler (name)            Use given sampler (defaults to nearest pixel)
 --metric (name) [args]      Use alterntive distance function

 Available Modes
 1. StairCaseDescend         move towards smallest distance
 2. StairCaseAscend          move towards largest distance
 3. StairCaseClosest         move towards closest distance
 4. StairCaseFarthest        move towards farthest distance

10. ImgDiff [options] (image one) (image two) [output image]
 Highlights differences between two images.
 By default differeces are hilighted based on distance ranging from hilight color to white
 -o (number)[%]              Overlay hilight color at given opacity
 -i                          Match identical pixels instead of differences
 -x                          Output original pixels instead of hilighting them
 -c (color)                  Change hilight color (default is magenta)

11. AllColors [options] [output image]
 Creates an image with every possible 24-bit color ordered by chosen pattern.
 -p (pattern)                Sort by Pattern (default BitOrder)
 -s (space)                  Sort by color space components (instead of pattern)
 -so (c,...)                 Change priority order of components (default 1,2,3,4)
 -np                         Use single threaded sort function instead of parallel sort

 Available Patterns
 1. BitOrder                Numeric order
 2. AERT                    AERT brightness
 3. HSP                     HSP color model brightness
 4. WCAG2                   WCAG2 relative luminance
 5. SMPTE240M               Luminance SMPTE 240M (1999)
 6. Luminance709            Luminance BT.709
 7. Luminance601            Luminance BT.601
 8. Luminance2020           Luminance BT.2020

 Available Spaces
  1. RGB                     
  2. HSV                     
  3. HSL                     
  4. HSI                     
  5. YCbCr                   
  6. CieLab                  
  7. CieLch                  
  8. CieLchuv                
  9. CieLuv                  
 10. CieXyy                  
 11. CieXyz                  
 12. Cmyk                    
 13. HunterLab               
 14. LinearRgb               
 15. Lms                     

12. SpearGraphic [options] [output image]
 Creates a spear graphic
 -g (name)                   Choose which graphic to create
 -bg (color)                 Change Background color (default transparent)
 -rs (number)                Random Int32 seed value (defaults to system picked)

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
 -p                          Color pixel if prime (true if -f not specified)
 -f                          Color pixel based on number of divisors; dot size is proportional to divisor count
 -6m                         Color primes depending on if they are 6*m+1 or 6*m-1
 -c (x,y)                    Center x,y coordinate (default 0,0)
 -m (mapping)                Mapping used to translate x,y into an index number (default spiral)
 -s (number)                 Spacing between points (default 1)
 -ds (number)                Maximum dot size in pixels; decimals allowed (default 1.0)
 -dt (dot type)              Dot used for drawing (default circle)
 -c(1,2,3,4) (color)         Colors to be used depending on mode. (setting any of the colors is optional)

 Color Mappings
 default: c1=background  c2=primes
 -f     : c1=background  c2=primes  c3=composites
 -6m    : c1=background  c2=6m-1    c3=composites  c4=6m+1

 Available Mappings:
 1. Linear                  Linear mapping left to right, top to bottom
 2. Diagonal                Diagonal winding from top left
 3. Spiral                  Spiral mapping inside to outside

 Available Dot Types:
 1. Blob                    Draws a spherical fading dot
 2. Circle                  Draws a regular circle
 3. Square                  Draws a regular square

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

Note: Colors may be specified as a name or as a hex value
Available Colors:
F0F8FFFF  AliceBlue
FAEBD7FF  AntiqueWhite
00FFFFFF  Aqua
7FFFD4FF  Aquamarine
F0FFFFFF  Azure
F5F5DCFF  Beige
FFE4C4FF  Bisque
000000FF  Black
FFEBCDFF  BlanchedAlmond
0000FFFF  Blue
8A2BE2FF  BlueViolet
A52A2AFF  Brown
DEB887FF  BurlyWood
5F9EA0FF  CadetBlue
7FFF00FF  Chartreuse
D2691EFF  Chocolate
FF7F50FF  Coral
6495EDFF  CornflowerBlue
FFF8DCFF  Cornsilk
DC143CFF  Crimson
00FFFFFF  Cyan
00008BFF  DarkBlue
008B8BFF  DarkCyan
B8860BFF  DarkGoldenrod
A9A9A9FF  DarkGray
006400FF  DarkGreen
BDB76BFF  DarkKhaki
8B008BFF  DarkMagenta
556B2FFF  DarkOliveGreen
FF8C00FF  DarkOrange
9932CCFF  DarkOrchid
8B0000FF  DarkRed
E9967AFF  DarkSalmon
8FBC8BFF  DarkSeaGreen
483D8BFF  DarkSlateBlue
2F4F4FFF  DarkSlateGray
00CED1FF  DarkTurquoise
9400D3FF  DarkViolet
FF1493FF  DeepPink
00BFFFFF  DeepSkyBlue
696969FF  DimGray
1E90FFFF  DodgerBlue
B22222FF  Firebrick
FFFAF0FF  FloralWhite
228B22FF  ForestGreen
FF00FFFF  Fuchsia
DCDCDCFF  Gainsboro
F8F8FFFF  GhostWhite
FFD700FF  Gold
DAA520FF  Goldenrod
808080FF  Gray
008000FF  Green
ADFF2FFF  GreenYellow
F0FFF0FF  Honeydew
FF69B4FF  HotPink
CD5C5CFF  IndianRed
4B0082FF  Indigo
FFFFF0FF  Ivory
F0E68CFF  Khaki
E6E6FAFF  Lavender
FFF0F5FF  LavenderBlush
7CFC00FF  LawnGreen
FFFACDFF  LemonChiffon
ADD8E6FF  LightBlue
F08080FF  LightCoral
E0FFFFFF  LightCyan
FAFAD2FF  LightGoldenrodYellow
D3D3D3FF  LightGray
90EE90FF  LightGreen
FFB6C1FF  LightPink
FFA07AFF  LightSalmon
20B2AAFF  LightSeaGreen
87CEFAFF  LightSkyBlue
778899FF  LightSlateGray
B0C4DEFF  LightSteelBlue
FFFFE0FF  LightYellow
00FF00FF  Lime
32CD32FF  LimeGreen
FAF0E6FF  Linen
FF00FFFF  Magenta
800000FF  Maroon
66CDAAFF  MediumAquamarine
0000CDFF  MediumBlue
BA55D3FF  MediumOrchid
9370DBFF  MediumPurple
3CB371FF  MediumSeaGreen
7B68EEFF  MediumSlateBlue
00FA9AFF  MediumSpringGreen
48D1CCFF  MediumTurquoise
C71585FF  MediumVioletRed
191970FF  MidnightBlue
F5FFFAFF  MintCream
FFE4E1FF  MistyRose
FFE4B5FF  Moccasin
FFDEADFF  NavajoWhite
000080FF  Navy
FDF5E6FF  OldLace
808000FF  Olive
6B8E23FF  OliveDrab
FFA500FF  Orange
FF4500FF  OrangeRed
DA70D6FF  Orchid
EEE8AAFF  PaleGoldenrod
98FB98FF  PaleGreen
AFEEEEFF  PaleTurquoise
DB7093FF  PaleVioletRed
FFEFD5FF  PapayaWhip
FFDAB9FF  PeachPuff
CD853FFF  Peru
FFC0CBFF  Pink
DDA0DDFF  Plum
B0E0E6FF  PowderBlue
800080FF  Purple
663399FF  RebeccaPurple
FF0000FF  Red
BC8F8FFF  RosyBrown
4169E1FF  RoyalBlue
8B4513FF  SaddleBrown
FA8072FF  Salmon
F4A460FF  SandyBrown
2E8B57FF  SeaGreen
FFF5EEFF  SeaShell
A0522DFF  Sienna
C0C0C0FF  Silver
87CEEBFF  SkyBlue
6A5ACDFF  SlateBlue
708090FF  SlateGray
FFFAFAFF  Snow
00FF7FFF  SpringGreen
4682B4FF  SteelBlue
D2B48CFF  Tan
008080FF  Teal
D8BFD8FF  Thistle
FF6347FF  Tomato
FFFFFF00  Transparent
40E0D0FF  Turquoise
EE82EEFF  Violet
F5DEB3FF  Wheat
FFFFFFFF  White
F5F5F5FF  WhiteSmoke
FFFF00FF  Yellow
9ACD32FF  YellowGreen

```