#!/bin/bash

# https://stackoverflow.com/questions/41731740/how-to-pass-an-array-into-a-bash-function

# im="/mnt/d/Software/visual/ImageMagickQ16HDRI/magick.exe"
dotnet="/mnt/c/Program Files/dotnet/dotnet.exe"
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
			row="$row|![$n-$i]($3/$n-$i.png \"$n-$i\")"
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
		"$dotnet" run -p "../src" "--no-build" -- "$4" $5 "img/$n.png" "$2/$n-$3.png"
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

# make-1
# make-2
# make-3
make-4

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