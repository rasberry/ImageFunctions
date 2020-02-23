using System;

namespace ImageFunctions.Helpers
{
	public static class MathHelpers
	{
		public static int IntCeil(int num, int den)
		{
			int floor = num / den;
			int extra = num % den == 0 ? 0 : 1;
			return floor + extra;
		}

		public static double Fractional(this double number)
		{
			//return number - Math.Truncate(number); //TODO returns negative numbers - don't know why
			return Math.Abs(number % 1.0);
		}
		public static double Integral(this double number)
		{
			return Math.Truncate(number);
		}

		//start: top left
		//run  : right to left
		//fill : top to bottom
		public static (int,int) LinearToXY(int position, int width)
		{
			int y = position / width;
			int x = position % width;
			return (x,y);
		}

		public static int XYToLinear(int x, int y, int width)
		{
			return y * width + x;
		}

		//start: top left
		//run  : diagonal right + up
		//fill : top left to bottom right
		public static (int,int) DiagonalToXY(long position)
		{
			if (position < 0) {
				throw new ArgumentOutOfRangeException("position must be positive");
			}
			//X 0,0,1,0,1,2,0,1,2,3,0,1,2,3,4,0,1,2,3,4,5
			//Y 0,1,0,2,1,0,3,2,1,0,4,3,2,1,0,5,4,3,2,1,0
			//X https://oeis.org/A002262
			//Y https://oeis.org/A025581

			long t = (long)Math.Floor((1.0 + Math.Sqrt(1.0 + 8.0 * position))/2.0);
			int x = (int)(position - (t * (t - 1)) / 2);
			int y = (int)(((t - 1)*(t / 2 + 1)) - position);
			return (x,y);
		}

		public static long XYToDiagonal(int x, int y)
		{
			//solve([x = p - (t * (t - 1)) / 2, y = ((t - 1)*(t / 2 + 1)) - p],[p,t]);
			//p1=(y^2+x*(2*y+3)+y+x^2)/2

			///p    x y t x+y p
			/// 0 - 0 0 1 0    0
			/// 1 - 0 1 2 1    1
			/// 2 - 1 0 2 1    2
			/// 3 - 0 2 3 2    3
			/// 4 - 1 1 3 2    4
			/// 5 - 2 0 3 2    5
			/// 6 - 0 3 4 3    6
			/// 7 - 1 2 4 3    7
			/// 8 - 2 1 4 3    8
			/// 9 - 3 0 4 3    9
			///10 - 0 4 5 4   10
			///11 - 1 3 5 4   11
			///12 - 2 2 5 4   12
			///13 - 3 1 5 4   13
			///14 - 4 0 5 4   14
			///15 - 0 5 6 5   15
			///16 - 1 4 6 5   16
			///17 - 2 3 6 5   17
			///18 - 3 2 6 5   18
			///19 - 4 1 6 5   19
			///20 - 5 0 6 5   20

			long pos = (y*y + x*(2*y + 3) + y + x*x)/2;
			return pos;
		}

		public static (int,int) SpiralSquareToXY(long position, int cx = 0, int cy = 0)
		{
			// https://math.stackexchange.com/questions/163080/on-a-two-dimensional-grid-is-there-a-formula-i-can-use-to-spiral-coordinates-in

			position++; //1 based spiral
			long k = (long)Math.Ceiling((Math.Sqrt(position)-1.0)/2.0);
			long t = 2 * k + 1;
			long m = t * t;
			t-=1;
			/// Console.WriteLine($"p={position} k={k} t={t} m={m} (m-t)={m-t}");
			if (position >= m-t) {
				var x = k-(m-position);
				var y = k;
				return ((int)x,(int)y);
			}
			m -= t;
			if (position >= m-t) {
				var x = -k;
				var y = k-(m-position);
				return ((int)x,(int)y);
			}
			m -= t;
			if (position >= m-t) {
				var x = -k+(m-position);
				var y = -k;
				return ((int)x,(int)y);
			}
			else {
				var x = k;
				var y = -k+(m-position-t);
				return ((int)x,(int)y);
			}
		}

		public static long XYToSpiralSquare(int x,int y, int cx = 0, int cy = 0)
		{
			y = -y;
			// https://www.reddit.com/r/dailyprogrammer/comments/3ggli3/20150810_challenge_227_easy_square_spirals/
			if (x >= y) {
				if (x > -y) {
					long m = 2*x-1; m *= m;
					return m + Math.Abs(x) + y - 1;
				}
				else {
					long m = 2*y+1; m *= m;
					return m + Math.Abs(7*y) + x - 1;
				}
			}
			else {
				if (x > -y) {
					long m = 2*y-1; m *= m;
					return m + Math.Abs(3*y) - x - 1;
				}
				else {
					long m = 2*x+1; m *= m;
					return m + Math.Abs(5*x) - y - 1;
				}
			}
		}

		#if false
		//TODO this almost works
		// https://www.reddit.com/r/dailyprogrammer/comments/3ggli3/20150810_challenge_227_easy_square_spirals/
		public static (int,int) SpiralSquareToXY(long num)
		{
			//64 63 62 61 60 59 58 57 56
			//65 36 35 34 33 32 31 30 55
			//66 37 16 15 14 13 12 29 54
			//67 38 17  4  3  2 11 28 53
			//68 39 18  5  0  1 10 27 52
			//69 40 19  6  7  8  9 26 51
			//70 41 20 21 22 23 24 25 50
			//71 42 43 44 45 46 47 48 49


			if(num <= 1) {
				return (0,0);
			}
			long root = (long)Math.Sqrt(num);
			long diff = num - root * root;
			long which = diff / (root+1);
			long lx,ly;
			switch(which)
			{
				case 0:  lx = root+1;        ly = root-diff+1;   break;
				case 1:  lx = root*2-diff+2; ly = 0;             break;
				case 2:  lx = 0;             ly = root*3-diff+3; break;
				default: lx = root*4-diff+4; ly = root+1;        break;
			}
			return ((int)lx,(int)ly);
		}

		public static long XYToSpiralSquare(int x, int y)
		{
			long lx=x,ly=y;
			if(lx>=ly) {
				if(lx>-ly) {
					return (lx*2-1)*(lx*2-1)+ly+Math.Abs(lx);
				}
				else {
					return (ly*2+1)*(ly*2+1)+Math.Abs(ly*7)+lx;
				}
			}
			else {
				if(lx>-ly) {
					return (ly*2-1)*(ly*2-1)+Math.Abs(ly*3)-lx;
				}
				else {
					return (lx*2+1)*(lx*2+1)-ly+Math.Abs(lx*5);
				}
			}
		}
		#endif

		#if false
		//doesn't work
		public static (int,int) SpiralSquareToXY(long num)
		{
			double n = num;
			double m = Math.Floor(Math.Sqrt(n));
			double k ;
			if (m % 2 == 0) {
				k = Math.Floor((m-1.0)/2.0);
			}
			else {
				bool adjust = n < m*(m+1);
				k = Math.Floor(m/2.0 - (adjust ? 1.0 : 0.0));
			}

			// 2k(2k+1)     4*k*k+2*k
			// (2k+1)^2     4*k*k+4*k+1
			// 2(k+1)(2k+1) 4*k*k+6*k+2
			// 4(k+1)^2     4*k*k+8*k+4
			// 2(k+1)(2k+3) 4*k*k+10*k+6

			double fourkk = 4.0*k*k;
			double c1 = fourkk+2.0*k;
			double c2 = fourkk+4.0*k+1.0;
			if (c1 < n && n < c2) {
				int x = (int)(n - fourkk - 3.0*k);
				return (x,(int)k);
			}
			double c3 = fourkk+6.0*k+2.0;
			if (c2 < n && n < c3) {
				int y = (int)(fourkk + 5*k + 1.0 - n);
				return ((int)(k+1.0),y);
			}
			double c4 = fourkk+8.0*k+4.0;
			if (c3 < n && n < c4) {
				int x = (int)(fourkk + 7*k + 3 - n);
				return (x,(int)(-k-1.0));
			}
			double c5 = fourkk+10.0*k+6.0;
			if (c4 < n && n < c5) {
				int y = (int)(n - fourkk - 9*k - 5);
				return ((int)(-k-1.0),y);
			}
			return (0,0);
		}

		public static long XYToSpiralSquare(int x, int y)
		{
			return 0;
		}
		#endif
	}
}
/*
TODO try this one

#include <iostream>
#include <cstdlib>
#include <cmath>

using namespace std;

void get_loc_by_num(long, long);
void get_num_by_loc(long, long, long);

int main() {

    long size;
    cin >> size;
    cin.ignore(); // eat newline

    string line;

    getline(cin, line);

    size_t idx_space = line.find(" ");
    if(idx_space == string::npos) {
        long num = atoi(line.c_str());

        get_loc_by_num(size, num);
    }else {
        long x = atoi(line.substr(0, idx_space).c_str());
        long y = atoi(line.substr(idx_space + 1, line.size() - idx_space - 1).c_str());

        get_num_by_loc(size, x, y);
    }

    return 0;
}

void get_loc_by_num(long size, long num) {

    // find right bottom corner(1, 9, 25, 49, ...)
    long r = static_cast<long>(sqrt(num)); // length of the special square
    if(r % 2 == 0) {
        r -= 1;
    }

    long num_corner = r * r; // num of corner point
    long x = (size + 1) / 2 + (r - 1) / 2; // x of corner point
    long y = x; // y of corner point

    long delta = num - num_corner;
    if(delta == 0) {
        // no op
    }else if(delta <= r + 1){
        x += 1;
        y -= delta - 1;
    }else if(delta <= 2 * r + 2) {
        x -= delta - r - 2;
        y -= r;
    }else if(delta <= 3 * r + 3) {
        x -= r;
        y += delta - 3 * r - 2;
    }else if(delta <= 4 * r + 4) {
        x -= delta - 4 * r - 3;
        y += 1;
    }else {
        throw "impossible";
    }

    cout << "(" << x << "," << y << ")" << endl;

}

void get_num_by_loc(long size, long x, long y) {

    long center = (size + 1) / 2;
    long r = std::max(std::abs(x - center), std::abs(y - center)) - 1; // length of the special square
    r = 2 * r + 1;

    long cx = center + (r - 1) / 2; // x of corner point
    long cy = cx; // y of corner point
    long num = r * r; // num of corner point

    long deltax = x - cx;
    long deltay = y - cy;

    if(deltax == 0 && deltay == 0) {
        // do nothing
    }else if(deltax == 1) {
        num += -deltay + 1;
    }else if(deltay == -r) {
        num += r + 2 - deltax;
    }else if(deltax == -r) {
        num += 3 * r + 2 + deltay;
    }else if(deltay == 1) {
        num += 4 * r + 3 + deltax;
    }else {
        throw "impossible";
    }

    cout << num << endl;
}
 */

/*
public void calculatePosition(string sizeInput, string input)
{
	long size = Convert.ToInt64(sizeInput);
	string[] inputSplit=input.Split();
	if(inputSplit.Length==1)
	{
		long[] result= getCoord(Convert.ToInt64(input));
		long offset=(size-result[2])/2;
		Console.WriteLine("("+(result[0]+offset)+", "+(result[1]+offset)+")");
	}
	else
	{
		long x = Convert.ToInt64(inputSplit[0]);
		long y = Convert.ToInt64(inputSplit[1]);
		Console.WriteLine(getNum(x,y,size));
	}
}

public long[] getCoord(long num)
{
	if(num==1)
		return new long[] {0,0,-1};
	long root = (long)Math.Sqrt(num);
	long diff = num-root*root;
	switch((long)(diff/(root+1)))
	{
		case 0: return new long[] {root+1,root-diff+1,root};
		case 1: return new long[] {root*2-diff+2,0,root};
		case 2: return new long[] {0,root*3-diff+3,root};
		default: return new long[] {root*4-diff+4,root+1,root};
	}

	return null;
}

public long getNum(long shiftedX, long shiftedY, long size)
{
	long x=shiftedX-(size+1)/2;
	long y=-shiftedY+(size+1)/2;
	if(x>=y)
	{
		if(x>-y)
			return (x*2-1)*(x*2-1)+y+Math.Abs(x);
		else
			return (y*2+1)*(y*2+1)+Math.Abs(y*7)+x;
	}
	else
	{
		if(x>-y)
			return (y*2-1)*(y*2-1)+Math.Abs(y*3)-x;
		else
			return (x*2+1)*(x*2+1)-y+Math.Abs(x*5);
	}
}
 */

 /*
function spiral(n) {
    // given n an index in the squared spiral
    // p the sum of point in inner square
    // a the position on the current square
    // n = p + a

    var r = Math.floor((Math.sqrt(n + 1) - 1) / 2) + 1;

    // compute radius : inverse arithmetic sum of 8+16+24+...=
    var p = (8 * r * (r - 1)) / 2;
    // compute total point on radius -1 : arithmetic sum of 8+16+24+...

    var en = r * 2;
    // points by face

    var a = (1 + n - p) % (r * 8);
    // compute de position and shift it so the first is (-r,-r) but (-r+1,-r)
    // so square can connect

    var pos = [0, 0, r];
    switch (Math.floor(a / (r * 2))) {
        // find the face : 0 top, 1 right, 2, bottom, 3 left
        case 0:
            {
                pos[0] = a - r;
                pos[1] = -r;
            }
            break;
        case 1:
            {
                pos[0] = r;
                pos[1] = (a % en) - r;

            }
            break;
        case 2:
            {
                pos[0] = r - (a % en);
                pos[1] = r;
            }
            break;
        case 3:
            {
                pos[0] = -r;
                pos[1] = r - (a % en);
            }
            break;
    }
    console.log("n : ", n, " r : ", r, " p : ", p, " a : ", a, "  -->  ", pos);
    return pos;
}
  */