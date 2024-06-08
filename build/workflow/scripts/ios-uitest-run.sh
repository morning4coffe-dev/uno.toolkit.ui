﻿#!/bin/bash
set -euo pipefail
IFS=$'\n\t'


if [ "$UITEST_TEST_MODE_NAME" == 'Automated' ];
then
	export TEST_FILTERS="FullyQualifiedName !~ Uno.Toolkit.UITest.RuntimeTests";
elif [ "$UITEST_TEST_MODE_NAME" == 'RuntimeTests' ];
then
	export TEST_FILTERS="FullyQualifiedName ~ Uno.Toolkit.UITest.RuntimeTests";
fi

export UNO_UITEST_PLATFORM=iOS
export BASE_ARTIFACTS_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/ios/$XAML_FLAVOR_BUILD/$UITEST_TEST_MODE_NAME
export UNO_UITEST_IOS_PROJECT_PATH=$BUILD_SOURCESDIRECTORY/samples/$SAMPLE_PROJECT_NAME/$SAMPLE_PROJECT_NAME.Mobile
export UNO_UITEST_IOSBUNDLE_PATH=$UNO_UITEST_IOS_PROJECT_PATH/bin/Release/net8.0-ios/iossimulator-x64/$SAMPLE_PROJECT_NAME.Mobile.app
export UNO_UITEST_SCREENSHOT_PATH=$BASE_ARTIFACTS_PATH/screenshots
export UNO_UITEST_PROJECT_PATH=$BUILD_SOURCESDIRECTORY/src/Uno.Toolkit.UITest
export UNO_UITEST_PROJECT=$UNO_UITEST_PROJECT_PATH/Uno.Toolkit.UITest.csproj
export UNO_UITEST_LOGFILE=$UNO_UITEST_SCREENSHOT_PATH/nunit-log.txt
export UNO_UITEST_IOS_PROJECT=$UNO_UITEST_IOS_PROJECT_PATH/$SAMPLE_PROJECT_NAME.Mobile.csproj
export UNO_UITEST_BINARY=$BUILD_SOURCESDIRECTORY/build/toolkit-uitest-binaries/Uno.Toolkit.UITest.dll
export UNO_UITEST_NUNIT_VERSION=3.12.0
export UNO_UITEST_NUGET_URL=https://dist.nuget.org/win-x86-commandline/v5.7.0/nuget.exe
export UNO_ORIGINAL_TEST_RESULTS=$BUILD_SOURCESDIRECTORY/build/$UNO_TEST_RESULTS_FILE_NAME
export UNO_UITEST_RUNTIMETESTS_RESULTS_FILE_PATH=$UNO_ORIGINAL_TEST_RESULTS
export UNO_TESTS_RESPONSE_FILE=$BUILD_SOURCESDIRECTORY/build/nunit.response
export UNO_UITEST_SIMULATOR_VERSION="com.apple.CoreSimulator.SimRuntime.iOS-17-0"
export UNO_UITEST_SIMULATOR_NAME="iPad Pro (12.9-inch) (6th generation)"

export UITEST_TEST_TIMEOUT=120m

echo "Listing iOS simulators"
xcrun simctl list devices --json

/Applications/Xcode.app/Contents/Developer/Applications/Simulator.app/Contents/MacOS/Simulator &

# Prime the output directory
mkdir -p $UNO_UITEST_SCREENSHOT_PATH/_logs

cd $UNO_UITEST_IOS_PROJECT_PATH
dotnet build -f net8.0-ios -c Release /p:RuntimeIdentifier=iossimulator-x64 /p:IsUiAutomationMappingEnabled=True /bl:$BASE_ARTIFACTS_PATH/ios-$XAML_FLAVOR_BUILD-uitest.binlog

##
## Pre-install the application to avoid https://github.com/microsoft/appcenter/issues/2389
##

## Install iOS 17.0 simulators
xcodes runtimes install --keep-archive 'iOS 17.0' || true

# Wait while ios runtime 17.0 is not having simulators. The install process may
# take a few seconds and "simctl list devices" may not return devices.
while true; do
	export UITEST_IOSDEVICE_ID=`xcrun simctl list -j | jq -r --arg sim "$UNO_UITEST_SIMULATOR_VERSION" --arg name "$UNO_UITEST_SIMULATOR_NAME" '.devices[$sim] | .[] | select(.name==$name) | .udid'`
	export UITEST_IOSDEVICE_DATA_PATH=`xcrun simctl list -j | jq -r --arg sim "$UNO_UITEST_SIMULATOR_VERSION" --arg name "$UNO_UITEST_SIMULATOR_NAME" '.devices[$sim] | .[] | select(.name==$name) | .dataPath'`

	if [ -n "$UITEST_IOSDEVICE_ID" ]; then
		break
	fi

	echo "Waiting for the simulator to be available"
	sleep 5
done

echo "Simulator Data Path: $UITEST_IOSDEVICE_DATA_PATH"
cp "$UITEST_IOSDEVICE_DATA_PATH/../device.plist" $UNO_UITEST_SCREENSHOT_PATH/_logs

echo "Starting simulator: [$UITEST_IOSDEVICE_ID] ($UNO_UITEST_SIMULATOR_VERSION / $UNO_UITEST_SIMULATOR_NAME)"

# check for the presence of idb, and install it if it's not present
export PATH=$PATH:~/.local/bin

if ! command -v idb &> /dev/null
then
	echo "Installing idb"
	brew install pipx
	# # https://github.com/microsoft/appcenter/issues/2605#issuecomment-1854414963
	brew tap facebook/fb
	brew install idb-companion
	pipx install fb-idb
else
	echo "Using idb from:" `command -v idb`
fi

xcrun simctl boot "$UITEST_IOSDEVICE_ID" || true

idb install --udid "$UITEST_IOSDEVICE_ID" "$UNO_UITEST_IOSBUNDLE_PATH"

cd $UNO_UITEST_PROJECT_PATH

## Run tests
dotnet test \
	-c Release \
	-l:"console;verbosity=normal" \
	--logger "nunit;LogFileName=$UNO_UITEST_RUNTIMETESTS_RESULTS_FILE_PATH" \
	--filter "$TEST_FILTERS" \
	--blame-hang-timeout $UITEST_TEST_TIMEOUT \
	-v m \
	-property:FrameworkLineage=$XAML_FLAVOR_BUILD \
	|| true

echo "Current system date"
date

## Copy the results file to the results folder
cp $UNO_UITEST_RUNTIMETESTS_RESULTS_FILE_PATH $BASE_ARTIFACTS_PATH

# export the simulator logs
export LOG_FILEPATH=$UNO_UITEST_SCREENSHOT_PATH/_logs
export TMP_LOG_FILEPATH=/tmp/DeviceLog-`date +"%Y%m%d%H%M%S"`.logarchive
export LOG_FILEPATH_FULL=$LOG_FILEPATH/DeviceLog-`date +"%Y%m%d%H%M%S"`.txt

xcrun simctl spawn booted log collect --output $TMP_LOG_FILEPATH
log show --style syslog $TMP_LOG_FILEPATH > $LOG_FILEPATH_FULL
