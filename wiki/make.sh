#!/bin/bash

# https://stackoverflow.com/questions/41731740/how-to-pass-an-array-into-a-bash-function

# im="/mnt/d/Software/visual/ImageMagickQ16HDRI/magick.exe"
# dotnet="/mnt/c/Program Files/dotnet/dotnet.exe"
dotnet="dotnet"
src="img/*"

function pad() {
	printf "%-$1s" "$2"
}

function maxlen() {
	local -n maxlen_r="$1"
	maxlen=0
	for n in "${maxlen_r[@]}"; do
		len="${#n}"
		if [ "$len" -gt "$maxlen" ]; then maxlen="$len"; fi
	done
	echo "$maxlen"
}

# optionsarray name outfolder opt...
function md() {
	echo "# $2 #"
	s=4; e="$#"
	tit="| Image   | Default |"
	sep="|---------|---------|"
	while [ $s -le $e ]; do
		tit="$tit ${!s} |"
		sep="$sep---|"
		s=$((s + 1))
	done
	echo "$tit"
	echo "$sep"

	local -n md_r="$1"
	max=$(maxlen "md_r")
	for n in "${md_r[@]}"; do
		p=$(pad "$max" "$n")
		row="|$p"
		s=3;
		while [ $s -le $e ]; do
			i=$((s - 2))
			row="$row|![$n-$i](img/$3-$n-$i.png \"$n-$i\")"
			s=$((s + 1))
		done
		echo "$row"
	done

}

# imagearray outfolder index action options
function makeset() {
	local -n makeset_r="$1"
	if [ ! -d "$2" ]; then mkdir "$2"; fi
	for n in "${makeset_r[@]}"; do
		"$dotnet" run -p "../src" "--no-build" -- "$4" $5 "img/$n.png" "img/$2-$n-$3.png"
	done
}

# imagearray optionsarray action name outfolder
function makepermute() {
	local -n makepermute_r="$2"

	#default
	makeset "$1" "$5" "1" "$3"
	#with options
	i=2
	for o in "${makepermute_r[@]}"; do
		makeset "$1" "$5" "$i" "$3" "$o"
		i=$((i + 1))
	done
	md "$1" "$4" "$5" "${makepermute_r[@]}"
}

function make-1() {
	make1_r=("boy" "building" "cats" "cloud" "cookie" "creek" "flower")
	opts_r=("-p" "-s 3" "-r 3")
	makepermute "make1_r" "opts_r" "1" "PixelateDetails" "img-1"
}

function make-2() {
	make2_r=("fractal" "handle" "harddrive" "lego" "pool" "rainbow" "road")
	opts_r=("-g" "-a")
	makepermute "make2_r" "opts_r" "2" "Derivatives" "img-2"
}

function make-3() {
	make3_r=("rock-p" "scorpius-p" "shack-p" "shell-p" "skull-p" "spider-p" "toes-p")
	opts_r=("-t 2" "-t 10" "--metric 1" "--sampler 11")
	makepermute "make3_r" "opts_r" "3" "AreaSmoother" "img-3"
}

function make-4() {
	make4_r=("rock-p" "scorpius-p" "shack-p" "shell-p" "skull-p" "spider-p" "toes-p")
	opts_r=("-H" "-V")
	makepermute "make4_r" "opts_r" "4" "AreaSmoother2" "img-4"
}

function make-5() {
	make5_r=("zebra" "boy" "building" "cats" "cloud" "cookie" "creek")
	opts_r=("-z 3")
	makepermute "make5_r" "opts_r" "5" "ZoomBlur" "img-5"
}

function make-6() {
	make6_r=("flower" "fractal" "handle" "harddrive" "lego" "pool" "rainbow")
	opts_r=("-rp 50%" "-s 2" "-ccw")
	makepermute "make6_r" "opts_r" "6" "Swirl" "img-6"
}

function make-7() {
	make7_r=("road" "rock" "scorpius" "shack" "shell" "skull" "spider")
	opts_r=("-e 2.5" "-m 2")
	makepermute "make7_r" "opts_r" "7" "Deform" "img-7"
}

function make-8() {
	make8_r=("toes" "zebra")
	opts_r=("-p 1234")
	makepermute "make8_r" "opts_r" "8" "Encrypt" "img-8"
}

function make-9() {
	make9_r=("boy" "building" "cats" "cloud" "cookie" "creek" "flower")
	opts_r=("-m 2" "-m 3" "-n 10")
	makepermute "make9_r" "opts_r" "9" "PixelRules" "img-9"
}

make-1
make-2
make-3
make-4
make-5
make-6
make-7
make-8
make-9

# boy
# building
# cats
# cloud
# cookie
# creek
# flower
# fractal
# handle
# harddrive
# lego
# pool
# rainbow
# road
# rock
# scorpius
# shack
# shell
# skull
# spider
# toes
# zebra