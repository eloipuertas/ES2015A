#! /bin/bash

# Import script
source $(dirname $0)/travis_wait.sh

# Export vars
export HOME=$(dirname `pwd`)
export UNITY_ROOT=$HOME/unity-editor-5.1.0f3
export BUILD_DIR=$HOME/Build
export project="MarchWars"

echo "Home: $HOME"
echo "UnityRoot: $UNITY_ROOT"
echo "User: $USER"

# Variables
BUILD_LINUX=-1
BUILD_WIN=-1
BUILD_OSX=-1

# Create folder if it does not exist
mkdir -p Assets

# Monkey-patch ProjectSettings
echo "Monkey-patching ProjectSettings and AudioManager assets"
sed -i 's/displayResolutionDialog: 1/displayResolutionDialog: 0/g' March\ Death/ProjectSettings/ProjectSettings.asset
sed -i 's/m_DisableAudio: 0/m_DisableAudio: 1/g'  March\ Death/ProjectSettings/AudioManager.asset

# OSX can't be built from linux
BUILD_OSX=0
# Takes too much time
BUILD_WIN=0

echo "Attempting to start dummy audio driver"
sudo modprobe snd-dummy

# Build navmesh first
echo -e "\nAttempting to build Recast/Detour for Linux [64 bits]"
cd March\ Death/Assets/UnityRecast
mkdir Build
cd Build
cmake ..
make
cp Lib/libRecast.so /usr/lib/Recast.so
cp Lib/libRecast.so /usr/lib64/Recast.so # Should not work
cp Lib/libRecast.so /usr/lib64/libRecast.so # Should not work
mv Lib/libRecast.so /usr/lib/libRecast.so
#mv Lib/libRecast.so ../../Plugins/x86_64/Recast.so
cd ..

echo -e "\nAttempting to build Recast/Detour for Linux [32 bits]"
rm -rf Build
mkdir Build
cd Build
cmake -DBUILD_32_BITS ..
make
cp Lib/libRecast.so /usr/lib32/Recast.so
mv Lib/libRecast.so /usr/lib32/libRecast.so
#mv Lib/libRecast.so ../../Plugins/x86/Recast.so

cd $HOME/ES2015A

# Build Unity project
echo -e "\nAttempting to build $project for Linux"
sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
    $UNITY_ROOT/Editor/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile /dev/stdout \
      -projectPath "$HOME/ES2015A/March Death" \
      -buildLinuxUniversalPlayer "$BUILD_DIR/linux/$project" \
      -quit

BUILD_LINUX=$?

if [ $BUILD_LINUX == 0 ]; then

    sudo -E ln -s $BUILD_DIR/linux/$project.x86_64 $BUILD_DIR/linux/$project

fi

if [ "$TRAVIS_BRANCH" == "devel-travis_cache" ] && [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
    echo -e "\n\033[32;1mUpload to cache server\033[0m\n"
    (touch $HOME/.RSYNC_LOCK; \
        echo -e "\t> Library" && \
            sudo -E tar -zcf "$(pwd)/Library.tar.gz" "$HOME/ES2015A/March Death/Library" && \
            sudo -E rsync -a "$(pwd)/Library.tar.gz" ${CACHE_HOST}; \
    rm $HOME/.RSYNC_LOCK) &
fi

if [ $BUILD_WIN == 0 ] && [ $BUILD_LINUX == 0 ] && [ $BUILD_OSX == 0 ]; then
    echo -e "\n\033[32;1mBuild Completed Successfully\033[0m\n"
    exit 0
fi

echo -e "\n\033[31;1mBuild Failed\033[0m\n"
echo -e "\tWindows: ${BUILD_WIN}\n"
echo -e "\tOS X: ${BUILD_OSX}\n"
echo -e "\tLinux: ${BUILD_LINUX}\n"

# Notify on github
COMMIT_AUTHOR=`git log -1 | grep -Po "(?<=Author: ).*(?= <)"`
curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
    https://api.github.com/repos/eloipuertas/ES2015A/issues/193/comments \
    -d "{\"body\":\"Commit failed to **build**.\n\nCommit by: @${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/eloipuertas/ES2015A/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null

exit 1
