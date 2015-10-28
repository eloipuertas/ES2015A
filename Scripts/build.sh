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

if [ "$TRAVIS_OS_NAME" == "osx" ]; then

    echo "Attempting to build $project for Windows"
    travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile $(pwd)/unity.log \
      -projectPath "$(pwd)/March Death" \
      -buildWindowsPlayer "$BUILD_DIR/windows/$project" \
      -quit

    BUILD_WIN=$?

    if [ $BUILD_WIN == 0 ]; then

        echo "Attempting to build $project for OS X"
        travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
          -batchmode \
          -nographics \
          -silent-crashes \
          -logFile $(pwd)/unity.log \
          -projectPath "$(pwd)/March Death" \
          -buildOSXUniversalPlayer "$BUILD_DIR/osx/$project" \
          -quit

        BUILD_OSX=$?

        if [ $BUILD_OSX == 0 ]; then

            echo "Attempting to build $project for Linux"
            travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
              -batchmode \
              -nographics \
              -silent-crashes \
              -logFile $(pwd)/unity.log \
              -projectPath "$(pwd)/March Death" \
              -buildLinuxUniversalPlayer "$BUILD_DIR/linux/$project" \
              -quit

            BUILD_LINUX=$?

            if [ $BUILD_LINUX == 0 ]; then

              ln -s $BUILD_DIR/linux/$project.x86_64 $BUILD_DIR/linux/$project

            fi

        fi

    fi

else

    # OSX can't be built from linux
    BUILD_OSX=0
    # Takes too much time
    BUILD_WIN=0

    echo "Attempting to start dummy audio driver"
    sudo modprobe snd-dummy

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

fi

if [ "$TRAVIS_BRANCH" == "devel-travis_cache" ] && [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
    echo -e "\n\033[32;1mUpload to cache server\033[0m\n"
    sudo -E rsync -rlptgD --delete-after "$HOME/ES2015A/March Death/Temp" ${CACHE_HOST}
    sudo -E rsync -rlptgD --delete-after "$HOME/ES2015A/March Death/Obj" ${CACHE_HOST}
    sudo -E rsync -rlptgD --delete-after "$HOME/ES2015A/March Death/Library" ${CACHE_HOST}
    sudo -E rsync -rlptgD --delete-after $BUILD_DIR ${CACHE_HOST}
fi

if [ $BUILD_WIN == 0 ] && [ $BUILD_LINUX == 0 ] && [ $BUILD_OSX == 0 ]; then
    echo -e "\n\033[32;1mBuild Completed Successfully\033[0m\n"
    exit 0
fi

echo -e "\n\033[31;1mBuild Failed\033[0m\n"
echo -e "\tWindows: ${BUILD_WIN}\n"
echo -e "\tOS X: ${BUILD_OSX}\n"
echo -e "\tLinux: ${BUILD_LINUX}\n"

exit 1
