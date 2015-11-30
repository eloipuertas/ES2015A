#! /bin/bash

# Source config, env and travis_wait
. "$(dirname "$0")/Config/env.sh"  # Sets $TRAVIS_SCRIPTS and $CURRENT_DIR, among others
. "$TRAVIS_SCRIPTS/Config/config.cfg"
. "$TRAVIS_SCRIPTS/travis_wait.sh"

echo "Home: $HOME"
echo "UnityRoot: $UNITY_ROOT"
echo "User: $USER"

# Variables
BUILD_LINUX=-1

# Create folder if it does not exist
mkdir -p Assets

# Monkey-patch ProjectSettings
echo "Monkey-patching ProjectSettings and AudioManager assets"
sed -i 's/displayResolutionDialog: 1/displayResolutionDialog: 0/g' "$PROJECT_PATH/ProjectSettings/ProjectSettings.asset"
sed -i 's/m_DisableAudio: 0/m_DisableAudio: 1/g' "$PROJECT_PATH/ProjectSettings/AudioManager.asset"

echo "Attempting to start dummy audio driver"
sudo modprobe snd-dummy

# Build navmesh first
echo -e "\nAttempting to build Recast/Detour for Linux [64 bits]"
cd March\ Death/Assets/UnityRecast
mkdir Build
cd Build
cmake ..
make
sudo -E cp Lib/libRecast.so /usr/lib/Recast.so
sudo -E mv Lib/libRecast.so /usr/lib/libRecast.so
sudo -E chmod 0777 /usr/lib/Recast.so
sudo -E chmod 0777 /usr/lib/libRecast.so

cd $HOME/$REPOSITORY_NAME

# Build Unity project
echo -e "\nAttempting to build $COMPILED_NAME for Linux"
sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
    $UNITY_ROOT/Editor/Unity \
      -batchmode \
      -nographics \
      -silent-crashes \
      -logFile /dev/stdout \
      -projectPath "$PROJECT_PATH" \
      -buildLinuxUniversalPlayer "$BUILD_DIR/linux/$COMPILED_NAME" \
      -quit

BUILD_LINUX=$?

if [ $USE_CACHE == 1 ]
then
    if [ "$TRAVIS_BRANCH" == "devel-travis_cache" ] && [ "$TRAVIS_PULL_REQUEST" == "false" ]
    then
        echo -e "\n\033[32;1mUpload to cache server\033[0m\n"
        (touch $HOME/.RSYNC_LOCK; \
            echo -e "\t> Library" && \
                sudo -E tar -zcf "$CURRENT_DIR/Library.tar.gz" "$PROJECT_PATH/Library" && \
                sudo -E rsync -a "$CURRENT_DIR/Library.tar.gz" ${CACHE_HOST}; \
        rm $HOME/.RSYNC_LOCK) &
    fi
fi

if [ $BUILD_LINUX == 0 ]
then
    sudo -E ln -s $BUILD_DIR/linux/$COMPILED_NAME.x86_64 $BUILD_DIR/linux/$COMPILED_NAME

    echo -e "\n\033[32;1mBuild Completed Successfully\033[0m\n"
    exit 0
fi

echo -e "\n\033[31;1mBuild Failed\033[0m\n"
echo -e "\tLinux: ${BUILD_LINUX}\n"

# Notify on github
COMMIT_AUTHOR=`git log -1 | grep -Po "(?<=Author: ).*(?= <)"`

case $GITHUB_NOTIFICATIONS in
    none)
        ;;
    issue)
        curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
            https://api.github.com/repos/$TRAVIS_REPO_SLUG/issues \
            -d "{\"title\":\"Commit failed to build [$COMMIT_AUTHOR]\",\"body\":\"Commit by: @${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/$TRAVIS_REPO_SLUG/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null
        ;;
    comment)
        curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
            https://api.github.com/repos/$TRAVIS_REPO_SLUG/issues/$GITHUB_ISSUE_ID/comments \
            -d "{\"body\":\"Commit failed to **build**.\n\nCommit by: @${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/$TRAVIS_REPO_SLUG/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null
        ;;
esac

exit 1
