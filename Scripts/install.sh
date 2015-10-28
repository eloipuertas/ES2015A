#! /bin/bash

if [ "$TRAVIS_OS_NAME" == "osx" ]; then

    curl -o Axel.pkg https://raw.githubusercontent.com/rudix-mac/packages/2015/10.9/axel-2.4-0.pkg
    sudo installer -package Axel.pkg -target /

    echo 'Downloading from http://netstorage.unity3d.com/unity/3757309da7e7/MacEditorInstaller/Unity-5.2.2f1.pkg: '
    #curl -o Unity.pkg http://netstorage.unity3d.com/unity/3757309da7e7/MacEditorInstaller/Unity-5.2.2f1.pkg
    axel -q -n 10 -o Unity.pkg http://netstorage.unity3d.com/unity/3757309da7e7/MacEditorInstaller/Unity-5.2.2f1.pkg

    echo 'Installing Unity.pkg'
    sudo installer -dumplog -package Unity.pkg -target /

else

    sudo -E apt-get -yq update
    sudo -E apt-get -yq --no-install-suggests --no-install-recommends --force-yes install build-essential debconf gconf-service lib32gcc1 lib32stdc++6 libasound2 libc6 libc6-i386 libcairo2 libcap2 libcups2 libdbus-1-3 libexpat1 libfontconfig1 libfreetype6 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libgl1-mesa-glx libglib2.0-0 libglu1-mesa libgtk2.0-0 libnspr4 libnss3 libpango1.0-0 libstdc++6 libx11-6 libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxtst6 zlib1g libpng12-0 xvfb

    export HOME=$(dirname `pwd`)
    export UNITY_ROOT=$HOME

    source $(dirname $0)/travis_wait.sh

    echo 'Downloading from http://download.unity3d.com/download_unity/unity-editor-installer-5.1.0f3+2015091501.sh'
    $(pwd)/Scripts/axel -q -n 10 -o Unity.sh http://download.unity3d.com/download_unity/unity-editor-installer-5.1.0f3+2015082501.sh

    echo 'Background downloading cache'
    # Use ; and not && to actually do all of them, even if one doesn't succeed
    (touch $HOME/.RSYNC_LOCK; \
        echo -e "\t> Temp"    && sudo -E rsync -rlptgD ${CACHE_HOST}Temp    $HOME/ES2015A/March\ Death/; \
        echo -e "\t> Obj"     && sudo -E rsync -rlptgD ${CACHE_HOST}Obj     $HOME/ES2015A/March\ Death/; \
        echo -e "\t> Library" && sudo -E rsync -rlptgD ${CACHE_HOST}Library $HOME/ES2015A/March\ Death/; \
        echo -e "\t> Build"   && sudo -E rsync -rlptgD ${CACHE_HOST}Build   $HOME/; \
    rm $HOME/.RSYNC_LOCK) &

    echo 'Monkey-patching installer for non sudo execution and no input'
    sed -i '41,44d;49,50d' ./Unity.sh

    echo 'Extracting Unity from monkey-patched Unity.sh'
    chmod +x ./Unity.sh
    ./Unity.sh -o $UNITY_ROOT

    echo "Install Working Dir: `pwd`"
    echo "Installed to: ${UNITY_ROOT}"

    echo 'Creating cache and local'
    mkdir -p $HOME/.cache/unity3d
    mkdir -p $HOME/.local/share/unity3d/Unity

    echo -n "Waiting for cache to end downloading."
    while [ -f "$HOME/.RSYNC_LOCK" ]
    do
        echo -n "."
        sleep 2
    done

    echo -e "\n\033[32;1mDone installing\033[0m\n"

fi
