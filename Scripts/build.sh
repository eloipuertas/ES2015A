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

# Variables
BUILD_LINUX=-1
BUILD_WIN=-1
BUILD_OSX=-1

# Create folder if it does not exist
mkdir -p Assets

# Monkey-patch ProjectSettings
sed -i 's/displayResolutionDialog: 1/displayResolutionDialog: 0/g' March\ Death/ProjectSettings/ProjectSettings.asset

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

    echo "Attempting to build $project for Linux"
    $UNITY_ROOT/Editor/Unity \
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

        echo -e "\n\n-------------------------\n\n"
        echo "Attempting to build $project for Windows"
        $UNITY_ROOT/Editor/Unity \
          -batchmode \
          -nographics \
          -silent-crashes \
          -logFile $(pwd)/unity.log \
          -projectPath "$(pwd)/March Death" \
          -buildWindowsPlayer "$BUILD_DIR/windows/$project" \
          -quit

        BUILD_WIN=$?

    fi

fi

if [ $BUILD_WIN == 0 ] && [ $BUILD_LINUX == 0 ] && [ $BUILD_OSX == 0 ]; then
    echo -e "\n\033[32;1mBuild Completed Successfully\033[0m\n"
    return 0
fi

echo -e "\nLog:\n"
cat $(pwd)/unity.log

echo -e "\n\033[31;1mBuild Failed\033[0m\n"
echo -e "\tWindows: ${BUILD_WIN}\n"
echo -e "\tOS X: ${BUILD_OSX}\n"
echo -e "\tLinux: ${BUILD_LINUX}\n"

return 1
