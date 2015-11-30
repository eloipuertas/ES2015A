# PLEASE DO NOT TOUCH THIS FILE UNLESS YOU KNOW EXACTLY WHAT YOU ARE DOING!

# Setup paths and names
TRAVIS_SCRIPTS="$(dirname "$0")"
CURRENT_DIR="$(pwd)"
REPOSITORY_NAME="${CURRENT_DIR##*/}"
PROJECT_NAME=""

# Export for other programs
export PATH="$PATH:$CURRENT_DIR/bin"
export HOME=$(dirname "$CURRENT_DIR")

# Setup Unity 3D paths
UNITY_INSTALL=$HOME
UNITY_ROOT=$UNITY_INSTALL/unity-editor-5.1.0f3
UNITY_URL="http://download.unity3d.com/download_unity/unity-editor-installer-5.1.0f3+2015091501.sh"
BUILD_DIR=$HOME/Build

# Find Unity project folder
# Find ProjectSettings folder
PROJECT_NAME=$(find $HOME/$REPOSITORY_NAME -type d -iname "ProjectSettings" -print0)
# Find base dirname (parent dir)
PROJECT_NAME=$(dirname "$PROJECT_NAME")
# Get only the path beyond $REPOSITORY_NAME
PROJECT_NAME=${PROJECT_NAME##*/$REPOSITORY_NAME/}

# If project is the very same root folder, use .
if [ -z "$PROJECT_NAME" ]
then
    PROJECT_NAME="."
fi

# Build project path
PROJECT_PATH="$HOME/$REPOSITORY_NAME/$PROJECT_NAME"
