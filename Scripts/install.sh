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

    ##### LINUX BASED #####
    export INSTALLER_BASED=1
    export HOME=$(dirname `pwd`)
    export UNITY_ROOT=$HOME

    source $(dirname $0)/travis_wait.sh

    echo "Cached Unity: `if [ -f "$UNITY_ROOT/unity-editor-5.1.0f3/Editor/Unity" ]; then echo "True"; else echo "False"; fi`"

    if [ $INSTALLER_BASED == 1 ]; then

        echo 'Downloading from http://download.unity3d.com/download_unity/unity-editor-installer-5.1.0f3+2015091501.sh'
        $(pwd)/Scripts/axel -q -n 10 -o Unity.sh http://download.unity3d.com/download_unity/unity-editor-installer-5.1.0f3+2015082501.sh

        echo 'Monkey-patching installer for non sudo execution and no input'
        sed -i '41,44d;49,50d' ./Unity.sh

        echo 'Extracting Unity from monkey-patched Unity.sh'
        chmod +x ./Unity.sh
        ./Unity.sh -o $UNITY_ROOT

        echo "Install Working Dir: `pwd`"
        echo "Installed to: ${UNITY_ROOT}"

    else

        # Fake UNITY_ROOT
        export UNITY_ROOT=$UNITY_ROOT/unity-editor-5.1.0f3

        echo 'Downloading from http://download.unity3d.com/download_unity/unity-editor-5.1.0f3+2015091501_amd64.deb'
        $(pwd)/Scripts/axel -q -n 10 -o Unity.deb http://download.unity3d.com/download_unity/unity-editor-5.1.0f3+2015091501_amd64.deb

        echo 'Unpacking Unity3D'
        mkdir -p $UNITY_ROOT
        ar -x Unity.deb
        tar xzf data.tar.gz -C $UNITY_ROOT

    fi

    echo 'Creating cache and local'
    mkdir -p $HOME/.cache/unity3d
    mkdir -p $HOME/.local/share/unity3d/Unity

fi
