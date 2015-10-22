# Export vars
export HOME=$(dirname `pwd`)
export UNITY_ROOT=$HOME/unity-editor-5.1.0f3
export BUILD_DIR=$HOME/Build/$TRAVIS_OS_NAME
export project="MarchWars"

touch $(pwd)/men.out
touch $(pwd)/elves.out

$BUILD_DIR/$project -nographics -batchmode --test=$(pwd)/men.out --player-race=MEN --test-time=15000 && \
    $BUILD_DIR/$project -nographics -batchmode --test=$(pwd)/elves.out --player-race=ELVES --test-time=15000

MEN_ERRORS=-1
ELVES_ERRORS=-1

if [ $? == 0 ]; then

    MEN_ERRORS=`cat $(pwd)/men.out | tail -n1`
    ELVES_ERRORS=`cat $(pwd)/men.out | tail -n1`

    if [ $MEN_ERRORS == 0 ] && [ $ELVES_ERRORS == 0 ]; then

        return 0

    fi

    echo "Men execution - Exception log"
    cat $(pwd)/men.out

    echo -e "\n\n-----------------\n\n"

    echo "Elves execution - Exception log"
    cat $(pwd)/elves.out

fi

return $(($MEN_ERRORS+$ELVES_ERRORS))
