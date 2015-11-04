# Export vars
export HOME=$(dirname `pwd`)
export UNITY_ROOT=$HOME/unity-editor-5.1.0f3
export BUILD_DIR=$HOME/Build/$TRAVIS_OS_NAME
export project="MarchWars"

touch $(pwd)/men.out
touch $(pwd)/elves.out

MEN_ERRORS=-1
ELVES_ERRORS=-1

echo -e "\n\033[32;1mRunning tests\033[0m\n"

if [ "$TRAVIS_OS_NAME" == "osx" ]; then

    $BUILD_DIR/$project -nographics -batchmode --test=$(pwd)/men.out --player-race=MEN --test-time=15000 && \
        $BUILD_DIR/$project -nographics -batchmode --test=$(pwd)/elves.out --player-race=ELVES --test-time=15000

else

    sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
        $BUILD_DIR/$project -batchmode --test=$(pwd)/men.out --player-race=MEN --test-time=15000 \
    && \
    sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
        $BUILD_DIR/$project -batchmode --test=$(pwd)/elves.out --player-race=ELVES --test-time=15000

fi

# Save test results
TESTS_RESULT=$?

# Wait for cache upload if appliable
if [ "$TRAVIS_BRANCH" == "devel-travis_cache" ] && [ "$TRAVIS_PULL_REQUEST" == "false" ]; then

    echo -n "Waiting for cache to end uploading."
    while [ -f "$HOME/.RSYNC_LOCK" ]
    do
        echo -n "."
        sleep 2
    done

fi

# Exit or show errors
if [ $TESTS_RESULT == 0 ]; then

    MEN_ERRORS=`cat $(pwd)/men.out | tail -n1`
    ELVES_ERRORS=`cat $(pwd)/men.out | tail -n1`

    if [ $MEN_ERRORS == 0 ] && [ $ELVES_ERRORS == 0 ]; then

        echo -e "\n\033[32;1mTests Completed Successfully\033[0m\n"
        exit 0

    fi

    echo -e "\n\033[31;1mMen execution - Exception log\033[0m\n"
    cat $(pwd)/men.out

    echo -e "\n\n-----------------\n\n"

    echo -e "\n\033[31;1mElves execution - Exception log\033[0m\n"
    cat $(pwd)/elves.out

    # Notify on github
    COMMIT_AUTHOR=`git log -1 | grep -Po "(?<=Author: ).*(?= <)"`
    curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
        https://api.github.com/repos/eloipuertas/ES2015A/issues/193/comments \
        -d "{\"body\":\"Commit by: @${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/eloipuertas/ES2015A/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null

fi

exit $(($MEN_ERRORS+$ELVES_ERRORS))
