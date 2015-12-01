#! /bin/bash

sudo -E apt-get -yq update
sudo -E apt-get -yq --no-install-suggests --no-install-recommends --force-yes install build-essential debconf gconf-service lib32gcc1 lib32stdc++6 libasound2 libc6 libc6-i386 libcairo2 libcap2 libcups2 libdbus-1-3 libexpat1 libfontconfig1 libfreetype6 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libgl1-mesa-glx libglib2.0-0 libglu1-mesa libgtk2.0-0 libnspr4 libnss3 libpango1.0-0 libstdc++6 libx11-6 libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxtst6 zlib1g libpng12-0 xvfb rsync cmake

# Print some debug info
. "$(dirname "$0")/debug.sh"

# Source config, env and travis_wait
. "$(dirname "$0")/Config/env.sh"  # Sets $TRAVIS_SCRIPTS and $CURRENT_DIR, among others
. "$TRAVIS_SCRIPTS/Config/config.cfg"
. "$TRAVIS_SCRIPTS/travis_wait.sh"

echo "Downloading from $UNITY_URL"
"$TRAVIS_SCRIPTS/bin/axel" -q -n 10 -o Unity.sh $UNITY_URL

if [ $USE_CACHE == 1 ]
then
    echo 'Background downloading cache'
    # Use ; and not && to actually do all of them, even if one doesn't succeed
    (touch $HOME/.CACHE_LOCK; \
        echo -e "\t> Library" && \
            "$TRAVIS_SCRIPTS/bin/axel" -q -n 10 ${CACHE_URL}Library.tar.gz; \
    rm $HOME/.CACHE_LOCK) &
fi

(touch $HOME/.TIMESTAMPS_LOCK; \
    echo "Fixing timestamps" && \
    "$TRAVIS_SCRIPTS/deploy.sh" > /dev/null 2>&1; \
rm $HOME/.TIMESTAMPS_LOCK) &

echo 'Monkey-patching installer for non sudo execution and no input'
sed -i '41,44d;49,50d' ./Unity.sh

echo 'Extracting Unity from monkey-patched Unity.sh'
chmod +x ./Unity.sh
./Unity.sh -o $UNITY_INSTALL

echo "Install Working Dir: `pwd`"
echo "Installed to: ${UNITY_INSTALL}"

echo 'Creating cache and local'
mkdir -p $HOME/.cache/unity3d
mkdir -p $HOME/.local/share/unity3d/Unity

# Up until here server date might be biased :(
# So we must wait for it to finish
echo -n "Waiting for timestamp fixing to end."
while [ -f "$HOME/.TIMESTAMPS_LOCK" ]
do
    echo -n "."
    sleep 2
done

# Wait for cache to finish
if [ $USE_CACHE == 1 ]
then
    echo -en "\nWaiting for cache to end downloading."
    while [ -f "$HOME/.CACHE_LOCK" ]
    do
        echo -n "."
        sleep 2
    done

    echo -e "\nUnpacking Library.tar.gz"
    tar -xzf "$CURRENT_DIR/Library.tar.gz" -C /
fi

echo -e "\n\033[32;1mDone installing\033[0m\n"
