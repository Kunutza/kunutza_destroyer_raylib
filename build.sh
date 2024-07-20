directory=$(pwd)
name='kunutza_destroyer2_raylib'
game_dir="$directory/$name"

files="main.c"
libs='-lraylib -lm'

#gcc -g -fsanitize=address $files $libs -o $name && "$game_dir"
gcc $files $libs -o $name && "$game_dir"

