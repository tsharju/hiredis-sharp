all: lib example

lib:
	@mkdir -p build
	@gmcs -t:library src/LibHiredis.cs src/RedisClient.cs -out:build/LibHiredis.dll

example: lib
	@mkdir -p bin
	@gmcs -r:build/LibHiredis.dll src/Example.cs -out:build/Example.exe

clean:
	@rm -rf build/*.dll
	@rm -rf build/*.exe
