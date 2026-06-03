debug:
	msbuild -restore HffArchipelagoClient.sln /property:Configuration="Debug"

release:
	msbuild -restore HffArchipelagoClient.sln /property:Configuration="Release"

copy:
	-pkill Human
	cp build/bin/output/HffArchipelagoClient.dll ~/.steam/steam/steamapps/common/"Human Fall Flat"/BepInEx/plugins/HffArchipelagoClient.dll

run: copy
	(steam steam://rungameid/477160 &)

.PHONY: clean

clean:
	rm -rf build/obj
	rm -rf build/bin
