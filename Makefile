REL_PATH := release

.PHONY: release
release:
	rm -rf $(REL_PATH)
	cp -r bin/Release/net8.0-windows7.0 release/
	cd $(REL_PATH); \
		rm Assembly-CSharp.dll libbrotli.dll *.deps.json *.pdb \
		rm -f *.txt
	cp README.md $(REL_PATH)/README.txt
	cp docs/sample.txt release/

.PHONY: clean
clean:
	rm -rf bin/ obj/ $(REL_PATH)/

