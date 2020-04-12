VERSION="1.2.0"

declare -a TARGET_RIDS=(
    "win10-x64"
    "linux-arm"
    "linux-x64"
)

for RID in "${TARGET_RIDS[@]}"
do
    dotnet publish -r $RID -c Release //p:PublishSingleFile=true //p:PublishTrimmed=true -o ./bin
    if [[ $RID == win* ]]
    then
        mv ./bin/NetworkUptimeMonitor.exe ./bin/NetworkUptimeMonitor-$VERSION-$RID.exe
    else
        mv ./bin/NetworkUptimeMonitor ./bin/NetworkUptimeMonitor-$VERSION-$RID
    fi
    rm ./bin/*.pdb
done
