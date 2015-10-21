#! /bin/bash

project="unity"
mkdir -p Assets

source $(dirname $0)/travis_wait.sh

if [ "$TRAVIS_OS_NAME" == "osx" ]; then

    echo "Attempting to build $project for Windows"
    travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile $(pwd)/unity.log \
      -projectPath $(pwd) \
      -buildWindowsPlayer "$(pwd)/Build/windows/$project.exe" \
      -quit

    BUILD_WIN=$?

    echo "Attempting to build $project for OS X"
    travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile $(pwd)/unity.log \
      -projectPath $(pwd) \
      -buildOSXUniversalPlayer "$(pwd)/Build/osx/$project.app" \
      -quit

    BUILD_OSX=$?

    echo "Attempting to build $project for Linux"
    travis_wait /Applications/Unity/Unity.app/Contents/MacOS/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile $(pwd)/unity.log \
      -projectPath $(pwd) \
      -buildLinuxUniversalPlayer "$(pwd)/Build/linux/$project" \
      -quit

    BUILD_LINUX=$?

else

    # Hacks which should not be necessary
    #alias g++='g++ -fnon-call-exceptions'
    #alias gcc='gcc -fnon-call-exceptions'
    #CC="gcc -fnon-call-exceptions"

    export HOME=$(dirname `pwd`)
    export UNITY_ROOT=$HOME/unity-editor-5.1.0f3

    echo "Home: $HOME"
    echo "UnityRoot: $UNITY_ROOT"

    #echo "Attempting to start a virtual X Server"
    #Xvfb :99 &
    #export DISPLAY=:99

    echo "Attempting to build $project for Linux"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' $UNITY_ROOT/Editor/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile /dev/stdout \
      -projectPath $(pwd) \
      -buildLinuxUniversalPlayer "$HOME/Build/linux/$project" \
      -quit

    BUILD_LINUX=$?

    if [ $BUILD_LINUX == 0 ]; then

        echo -e "\n\n-------------------------\n\n"
        echo "Attempting to build $project for Windows"
        xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' $UNITY_ROOT/Editor/Unity \
          -batchmode \
          -nographics \
          -silent-crashes \
          -logFile /dev/stdout \
          -projectPath $(pwd) \
          -buildWindowsPlayer "$HOME/Build/windows/$project.exe" \
          -quit

        BUILD_WIN=$?

    else

        BUILD_WIN=-1

    fi

    BUILD_OSX=0

fi

if [ $BUILD_WIN == 0 ] && [ $BUILD_LINUX == 0 ] && [ $BUILD_OSX == 0 ]; then
    sleep 5
    echo -e "\n\033[32;1mBuild Completed Successfully\033[0m\n"
    exit 0
fi

echo -e "\n\033[31;1mBuild Failed\033[0m\n"
echo -e "\tWindows: ${BUILD_WIN}\n"
echo -e "\tOS X: ${BUILD_OSX}\n"
echo -e "\tLinux: ${BUILD_LINUX}\n"

exit 1
