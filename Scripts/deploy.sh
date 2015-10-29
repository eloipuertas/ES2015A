#!/bin/sh

gittouch() {
    NEWDATE="$(git log -1 --format=%ci "$1")"
    sudo -E date --set="$NEWDATE"
    touch -ch -d "$NEWDATE" "$1"
}

OLDDATE="$(date +"%Y%m%d %H:%M:%S")"

git ls-files |
    while IFS= read -r file; do
        gittouch "$file"
    done

sudo -E date --set="$OLDDATE"
