directory=$(pwd)
name='kunutza_destroyer2'
game_dir="$directory/$name"

files="main.c"
libs='-lraylib'

#gcc -g -fsanitize=address $files $libs -o $name && "$game_dir"
gcc $files $libs -o $name && "$game_dir"

