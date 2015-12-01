#! /bin/bash

# Source config, env and travis_wait
. "$(dirname "$0")/Config/env.sh"  # Sets $TRAVIS_SCRIPTS and $CURRENT_DIR, among others
. "$TRAVIS_SCRIPTS/Config/config.cfg"
. "$TRAVIS_SCRIPTS/travis_wait.sh"

if [ $SKIP_TESTS == 1 ]
then
    echo -e "\n\033[32;1mSkipping tests\033[0m\n"
    exit 0
fi

touch $HOME/men.out
touch $HOME/elves.out

MEN_ERRORS=-1
ELVES_ERRORS=-1

echo -e "\n\033[32;1mRunning tests\033[0m\n"

sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
    "$BUILD_DIR/linux/$COMPILED_NAME" -batchmode --test=$HOME/men.out --player-race=MEN --test-time=15000 \
&& \
sudo -E xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' \
    "$BUILD_DIR/linux/$COMPILED_NAME" -batchmode --test=$HOME/elves.out --player-race=ELVES --test-time=15000

# Save test results
TESTS_RESULT=$?

# Wait for cache upload if appliable
if [ $USE_CACHE == 1 ]
    if [ "$TRAVIS_BRANCH" == "devel-travis_cache" ] && [ "$TRAVIS_PULL_REQUEST" == "false" ]
    then
        echo -n "Waiting for cache to end uploading."
        while [ -f "$HOME/.RSYNC_LOCK" ]
        do
            echo -n "."
            sleep 2
        done
    fi
fi

# Exit or show errors
if [ $TESTS_RESULT == 0 ]
then
    MEN_ERRORS=`cat $HOME/men.out | tail -n1`
    ELVES_ERRORS=`cat $HOME/men.out | tail -n1`

    if [ $MEN_ERRORS == 0 ] && [ $ELVES_ERRORS == 0 ]
    then
        echo -e "\n\033[32;1mTests Completed Successfully\033[0m\n"
        exit 0
    fi

    echo -e "\n\033[31;1mMen execution - Exception log\033[0m\n"
    cat $HOME/men.out

    echo -e "\n\n-----------------\n\n"

    echo -e "\n\033[31;1mElves execution - Exception log\033[0m\n"
    cat $HOME/elves.out

    # Notify on github
    COMMIT_AUTHOR=`git log -1 | grep -Po "(?<=Author: ).*(?= <)"`

    if [[ "$COMMIT_AUTHOR" == *" "* ]]
    then
        COMMIT_AUTHOR="Unkown GitHub username ($COMMIT_AUTHOR)"
    else
        COMMIT_AUTHOR="@$COMMIT_AUTHOR"
    fi

    case $GITHUB_NOTIFICATIONS in
        none)
            ;;
        issue)
            curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
                https://api.github.com/repos/$TRAVIS_REPO_SLUG/issues \
                -d "{\"title\":\"Commit failed tests [$COMMIT_AUTHOR]\",\"body\":\"Commit by: ${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/$TRAVIS_REPO_SLUG/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null
            ;;
        comment)
            curl -i -X POST -H "Authorization: token ${GITHUB_TOKEN}" -H "Content-Type: application/json" \
                https://api.github.com/repos/$TRAVIS_REPO_SLUG/issues/$GITHUB_ISSUE_ID/comments \
                -d "{\"body\":\"Commit failed **tests**.\n\nCommit by: ${COMMIT_AUTHOR}\nBranch: ${TRAVIS_BRANCH}\nCommit hash: ${TRAVIS_COMMIT}\nDetailed log: https://travis-ci.org/$TRAVIS_REPO_SLUG/builds/${TRAVIS_BUILD_ID}\"}" > /dev/null
            ;;
    esac
fi

exit $(($MEN_ERRORS+$ELVES_ERRORS))
