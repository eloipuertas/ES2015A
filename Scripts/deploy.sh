# Adapted to use HEAD rather than the new commit ref
get_file_rev() {
    git rev-list -n 1 HEAD "$1"
}

# Same as Giel's answer above
update_file_timestamp() {
    file_time=`git show --pretty=format:%ai --abbrev-commit "$(get_file_rev "$1")" | head -n 1`
    sudo touch -d "$file_time" "$1"
}

# Loop through and fix timestamps on all files in our CDN directory
old_ifs=$IFS
IFS=$'\n' # Support files with spaces in them
for file in $(git ls-files | grep "$cdn_dir")
do
    update_file_timestamp "${file}"
done
IFS=$old_ifs
