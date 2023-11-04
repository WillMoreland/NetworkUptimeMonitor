source ./ci/env.sh

declare -a TARGET_RIDS=(
    "win-x64"
    "linux-arm"
    "linux-x64"
)

rm -r $BIN_DIR/*

for RID in "${TARGET_RIDS[@]}"
do
    dotnet publish ./NetworkUptimeMonitor/NetworkUptimeMonitor.csproj -r $RID -c Release -o $BIN_DIR

    if [[ $RID == win* ]]
    then
        mv $BIN_DIR$APP_NAME.exe $BIN_DIR$APP_NAME-$VERSION-$RID.exe
    else
        mv $BIN_DIR$APP_NAME $BIN_DIR$APP_NAME-$VERSION-$RID
    fi

    rm $BIN_DIR*.pdb
done
